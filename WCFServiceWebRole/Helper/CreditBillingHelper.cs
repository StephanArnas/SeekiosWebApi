using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using System.Web;
using WCFServiceWebRole.Data.DTO;
using WCFServiceWebRole.Data.ERROR;
using WCFServiceWebRole.Enum;

namespace WCFServiceWebRole.Helper
{
    public static class CreditBillingHelper
    {
        #region ----- PUBLIC METHODS --------------------------------------------------------------------------

        /// <summary>
        /// Pay the amount of the operation
        /// Debit seekios free credit or user credit or both
        /// </summary>
        /// <param name="seekiosEntities">the database context</param>
        /// <param name="operationTypeEnum">the operation type</param>
        /// <param name="idUser">id user</param>
        /// <param name="idMode">id mode</param>
        /// <param name="idSeekios">id seekios</param>
        /// <param name="minUserCreditThreshold">the minimum left on the account</param>
        /// <returns>return 1 if it's working</returns>
        public static int PayOperationCost(seekios_dbEntities seekiosEntities
            , OperationType operationTypeEnum
            , int idUser
            , int? idMode
            , int idSeekios
            , int minUserCreditThreshold = 0)
        {
            //bool alreadyCommit = false;
            //using (DbContextTransaction dbContextTransaction = seekiosEntities.Database.BeginTransaction(System.Data.IsolationLevel.Snapshot))
            //{
            try
            {
                // Get user
                var userDb = (from u in seekiosEntities.user
                              where u.iduser == idUser
                              select u).FirstOrDefault();
                if (userDb == null) return -1;

                // Retrieves all seekioses for the current user, order by freecredits...
                var seekiosAndSeekiosProductionBdd = (from s in seekiosEntities.seekiosAndSeekiosProduction
                                                      where s.user_iduser == idUser
                                                      orderby s.freeCredit descending
                                                      select s).ToArray();
                int sumOfseekiosFreeCredits = seekiosAndSeekiosProductionBdd.Sum(s => s.freeCredit);
                int amountToPay = operationTypeEnum.GetAmount();
                int sumOfTotalCredit = (!userDb.remainingRequest.HasValue ? 0 : userDb.remainingRequest.Value) + sumOfseekiosFreeCredits + amountToPay;
                var uidSeekiosToNotify = string.Empty;

                // Can not pay the operation, not enough credit 
                if (sumOfTotalCredit < minUserCreditThreshold)
                {
                    // Delete the mode
                    if (idMode.HasValue)
                    {
                        // WARNING : Hotfix, the mobile app should display the tracking is not available for the seekios because the seekios will receive M01 (wait state)
                        //var result = new System.Data.Entity.Core.Objects.ObjectParameter("Result", 0);
                        //seekiosEntities.DeleteModeById(idMode, result);
                        SeekiosService.PrepareInstructionForNewMode(seekiosEntities
                            , null
                            , (from s in seekiosEntities.seekios where s.idseekios == idSeekios select s).First());
                    }
                    return -2;
                }

                // Pay the operation with free credit
                // Take the seekios with the value freeCredit more than the amount requested to pay
                var now = DateTime.UtcNow;
                var oneSeekiosToPay = seekiosAndSeekiosProductionBdd.FirstOrDefault(f => f.freeCredit > -amountToPay);
                if (oneSeekiosToPay != null)
                {
                    // Pay the operation (decrement the freeCredit value)
                    seekiosEntities.UpdateSeekiosFreeCreditById(oneSeekiosToPay.idseekios, amountToPay);
                    AddOperationInDatabase(seekiosEntities, now, idMode, idSeekios, idUser, operationTypeEnum, amountToPay, true);
                    uidSeekiosToNotify = oneSeekiosToPay.uidSeekios;
                    amountToPay = 0;
                }
                else
                {
                    // Not enought on one seekios but more than one seekios
                    var moreThanOneSeekiosToPay = seekiosAndSeekiosProductionBdd.Where(w => w.freeCredit > 0);
                    foreach (var seekiosToPay in moreThanOneSeekiosToPay)
                    {
                        if (amountToPay + seekiosToPay.freeCredit >= 0)
                        {
                            seekiosEntities.UpdateSeekiosFreeCreditById(seekiosToPay.idseekios, amountToPay);
                            AddOperationInDatabase(seekiosEntities, now, idMode, idSeekios, idUser, operationTypeEnum, amountToPay, true);
                            uidSeekiosToNotify = seekiosToPay.uidSeekios;
                            amountToPay = 0;
                            break;
                        }
                        else
                        {
                            seekiosEntities.UpdateSeekiosFreeCreditById(seekiosToPay.idseekios, -seekiosToPay.freeCredit);
                            AddOperationInDatabase(seekiosEntities, now, idMode, idSeekios, idUser, operationTypeEnum, -seekiosToPay.freeCredit, true);
                            amountToPay += seekiosToPay.freeCredit;
                        }
                    }
                }
                int userDebit = 0;
                int seekiosDebit = operationTypeEnum.GetAmount() - amountToPay;

                // The operation is not finish to pay, we need to use the user credit 
                if (amountToPay < 0)
                {
                    userDb.remainingRequest += amountToPay;
                    AddOperationInDatabase(seekiosEntities, now, idMode, idSeekios, idUser, operationTypeEnum, amountToPay, false);
                    userDebit = amountToPay;
                }

                // Save changes and commit
                seekiosEntities.SaveChanges();
                //dbContextTransaction.Commit();
                //alreadyCommit = true;

                // Broadcast user devices
                SignalRHelper.BroadcastUser(HubProxyEnum.CreditsHub
                    , SignalRHelper.METHOD_REFRESH_CREDIT
                    , new object[]
                    {
                        userDb.iduser,
                        uidSeekiosToNotify,
                        userDebit,
                        seekiosDebit,
                        now
                    });
                return 1;
            }
            catch (Exception ex)
            {
                //if (!alreadyCommit) dbContextTransaction.Rollback();
                throw ex;
            }
            //finally
            //{
            //    dbContextTransaction.Dispose();
            //}
            //}
        }

        /// <summary>
        /// Return true if the seekios has enough credit to do the operation
        /// </summary>
        /// <param name="uidSeekios">seekios unique identifier</param>
        /// <param name="minThreshold">the minimum to have</param>
        public static bool SeekiosCanAffordAction(seekios_dbEntities seekiosEntities
            , string uidSeekios
            , int minThreshold = 0)
        {
            var user = (from sp in seekiosEntities.seekiosProduction
                        where sp.uidSeekios == uidSeekios
                        join s in seekiosEntities.seekios on sp.idseekiosProduction equals s.idseekios
                        join u in seekiosEntities.user on s.user_iduser equals u.iduser
                        select u).Take(1).FirstOrDefault();
            if (user == null) return false;
            int creditsOfferedLeft = (from s in seekiosEntities.seekios
                                      where s.user_iduser == user.iduser
                                      join sp in seekiosEntities.seekiosProduction on s.idseekios equals sp.idseekiosProduction
                                      select sp.freeCredit).Sum();
            int totalCredits = (user.remainingRequest != null ? user.remainingRequest.Value : 0) + creditsOfferedLeft;
            return (totalCredits > minThreshold);
        }

        /// <summary>
        /// Give credits to the user account
        /// </summary>
        /// <param name="seekiosEntities">the database context</param>
        /// <param name="userBdd">user from the database</param>
        /// <param name="boughtPackBdd">the credit pack from the database</param>
        /// <param name="purchase">the purchase bought by the user</param>
        /// <param name="timeOfPayment">datetime of the payment</param>
        /// <returns>return 1 if it's working</returns>
        public static int GiveCreditsToUser(seekios_dbEntities seekiosEntities /*, bool isSubscription*/
            , user userBdd
            , packCreditAndOperationType boughtPackBdd
            , PurchaseDTO purchase
            , DateTime? timeOfPayment)
        {
            //using (DbContextTransaction dbContextTransaction = seekiosEntities.Database.BeginTransaction(System.Data.IsolationLevel.Snapshot))
            //{
            try
            {
                // useless : subscription
                //// if subscription, have to seek for the operation type for subscription, we also could do a +x given the id of each pack, but it's more dirty... (+add index for operationName)
                //if (isSubscription) 
                //{
                //    string actualPackId = boughtPack.operationName + (isSubscription ? "Subscription" : "");
                //    var opType = (from o in seekiosEntities.operationType where o.operationName == actualPackId select o).Take(1).FirstOrDefault();
                //    if (opType == null) return 7000;//le pack est introuvable...
                //}

                // Create the operation transaction
                var dateTransactionNow = timeOfPayment.HasValue ? timeOfPayment.Value : DateTime.UtcNow;
                var purchaseDb = new operationFromStore()
                {
                    creditsPurchased = boughtPackBdd.rewarding,//boughtPack.rewarding, //on va enlever ca ?
                    idPack = boughtPackBdd.idcreditPack,
                    dateTransaction = dateTransactionNow,
                    idUser = purchase.IdUser,
                    isPackPremium = false,//isSubscription,
                    refStore = purchase.InnerData,
                    status = "OK", // TODO : insertPurchase when user clicks on buy, put status Pending and when validated put OK. else put Canceled.
                    versionApp = purchase.VersionApp,
                };
                // Create an operation
                var operationToAdd = new operation()
                {
                    dateBeginOperation = dateTransactionNow,
                    dateEndOperation = dateTransactionNow,
                    mode_idmode = null,
                    seekios_idseekios = null,
                    user_iduser = purchase.IdUser,
                    operationType_idoperationType = boughtPackBdd.idoperationType,
                    amount = boughtPackBdd.amount, // not that uselful
                    isOnSeekios = false,
                };
                // Add credit to user account
                userBdd.remainingRequest += boughtPackBdd.amount;
                // Add operation and and operation transaction
                seekiosEntities.operation.Add(operationToAdd);
                seekiosEntities.operationFromStore.Add(purchaseDb);
                seekiosEntities.SaveChanges();
                //dbContextTransaction.Commit();
            }
            catch (Exception ex)
            {
                //dbContextTransaction.Rollback();
                throw ex;
            }
            finally
            {
                //dbContextTransaction.Dispose();
                //seekiosEntities.Dispose();
            }
            //}
            return 1;
        }

        /// <summary>
        /// Check if the user remains credits
        /// </summary>
        /// <param name="seekiosEntities">the database context</param>
        /// <param name="user">user object</param>
        /// <param name="minThreshold">the minimum to compare</param>
        /// <returns>true : the user still have credits | false : the user don't have enough credits</returns>
        public static bool UserCanAffordAction(seekios_dbEntities seekiosEntities
            , user user
            , int minThreshold = 0)
        {
            if (user == null) return false;
            int creditsOfferedLeft = (from s in seekiosEntities.seekios
                                      where s.user_iduser == user.iduser
                                      join sp in seekiosEntities.seekiosProduction on s.idseekios equals sp.idseekiosProduction
                                      select sp.freeCredit).Sum();
            int totalCredits = (user.remainingRequest != null ? user.remainingRequest.Value : 0) + creditsOfferedLeft;
            return (totalCredits > minThreshold);
        }

        /// <summary>
        /// Add operation to the context database
        /// </summary>
        /// <param name="seekiosEntities">the context database</param>
        /// <param name="now">begining time</param>
        /// <param name="idMode">id mode</param>
        /// <param name="idSeekios">id seekios</param>
        /// <param name="idUser">id user</param>
        /// <param name="operationTypeEnum">the operation type</param>
        /// <param name="amount">the amount to debit</param>
        /// <param name="isOnSeekios">is the transaction concern a seekios</param>
        public static void AddOperationInDatabase(seekios_dbEntities seekiosEntities
             , DateTime now
            , int? idMode
            , int idSeekios
            , int idUser
            , OperationType operationTypeEnum
            , int amount
            , bool isOnSeekios)
        {
            var operationDb = new operation
            {
                dateBeginOperation = now,
                mode_idmode = idMode,
                seekios_idseekios = idSeekios,
                user_iduser = idUser,
                operationType_idoperationType = (int)operationTypeEnum,
                amount = amount,
                isOnSeekios = isOnSeekios,
                dateEndOperation = DateTime.UtcNow
            };
            seekiosEntities.operation.Add(operationDb);
        }

        #endregion
    }
}