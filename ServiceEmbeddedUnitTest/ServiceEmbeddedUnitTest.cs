using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceSeekiosUnitTest.Helper;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Z.EntityFramework.Plus;
using WCFServiceWebRole.Enum;

namespace ServiceEmbeddedUnitTest
{
    [TestClass]
    public class ServiceEmbeddedUnitTest
    {
        #region ----- PRIVATE VARIABLES -----------------------------------------------------------------------

        private const string BASE_URL_PROD = "http://seekiosembedded.cloudapp.net/SES.svc/";
        private const string BASE_URL_STAGING = "http://8889c4a6e7244ea2a5e81921feb858af.cloudapp.net/SES.svc/";
        private const string BASE_URL_LOCAL = "";
        private static string BASE_URL = BASE_URL_PROD;

        private static string TIMESTAMP { get { return ((int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds)).ToString(); } }
        private static TimeSpan _onDemandResponseTimeout = new TimeSpan(0, 3, 5);

        private const string UID_SEEKIOS = "NoToUch1";
        private const int ID_SEEKIOS = 1;
        private const string CELL_DATA = "208;10;4f45,c319,208,10;1938,0daf,208,10;1055,c319,208,10";
        private const int BATTERY_LIFE_SEEKIOS = 60;
        private const int SIGNAL_QUALITY_SEEKIOS = 50;
        private const string LATITUDE = "43.442871";
        private const string LONGITUDE = "-0.327639";
        private const string ALTITUDE = "0";
        private const string ACCURACY = "0";
        private const int ID_DEVICE = 1766;

        private static string URL_GSI = BASE_URL + "GSI";
        private static string URL_RODR = BASE_URL + "RODR";
        private static string URL_RODRBCD = BASE_URL + "RODRBCD";
        private static string URL_NSOOZ = BASE_URL + "NSOOZ";
        private static string URL_NSM = BASE_URL + "NSM";
        private static string URL_ANTL = BASE_URL + "ANTL";
        private static string URL_ANZTL = BASE_URL + "ANZTL";
        private static string URL_ANDMTL = BASE_URL + "ANDMTL";
        private static string URL_SSOS = BASE_URL + "SSOS";
        private static string URL_SSOSL = BASE_URL + "SSOSL";
        private static string URL_SSOSLBCD = BASE_URL + "SSOSLBCD";

        #endregion

        #region ----- CONSTRUCTOR -----------------------------------------------------------------------------

        public ServiceEmbeddedUnitTest()
        {

        }

        #endregion

        #region ----- PUBLIC METHODS --------------------------------------------------------------------------

        [TestMethod]
        public void GetSeekiosInstructions()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                try
                {
                    var result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}"
                        , URL_GSI
                        , UID_SEEKIOS
                        , BATTERY_LIFE_SEEKIOS
                        , SIGNAL_QUALITY_SEEKIOS
                        , 0
                        , TIMESTAMP));
                    var listOfInstruction = JsonConvert.DeserializeObject<List<string>>(result);
                    Assert.IsTrue(true);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void RespondOnDemandRequest()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                try
                {
                    RemoveAllSeekiosInstructions();
                    var result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/{8}"
                        , URL_RODR
                        , UID_SEEKIOS
                        , BATTERY_LIFE_SEEKIOS
                        , SIGNAL_QUALITY_SEEKIOS
                        , LATITUDE
                        , LONGITUDE
                        , ALTITUDE
                        , ACCURACY
                        , TIMESTAMP));
                    if (result == "1") Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void RespondOnDemandRequestByCellsData()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                try
                {
                    RemoveAllSeekiosInstructions();
                    var result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}"
                        , URL_RODRBCD
                        , UID_SEEKIOS
                        , BATTERY_LIFE_SEEKIOS
                        , SIGNAL_QUALITY_SEEKIOS
                        , CELL_DATA
                        , TIMESTAMP));
                    if (result == "1") Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void NotifySeekiosOutOfZone()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                try
                {
                    SetMode(ModeDefinitions.ModeZone);
                    var result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/{8}"
                        , URL_NSOOZ
                        , UID_SEEKIOS
                        , BATTERY_LIFE_SEEKIOS
                        , SIGNAL_QUALITY_SEEKIOS
                        , LATITUDE
                        , LONGITUDE
                        , ALTITUDE
                        , ACCURACY
                        , TIMESTAMP));
                    if (result == "1") Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void NotifySeekiosMoved()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                try
                {
                    SetMode(ModeDefinitions.ModeDontMove);
                    var result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}"
                        , URL_NSM
                        , UID_SEEKIOS
                        , BATTERY_LIFE_SEEKIOS
                        , SIGNAL_QUALITY_SEEKIOS
                        , TIMESTAMP));
                    if (result == "1") Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void AddNewTrackingLocation()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                try
                {
                    SetMode(ModeDefinitions.ModeTracking);
                    var result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/{8}"
                        , URL_ANTL
                        , UID_SEEKIOS
                        , BATTERY_LIFE_SEEKIOS
                        , SIGNAL_QUALITY_SEEKIOS
                        , LATITUDE
                        , LONGITUDE
                        , ALTITUDE
                        , ACCURACY
                        , TIMESTAMP));
                    if (result == "1") Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void AddNewZoneTrackingLocation()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                try
                {
                    SetMode(ModeDefinitions.ModeZone);
                    var result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/{8}"
                        , URL_ANZTL
                        , UID_SEEKIOS
                        , BATTERY_LIFE_SEEKIOS
                        , SIGNAL_QUALITY_SEEKIOS
                        , LATITUDE
                        , LONGITUDE
                        , ALTITUDE
                        , ACCURACY
                        , TIMESTAMP));
                    if (result == "1") Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void AddNewDontMoveTrackingLocation()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                try
                {
                    SetMode(ModeDefinitions.ModeDontMove);
                    var result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/{8}"
                        , URL_ANDMTL
                        , UID_SEEKIOS
                        , BATTERY_LIFE_SEEKIOS
                        , SIGNAL_QUALITY_SEEKIOS
                        , LATITUDE
                        , LONGITUDE
                        , ALTITUDE
                        , ACCURACY
                        , TIMESTAMP));
                    if (result == "1") Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void SendSOSLocation()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                try
                {
                    //SetMode(ModeDefinitions.ModeDontMove);
                    var result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/{8}"
                        , URL_SSOSL
                        , UID_SEEKIOS
                        , BATTERY_LIFE_SEEKIOS
                        , SIGNAL_QUALITY_SEEKIOS
                        , LATITUDE
                        , LONGITUDE
                        , ALTITUDE
                        , ACCURACY
                        , TIMESTAMP));
                    if (result == "1") Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void SendSOS()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                try
                {
                    SetMode(ModeDefinitions.ModeDontMove);
                    var result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}"
                        , URL_SSOS
                        , UID_SEEKIOS
                        , BATTERY_LIFE_SEEKIOS
                        , SIGNAL_QUALITY_SEEKIOS
                        , TIMESTAMP));
                    if (result == "1") Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void SendSOSLocationByCellsData()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                try
                {
                    RemoveAllSeekiosInstructions();
                    var result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}"
                        , URL_SSOSLBCD
                        , UID_SEEKIOS
                        , BATTERY_LIFE_SEEKIOS
                        , SIGNAL_QUALITY_SEEKIOS
                        , CELL_DATA
                        , TIMESTAMP));
                    if (result == "1") Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        #endregion

        #region ----- PRIVATE METHODS -------------------------------------------------------------------------

        private void RemoveAllSeekiosInstructions()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                // remove all seekios instruction
                seekiosEntities.seekiosInstruction
                        .Where(x => x.seekiosProduction_idseekiosProduction == ID_SEEKIOS)
                        .Delete(x => x.BatchSize = 1000);
                // add a new seekios instruction (it's require because of this line in the SES : 
                // if (onDemandInstructionDate == null || onDemandInstructionDate.Add(_onDemandResponseTimeout) < DateTime.UtcNow)
                var seekiosInstructionToAdd = new seekiosInstruction()
                {
                    dateCreation = DateTime.UtcNow,
                    instructionType = (int)InstructionType.OnDemand,
                    instruction = "#M02&",
                    seekiosProduction_idseekiosProduction = ID_SEEKIOS
                };
                seekiosEntities.seekiosInstruction.Add(seekiosInstructionToAdd);
                seekiosEntities.SaveChanges();
            }
        }

        private void SetMode(ModeDefinitions mode)
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                // set device 
                //var deviceDb = seekiosEntities.device.FirstOrDefault(x => x.iddevice == ID_DEVICE);
                //if (deviceDb != null)
                //{
                //    deviceDb = new device()
                //    {
                //        countryCode = "fr",
                //        deviceName = "deviceUnitTest",
                //        doNotDisturb = 1,
                //        macAdress = string.Empty,
                //        notificationPlayerId = string.Empty,
                //        os = "iOS",
                //        plateform = "iOS",
                //        uidDevice = Guid.NewGuid().ToString(),
                //        user_iduser = 
                //    };
                //}
                // update the mode
                if (seekiosEntities.mode.Any(x => x.seekios_idseekios == ID_SEEKIOS))
                {
                    seekiosEntities.mode
                        .Where(x => x.seekios_idseekios == ID_SEEKIOS)
                        .OrderBy(x => x.idmode)
                        .Take(1)
                        .Update(x => new mode() { modeDefinition_idmodeDefinition = (int)mode } );
                }
                else
                {
                    // add the mode
                    var modeToAdd = new mode
                    {
                        device_iddevice = ID_DEVICE,
                        countOfTriggeredAlert = 0,
                        dateModeCreation = DateTime.UtcNow,
                        lastTriggeredAlertDate = null,
                        modeDefinition_idmodeDefinition = (int)mode,
                        seekios_idseekios = ID_SEEKIOS,
                        statusDefinition_idstatusDefinition = (int)StatusDefinition.RAS,
                        trame = "1"
                    };
                    seekiosEntities.mode.Add(modeToAdd);
                    seekiosEntities.SaveChanges();
                }
            }
        }

        #endregion
    }
}