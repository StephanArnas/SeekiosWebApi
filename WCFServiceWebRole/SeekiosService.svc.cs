using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Activation;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.Data.Entity;
using System.Configuration;
using Microsoft.ApplicationInsights.Extensibility;
using System.ServiceModel.Web;
using WCFServiceWebRole.Data.DB;
using WCFServiceWebRole.Data.DTO;
using WCFServiceWebRole.Data.ERROR;
using WCFServiceWebRole.Data.LOCAL;
using WCFServiceWebRole.Enum;
using WCFServiceWebRole.ExceptionLog;
using WCFServiceWebRole.Extension;
using WCFServiceWebRole.Helper;
using WCFServiceWebRole.Security;
using Newtonsoft.Json;
using System.ServiceModel;
using Z.EntityFramework.Plus;

// scale performance : 
// http://www.dotnetfunda.com/articles/show/3485/11-tips-to-improve-wcf-restful-services-performance

namespace WCFServiceWebRole
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall
        , ConcurrencyMode = ConcurrencyMode.Multiple)
        , AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
        , ErrorHandlerAttribute(typeof(ErrorHandler))]
    public class SeekiosService : ISeekiosService
    {
        #region ----- VARIABLES -------------------------------------------------------------------------------

        public static bool IsStaging = false;// automatically updated by CheckEnvironnment()

        private static List<string> _languageCreditPack = new List<string> { "fr", "en" };

        private static string START_TRAME = "#M{0:00}";
        private static string FOOTER_TRAME = "&";

        private static object AccessToSeekiosInstructions = new object();
        private const int REQUEST_TIMEOUT = 150000;
        private const int MAX_COUNT_OF_ELEMENTS_IN_SEARCH_REQUEST = 10;
        private const int MAX_DAY_FOR_VALIDATION_USER_ACCOUNT = -14;
        private const int MAX_RETURN_LOCATION = 300;
        private const int MAX_RETURN_OPERATION = 300;

        private static string SALT_SHA1 = "kdhn54sdfuijuyku740984";
        private static int CREDIT_CREATION_SEEKIOS = 60;
        private static double DEFAULT_LATITUDE = 43.489498;
        private static double DEFAULT_LONGITUDE = -1.534283;

        private const string SEEKIOS_CNX_STR = "seekios";
        private const string SEEKIOS_DB_NAME_PROD = "u2b7kcodrq";

        private const string SUBSCRIPTION_SUFFIX = "_subscription";
        private const string IOS_PACK_SUFFIX = "_pack";
        private const string PLAYSTORE_PRIVATE_KEY = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEArUR0ggqbrL8muNt/VtcM+AjrT4Yolg4GV0fxBXcejnU0KnVFk/7joyRNbLwYuL6BJfvJZ7kZxra2T9gd5j7lXwNsmI16jmbllCOeMIWnpwzGWhchdCC84xfiCJyrXxl8u6bsmam4NStHwZnRqFG789bPvZYDF5syOmru+2pGoENfxcPy0Oltn41Em0+XpZnJn1ntHzTUFcay1CCwSrUJPQsuPIJijCV48sBIWQjKCqANfMI9VI4gtHep82aJUjfacwl+WFJFI95XQH7yrL8AInzY/9dC7k38cBMUS8FMHfPlbOY0ZfUNpo5u1zN7Y6QJX107XMGx0XtiMXk3MSEgAQIDAQAB";
        private const string PLAYSTORE_PRIVATE_ALGO = "SHA1";
        private static Dictionary<string, string> IOS_PACKNAMES_MAPPING;

        public static Microsoft.ApplicationInsights.TelemetryClient Telemetry;
        private static string APPLICATION_INSIGHT_PROD = "159537ac-ded0-494b-992f-bf5c2718fc18";
        private static string APPLICATION_INSIGHT_STAG = "159537ac-ded0-494b-992f-bf5c2718fc18";

        public JsonSerializerSettings JsonSetting
        {
            get
            {
                return new JsonSerializerSettings()
                {
                    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                    DateParseHandling = DateParseHandling.DateTime,
                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                };
            }
        }

        #endregion

        #region ----- CONSTRUCTOR -----------------------------------------------------------------------------

        /// <summary>
        /// Static constructor
        /// </summary>
        static SeekiosService()
        {
            CheckEnvironnment();
            IOS_PACKNAMES_MAPPING = new Dictionary<string, string>();
            IOS_PACKNAMES_MAPPING.Add("epopee", "epic");
            IOS_PACKNAMES_MAPPING.Add("aventure", "adventure");

            Telemetry = new Microsoft.ApplicationInsights.TelemetryClient();
            if (IsStaging)
            {
                TelemetryConfiguration.Active.InstrumentationKey = APPLICATION_INSIGHT_STAG;
                Telemetry.Context.User.Id = /*Environment.UserName*/"seekios/staging";
            }
            else
            {
                TelemetryConfiguration.Active.InstrumentationKey = APPLICATION_INSIGHT_PROD;
                Telemetry.Context.User.Id = /*Environment.UserName + */"seekios/prod";
            }
            Telemetry.Context.Session.Id = Guid.NewGuid().ToString();
            Telemetry.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
        }

        /// <summary>
        /// Setup IsStaging variable, depends on the connection string in the webconfig file 
        /// </summary>
        private static void CheckEnvironnment()
        {
            // initialize the values here (e.g. extract data from database or webconfig)
            Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            if (rootWebConfig != null && rootWebConfig.ConnectionStrings != null)
            {
                foreach (ConnectionStringSettings connection in rootWebConfig.ConnectionStrings.ConnectionStrings)
                {
                    // could be more precise (check real two names)
                    if (connection.Name.Contains(SEEKIOS_CNX_STR))
                    {
                        IsStaging = !connection.ConnectionString.Contains(SEEKIOS_DB_NAME_PROD);
                    }
                }
            }
        }

        #endregion

        #region ----- METHODS FROM THE INTERFACE --------------------------------------------------------------

        #region (Ex 0x0000 -> 0x0049) Login

        /// <summary>
        /// Give a token to the user
        /// </summary>
        /// <param name="email">user email</param>
        /// <param name="password">user password</param>
        /// <returns>token</returns>
        public DBToken Login(string email, string password)
        {
            // if loop account, no need to track it
            if (email != "ServiceRequestLoop@thingsoftomorrow.com")
            {
                Telemetry.TrackEvent("Login");
            }

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var exception = new System.Data.Entity.Core.Objects.ObjectParameter("ReturnException", 0);
                var token = new System.Data.Entity.Core.Objects.ObjectParameter("ReturnToken", string.Empty);
                var dateCreation = new System.Data.Entity.Core.Objects.ObjectParameter("ReturnDateCreation", DateTime.Now);
                var dateExpires = new System.Data.Entity.Core.Objects.ObjectParameter("ReturnDateExpires", DateTime.Now);

                seekiosEntities.GenerateToken(email, password, exception, token, dateCreation, dateExpires);

                int returnException = 0;
                int.TryParse(exception.Value.ToString(), out returnException);
                if (returnException == -1) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0003", "email invalid"
                    , "email not recognized"), HttpStatusCode.NotFound);
                if (returnException == -2) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0004", "password invalid"
                    , "password not correct with the email"), HttpStatusCode.NotFound);

                DateTime returnDateCreation = DateTime.MinValue;
                DateTime returnDateExpires = DateTime.MinValue;

                DateTime.TryParse(dateCreation.Value.ToString(), out returnDateCreation);
                DateTime.TryParse(dateExpires.Value.ToString(), out returnDateExpires);

                var tokenToAdd = new token()
                {
                    authToken = token.Value.ToString(),
                    dateCreationToken = returnDateCreation.ToUniversalTime(),
                    dateExpiresToken = returnDateExpires.ToUniversalTime()
                };

                return DBToken.TokenToDBToken(tokenToAdd);
            }
        }

        #endregion

        #region (Ex 0x0050 -> 0x0099) Device

        /// <summary>
        /// Register the device in the BDD
        /// </summary>
        /// <param name="deviceModel">device model</param>
        /// <param name="platform">platform (iOS/Android/WP/Web/etc.</param>
        /// <param name="version">Version of the platform</param>
        /// <param name="iudDevice">Unique ID to identify the device</param>
        /// <param name="countryCode">Language device</param>
        public int RegisterDevice(string deviceModel
            , string platform
            , string version
            , string uidDevice
            , string countryCode)
        {
            Telemetry.TrackEvent("RegisterDeviceForNotification");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);
                var dateConnection = DateTime.UtcNow;

                // update the last connection date
                userDb.dateLastConnection = dateConnection;
                seekiosEntities.SaveChanges();

                // get the user device
                var userDeviceDb = (from d in seekiosEntities.device
                                    where d.uidDevice == uidDevice && d.user_iduser == userDb.iduser
                                    select d).FirstOrDefault();
                if (userDeviceDb == null)
                {
                    // if it's not exist, we create a new device
                    userDeviceDb = new device()
                    {
                        user_iduser = userDb.iduser,
                        deviceName = deviceModel,
                        notificationPlayerId = string.Empty,
                        lastUseDate = dateConnection,
                        uidDevice = uidDevice,
                        os = version,
                        plateform = platform,
                        countryCode = countryCode,
                        doNotDisturb = 1
                    };
                    seekiosEntities.device.Add(userDeviceDb);
                    seekiosEntities.SaveChanges();
                }
                else
                {
                    // if it's exit, we update the last use date and the country code
                    userDeviceDb.notificationPlayerId = string.Empty;
                    userDeviceDb.countryCode = countryCode;
                    userDeviceDb.lastUseDate = dateConnection;
                    seekiosEntities.SaveChanges();
                }

                // we add a new connection line
                seekiosEntities.connection.Add(new connection()
                {
                    dateConnection = dateConnection,
                    device_iddevice = userDeviceDb.iddevice,
                    ipv4 = string.Empty,
                    ipv6 = string.Empty,
                    user_iduser = userDb.iduser
                });
                seekiosEntities.SaveChanges();
                return 1;
            }
        }

        /// <summary>
        /// Unregister the device in the BDD
        /// </summary>
        /// <param name="iudDevice">Unique ID to identify the device</param>
        public int UnregisterDevice(string uidDevice)
        {
            Telemetry.TrackEvent("UnregisterDeviceForNotification");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);

                var dateConnection = DateTime.UtcNow;
                var idUser = userDb.iduser;

                // remove the device
                var userDeviceDb = (from d in seekiosEntities.device
                                    where d.uidDevice == uidDevice
                                    select d).FirstOrDefault();
                if (userDeviceDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0050", "device not found", "the uidDevice does not match with any device"), HttpStatusCode.NotFound);
                seekiosEntities.device.Remove(userDeviceDb);
                return seekiosEntities.SaveChanges();
            }
        }

        #endregion

        #region (Ex 0x0100 -> 0x0199) User

        /// <summary>
        /// Get specified user datas
        /// </summary>
        public UserEnvironment UserEnvironment(string idApp
            , string platform
            , string deviceModel
            , string version
            , string uidDevice
            , string countryCode)
        {
            int idPlatformApp = 0;
            if (platform == "iOS") idPlatformApp = 2;
            else if (platform == "Android") idPlatformApp = 1;
            else idPlatformApp = 3;

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);
                if (userDb != null && userDb.email != "ServiceRequestLoop@thingsoftomorrow.com")
                {
                    Telemetry.TrackEvent("UserEnvironment");
                }

                var lastBlockingVersionAvailableNumber = (from vn in seekiosEntities.versionApplication
                                                          where vn.isNeedUpdate == 1 && vn.plateforme == idPlatformApp
                                                          orderby vn.version_dateCreation descending
                                                          select vn.versionNumber).Take(1).FirstOrDefault();
                if (lastBlockingVersionAvailableNumber != null)
                {
                    if (idPlatformApp == (int)PlatformEnum.Android)
                    {
                        if (CalcultateChangset(lastBlockingVersionAvailableNumber, 4) > CalcultateChangset(idApp, 4))
                        {
                            throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0102", "update for Android", "you need to do the update for Android"), HttpStatusCode.NotFound);
                        }
                    }
                    else if (idPlatformApp == (int)PlatformEnum.iOS)
                    {
                        if (CalcultateChangset(lastBlockingVersionAvailableNumber, 3) > CalcultateChangset(idApp, 3))
                        {
                            throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0103", "update for iOS", "you need to do the update for iOS"), HttpStatusCode.NotFound);
                        }
                    }
                }
                return GetUserEnvironment(seekiosEntities, userDb, platform, deviceModel, version, uidDevice, countryCode);
            }
        }

        /// <summary>
        /// Get user object
        /// </summary>
        public DBUser User()
        {
            Telemetry.TrackEvent("User");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);
                return DBUser.UserToDBUser(userDb);
            }
        }

        /// <summary>
        /// Insert user object
        /// </summary>
        /// <param name="seekios">user to add</param>
        public int InsertUser(DBUser user)
        {
            Telemetry.TrackEvent("InsertUser");

            if (string.IsNullOrEmpty(user.Email)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0120", "invalid email", "email is empty"), HttpStatusCode.NotFound);
            if (!user.Email.IsValidEmail()) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0121", "invalid email", "the syntax of the email is not valid"), HttpStatusCode.NotFound);
            if (string.IsNullOrEmpty(user.Password)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0122", "invalid password", "password is empty"), HttpStatusCode.NotFound);
            if (user.Password.Length < 8) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0123", "password too short", "the minimum length of the password require is 8 characters"), HttpStatusCode.NotFound);
            if (string.IsNullOrEmpty(user.FirstName)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0124", "empty first name", "you must define a first name"), HttpStatusCode.NotFound);
            if (string.IsNullOrEmpty(user.LastName)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0125", "empty last name", "you must define a last name"), HttpStatusCode.NotFound);
            if (user.IdCountryResource < 1 || user.IdCountryResource > 2) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0126", "invalid country code", "country code invalid, check the documentation online"), HttpStatusCode.NotFound);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // si l'email existe déjà dans la base, pas besoin de créer de compte
                // warning : ici on va douiller si pas d'index (email)
                string email = user.Email.Trim();
                var userToAdd = (from u in seekiosEntities.user
                                 where u.email == email
                                 select u).FirstOrDefault();
                if (userToAdd != null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0127", "user already exist", "this email is not available"), HttpStatusCode.Unauthorized);

                var now = DateTime.UtcNow;
                var validationToken = TokenGenerator.Generate(email);

                try
                {
                    userToAdd = new user()
                    {
                        email = email,
                        password = user.Password,
                        firstName = user.FirstName.Trim().FirstCharToUpper(),
                        lastName = user.LastName.Trim().FirstCharToUpper(),
                        phoneNumber = null,
                        remainingRequest = 0, //PAR DEFAUT A ZERO // 20 Credit offert à la création d'un compte ? 
                        userPicture = (user.UserPicture == null) ? null : Convert.FromBase64String(user.UserPicture),
                        isValidate = false,
                        defaultTheme = 0,
                        socialNetworkUserId = null,
                        socialNetworkType = 0,
                        dateCreation = now,
                        validationToken = validationToken,
                        resetPasswordToken = null,
                        countryResources_idcountryResources = user.IdCountryResource
                    };
                    seekiosEntities.user.Add(userToAdd);
                    seekiosEntities.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0128", "impossible to create user account", "this email might be not available, ex : " + ex.Message), HttpStatusCode.Unauthorized);
                }

                SendGridHelper.SendEmailValidationEmail(seekiosEntities
                    , email
                    , user.FirstName
                    , user.LastName
                    , validationToken
                    , user.IdCountryResource);

                return 1;
            }
        }

        /// <summary>
        /// Update user object
        /// </summary>
        /// <param name="seekios">user to update</param>
        /// <returns>user environment</returns>
        public int UpdateUser(string uidDevice, DBUser user)
        {
            Telemetry.TrackEvent("UpdateUser");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get user
                var userDb = IsUserAuthentified(seekiosEntities);

                // Check input
                if (string.IsNullOrEmpty(user.Email)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0130", "invalid email", "email is empty"), HttpStatusCode.NotFound);
                if (!user.Email.IsValidEmail()) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0131", "invalid email", "the syntax of the email is not valid"), HttpStatusCode.NotFound);
                if (user.Email != userDb.email) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0132", "invalid email", "the email does not match with the token's user account"), HttpStatusCode.NotFound);
                if (string.IsNullOrEmpty(user.Password)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0133", "password not found", "you must define a password"), HttpStatusCode.NotFound);
                if (string.IsNullOrEmpty(user.FirstName)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0134", "first name not found", "you must define a first name"), HttpStatusCode.NotFound);
                if (string.IsNullOrEmpty(user.LastName)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0135", "last name not found", "you must define a last name"), HttpStatusCode.NotFound);
                if (user.IdCountryResource < 1 || user.IdCountryResource > 2) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0136", "country code invalid", "country code invalid, check the documentation online"), HttpStatusCode.NotFound);
                if (string.IsNullOrEmpty(uidDevice)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0137", "uidDevice is empty", "you need to provide an unique uidDevice"), HttpStatusCode.NotFound);

                // Update database
                userDb.password = user.Password;
                userDb.firstName = user.FirstName.Trim().FirstCharToUpper();
                userDb.lastName = user.LastName.Trim().FirstCharToUpper();
                userDb.userPicture = user.UserPicture == null ? null : Convert.FromBase64String(user.UserPicture);
                userDb.countryResources_idcountryResources = user.IdCountryResource;
                seekiosEntities.SaveChanges();

                // Broadcast user
                SignalRHelper.BroadcastUser(HubProxyEnum.UserHub, SignalRHelper.METHOD_UPDATE_USER, new object[]
                {
                    userDb.iduser.ToString(),
                    uidDevice,
                    JsonConvert.SerializeObject(DBUser.UserToDBUser(userDb), JsonSetting)
            });
                return 1;
            }
        }

        /// <summary>
        /// Delete user object
        /// </summary>
        /// <param name="id">id of the user</param>
        [Obsolete("The method is not implemented in the interface / the code is not finished")]
        public int DeleteUser()
        {
            Telemetry.TrackEvent("DeleteUser");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);
                var listSeekiosToDelete = (from s in seekiosEntities.seekios
                                           where s.user_iduser == userDb.iduser
                                           select s);
                foreach (var seekiosToDelete in listSeekiosToDelete)
                {
                    DeleteSeekios(string.Empty, seekiosToDelete.idseekios.ToString());
                }
                // faire procédure stocké pour delete user + token + seekios + device
                seekiosEntities.user.Remove(userDb);
                return seekiosEntities.SaveChanges();
            }
        }

        /// <summary>
        /// Validate the user account (the link is sent by email to the user)
        /// </summary>
        /// <param name="token">validation token</param>
        /// <returns>
        /// -1 : the date for validation has expired
        /// 0  : already validate
        /// 1  : the account has been validate
        /// </returns>
        public int ValidateUser(string token)
        {
            Telemetry.TrackEvent("ValidateUser");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var user = (from u in seekiosEntities.user
                            where u.validationToken == token
                            select u).FirstOrDefault();
                if (user == null) return -2;
                if (user.isValidate ?? false) return 0;
                if (user.dateCreation < DateTime.UtcNow.AddDays(MAX_DAY_FOR_VALIDATION_USER_ACCOUNT)) return -1;
                user.isValidate = true;
                return seekiosEntities.SaveChanges();
            }
        }

        /// <summary>
        /// Check if the email matches with an user
        /// </summary>
        /// <param name="email">user email</param>
        /// <returns>
        /// 0 if the user does not exist
        /// 1 if the user exist
        /// </returns>
        public int UserExists(string email)
        {
            Telemetry.TrackEvent("UserExists");

            if (!email.IsValidEmail()) return -1;
            //var socialNetworkType = 0;
            //if (!int.TryParse(socialNetworkTypeStr, out socialNetworkType)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x021", "socialNetworkTypeStr is not a valid type", "socialNetworkTypeStr must be a Int"), HttpStatusCode.NotFound);
            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // get the user for that email
                var userDb = (from u in seekiosEntities.user
                              where u.email == email.Trim()
                              select u).FirstOrDefault();

                // if user doesn't exists exception
                if (userDb == null) return 0;
                // if user exists but isn't valide and is created since at least 2 days return 3
                // if (!userDb.isValidate ?? false && userDb.dateCreation < DateTime.UtcNow.AddDays(-2)) return 3;//why is this still here ?
                // if user exists and is linked to the right socialNetwork return 1
                //if (userDb.socialNetworkType == socialNetworkType) return 1;
                // if user exists but is linked with an another socialNetwork return 2
                return 1;
            }
        }

        /// <summary>
        /// 
        /// Send an email with a link to reset the password
        /// </summary>
        /// <param name="email">user email</param>
        /// <returns>
        /// 0 : user not found
        /// 1 : works
        /// </returns>
        public int AskForNewPassword(string email)
        {
            Telemetry.TrackEvent("AskForNewPassword");

            if (!email.IsValidEmail()) return -2;
            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var user = (from u in seekiosEntities.user
                            where u.email == email.Trim()
                            select u).FirstOrDefault();
                if (user == null) return -1;

                // generate a password token
                var resetPasswordToken = TokenGenerator.Generate(email);
                // language and country resources
                var preferredLanguage = ResourcesHelper.GetPreferredLanguage(seekiosEntities, user.iduser);
                var idcountryResources = ResourcesHelper.GetCountryResources(preferredLanguage);
                // send an email to the user with a link to reset the password
                SendGridHelper.SendResetPasswordRequestEmail(seekiosEntities
                    , email
                    , user.firstName
                    , user.lastName
                    , resetPasswordToken
                    , idcountryResources);
                // update the password token in bdd
                user.resetPasswordToken = resetPasswordToken;
                seekiosEntities.SaveChanges();
                return 1;
            }
        }

        /// <summary>
        /// Send an email with a new password
        /// </summary>
        /// <param name="token">reset password token</param>
        /// <returns>
        /// 0 : user not found
        /// 1 : works
        /// </returns>
        public int SendNewPassword(string token)
        {
            Telemetry.TrackEvent("SendNewPassword");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var user = (from u in seekiosEntities.user
                            where u.resetPasswordToken == token
                            select u).FirstOrDefault();
                if (user == null) return -1;

                var newPassword = TokenGenerator.Generate(user.email).Substring(0, 8);
                user.password = SeekiosTools.CryptographyHelper.CalculatePasswordMD5Hash(user.email, newPassword);
                user.resetPasswordToken = null;
                // language and country resources
                var preferredLanguage = ResourcesHelper.GetPreferredLanguage(seekiosEntities, user.iduser);
                var idcountryResources = ResourcesHelper.GetCountryResources(preferredLanguage);
                // send an email to the user with his new password
                SendGridHelper.SendResetedPasswordEmail(seekiosEntities
                    , user.email
                    , user.firstName
                    , user.lastName
                    , newPassword
                    , idcountryResources);
                seekiosEntities.SaveChanges();
                return 1;
            }
        }

        #endregion

        #region (Ex 0x0200 -> 0x0299) Seekios

        /// <summary>
        /// Return true if the imei number is contain in the bdd and 
        /// the imei and the pin are matching
        /// </summary>
        /// <param name="imei">IMEI</param>
        /// <param name="pin">Pin Code</param>
        [Obsolete("The method is not implemented in the interface")]
        public int IsIMEIAndPinValid(string imei, string pin)
        {
            Telemetry.TrackEvent("IsIMEIAndPinValid");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var seekiosAndSeekiosProduction = (from s in seekiosEntities.seekiosAndSeekiosProduction
                                                   where s.imei == imei
                                                   select s).FirstOrDefault();
                // if seekios doesn't exists
                if (seekiosAndSeekiosProduction == null) return -1;
                // if pin code isn't correct
                if (pin != GetPinCodeFromIMEI(imei)) return -2;
                // the seekios is available
                if (!seekiosAndSeekiosProduction.idseekios.HasValue) return 1;
                // the seekios is linked with a user acount
                return 0;
            }
        }

        /// <summary>
        /// Get the seekios list from a user
        /// </summary>
        /// <param name="id">id user</param>
        /// <returns>return the seekios list of the user</returns>
        public IEnumerable<DBSeekios> Seekios()
        {
            Telemetry.TrackEvent("Seekios");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);
                return GetSeekiosByUser(seekiosEntities, userDb.iduser);
            }
        }

        /// <summary>
        /// Add a new seekios 
        /// </summary>
        /// <param name="seekios">new seekios</param>
        /// <returns>seekios object</returns>
        public DBSeekios InsertSeekios(string uidDevice, DBSeekios seekios)
        {
            Telemetry.TrackEvent("InsertSeekios");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);

                if (string.IsNullOrEmpty(seekios.SeekiosName)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0220", "SeekiosName is empty", "SeekiosName require a value"), HttpStatusCode.NotFound);
                if (string.IsNullOrEmpty(seekios.Imei)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0221", "invalid imei", "imei is require"), HttpStatusCode.NotFound);
                if (seekios.Imei.Length != 15) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0222", "invalid imei", "the imei lenght should be 15 characters"), HttpStatusCode.Unauthorized);
                if (string.IsNullOrEmpty(seekios.PinCode)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0223", "invalid pin code", "pin code is require"), HttpStatusCode.NotFound);
                if (seekios.PinCode.Length != 4) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0224", "invalid pin code", "the pin code lenght should be 4 characters"), HttpStatusCode.Unauthorized);

                var seekiosProduction = (from sp in seekiosEntities.seekiosAndSeekiosProduction
                                         where sp.imei == seekios.Imei
                                         select sp).FirstOrDefault();

                if (seekiosProduction == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0225", "invalid imei", "the imei does not exist"), HttpStatusCode.NotFound);
                if (seekiosProduction.idseekios.HasValue) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0226", "seekios is not available", "the seekios is already associate with an user account"), HttpStatusCode.NotFound);
                if (seekios.PinCode != GetPinCodeFromIMEI(seekios.Imei)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0227", "imei and pin code invalid", "the imei and the pin code are not compatible"), HttpStatusCode.NotFound);

                var now = DateTime.UtcNow;
                if (!seekiosProduction.dateFirstRegistration.HasValue)
                {
                    seekiosEntities.UpdateSeekiosProductionFirstAssociation(seekios.Imei, CREDIT_CREATION_SEEKIOS);
                }
                var seekiosToAdd = new seekios
                {
                    idseekios = seekiosProduction.idseekiosProduction,
                    user_iduser = userDb.iduser,
                    seekiosName = seekios.SeekiosName,
                    seekiosPicture = seekios.SeekiosPicture != null ? Convert.FromBase64String(seekios.SeekiosPicture) : null,
                    seekios_dateCretaion = now,
                    batteryLife = 0,
                    signalQuality = 0,
                    dateLastCommunication = null,
                    lastKnownLocation_longitude = DEFAULT_LONGITUDE,
                    lastKnownLocation_latitude = DEFAULT_LATITUDE,
                    lastKnownLocation_altitude = null,
                    lastKnownLocation_accuracy = null,
                    lastKnownLocation_dateLocationCreation = null,
                    hasGetLastInstruction = 0,
                    isAlertLowBattery = 0,
                    isInPowerSaving = 0,
                    powerSaving_hourStart = 0,
                    powerSaving_hourEnd = 0,
                    alertSOS_idalert = null,
                    lastKnowLocation_idlocationDefinition = (int)LocationDefinition.OnDemand,
                    dateLastOnDemandRequest = null,
                    dateLastSOSSent = null,
                    isLastSOSRead = 1,
                    sendNotificationOnNewTrackingLocation = 1,
                    sendNotificationOnNewDontMoveLocation = 1,
                    sendNotificationOnNewOutOfZoneLocation = 1
                };
                seekiosEntities.seekios.Add(seekiosToAdd);
                seekiosEntities.SaveChanges();
                var seekiosDb = DBSeekios.SeekiosToDBSeekios(seekiosToAdd, seekiosProduction);
                // Broadcast user
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_INSERT_SEEKIOS, new object[]
                {
                    userDb.iduser,
                    uidDevice,
                    JsonConvert.SerializeObject(seekiosDb, JsonSetting)
                });
                return seekiosDb;
            }
        }

        /// <summary>
        /// Update the seekios
        /// </summary>
        /// <param name="seekios">new seekios data</param>
        /// <returns>return 1 if the operation has succeed</returns>
        public int UpdateSeekios(string uidDevice, DBSeekios seekios)
        {
            Telemetry.TrackEvent("UpdateSeekios");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);

                if (seekios.Idseekios <= 0) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0230", "invalid idseekios", "the seekios need a valid idseekios"), HttpStatusCode.NotFound);
                if (string.IsNullOrEmpty(seekios.SeekiosName)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0231", "SeekiosName is empty", "SeekiosName require a value"), HttpStatusCode.NotFound);

                var seekiosDb = (from s in seekiosEntities.seekios
                                 where s.idseekios == seekios.Idseekios
                                 select s).FirstOrDefault();
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0232", "no seekios found", "the id does not match with any seekios id"), HttpStatusCode.NotFound);
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0233", "seekios unauthorized", "the seekios does not belong to you"), HttpStatusCode.Unauthorized);
                if (seekios.AlertSOS_idalert.HasValue && seekios.AlertSOS_idalert.Value > 0)
                {
                    var alertSosDb = (from a in seekiosEntities.alert
                                      where a.idalert == seekios.AlertSOS_idalert.Value
                                      select a).FirstOrDefault();
                    if (alertSosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0234", "alert sos not found", "the AlertSOS_idalert does not match with an idalert"), HttpStatusCode.NotFound);
                }
                seekiosDb.seekiosName = seekios.SeekiosName;
                seekiosDb.seekiosPicture = seekios.SeekiosPicture == null ? null : Convert.FromBase64String(seekios.SeekiosPicture);
                seekiosDb.isAlertLowBattery = (byte)(seekios.IsAlertLowBattery ? 1 : 0);
                seekiosDb.isInPowerSaving = (byte)(seekios.IsInPowerSaving ? 1 : 0);
                seekiosDb.powerSaving_hourStart = seekios.PowerSaving_hourStart;
                seekiosDb.powerSaving_hourEnd = seekios.PowerSaving_hourEnd;
                seekiosDb.alertSOS_idalert = seekios.AlertSOS_idalert.HasValue && seekios.AlertSOS_idalert.Value == 0 ? null : seekios.AlertSOS_idalert;
                seekiosEntities.SaveChanges();
                // Broadcast user
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_UPDATE_SEEKIOS, new object[]
                {
                    userDb.iduser,
                    uidDevice,
                    JsonConvert.SerializeObject(DBSeekios.SeekiosToDBSeekios(seekiosDb), JsonSetting)
                });
                return 1;
            }
        }

        /// <summary>
        /// Delete a seekios
        /// </summary>
        /// <param name="id">seekios id</param>
        /// <returns>return 1 if the operation has succeed</returns>
        public int DeleteSeekios(string uidDevice, string idseekios)
        {
            Telemetry.TrackEvent("DeleteSeekios");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);

                int idSeekios = 0;
                if (!int.TryParse(idseekios, out idSeekios)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0240", "invalid idseekios", "the idseekios must be an Int"), HttpStatusCode.NotFound);
                var seekiosDb = (from s in seekiosEntities.seekios
                                 where s.idseekios == idSeekios
                                 select s).FirstOrDefault();
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0241", "no seekios found", "the id does not match with any seekios id"), HttpStatusCode.NotFound);
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0242", "seekios unauthorized", "the seekios does not belong to you"), HttpStatusCode.Unauthorized);
                // Remove seekios dependencies power saving
                if (seekiosDb.isInPowerSaving == 1)
                {
                    AddSeekiosInstruction(seekiosEntities
                        , seekiosDb
                        , (from sp in seekiosEntities.seekiosProduction
                           where sp.idseekiosProduction == seekiosDb.idseekios
                           select sp).First()
                        , new SeekiosInstruction
                        {
                            DateInstruction = DateTime.UtcNow,
                            TrameInstruction = "#P0&",
                            TypeInstruction = InstructionType.ChangePowerSavingConfig
                        });
                }
                // If the seekios is in a mode, we delete the mode
                if (seekiosEntities.mode.Any(x => x.seekios_idseekios == idSeekios))
                {
                    PrepareInstructionForNewMode(seekiosEntities, null, seekiosDb);
                }
                // Remove seekios and dependencies
                // Backup in history table : modes / alerts / recipients / locations / alert sos / seekios
                var result = new System.Data.Entity.Core.Objects.ObjectParameter("Result", 0);
                seekiosEntities.DeleteSeekiosById(idSeekios, result);
                int integerResult = 0;
                int.TryParse(result.Value.ToString(), out integerResult);
                if (integerResult != 1) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0243", "DelteSeekios failed", string.Format("DeleteSeekios return {0} for the user id {1} on the seekios id {2}", integerResult, userDb.iduser, idSeekios)), HttpStatusCode.NotFound);
                // Broadcast user
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_DELETE_SEEKIOS, new object[]
                {
                    userDb.iduser,
                    uidDevice,
                    seekiosDb.idseekios
                });
                return 1;
            }
        }

        /// <summary>
        /// Send a request to the seekios to update the seekios position
        /// </summary>
        /// <param name="idSeekios">id du seekios</param>
        public int RefreshSeekiosLocation(string uidDevice, string idseekios)
        {
            Telemetry.TrackEvent("RefreshSeekiosLocation");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);

                int idSeekios = 0;
                if (!int.TryParse(idseekios, out idSeekios)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0250", "invalid idseekios", "the idseekios must be an Int"), HttpStatusCode.NotFound);
                // Check if the user still have credit
                if (!CreditBillingHelper.UserCanAffordAction(seekiosEntities, userDb, 0)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0251", "not enough credit", "you have no credit to execute this action"), HttpStatusCode.Unauthorized);
                // Get the seekios
                var seekiosDb = (from sp in seekiosEntities.seekios
                                 where sp.idseekios == idSeekios
                                 select sp).FirstOrDefault();
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0252", "no seekios found", "the id does not match with any seekios id"), HttpStatusCode.NotFound);
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0253", "seekios unauthorized", "the seekios does not belong to you"), HttpStatusCode.Unauthorized);
                var sendDate = DateTime.UtcNow;
                // On stoque l'instruction à donner au seekios
                var instructionTrame = string.Format("{0}{1}", string.Format(START_TRAME, 2), FOOTER_TRAME);
                var instruction = new SeekiosInstruction
                {
                    DateInstruction = DateTime.UtcNow,
                    TrameInstruction = instructionTrame,
                    TypeInstruction = InstructionType.OnDemand
                };
                seekiosDb.dateLastOnDemandRequest = sendDate;
                if (AddSeekiosInstruction(seekiosEntities
                    , seekiosDb
                    , (from sp in seekiosEntities.seekiosProduction
                       where sp.idseekiosProduction == seekiosDb.idseekios
                       select sp).First()
                    , instruction) == 0) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0254", "something wrong", "something wrong while we were trying to communicate with the seekios"), HttpStatusCode.NotFound);
                // Broadcast user
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_REFRESH_SEEKIOS_LOCATION, new object[]
                {
                    userDb.iduser,
                    uidDevice,
                    seekiosDb.idseekios
                });
                return 1;
            }
        }

        /// <summary>
        /// Send a request to the seekios to update the seekios battery level
        /// </summary>
        /// <param name="idseekios">seekios id</param>
        /// <returns>return 1 if it's working</returns>
        public int RefreshSeekiosBatteryLevel(string uidDevice, string idseekios)
        {
            Telemetry.TrackEvent("RefreshSeekiosBatteryLevel");

            // add the instruction, the seekios will give us the battery level when 
            // it will make the GetInstruction (http request)
            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);

                int idSeekios = 0;
                if (!int.TryParse(idseekios, out idSeekios)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0260", "invalid idseekios", "the idseekios must be an Int"), HttpStatusCode.NotFound);

                // Check if the user still have credit
                if (!CreditBillingHelper.UserCanAffordAction(seekiosEntities, userDb, 0)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0261", "not enough credit", "you have no credit to execute this action"), HttpStatusCode.Unauthorized);
                // Get the seekios
                var seekiosDb = (from s in seekiosEntities.seekios
                                 where s.idseekios == idSeekios
                                 select s).FirstOrDefault();
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0262", "no seekios found", "the id does not match with any seekios id"), HttpStatusCode.NotFound);
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0263", "seekios unauthorized", "the seekios does not belong to you"), HttpStatusCode.Unauthorized);

                var instruction = new SeekiosInstruction
                {
                    DateInstruction = DateTime.UtcNow,
                    TrameInstruction = "#F01&",
                    TypeInstruction = InstructionType.SendBatteryLevel
                };

                if (AddSeekiosInstruction(seekiosEntities
                    , seekiosDb
                    , (from sp in seekiosEntities.seekiosProduction
                       where sp.idseekiosProduction == seekiosDb.idseekios
                       select sp).First()
                    , instruction) == 0) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0264", "something wrong", "something wrong while we were trying to communicate with the seekios"), HttpStatusCode.NotFound);
                // Broadcast user
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_REFRESH_SEEKIOS_BATTERY_LEVEL, new object[]
                {
                    userDb.iduser,
                    uidDevice,
                    seekiosDb.idseekios
                });
                return 1;
            }
        }

        /// <summary>
        /// Enable the mode power saving on the seekios
        /// </summary>
        /// <param name="idssekios">seekios id</param>
        /// <param name="hourInDayStr"></param>
        /// <returns>return 1 if it's working</returns>
        [Obsolete("The method is not implemented in the interface")]
        public int EnablePowerSaving(string idssekios, string hourinday)
        {
            // TODO : check with the embedded team how hourInDayStr is working ? 
            // Normaly in BDD we have two parameters : start and stop. So how hourInDayStr is working ?
            Telemetry.TrackEvent("EnablePowerSaving");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);
                // verify the date
                int idSeekios = 0, hourInDay = 0;
                if (!int.TryParse(idssekios, out idSeekios)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0270", "invalid idseekios", "the idseekios must be an Int"), HttpStatusCode.NotFound);
                if (!int.TryParse(hourinday, out hourInDay)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0271", "invalid hour", "the hourInDayStr must be an Int"), HttpStatusCode.NotFound);
                // get seekios
                var seekiosDb = (from s in seekiosEntities.seekios
                                 where s.idseekios == idSeekios
                                 select s).FirstOrDefault();
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0272", "seekios not found", "the id does not match with any seekios id"), HttpStatusCode.NotFound);
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0273", "seekios unauthorized", "the seekios does not belong to you"), HttpStatusCode.Unauthorized);

                var hourOfDayPowerSaving = seekiosDb.powerSaving_hourEnd - seekiosDb.powerSaving_hourStart;
                if (seekiosDb.isInPowerSaving == 1 && hourOfDayPowerSaving == hourInDay) return 0;

                var instruction = new SeekiosInstruction
                {
                    DateInstruction = DateTime.UtcNow,
                    TrameInstruction = string.Format("#P1{0}&", hourInDay == 0 ? string.Empty : hourInDay.ToString()),
                    TypeInstruction = InstructionType.ChangePowerSavingConfig
                };

                if (AddSeekiosInstruction(seekiosEntities
                    , seekiosDb
                    , (from sp in seekiosEntities.seekiosProduction
                       where sp.idseekiosProduction == seekiosDb.idseekios
                       select sp).First()
                    , instruction) == 0) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0274", "something wrong", "something wrong while we were trying to communicate with the seekios"), HttpStatusCode.NotFound);

                seekiosDb.isInPowerSaving = 1;
                return seekiosEntities.SaveChanges();
            }
        }

        /// <summary>
        /// Disable the mode power saving on the seekios
        /// </summary>
        /// <param name="idssekios">seekios id</param>
        /// <returns>return 1 if it's working</returns>
        [Obsolete("The method is not implemented in the interface")]
        public int DisablePowerSaving(string idssekios)
        {
            Telemetry.TrackEvent("DisablePowerSaving");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);

                int idSeekios = 0;
                if (!int.TryParse(idssekios, out idSeekios)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0280", "invalid idseekios", "the idseekios must be an Int"), HttpStatusCode.NotFound);
                // get the seekios
                var seekiosDb = (from s in seekiosEntities.seekios
                                 where s.idseekios == idSeekios
                                 select s).FirstOrDefault();
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0281", "seekios not found", "the id does not match with any seekios id"), HttpStatusCode.NotFound);
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0282", "seekios unauthorized", "the seekios does not belong to you"), HttpStatusCode.Unauthorized);
                if (seekiosDb.isInPowerSaving == 0) return 0;

                var instruction = new SeekiosInstruction
                {
                    DateInstruction = DateTime.UtcNow,
                    TrameInstruction = "#P0&",
                    TypeInstruction = InstructionType.ChangePowerSavingConfig
                };

                if (AddSeekiosInstruction(seekiosEntities
                    , seekiosDb
                    , (from sp in seekiosEntities.seekiosProduction
                       where sp.idseekiosProduction == seekiosDb.idseekios
                       select sp).First()
                    , instruction) == 0) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0283", "something wrong", "something wrong while we were trying to communicate with the seekios"), HttpStatusCode.NotFound);

                seekiosDb.isInPowerSaving = 0;
                return seekiosEntities.SaveChanges();
            }
        }

        #endregion

        #region (Ex 0x0300 -> 0x0349) Seekios (Methods for mass production)

        /// <summary>
        /// Data for mass production
        /// </summary>
        public IEnumerable<SeekiosHardwareReportDTO> GetSeekiosHardwareReport()
        {
            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var reports = (from hr in seekiosEntities.seekiosHardwareReport
                               join sp in seekiosEntities.seekiosProduction on hr.SeekiosProdID equals sp.idseekiosProduction
                               select new
                               {
                                   hr.AdcBattery,
                                   hr.AdcUSB,
                                   hr.BatteryLife,
                                   hr.BLEAdvertizing,
                                   hr.BLEConfig,
                                   hr.BLEConnection,
                                   hr.Bouton,
                                   hr.CalendarInterrupt,
                                   hr.CalendarTime,
                                   hr.CalendarTimestamp,
                                   hr.DataFlashRead,
                                   hr.DataFlashWrite,
                                   hr.DateDuTest,
                                   hr.GPSPosit,
                                   hr.GPSUSART,
                                   hr.GSMUSART,
                                   hr.GSM_GPRS,
                                   hr.GSPFrames,
                                   hr.IMUgetaccel,
                                   hr.IMUInterrupt,
                                   hr.LEDs,
                                   hr.SeekiosHardwareReportID,
                                   hr.SeekiosProdID,
                                   sp.imei
                               });

                var source = new List<SeekiosHardwareReportDTO>();
                foreach (var report in reports)
                {
                    source.Add(new SeekiosHardwareReportDTO
                    {
                        AdcBattery = report.AdcBattery,
                        AdcUSB = report.AdcUSB,
                        BatteryLife = report.BatteryLife,
                        BLEAdvertizing = report.BLEAdvertizing,
                        BLEConfig = report.BLEConfig,
                        BLEConnection = report.BLEConnection,
                        Bouton = report.Bouton,
                        CalendarInterrupt = report.CalendarInterrupt,
                        CalendarTime = report.CalendarTime,
                        CalendarTimestamp = report.CalendarTimestamp,
                        DataFlashRead = report.DataFlashRead,
                        DataFlashWrite = report.DataFlashWrite,
                        DateDuTest = report.DateDuTest,
                        GPSPosit = report.GPSPosit,
                        GPSUSART = report.GPSUSART,
                        GSMUSART = report.GSMUSART,
                        GSM_GPRS = report.GSM_GPRS,
                        GSPFrames = report.GSPFrames,
                        IMUgetaccel = report.IMUgetaccel,
                        IMUInterrupt = report.IMUInterrupt,
                        LEDs = report.LEDs,
                        SeekiosHardwareReportID = report.SeekiosHardwareReportID,
                        SeekiosProdID = report.SeekiosProdID,
                        IMEI = report.imei
                    });
                }
                return source;
            }
        }

        /// <summary>
        /// Data for mass production
        /// </summary>
        public IEnumerable<SeekiosIMEIAndPinDTO> GetSeekiosIMEIAndPIN()
        {
            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var source = new List<SeekiosIMEIAndPinDTO>();
                var lsSeekios = (from sp in seekiosEntities.seekiosProduction
                                 join srh in seekiosEntities.seekiosHardwareReport on sp.idseekiosProduction equals srh.SeekiosProdID
                                 where srh.AdcBattery == true
                                 && srh.AdcUSB == true
                                 && srh.LEDs == true
                                 && srh.IMUgetaccel == true
                                 && srh.IMUInterrupt == true
                                 && srh.GSPFrames == true
                                 && srh.GSM_GPRS == true
                                 && srh.GSMUSART == true
                                 && srh.GPSUSART == true
                                 && srh.DataFlashRead == true
                                 && srh.DataFlashWrite == true
                                 && srh.CalendarTime == true
                                 && srh.CalendarInterrupt == true
                                 && srh.Bouton == true
                                 && srh.BLEConnection == true
                                 && srh.BLEConfig == true
                                 && srh.BLEAdvertizing == true
                                 select sp);

                foreach (var seekios in lsSeekios)
                {
                    source.Add(new SeekiosIMEIAndPinDTO
                    {
                        IdSeekios = seekios.idseekiosProduction,
                        IMEI = seekios.imei,
                        PIN = GetPinCodeFromIMEI(seekios.imei)
                    });
                }

                return source.Distinct().ToArray();
            }
        }

        /// <summary>
        /// Return the pin code from a imei number
        /// </summary>
        /// <param name="imei">imei number</param>
        /// <param name="qui">password to access to this method</param>
        /// <returns></returns>
        public string Imei2Pin(string imei, string qui)
        {
            Telemetry.TrackEvent("Imei2Pin");
            if (!(imei.Trim().Length == 15 && "phenixelectronique".Equals(qui))) return string.Empty;
            return GetPinCodeFromIMEI(imei.Trim());
        }

        #endregion

        #region (Ex 0x0350 -> 0x0449) Locations

        /// <summary>
        /// Get the locations of a seekios between a rage of dates with a limit of max 300 lignes
        /// </summary>
        /// <param name="id">seekios id</param>
        /// <param name="lowerDate">first date</param>
        /// <param name="upperDate">last date</param>
        /// <returns>List of locations</returns>
        public IEnumerable<DBLocation> Locations(string idseekios, string lowerdate, string upperdate)
        {
            Telemetry.TrackEvent("Locations");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);

                int idSeekios = 0;
                if (!int.TryParse(idseekios, out idSeekios)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0350", "invalid idseekios", "the idseekios must be an Int"), HttpStatusCode.NotFound);
                DateTime lowerDate = DateTime.MinValue;
                if (!GetDateTimeFromString(lowerdate, out lowerDate)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0351", "invalid lowerdate", "the lowerdate fomat is invalid"), HttpStatusCode.NotFound);
                DateTime upperDate = DateTime.MinValue;
                if (!GetDateTimeFromString(upperdate, out upperDate)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0352", "invalid upperdate", "the upperdate fomat is invalid"), HttpStatusCode.NotFound);

                var seekiosDb = (from s in seekiosEntities.seekios
                                 where s.idseekios == idSeekios
                                 select s).FirstOrDefault();
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0353", "no seekios found", "the id does not match with any seekios id"), HttpStatusCode.NotFound);
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0354", "seekios unauthorized", "the seekios does not belong to you"), HttpStatusCode.Unauthorized);

                var source = new List<DBLocation>();
                var locations = (from l in seekiosEntities.location
                                 where l.seekios_idseekios == seekiosDb.idseekios
                                 && l.dateLocationCreation >= lowerDate && l.dateLocationCreation <= upperDate
                                 orderby l.dateLocationCreation descending
                                 select l).Take(MAX_RETURN_LOCATION);
                foreach (var location in locations)
                {
                    source.Add(DBLocation.LocationToDbLocation(location));
                }
                if (source.Count >= 300)
                {
                    Telemetry.TrackEvent("LocationsMaximumReached:" + MAX_RETURN_LOCATION);
                }
                return source;
            }
        }

        /// <summary>
        /// Get the first date and the last date of the locations 
        /// </summary>
        /// <param name="id">seekios id</param>
        /// <returns>Return an object which wrap lower date and upper date</returns>
        public LocationUpperLowerDates LowerDateAndUpperDate(string idseekios)
        {
            Telemetry.TrackEvent("LowerDateAndUpperDate");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);

                int idSeekios = 0;
                if (!int.TryParse(idseekios, out idSeekios)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0360", "invalid idseekios", "the idseekios must be an Int"), HttpStatusCode.NotFound);

                var seekiosDb = (from s in seekiosEntities.seekios
                                 where s.idseekios == idSeekios
                                 select s).FirstOrDefault();
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0361", "no seekios found", "the id does not match with any seekios id"), HttpStatusCode.NotFound);
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0362", "seekios unauthorized", "the seekios does not belong to you"), HttpStatusCode.Unauthorized);

                var upperDate = new System.Data.Entity.Core.Objects.ObjectParameter("UpperDate", DateTime.Now);
                var lowerDate = new System.Data.Entity.Core.Objects.ObjectParameter("LowerDate", DateTime.Now);
                seekiosEntities.GetLowerDateAndUpperDate(seekiosDb.idseekios, upperDate, lowerDate);

                var upperDateTime = DateTime.MinValue;
                var lowerDateTime = DateTime.MinValue;

                if (!DateTime.TryParse(upperDate.Value.ToString(), out upperDateTime)) return new LocationUpperLowerDates() { LowerDate = null, UppderDate = null };
                if (!DateTime.TryParse(lowerDate.Value.ToString(), out lowerDateTime)) return new LocationUpperLowerDates() { LowerDate = null, UppderDate = upperDateTime };
                return new LocationUpperLowerDates() { LowerDate = lowerDateTime, UppderDate = upperDateTime };
            }
        }

        /// <summary>
        /// Get the locations of a seekios depends on the idmode
        /// </summary>
        /// <param name="idseekios">seekios id</param>
        /// <param name="idmode">mode id</param>
        /// <returns>List of locations</returns>
        public IEnumerable<DBLocation> LocationsByMode(string idmode)
        {
            Telemetry.TrackEvent("LocationsByMode");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);

                int idMode = 0;
                if (!int.TryParse(idmode, out idMode)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0370", "invalid idmode", "the idmode must be an Int"), HttpStatusCode.NotFound);

                var source = new List<DBLocation>();
                var locations = (from l in seekiosEntities.location
                                 where l.mode_idmode == idMode
                                 orderby l.dateLocationCreation descending
                                 select l).Take(MAX_RETURN_LOCATION);
                foreach (var location in locations)
                {
                    source.Add(DBLocation.LocationToDbLocation(location));
                }
                if (source.Count >= 300)
                {
                    Telemetry.TrackEvent("LocationsMaximumReached:" + MAX_RETURN_LOCATION);
                }
                return source;
            }
        }

        #endregion

        #region (Ex 0x0450 -> 0x0649) Modes

        /// <summary>
        /// Get the list of the modes that belong to a user
        /// </summary>
        /// <returns>list of the modes</returns>
        public IEnumerable<DBMode> Modes()
        {
            Telemetry.TrackEvent("Modes");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);
                return GetModeByUser(seekiosEntities, userDb.iduser);
            }
        }

        /// <summary>
        /// Add a mode tracking
        /// </summary>
        /// <param name="modeToAdd">mode tracking</param>
        /// <returns>new id mode tracking</returns>
        public int InsertModeTracking(string uidDevice, DBMode modeToAdd)
        {
            Telemetry.TrackEvent("InsertModeTracking");

            if (modeToAdd == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0460", "mode is null", "a mode must be define"), HttpStatusCode.NotFound);
            if (modeToAdd.Device_iddevice < 1) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0461", "invalid device id", "the device id is incorrect, must be greater than 0"), HttpStatusCode.Unauthorized);
            if (modeToAdd.Seekios_idseekios < 1) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0462", "invalid seekios id", "the seekios id is incorrect, must be greater than 0"), HttpStatusCode.Unauthorized);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);
                // Put this user credit verification in the InsertMode stored procedure
                modeToAdd.ModeDefinition_idmodeDefinition = (int)ModeDefinitions.ModeTracking;
                var idmode = InsertMode(seekiosEntities
                    , userDb
                    , modeToAdd
                    , "0x0463"
                    , "0x0464"
                    , "0x0465"
                    , "0x0466"
                    , "0x0467");
                // Broadcast user
                modeToAdd.Idmode = idmode;
                SignalRHelper.BroadcastUser(HubProxyEnum.TrackingHub, SignalRHelper.METHOD_INSERT_MODE_TRACKING, new object[]
                {
                    userDb.iduser,
                    uidDevice,
                    JsonConvert.SerializeObject(modeToAdd, JsonSetting)
                });
                return idmode;
            }
        }

        /// <summary>
        /// Add a mode zone with alert(s) and recipient(s)
        /// </summary>
        /// <param name="modeToAdd">mode zone to add</param>
        /// <param name="alertsWithRecipients">list alerts with recipients</param>
        /// <returns>new id mode zone</returns>
        public int InsertModeZone(string uidDevice, DBMode modeToAdd, List<BDAlertWithRecipient> alertsWithRecipients)
        {
            if (modeToAdd == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0470", "mode is null", "a mode must be define"), HttpStatusCode.NotFound);
            if (modeToAdd.Device_iddevice < 1) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0471", "invalid device id", "the device id is incorrect, must be greater than 0"), HttpStatusCode.Unauthorized);
            if (modeToAdd.Seekios_idseekios < 1) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0472", "invalid seekios id", "the seekios id is incorrect, must be greater than 0"), HttpStatusCode.Unauthorized);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Add mode
                var userDb = IsUserAuthentified(seekiosEntities);
                modeToAdd.ModeDefinition_idmodeDefinition = (int)ModeDefinitions.ModeZone;
                var idmode = InsertMode(seekiosEntities
                    , userDb
                    , modeToAdd
                    , "0x0473"
                    , "0x0474"
                    , "0x0475"
                    , "0x0476"
                    , "0x0477");
                // Add alerts
                if (alertsWithRecipients?.Count() > 0 && idmode > 0)
                {
                    foreach (var alert in alertsWithRecipients)
                    {
                        alert.Mode_idmode = idmode;
                        InsertAlertWithRecipients(seekiosEntities, alert); // each time = 2 database requests
                    }
                }
                // Broadcast user
                modeToAdd.Idmode = idmode;
                SignalRHelper.BroadcastUser(HubProxyEnum.ZoneHub, SignalRHelper.METHOD_INSERT_MODE_ZONE, new object[]
                {
                    userDb.iduser,
                    uidDevice,
                    JsonConvert.SerializeObject(modeToAdd, JsonSetting)
                });
                return idmode;
            }
        }

        /// <summary>
        /// Add a mode don't move with alert(s) and recipient(s)
        /// </summary>
        /// <param name="modeToAdd">mode don't move to add</param>
        /// <param name="alertsWithRecipients">list alerts with recipients</param>
        /// <returns>new id mode don't move</returns>
        public int InsertModeDontMove(string uidDevice, DBMode modeToAdd, List<BDAlertWithRecipient> alertsWithRecipients)
        {
            if (modeToAdd == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0480", "mode is null", "a mode must be define"), HttpStatusCode.NotFound);
            if (modeToAdd.Device_iddevice < 1) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0481", "invalid device id", "the device id is incorrect, must be greater than 0"), HttpStatusCode.Unauthorized);
            if (modeToAdd.Seekios_idseekios < 1) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0482", "invalid seekios id", "the seekios id is incorrect, must be greater than 0"), HttpStatusCode.Unauthorized);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Add mode
                var userDb = IsUserAuthentified(seekiosEntities);
                modeToAdd.ModeDefinition_idmodeDefinition = (int)ModeDefinitions.ModeDontMove;
                var idmode = InsertMode(seekiosEntities
                    , userDb
                    , modeToAdd
                    , "0x0483"
                    , "0x0484"
                    , "0x0485"
                    , "0x0486"
                    , "0x0487");
                // Add alerts
                if (alertsWithRecipients?.Count() > 0 && idmode > 0)
                {
                    foreach (var alert in alertsWithRecipients)
                    {
                        alert.Mode_idmode = idmode;
                        InsertAlertWithRecipients(seekiosEntities, alert); // each time = 2 database requests
                    }
                }
                // Broadcast user
                modeToAdd.Idmode = idmode;
                SignalRHelper.BroadcastUser(HubProxyEnum.DontMoveHub, SignalRHelper.METHOD_INSERT_MODE_DONT_MOVE, new object[]
                {
                    userDb.iduser,
                    uidDevice,
                    JsonConvert.SerializeObject(modeToAdd, JsonSetting)
                });
                return idmode;
            }
        }

        /// <summary>
        /// Update a mode (could update those properties : trame, statusDefinition_idstatusDefinition, countOfTriggeredAlert, device_iddevice)
        /// </summary>
        /// <param name="mode">mode to update</param>
        /// <returns>return 1 if it's working</returns>
        [Obsolete("The method is not implemented in the interface / need to add parameter List<BDAlertWithRecipient> alertsWithRecipients")]
        public int UpdateMode(DBMode mode)
        {
            Telemetry.TrackEvent("UpdateMode");

            if (mode == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0490", "mode is null", "a mode must be define"), HttpStatusCode.NotFound);
            if (mode.Idmode < 1) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0491", "invalid mode id", "the mode id is incorrect, must be greater than 0"), HttpStatusCode.Unauthorized);
            if (mode.Device_iddevice < 1) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0492", "invalid device id", "the device id is incorrect, must be greater than 0"), HttpStatusCode.Unauthorized);
            if (mode.Seekios_idseekios < 1) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0493", "invalid seekios id", "the seekios id is incorrect, must be greater than 0"), HttpStatusCode.Unauthorized);
            if (mode.ModeDefinition_idmodeDefinition != (int)ModeDefinitions.ModeTracking
                || mode.ModeDefinition_idmodeDefinition != (int)ModeDefinitions.ModeZone
                || mode.ModeDefinition_idmodeDefinition != (int)ModeDefinitions.ModeDontMove) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0494", "invalid mode definition", "the mode definition is incorrect"), HttpStatusCode.Unauthorized);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the user
                var userDb = IsUserAuthentified(seekiosEntities);
                // Check if the user still have credit
                if (!CreditBillingHelper.UserCanAffordAction(seekiosEntities, userDb, 0)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0495", "not enough credit", "you have no credit to execute this action"), HttpStatusCode.Unauthorized);
                // Get the mode to update
                var modeToUpdate = (from m in seekiosEntities.mode
                                    where m.idmode == mode.Idmode
                                    select m).FirstOrDefault();
                if (modeToUpdate == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0496", "no mode found", "there is no mode matching with the id mode"), HttpStatusCode.Unauthorized);
                // Get the seekios
                var seekiosDb = (from s in seekiosEntities.seekios
                                 where s.idseekios == modeToUpdate.seekios_idseekios
                                 select s).FirstOrDefault();
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0497", "no seekios found", "the id does not match with any seekios id"), HttpStatusCode.NotFound);
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0498", "seekios unauthorized", "the seekios does not belong to you"), HttpStatusCode.Unauthorized);

                modeToUpdate.trame = mode.Trame;
                modeToUpdate.statusDefinition_idstatusDefinition = mode.StatusDefinition_idstatusDefinition;
                modeToUpdate.countOfTriggeredAlert = mode.CountOfTriggeredAlert;
                modeToUpdate.device_iddevice = mode.Device_iddevice;
                // Send the new instruction
                PrepareInstructionForNewMode(seekiosEntities, modeToUpdate, seekiosDb);
                return 1;
            }
        }

        /// <summary>
        /// Delete a mode
        /// </summary>
        /// <param name="id">mode id</param>
        /// <returns>return 1 if it's working</returns>
        public int DeleteMode(string uidDevice, string idmode)
        {
            Telemetry.TrackEvent("DeleteMode");

            int idMode = 0;
            if (!int.TryParse(idmode, out idMode)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0500", "invalid idmode", "the idmode must be an Int"), HttpStatusCode.NotFound);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the user
                var userDb = IsUserAuthentified(seekiosEntities);
                // Verify if the mode belong to the user
                var seekiosDb = (from m in seekiosEntities.mode
                                 join s in seekiosEntities.seekios on m.seekios_idseekios equals s.idseekios
                                 where m.idmode == idMode
                                 select s).FirstOrDefault();
                // If the seekios exists
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0501", "no seekios found", "the id does not match with any seekios id"), HttpStatusCode.NotFound);
                // If the seekios belongs to the user
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0502", "mode unauthorized", "the idmode does not belong to you"), HttpStatusCode.Unauthorized);
                // Delete the mode
                var result = new System.Data.Entity.Core.Objects.ObjectParameter("Result", 0);
                // Delete the mode in database
                // result return 2 if we need to send an instruction to the seekios to remove the mode
                // result return 1 if we don't need to remove the mode on the seekios
                seekiosEntities.DeleteModeById(idMode, result);
                if (result.Value.ToString() == "0") throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0503", "something wrong", "the stored procedure DeleteMode does not work"), HttpStatusCode.NotFound);
                else if (result.Value.ToString() == "2")
                {
                    PrepareInstructionForNewMode(seekiosEntities, null, seekiosDb);
                }
                // Broadcast user
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_DELETE_MODE, new object[]
                {
                    userDb.iduser,
                    uidDevice,
                    idmode
                });
                return 1;
            }
        }

        /// <summary>
        /// Relance un mode depuis son état initial
        /// </summary>
        /// <param name="id">mode id</param>
        /// <returns>return 1 if it's working</returns>
        [Obsolete("The method is not implemented in the interface")]
        public int RestartMode(string idmode)
        {
            Telemetry.TrackEvent("RestartMode");

            int idMode = 0;
            if (!int.TryParse(idmode, out idMode)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0510", "invalid idmode", "the idmode must be an Int"), HttpStatusCode.NotFound);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // get the user
                var userDb = IsUserAuthentified(seekiosEntities);
                // get the mode
                var modeBdd = (from m in seekiosEntities.mode
                               where m.idmode == idMode
                               select m).FirstOrDefault();
                if (modeBdd == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0511", "no mode found", "the idmode does not match with any idmode"), HttpStatusCode.NotFound);
                // get the seekios
                var seekiosDb = (from s in seekiosEntities.seekios
                                 where s.idseekios == modeBdd.seekios_idseekios
                                 select s).FirstOrDefault();
                // if the seekios exists
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0512", "no seekios found", "the id does not match with any seekios id"), HttpStatusCode.NotFound);
                // if the seekios belongs to the user
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0513", "mode unauthorized", "the idmode does not belong to you"), HttpStatusCode.Unauthorized);
                // reset the statut definition (restart mode)
                modeBdd.statusDefinition_idstatusDefinition = 1;
                seekiosEntities.SaveChanges();
                // send the new instruction to the seekios
                PrepareInstructionForNewMode(seekiosEntities, modeBdd, seekiosDb);
            }
            return 1;
        }

        #endregion

        #region (Ex 0x0650 -> 0x0799) Alerts

        /// <summary>
        /// Get the list of the alerts that belong to a user
        /// </summary>
        public IEnumerable<DBAlert> Alerts()
        {
            Telemetry.TrackEvent("Alerts");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);
                return GetAlertByUser(seekiosEntities, userDb.iduser);
            }
        }

        /// <summary>
        /// Get the list of the alerts that belong to specific a mode
        /// </summary>
        /// <param name="id">id mode</param>
        /// <returns>list of alerts</returns>
        public IEnumerable<DBAlert> AlertsByMode(string idmode)
        {
            Telemetry.TrackEvent("AlertsByMode");

            int idMode = 0;
            if (!int.TryParse(idmode, out idMode)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0660", "invalid idmode", "the idmode must be an Int"), HttpStatusCode.NotFound);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // get the user
                var userDb = IsUserAuthentified(seekiosEntities);
                // verify if the mode belong to the user
                var seekiosDb = (from m in seekiosEntities.mode
                                 join s in seekiosEntities.seekios on m.seekios_idseekios equals s.idseekios
                                 where m.idmode == idMode
                                 select s).FirstOrDefault();
                // if the seekios exists
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0661", "no mode found", "the idmode does not match with any seekios id"), HttpStatusCode.NotFound);
                // if the seekios belongs to the user
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0662", "mode unauthorized", "the idmode does not belong to you"), HttpStatusCode.Unauthorized);
                // get the list of the alerts
                var source = new List<DBAlert>();
                var lsAlerts = (from a in seekiosEntities.alert
                                where a.mode_idmode == idMode
                                select a).ToArray();
                foreach (var alert in lsAlerts)
                {
                    source.Add(DBAlert.AlertToDbAlert(alert));
                }
                return source;
            }
        }

        /// <summary>
        /// Update the state IsRead of an alert SOS
        /// </summary>
        /// <param name="idalert">id alert</param>
        /// <returns>return 1 if it's working</returns>
        public int AlertSOSHasBeenRead(string uidDevice, string idseekios)
        {
            Telemetry.TrackEvent("AlertSOSHasBeenRead");

            int idSeekios = 0;
            if (!int.TryParse(idseekios, out idSeekios)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0670", "invalid idseekios", "the idseekios must be an Int"), HttpStatusCode.NotFound);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the user
                var userDb = IsUserAuthentified(seekiosEntities);
                // Verify if the mode belong to the user
                var seekiosDb = (from s in seekiosEntities.seekios
                                 where s.idseekios == idSeekios
                                 select s).FirstOrDefault();
                // If the seekios exists
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0671", "no alert found", "the idalert does not match with any seekios id"), HttpStatusCode.NotFound);
                // If the seekios belongs to the user
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0672", "alert unauthorized", "the alert does not belong to you"), HttpStatusCode.Unauthorized);
                // Update the value isRead, that mean the user seen the alert sos on the app
                seekiosDb.isLastSOSRead = 1;
                seekiosEntities.SaveChanges();
                // Broadcast user
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_ALERT_HAS_BEEN_READ, new object[]
                {
                    userDb.iduser,
                    uidDevice,
                    idseekios
                });
                return 1;
            }
        }

        /// <summary>
        /// Add an alert with the recipients associates
        /// </summary>
        /// <param name="alertWithRecipient">Alert with the recipients</param>
        /// <returns>return the id alert inserted</returns>
        public int InsertAlertSOSWithRecipient(string uidDevice, string idseekios, BDAlertWithRecipient alertWithRecipient)
        {
            Telemetry.TrackEvent("InsertAlertSOSWithRecipient");

            int idSeekios = 0;
            if (!int.TryParse(idseekios, out idSeekios)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0680", "invalid idseekios", "the idseekios must be an Int"), HttpStatusCode.NotFound);
            if (string.IsNullOrEmpty(alertWithRecipient.Title)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0681", "Title is empty", "the title of the alert must be define"), HttpStatusCode.NotFound);
            if (string.IsNullOrEmpty(alertWithRecipient.Content)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0682", "Content is empty", "the content of the alert must be define"), HttpStatusCode.NotFound);
            if (alertWithRecipient.LsRecipients?.Count == 0) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0683", "the list of recipient is empty", "you must define at least one recipient for the alert"), HttpStatusCode.NotFound);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the user
                var userDb = IsUserAuthentified(seekiosEntities);
                // Verify if the mode belong to the user
                var seekiosDb = (from s in seekiosEntities.seekios
                                 where s.idseekios == idSeekios
                                 select s).FirstOrDefault();
                // If the seekios exists
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0684", "no seekios found", "the id does not match with any seekios id"), HttpStatusCode.NotFound);
                // If the seekios belongs to the user
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0685", "seekios unauthorized", "the seekios does not belong to you"), HttpStatusCode.Unauthorized);
                // Insert the alert sos
                alertWithRecipient.AlertDefinition_idalertType = (int)AlertDefinition.SOS;
                alertWithRecipient.Mode_idmode = null;
                var idAlert = InsertAlertWithRecipients(seekiosEntities, alertWithRecipient);
                // Update the seekios mode
                seekiosDb.alertSOS_idalert = idAlert;
                seekiosEntities.SaveChanges();
                // Broadcast user
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_INSERT_ALERT_SOS_WITH_RECIPIENT, new object[]
                {
                    userDb.iduser,
                    uidDevice,
                    idseekios,
                    JsonConvert.SerializeObject(alertWithRecipient, JsonSetting)
                });
                return idAlert;
            }
        }

        /// <summary>
        /// Update a alert with the recipients associate
        /// </summary>
        /// <param name="alertWithRecipient">Alerts with reciepients</param>
        /// <returns>return 1 if it's working</returns>
        public int UpdateAlertSOSWithRecipient(string uidDevice, string idseekios, BDAlertWithRecipient alertWithRecipient)
        {
            Telemetry.TrackEvent("UpdateAlertSOSWithRecipient");

            int idSeekios = 0;
            if (!int.TryParse(idseekios, out idSeekios)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0690", "invalid idseekios", "the idseekios must be an Int"), HttpStatusCode.NotFound);
            if (string.IsNullOrEmpty(alertWithRecipient.Title)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0691", "Title is empty", "the title of the alert must be define"), HttpStatusCode.NotFound);
            if (string.IsNullOrEmpty(alertWithRecipient.Content)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0692", "Content is empty", "the content of the alert must be define"), HttpStatusCode.NotFound);
            if (alertWithRecipient.LsRecipients?.Count == 0) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0693", "the list of recipient is empty", "you must define at least one recipient for the alert"), HttpStatusCode.NotFound);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the user
                var userDb = IsUserAuthentified(seekiosEntities);
                // Verify if the mode belong to the user
                var seekiosDb = (from s in seekiosEntities.seekios
                                 where s.idseekios == idSeekios
                                 select s).FirstOrDefault();
                // If the seekios exists
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0694", "no seekios found", "the id does not match with any seekios id"), HttpStatusCode.NotFound);
                // If the seekios belongs to the user
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0695", "seekios unauthorized", "the seekios does not belong to you"), HttpStatusCode.Unauthorized);
                alertWithRecipient.AlertDefinition_idalertType = (int)AlertDefinition.SOS;
                alertWithRecipient.Mode_idmode = null;
                UpdateAlertWithRecipients(seekiosEntities, alertWithRecipient);
                // Broadcast user
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_INSERT_ALERT_SOS_WITH_RECIPIENT, new object[]
                {
                    userDb.iduser,
                    uidDevice,
                    idseekios,
                    JsonConvert.SerializeObject(alertWithRecipient, JsonSetting)
                });
                return 1;
            }
        }

        #endregion

        #region (Ex 0x0800 -> 0x0899) AlertRecipient

        /// <summary>
        /// Get the list of the alert recipient that belong to specific a mode
        /// </summary>
        /// <returns>list of the alert recipient</returns>
        public IEnumerable<DBAlertRecipient> AlertRecipients()
        {
            Telemetry.TrackEvent("AlertRecipients");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var userDb = IsUserAuthentified(seekiosEntities);
                return GetAlertRecipientByUser(seekiosEntities, userDb.iduser);
            }
        }

        #endregion

        #region (Ex 0x0800 -> 0x0899) PackCredit

        /// <summary>
        /// Get the list of credit packs
        /// </summary>
        /// <param name="language">language</param>
        /// <returns>list of the credit packs</returns>
        [Obsolete("The method is not implemented in the interface")]
        public IEnumerable<DBPackCredit> CreditPacksByLanguage(string language)
        {
            Telemetry.TrackEvent("CreditPackByLanguage");

            if (!_languageCreditPack.Contains(language)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0800", "invalid language", "the value of the language is invalid, go to seekios.com/api"), HttpStatusCode.NotFound);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var source = new List<DBPackCredit>();
                var packCredits = (from pc in seekiosEntities.packCredit
                                   where pc.language == language
                                   select pc).ToArray();
                foreach (var packCredit in packCredits)
                {
                    source.Add(DBPackCredit.PackCreditToDBPackCredit(packCredit));
                }
                return source;
            }
        }

        /// <summary>
        /// Get the list of the operations 
        /// </summary>
        /// <returns>list of the operations</returns>
        public IEnumerable<DBOperation> OperationHistoric()
        {
            Telemetry.TrackEvent("OperationHistoric");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // get the user
                var userDb = IsUserAuthentified(seekiosEntities);
                // get the list of the operation
                var lsOperations = (from o in seekiosEntities.operation
                                    where o.user_iduser == userDb.iduser
                                    orderby o.dateEndOperation descending
                                    select o).Take(MAX_RETURN_OPERATION).ToArray();

                var source = new List<DBOperation>();
                if (lsOperations?.Count() <= 0) return source;
                foreach (operation operation in lsOperations)
                {
                    source.Add(new DBOperation()
                    {
                        CA = operation.amount,
                        IsOnS = operation.isOnSeekios,
                        DB = operation.dateBeginOperation,
                        DE = operation.dateEndOperation,
                        IdO = operation.idoperation,
                        IdD = null,
                        IdM = operation.mode_idmode,
                        IdS = operation.seekios_idseekios,
                        IdU = operation.user_iduser,
                        Op = operation.operationType.idoperationType,
                    });
                }
                return source;
            }
        }

        /// <summary>
        /// Get the list of the transaction operations
        /// </summary>
        /// <returns>return a list of the transaction operations</returns>
        public IEnumerable<DBOperationFromStore> OperationFromStoreHistoric()
        {
            Telemetry.TrackEvent("OperationFromStoreHistoric");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // get the user
                var userDb = IsUserAuthentified(seekiosEntities);
                // get the list of the transaction operation
                var lsOperationTransactions = (from ot in seekiosEntities.operationFromStore
                                               where ot.idUser == userDb.iduser
                                               orderby ot.dateTransaction descending
                                               select ot).ToArray();
                var source = new List<DBOperationFromStore>();
                if (lsOperationTransactions?.Count() <= 0) return source;
                foreach (var operationTransaction in lsOperationTransactions)
                {
                    source.Add(new DBOperationFromStore()
                    {
                        IdOperationFromStore = operationTransaction.idoperationFromStore,
                        CreditsPurchased = operationTransaction.creditsPurchased,
                        DateTransaction = operationTransaction.dateTransaction,
                        IdPack = operationTransaction.idPack,
                        IsPackPremium = operationTransaction.isPackPremium,
                        IdUser = operationTransaction.idUser,
                        RefStore = operationTransaction.refStore,
                        Status = operationTransaction.status,
                        VersionApp = operationTransaction.versionApp,
                    });
                }
                return source;
            }
        }

        /// <summary>
        /// Gets a Purchase object from the app representing 
        /// </summary>
        /// <param name="purchase">purchase object (from the store)</param>
        /// <returns>return 1 if it's working</returns>
        public int InsertInAppPurchase(PurchaseDTO purchase)
        {
            Telemetry.TrackEvent("InsertInAppPurchase");

            if (string.IsNullOrEmpty(purchase.KeyProduct)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0830", "KeyProduct is empty", "the KeyProduct must be define"), HttpStatusCode.NotFound);
            //bool isSubscription = false;  // pack could be a subscription, a renew payment every month (not use currently)

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the user
                var userDb = IsUserAuthentified(seekiosEntities);
                if (userDb.iduser != purchase.IdUser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0831", "KeyProduct is empty", "the KeyProduct must be define"), HttpStatusCode.NotFound);

                DateTime? dateTimeOfPayment = null;
                if (purchase.StoreId == (int)PlatformEnum.Android)
                {
                    // exception 0x0832 -> 0x0836
                    IsPurchaseValidForAndroid(seekiosEntities, purchase, ref dateTimeOfPayment);
                }
                else if (purchase.StoreId == (int)PlatformEnum.iOS)
                {
                    // exception 0x0837
                    // TODO: no more additional checks with apple store (security issue) : add an additional security layer
                    purchase = IsPurchaseValidForiOS(purchase);
                }

                // Add the credit bought by the user
                var creditPackBought = (from p in seekiosEntities.packCreditAndOperationType
                                        where p.idProduct == purchase.KeyProduct
                                        select p).Take(1).FirstOrDefault();
                if (creditPackBought == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0838", "CreditPack is missing", string.Format("impossible to find the pack '{0}' in the database", purchase.KeyProduct)), HttpStatusCode.NotFound);

                if (CreditBillingHelper.GiveCreditsToUser(seekiosEntities /*isSubscription,*/
                    , userDb
                    , creditPackBought
                    , purchase
                    , dateTimeOfPayment) == 1)
                {
                    // Broadcast user devices
                    SignalRHelper.BroadcastUser(HubProxyEnum.CreditsHub, SignalRHelper.METHOD_REFRESH_CREDIT, new object[]
                    {
                        userDb.iduser,
                        string.Empty,
                        creditPackBought.rewarding,
                        0,
                        DateTime.UtcNow
                    });
                }
                else throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0839", "something wrong", "impossible to add the transaction in the database, rollback."), HttpStatusCode.NotFound);
                return 1;
            }
        }

        #endregion

        #region (Ex 0x0900 -> 0x0949) Version Application

        /// <summary>
        /// Check is a new version app is available or not
        /// </summary>
        /// <param name="id">application version number</param>
        /// <returns>1 : a new app version is available, 0 : no new app version</returns>
        [Obsolete("The method is not implemented in the interface / already implemented in UserEnvirnoment")]
        public int IsSeekiosVersionApplicationNeedForceUpdate(string id, string plateforme)
        {
            Telemetry.TrackEvent("IsSeekiosVersionApplicationNeedForceUpdate");

            int idPlateforme = 0;
            if (!int.TryParse(plateforme, out idPlateforme)) return -1;

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                if (seekiosEntities.versionApplication == null) return -1;

                var lastBlockingVersionAvailableNumber = (from vn in seekiosEntities.versionApplication
                                                          where vn.isNeedUpdate == 1 && vn.plateforme == idPlateforme
                                                          orderby vn.version_dateCreation descending
                                                          select vn.versionNumber).Take(1).FirstOrDefault();

                if (lastBlockingVersionAvailableNumber == null) return -1;


                if (idPlateforme == (int)PlatformEnum.Android)
                {
                    if (CalcultateChangset(lastBlockingVersionAvailableNumber, 4) > CalcultateChangset(id, 4))
                    {
                        return 1;
                    }
                }
                else if (idPlateforme == (int)PlatformEnum.iOS)
                {
                    if (CalcultateChangset(lastBlockingVersionAvailableNumber, 3) > CalcultateChangset(id, 3))
                    {
                        return 1;
                    }
                }

                return 0;
            }
        }

        #endregion

        #region (Ex 0x0950 -> 0x0999) Version Embedded (use for the update software)

        /// <summary>
        /// Return the las version of the embedded firmware (use for the update software)
        /// </summary>
        public DBVersionEmbedded LastEmbeddedVersion()
        {
            Telemetry.TrackEvent("LastEmbeddedVersion");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                return DBVersionEmbedded.VersionEmbeddedToDBVersionEmbedded((from v in seekiosEntities.versionEmbedded
                                                                             orderby v.dateVersionCreation descending
                                                                             select v).FirstOrDefault());
            }
        }

        /// <summary>
        /// Return the seekios version number (use for the update software)
        /// </summary>
        /// <param name="uidSeekios">unique seekios identifier</param>
        public string SeekiosVersion(string uidSeekios)
        {
            Telemetry.TrackEvent("SeekiosVersion");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                return (from v in seekiosEntities.versionEmbedded
                        join sp in seekiosEntities.seekiosProduction on v.idVersionEmbedded equals sp.versionEmbedded_idversionEmbedded
                        join s in seekiosEntities.seekios on sp.idseekiosProduction equals s.idseekios
                        where sp.uidSeekios == uidSeekios
                        select v.versionName).FirstOrDefault();
            }
        }

        /// <summary>
        /// Get the small seekios object (use for the update software)
        /// </summary>
        /// <param name="uidSeekios">unique seekios identifier</param>
        public ShortSeekiosDTO SeekiosName(string uidSeekios)
        {
            Telemetry.TrackEvent("SeekiosName");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var seekiosDb = (from s in seekiosEntities.seekios
                                 join sp in seekiosEntities.seekiosProduction on s.idseekios equals sp.idseekiosProduction
                                 where sp.uidSeekios == uidSeekios
                                 select s).FirstOrDefault();
                if (seekiosDb == null) return null;
                return ShortSeekiosDTO.SeekiosToShortSeekiosDTO(seekiosDb);
            }
        }

        /// <summary>
        /// Update the seekios version number (use for the update software)
        /// When the seekios do the firmware update, we update the database with the new version name
        /// </summary>
        /// <param name="uidSeekios">unique seekios identifier</param>
        /// <param name="versionName">version name to update</param>
        public int UpdateVersionEmbedded(string uidSeekios, string versionName)
        {
            Telemetry.TrackEvent("UpdateVersionEmbedded");

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                var version = (from v in seekiosEntities.versionEmbedded
                               where v.versionName == versionName
                               select v).FirstOrDefault();
                if (version == null) return -1;

                var seekiosProduction = (from s in seekiosEntities.seekiosProduction
                                         where s.uidSeekios == uidSeekios
                                         select s).FirstOrDefault();
                if (seekiosProduction == null) return -2;
                seekiosProduction.versionEmbedded_idversionEmbedded = version.idVersionEmbedded;
                seekiosProduction.lastUpdateConfirmed = 0;
                seekiosEntities.SaveChanges();
                return 1;
            }
        }

        #endregion

        #region (Ex 0x1000 -> 0x1099) Notifications

        /// <summary>
        /// Settings for notification
        /// </summary>
        /// <returns>return 1 if it's working</returns>
        public int UpdateNotificationSetting(string idSeekiosStr
            , string uidDevice
            , string SendNotificationOnNewTrackingLocationStr
            , string SendNotificationOnNewOutOfZoneLocationStr
            , string SendNotificationOnNewDontMoveLocationStr)
        {
            Telemetry.TrackEvent("UpdateNotificationSettings");

            int idSeekios = 0;
            bool sendNotificationOnNewTrackingLocation, sendNotificationOnNewOutOfZoneLocation, sendNotificationOnNewDontMoveLocation;
            if (!int.TryParse(idSeekiosStr, out idSeekios)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x1000", "error parsing idSeekios", "the idSeekios need to an Integer"), HttpStatusCode.Unauthorized);
            if (!bool.TryParse(SendNotificationOnNewTrackingLocationStr, out sendNotificationOnNewTrackingLocation)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x1001", "error parsing SendNotificationOnNewTrackingLocationStr", "the SendNotificationOnNewTrackingLocationStr need to an bool"), HttpStatusCode.Unauthorized);
            if (!bool.TryParse(SendNotificationOnNewOutOfZoneLocationStr, out sendNotificationOnNewOutOfZoneLocation)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x1002", "error parsing SendNotificationOnNewOutOfZoneLocationStr", "the SendNotificationOnNewOutOfZoneLocationStr need to an bool"), HttpStatusCode.Unauthorized);
            if (!bool.TryParse(SendNotificationOnNewDontMoveLocationStr, out sendNotificationOnNewDontMoveLocation)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x1003", "error parsing SendNotificationOnNewDontMoveLocationStr", "the SendNotificationOnNewDontMoveLocationStr need to an bool"), HttpStatusCode.Unauthorized);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the user
                var userDb = IsUserAuthentified(seekiosEntities);
                // Get the seekios
                var seekiosDb = (from s in seekiosEntities.seekios
                                 where s.idseekios == idSeekios
                                 select s).FirstOrDefault();
                if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x1004", "seekios not found", "seekios not found"), HttpStatusCode.Unauthorized);
                if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x1005", "invalid idSeekios", "the idSeekios does not belong to you"), HttpStatusCode.Unauthorized);
                // Update the notification settings
                seekiosDb.sendNotificationOnNewTrackingLocation = (sendNotificationOnNewTrackingLocation) ? (byte)1 : (byte)0;
                seekiosDb.sendNotificationOnNewOutOfZoneLocation = (sendNotificationOnNewOutOfZoneLocation) ? (byte)1 : (byte)0;
                seekiosDb.sendNotificationOnNewDontMoveLocation = (sendNotificationOnNewDontMoveLocation) ? (byte)1 : (byte)0;
                seekiosEntities.SaveChanges();
                // Broadcast user
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_UPDATE_SEEKIOS, new object[]
                {
                    userDb.iduser,
                    uidDevice,
                    seekiosDb
                });
                return 1;
            }
        }

        #endregion

        #region [Test Methods] Vodafone

        /// <summary>
        /// Test method for Vodafone M2M API. Not supposed to be in production
        /// </summary>
        /// <param name="requete"></param>
        /// <returns></returns>
        public int VodafonTestAPI(string imsi, string token)
        {
            Telemetry.TrackEvent("VodafonTestAPI");

            if (token != "sijfe56FQ6fjk56Lcsqk") throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0000", "token invalid", "the secret token is invalid"), HttpStatusCode.NotFound);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // get the seekios prod with imsi number
                var seekiosProdBdd = (from sp in seekiosEntities.seekiosProduction
                                      where sp.imsi == imsi
                                      select sp).FirstOrDefault();
                if (seekiosProdBdd == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0000", "imsi invalid", "the imsi does not match with any imsi in the database"), HttpStatusCode.NotFound);
                // send a sms wake up to the seekios in order to test the vodafone api 
                // (see the result in the bdd)
                SendSMSWakeUp(seekiosEntities, imsi);
                return 1;
            }
        }

        #endregion

        #endregion

        #region ----- PUBLIC METHODS --------------------------------------------------------------------------

        /// <summary>
        /// Get the platforms used by a user
        /// </summary>
        public static void GetPlatformsByUser(seekios_dbEntities seekiosEntities
            , int idUser
            , out bool containsAndroid
            , out bool containsiOS
            , out bool containsWeb
            , int doNotDisturb = -1)
        {
            containsAndroid = false;
            containsiOS = false;
            containsWeb = false;
            if (idUser <= 0) return;
            var platforms = (from u in seekiosEntities.user
                             join d in seekiosEntities.device on u.iduser equals d.user_iduser
                             where u.iduser == idUser
                             && d.doNotDisturb == 1 /* added that condition to prevent spamming of tracking + refresh credits */
                             select d.plateform.ToUpper()).ToList();

            var androidCount = platforms.FindAll(el => el.Contains("ANDROID")).Count;
            var iOSCount = platforms.FindAll(el => el.Contains("IOS")).Count();
            if (androidCount > 0) containsAndroid = true;
            if (iOSCount > 0) containsiOS = true;
            if (platforms.Count() - (androidCount + iOSCount) > 0) containsWeb = true;
        }

        #endregion

        #region ----- PRIVATES METHODS ----------------------------------------------------- (no telemetry) ---

        #region Authentification

        private static user IsUserAuthentified(seekios_dbEntities seekiosEntities)
        {
            var tokenHeader = WebOperationContext.Current.IncomingRequest.Headers["token"];
            // get the user from the token
            var user = (from u in seekiosEntities.user
                        join t in seekiosEntities.token on u.iduser equals t.user_iduser
                        where t.authToken == tokenHeader && t.dateExpiresToken > DateTime.UtcNow
                        select u).FirstOrDefault();
            // if the user does not exist, throw an exception
            if (user == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x00005", "unauthorized access", "no token"), HttpStatusCode.Unauthorized);
            return user;
        }

        #endregion

        #region Handle Get Data

        private UserEnvironment GetUserEnvironment(seekios_dbEntities seekiosEntities
            , user userDb
            , string platform
            , string deviceModel
            , string version
            , string uidDevice
            , string countryCode)
        {
            var dateConnection = DateTime.UtcNow;
            var idUser = userDb.iduser;
            var deviceDb = (from d in seekiosEntities.device
                            where d.user_iduser == idUser
                            && d.uidDevice == uidDevice
                            select d).Take(1).FirstOrDefault();
            if (deviceDb == null)
            {
                // if it's not exist, we create a new device
                deviceDb = new device()
                {
                    user_iduser = userDb.iduser,
                    deviceName = deviceModel,
                    notificationPlayerId = string.Empty,
                    lastUseDate = dateConnection,
                    uidDevice = uidDevice,
                    os = version,
                    plateform = platform,
                    countryCode = countryCode,
                    doNotDisturb = 1
                };
                seekiosEntities.device.Add(deviceDb);
                seekiosEntities.SaveChanges();
            }
            else
            {
                // if it's exit, we update the last use date and the country code
                deviceDb.notificationPlayerId = string.Empty;
                deviceDb.countryCode = countryCode;
                deviceDb.lastUseDate = dateConnection;
                deviceDb.doNotDisturb = 1;
                seekiosEntities.SaveChanges();
            }

            // we add a new connection line
            seekiosEntities.connection.Add(new connection()
            {
                dateConnection = dateConnection,
                device_iddevice = deviceDb.iddevice,
                ipv4 = string.Empty,
                ipv6 = string.Empty,
                user_iduser = userDb.iduser
            });
            userDb.dateLastConnection = dateConnection;
            seekiosEntities.SaveChanges();

            // get user data
            var userEnvironment = new UserEnvironment();
            userEnvironment.User = DBUser.UserToDBUser(userDb);
            userEnvironment.Device = DBDevice.DeviceToDBDevice(deviceDb);
            userEnvironment.LsSeekios = GetSeekiosByUser(seekiosEntities, idUser);
            if (userEnvironment.LsSeekios != null && userEnvironment.LsSeekios.Count() > 0)
            {
                userEnvironment.LsMode = GetModeByUser(seekiosEntities, idUser);
                userEnvironment.LsAlert = GetAlertByUser(seekiosEntities, idUser);
                userEnvironment.LsAlertRecipient = GetAlertRecipientByUser(seekiosEntities, idUser);
                userEnvironment.LsLocations = new List<DBLocation>();//GetLocationByUser(seekiosEntities, idUser, DateTime.MinValue, DateTime.UtcNow, false);
                userEnvironment.LastVersionEmbedded = GetLastVersionEmbedded(seekiosEntities);
                //userEnvironment.LsAlertFavorite = new List<DBAlertFavorite>();//GetAlertFavoriteByUser(idUser),
                //userEnvironment.LsFriend = new List<FriendUserDTO>();//GetFriendsByUser(idUser),
                //userEnvironment.LsSharing = new List<DBSharing>();//GetSharingByUser(idUser),
                //userEnvironment.LsFavoriteArea = new List<DBFavoriteArea>();//GetFavoritesAreaByUser(idUser),
            }
            userEnvironment.ServerSynchronisationDate = DateTime.UtcNow;
            return userEnvironment;
        }

        private IEnumerable<DBAlert> GetAlertByUser(seekios_dbEntities seekiosEntities, int idUser)
        {
            var source = new List<DBAlert>();
            var result = seekiosEntities.GetAlertAndAlertSOSByUser(idUser).ToArray();
            if (result != null)
            {
                foreach (var alert in result)
                {
                    source.Add(DBAlert.AlertToDbAlert(alert));
                }
            }
            return source;
        }

        private IEnumerable<DBAlertRecipient> GetAlertRecipientByUser(seekios_dbEntities seekiosEntities, int idUser)
        {
            var source = new List<DBAlertRecipient>();
            var result = seekiosEntities.GetAlertRecipientAndAlertRecipientSOSByUser(idUser).ToArray();
            if (result != null)
            {
                foreach (var alertRecipient in result)
                {
                    source.Add(DBAlertRecipient.AlertRecipientToDbAlertRecipient(alertRecipient));
                }
            }
            return source;
        }

        private IEnumerable<DBSeekios> GetSeekiosByUser(seekios_dbEntities seekiosEntities, int idUser)
        {
            // Warning : we are not using sharing yet, so we do not need to use the stored procedure GetAllSeekiosByUser
            var source = new List<DBSeekios>();
            var result = (from s in seekiosEntities.seekiosAndSeekiosProduction
                          where s.user_iduser == idUser
                          select s).ToArray();
            if (result != null)
            {
                foreach (var seekios in result)
                {
                    source.Add(DBSeekios.SeekiosAndSeekiosProductionToDBSeekios(seekios));
                }
            }
            return source;
        }

        private IEnumerable<DBMode> GetModeByUser(seekios_dbEntities seekiosEntities, int idUser)
        {
            var source = new List<DBMode>();
            var result = (from m in seekiosEntities.mode
                          join s in seekiosEntities.seekios on m.seekios_idseekios equals s.idseekios
                          where s.user_iduser == idUser
                          select m).ToArray();
            if (result != null)
            {
                foreach (var mode in result)
                {
                    source.Add(DBMode.ModeToDBMode(mode));
                }
            }
            return source;
        }

        private DBVersionEmbedded GetLastVersionEmbedded(seekios_dbEntities seekiosEntities)
        {
            return DBVersionEmbedded.VersionEmbeddedToDBVersionEmbedded(
                    (from ve in seekiosEntities.versionEmbedded
                     orderby ve.idVersionEmbedded descending
                     where ve.isBetaVersion == false
                     select ve).FirstOrDefault());
        }

        #endregion

        #region Handle Modes

        private static int InsertMode(seekios_dbEntities seekiosEntities
            , user userDb
            , DBMode modeToAdd
            , string exception1
            , string exception2
            , string exception3
            , string exception4
            , string exception5)
        {
            // Get the seekios from the database
            var seekiosDb = (from sp in seekiosEntities.seekios
                             where sp.idseekios == modeToAdd.Seekios_idseekios
                             select sp).FirstOrDefault();
            // If the seekios exists
            if (seekiosDb == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError(exception1, "no seekios found", "the id does not match with any seekios id"), HttpStatusCode.NotFound);
            // If the seekios belongs to the user
            if (seekiosDb.user_iduser != userDb.iduser) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError(exception2, "seekios unauthorized", "the seekios does not belong to you"), HttpStatusCode.Unauthorized);
            // Insert the mode in database
            var result = new System.Data.Entity.Core.Objects.ObjectParameter("ResultExceptionOrNewIdMode", -1);
            seekiosEntities.InsertMode(userDb.iduser
                , modeToAdd.Seekios_idseekios
                , modeToAdd.Device_iddevice
                , modeToAdd.Trame
                , modeToAdd.ModeDefinition_idmodeDefinition
                , modeToAdd.IsPowerSavingEnabled
                , modeToAdd.TimeRefreshTracking
                , modeToAdd.TimeDiffHours
                , modeToAdd.TimeActivation
                , modeToAdd.TimeDesactivation
                , modeToAdd.MaxLocation
                , result);
            // Get the output result from the stored procedure
            int resultOutput = 0;
            int.TryParse(result.Value.ToString(), out resultOutput);
            if (resultOutput == -1) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError(exception3, "something wrong", "the stored procedure InsertMode does not work"), HttpStatusCode.Unauthorized);
            if (resultOutput == -2) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError(exception4, "no device found", "the device_iddevice does not match with any device id"), HttpStatusCode.Unauthorized);
            if (resultOutput == -3) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError(exception5, "not enough credit", "you have no credit to execute this action"), HttpStatusCode.Unauthorized);
            // New mode
            var modeDb = new mode
            {
                idmode = resultOutput,
                modeDefinition_idmodeDefinition = modeToAdd.ModeDefinition_idmodeDefinition,
                statusDefinition_idstatusDefinition = (int)StatusDefinition.RAS,
                trame = modeToAdd.Trame,
                isPowerSavingEnabled = modeToAdd.IsPowerSavingEnabled,
                timeRefreshTracking = modeToAdd.TimeRefreshTracking,
                timeDiffHours = modeToAdd.TimeDiffHours,
                timeActivation = modeToAdd.TimeActivation,
                timeDesactivation = modeToAdd.TimeDesactivation,
                maxLocation = modeToAdd.MaxLocation
            };
            // Update the state power saving 
            if (modeToAdd.IsPowerSavingEnabled) seekiosDb.isInPowerSaving = 1;
            // Send the instruction to the seekios
            PrepareInstructionForNewMode(seekiosEntities, modeDb, seekiosDb);
            // Return the new id mode
            return resultOutput;
        }

        #endregion

        #region Handle Alerts

        /// <summary>
        /// Add an alert
        /// </summary>
        /// <param name="seekiosEntities">the database context</param>
        /// <param name="alert">alert to add</param>
        /// <returns>alert id</returns>
        private int InsertAlert(seekios_dbEntities seekiosEntities, DBAlert alert)
        {
            var alertToAdd = new alert
            {
                content = alert.Content,
                alertDefinition_idalertType = alert.AlertDefinition_idalertType,
                mode_idmode = alert.Mode_idmode,
                title = alert.Title,
                dateAlertCreation = DateTime.UtcNow,
            };

            seekiosEntities.alert.Add(alertToAdd);
            seekiosEntities.SaveChanges();
            return alertToAdd.idalert;
        }

        /// <summary>
        /// Add the alert and the alert recipient(s)
        /// </summary>
        /// <param name="seekiosEntities">the database context</param>
        /// <param name="alertWithRecipients">contains the alert and the alert recipient(s) associate(s)</param>
        /// <returns>the new id alert</returns>
        private int InsertAlertWithRecipients(seekios_dbEntities seekiosEntities, BDAlertWithRecipient alertWithRecipients)
        {
            // add the alert
            var idAlert = InsertAlert(seekiosEntities, new DBAlert
            {
                Mode_idmode = alertWithRecipients.Mode_idmode,
                Title = alertWithRecipients.Title,
                Content = alertWithRecipients.Content,
                AlertDefinition_idalertType = alertWithRecipients.AlertDefinition_idalertType,
                CreationDate = DateTime.UtcNow
            });

            // add the recipients
            if (alertWithRecipients.LsRecipients?.Count > 0)
            {
                foreach (var recipient in alertWithRecipients.LsRecipients)
                {
                    seekiosEntities.alertRecipient.Add(new alertRecipient
                    {
                        alert_idalert = idAlert,
                        nameRecipient = recipient.NameRecipient,
                        phoneNumber = recipient.PhoneNumber,
                        phoneNumberType = recipient.PhoneNumberType,
                        email = recipient.Email,
                        emailType = recipient.EmailType,
                        dateAlertRecipientCreation = DateTime.UtcNow
                    });
                }
                seekiosEntities.SaveChanges();
            }
            return idAlert;
        }

        /// <summary>
        /// Update the alert with the alert recipient(s)
        /// </summary>
        /// <param name="seekiosEntities">the database context</param>
        /// <param name="alertWithRecipients">contains the alert and the alert recipient(s) associate(s)</param>
        private void UpdateAlertWithRecipients(seekios_dbEntities seekiosEntities, BDAlertWithRecipient alertWithRecipients)
        {
            // update the alert
            var alertSOSToUpdate = (from a in seekiosEntities.alert
                                    where a.idalert == alertWithRecipients.Idalert
                                    select a).FirstOrDefault();
            if (alertSOSToUpdate == null) return;

            alertSOSToUpdate.title = alertWithRecipients.Title;
            alertSOSToUpdate.content = alertWithRecipients.Content;

            // delete the current recipients
            var recipientsToDelete = (from ar in seekiosEntities.alertRecipient
                                      where ar.alert_idalert == alertSOSToUpdate.idalert
                                      select ar).ToArray();
            seekiosEntities.alertRecipient.RemoveRange(recipientsToDelete);

            // add the new recipients
            if (alertWithRecipients.LsRecipients?.Count > 0)
            {
                foreach (var recipient in alertWithRecipients.LsRecipients)
                {

                    seekiosEntities.alertRecipient.Add(new alertRecipient
                    {
                        alert_idalert = alertSOSToUpdate.idalert,
                        nameRecipient = recipient.NameRecipient,
                        phoneNumber = recipient.PhoneNumber,
                        phoneNumberType = recipient.PhoneNumberType,
                        email = recipient.Email,
                        emailType = recipient.EmailType,
                        dateAlertRecipientCreation = DateTime.UtcNow
                    });
                }
                seekiosEntities.SaveChanges();
            }
        }

        #endregion

        #region Handle Credit Packs

        /// <summary>
        /// Validate the purchase made by the user on the PlayStore (with security layer)
        /// </summary>
        /// <param name="seekiosEntities">the database context</param>
        /// <param name="purchase">purchase made by the user</param>
        private void IsPurchaseValidForAndroid(seekios_dbEntities seekiosEntities, PurchaseDTO purchase, ref DateTime? dateTimeOfPayment)
        {
            var googleData = JsonConvert.DeserializeObject<GoogleStorePurchaseDTO>(purchase.InnerData);
            if (googleData == null) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0832"
                , "InnerData invalid", "the InnerData is invalid"), HttpStatusCode.NotFound);
            // the keys don't match, the data has been forged...
            if (purchase.KeyProduct != googleData.productId) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0833"
                , "KeyProduct and productId does not match", "KeyProduct and productId does not match"), HttpStatusCode.NotFound);
            //if (purchase.KeyProduct.EndsWith(SUBSCRIPTION_SUFFIX))// if it is a subscription pack
            //{
            //    isSubscription = true;
            //    purchase.KeyProduct = purchase.KeyProduct.Substring(0, purchase.KeyProduct.Length - (SUBSCRIPTION_SUFFIX).Length);//sets the pack name to a non-subscribed one
            //}
            long timestampOfPayment = 0;
            if (!long.TryParse(googleData.purchaseTime, out timestampOfPayment)) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0834"
                , "impossible to retrieve purchaseTime", "impossible to retrieve purchaseTime from the InnerData"), HttpStatusCode.NotFound);
            //checks the data and the signature with google play pubKey
            if (CheckSignatureForPurchase(purchase.InnerData, purchase.Signature) != 1) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0835"
                , "impossible to retrieve purchaseTime", "impossible to retrieve purchaseTime from the InnerData"), HttpStatusCode.NotFound);

            dateTimeOfPayment = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestampOfPayment).ToUniversalTime();
            var transactionBdd = (from ot in seekiosEntities.operationFromStore
                                  orderby ot.dateTransaction descending
                                  select ot).Take(1).FirstOrDefault();
            if (transactionBdd != null)
            {
                if (transactionBdd.dateTransaction >= dateTimeOfPayment) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0836"
                    , "a new transaction incomming", "a new transaction incomming, we can not go further"), HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Validate the purchase made by the user on the AppStore (without security layer)
        /// </summary>
        /// <param name="purchase">purchase made by the user</param>
        /// <returns>the purchase with the KeyProduct modified</returns>
        private PurchaseDTO IsPurchaseValidForiOS(PurchaseDTO purchase)
        {
            // should match with the packs on the AppStore
            string[] chunks = purchase.KeyProduct.Split('.');
            //if (chunks.Length == 5) // if it is a subscription pack
            //{
            //    isSubscription = true;
            //}
            if (chunks.Length < 4) throw new WebFaultException<DefaultCustomError>(new DefaultCustomError("0x0837", "invalid KeyProduct", "invalid KeyProduct for the AppStore"), HttpStatusCode.NotFound);
            string packName = chunks[3];
            packName = packName.ToLower();
            {
                string result = string.Empty;
                // if a mapping exists, then update the pack's name
                if (IOS_PACKNAMES_MAPPING.TryGetValue(packName, out result)) packName = result;
            }
            purchase.KeyProduct = packName + IOS_PACK_SUFFIX;
            return purchase;
        }

        /// <summary>
        /// Check the signature of the purchase
        /// </summary>
        /// <param name="dataContent">match to the InnerData</param>
        /// <param name="dataContent">match to the Signature</param>
        /// <returns>return 1 if it's working</returns>
        private int CheckSignatureForPurchase(string dataContent, string dataSignature)
        {
            byte[] dataOriginal = Encoding.Default.GetBytes(dataContent);
            if (dataSignature == null || dataSignature.Trim().Length == 0) return -1;
            byte[] signature = Convert.FromBase64String(dataSignature);
            using (var rsa = new RSACryptoServiceProvider())
            {
                string modulus; string exponent;
                const int BEGIN_MODULUS = 44;   // debut du modulo dans le fichier
                const int MODULUS_LENGTH = 342; // longueur du modulo (le modulo est suppose etre de 256 bytes mais avec base64 -> 256 * 1.33 = +/- 342.
                const int EXPONENT_LENGTH = 4;  // longueur de l'exposant (tjrs a la fin donc pas besoin de l'indice de debut)
                                                // ceci est l'extraction du modulo et exposant a partir de la cle publique (+conversion base64)
                var sb = new StringBuilder();
                {
                    // read the modulo
                    modulus = PLAYSTORE_PRIVATE_KEY.Substring(BEGIN_MODULUS, MODULUS_LENGTH);//N
                    sb.Append(modulus);
                    int m = MODULUS_LENGTH & 3;
                    if (0 != m)
                    {
                        for (int i = 0; i != m; ++i) sb.Append("=");
                    }
                    modulus = sb.ToString();
                }
                sb.Clear();
                {
                    // read the exponent
                    exponent = PLAYSTORE_PRIVATE_KEY.Substring(PLAYSTORE_PRIVATE_KEY.Length - EXPONENT_LENGTH, EXPONENT_LENGTH);//les 4 derniers sont E                
                    sb.Append(exponent);
                    int m = EXPONENT_LENGTH & 3;
                    if (0 != m)
                    {
                        for (int i = 0; i != m; ++i) sb.Append("=");
                    }
                    exponent = sb.ToString();
                }
                // le crypto provider C# ne supporte pas les .pem, il lui faut la cle en xml ^^
                // mais vu que google la fournit en pem, et qu'il faut la mettre dans la bd, on doit convertit tout ca pour garder ca simple
                // on pourrait aussi mettre ca dans la base, et le champ est vide, on le recree puis sinon on le prend tel quel
                rsa.FromXmlString(string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>", modulus, exponent));
                try
                {
                    if (!rsa.VerifyData(dataOriginal, PLAYSTORE_PRIVATE_ALGO, signature))  //on valide ici les data avec la cle pub
                    {
                        return -1;
                    }
                }
                catch (CryptographicException)
                {
                    return -1;
                }
            }
            return 1;
        }

        #endregion

        #region Specific Calculation

        /// <summary>
        /// Get the last number of a version number (XX.XX.XX.XX)
        /// </summary>
        /// <param name="number">version number</param>
        /// <param name="numberOfFigure">segment to return</param>
        /// <returns>return a segment of the version number</returns>
        private double CalcultateChangset(string number, int numberOfFigure)
        {
            var var1 = number.Split('.');
            if (var1.Count() == numberOfFigure)
            {
                return double.Parse(var1[numberOfFigure - 1]);
            }
            return 0;
        }

        /// <summary>
        /// Get PIN from IMEI
        /// </summary>
        /// <param name="imei">IMEI</param>
        /// <returns>Code PIN</returns>
        private string GetPinCodeFromIMEI(string imei)
        {
            var hash = GetSHA1HashData(imei + SALT_SHA1);

            var temp1 = double.Parse(hash);
            var temp2 = temp1 / 378;
            var temp3 = temp2.ToString("0.###############").Substring(0, 10);

            var digit1 = (double.Parse(temp3[0].ToString()) * 192).ToString()[0];
            var digit2 = (double.Parse(temp3[2].ToString()) * 451).ToString()[0];
            var digit3 = (double.Parse(temp3[4].ToString()) * 74).ToString()[0];
            var digit4 = (double.Parse(temp3[5].ToString()) * 96).ToString()[0];

            return string.Format("{0}{1}{2}{3}", digit1, digit2, digit3, digit4);
        }

        /// <summary>
        /// Calculate the SHA-1 of a string
        /// </summary>
        /// <param name="data">data to convert</param>
        /// <returns>SHA-1</returns>
        private string GetSHA1HashData(string data)
        {
            // create new instance of md5
            SHA1 sha1 = SHA1.Create();
            // convert the input text to array of bytes
            byte[] hashData = sha1.ComputeHash(Encoding.Default.GetBytes(data));
            // create new instance of StringBuilder to save hashed data
            StringBuilder returnValue = new StringBuilder();
            // loop for each byte and add it to StringBuilder
            for (int i = 0; i < hashData.Length; i++)
            {
                returnValue.Append(hashData[i].ToString());
            }
            // return hexadecimal string
            return returnValue.ToString();
        }

        /// <summary>
        /// Reformate le DateTime récupéré des paramètre, sous forme de chaine de caractère, 
        /// d'une URL pour le retransformer en DateTime
        /// </summary>
        /// <param name="dateString">datetime récupéré des paramètre d'une url</param>
        /// <param name="date">date qui contiendra le résultat</param>
        /// <returns>vrai si l'opération a réussi</returns>
        private bool GetDateTimeFromString(string dateString, out DateTime date)
        {
            var frFR = new CultureInfo("fr-FR");
            date = DateTime.MinValue;
            var formatedDateString = dateString.Replace("_", " ").Replace("!", ":");
            return DateTime.TryParse(formatedDateString, frFR, DateTimeStyles.None, out date);
        }

        #endregion

        #region Seekios Instructions

        /// <summary>
        /// Add a new instruction in BDD and send the instruction to the seekios
        /// (send sms wake up, the seekios will wake up and check the last instruction in BDD)
        /// </summary>
        /// <param name="seekiosEntities">the database context</param>
        /// <param name="seekiosAndSeekiosProduction">the seekios object</param>
        /// <param name="instruction">the instruction</param>
        /// <param name="sendSMSWakeUp">true : need to wake up the seekios</param>
        /// <returns>return the new id instruction from the BDD</returns>
        private static int AddSeekiosInstruction(seekios_dbEntities seekiosEntities
            , seekios seekiosDb
            , seekiosProduction seekiosProductionDb
            , SeekiosInstruction instruction
            , bool sendSMSWakeUp = true)
        {
            if (seekiosDb == null || seekiosDb.idseekios == 0) return 0;
            // get the list of instructions
            var seekiosInstructionsDb = (from i in seekiosEntities.seekiosInstruction
                                         where i.seekiosProduction_idseekiosProduction == seekiosDb.idseekios
                                         select i).ToList();
            // if the instruction is refresh battery, update value
            if (instruction.TypeInstruction == InstructionType.SendBatteryLevel)
            {
                seekiosDb.isRefreshingBattery = 1;
            }
            // new instruction to add
            var seekiosInstructionToAdd = new seekiosInstruction()
            {
                dateCreation = instruction.DateInstruction,
                instructionType = (int)instruction.TypeInstruction,
                instruction = instruction.TrameInstruction,
                seekiosProduction_idseekiosProduction = seekiosDb.idseekios
            };
            var idSeekiosInstructionAdded = 0;
            // if there is already an instruction for the seekios
            if (seekiosInstructionsDb?.Count > 0)
            {
                // if there is same message(s), we remove them
                var lsOldSameInstructions = seekiosInstructionsDb.Where(el => el.instructionType == (int)instruction.TypeInstruction).ToList();
                if (lsOldSameInstructions?.Count() > 0)
                {
                    seekiosEntities.seekiosInstruction.RemoveRange(lsOldSameInstructions);
                }
                // add the instruction
                seekiosEntities.seekiosInstruction.Add(seekiosInstructionToAdd);
                seekiosEntities.SaveChanges();
                idSeekiosInstructionAdded = seekiosInstructionToAdd.idseekiosInstruction;
            }
            // if there is not any instructions yet, we simply add it
            else
            {
                seekiosEntities.seekiosInstruction.Add(seekiosInstructionToAdd);
                seekiosEntities.SaveChanges();
                idSeekiosInstructionAdded = seekiosInstructionToAdd.idseekiosInstruction;
            }
            // we send an sms wake up
            if (idSeekiosInstructionAdded > 0 && sendSMSWakeUp/*&& !isSeekiosAlreadyWaitingForInstruction*/)
            {
                SendSMSWakeUp(seekiosEntities, seekiosProductionDb.imsi);
                seekiosDb.hasGetLastInstruction = 0;
                seekiosEntities.SaveChanges();
            }
            return idSeekiosInstructionAdded;
        }

        /// <summary>
        /// Format the instruction and add the instruction for modes
        /// </summary>
        /// <param name="seekiosEntities">seekios DbContext</param>
        /// <param name="modeDb">mode</param>
        /// <param name="seekiosDb">seekios</param>
        /// <param name="sendSMSWakeUp">should send a sms wakeup</param>
        public static void PrepareInstructionForNewMode(seekios_dbEntities seekiosEntities
            , mode modeDb
            , seekios seekiosDb
            , bool sendSMSWakeUp = true)
        {
            PrepareInstructionForNewMode(seekiosEntities, modeDb, seekiosDb, (from sp in seekiosEntities.seekiosProduction
                                                                              where sp.idseekiosProduction == seekiosDb.idseekios
                                                                              select sp).First());
        }

        /// <summary>
        /// Format the instruction and add the instruction for modes. 
        /// To check how the frames should be structured, check the "Synthaxe des trames VX.X" document
        /// </summary>
        /// <param name="seekiosEntities">seekios DbContext</param>
        /// <param name="modeDb">mode</param>
        /// <param name="seekiosDb">seekios</param>
        /// <param name="sendSMSWakeUp">should send a sms wakeup</param>
        public static void PrepareInstructionForNewMode(seekios_dbEntities seekiosEntities
            , mode modeDb
            , seekios seekiosDb
            , seekiosProduction seekiosProductionDb
            , bool sendSMSWakeUp = true)
        {
            var intructionTrame = string.Empty;
            // Format the trame for a mode
            if (modeDb != null)
            {
                // Version <= 1.006
                if (seekiosProductionDb.versionEmbedded_idversionEmbedded <= 10)
                {
                    intructionTrame = string.Format("{0}{1};{2};{3}{4}{5}"
                        , string.Format(START_TRAME, modeDb.modeDefinition_idmodeDefinition)
                        , modeDb.idmode
                        , modeDb.statusDefinition_idstatusDefinition == 1 ? 1 : 0
                        , modeDb.timeRefreshTracking
                        , modeDb.trame == string.Empty ? string.Empty : ";" + modeDb.trame
                        , FOOTER_TRAME);
                }
                // Version 1.007 
                else if (seekiosProductionDb.versionEmbedded_idversionEmbedded == 11)
                {
                    intructionTrame = string.Format("{0}{1};{2};{3};{4};{5}{6}{7}"
                        , string.Format(START_TRAME, modeDb.modeDefinition_idmodeDefinition)
                        , modeDb.idmode
                        , modeDb.statusDefinition_idstatusDefinition == 1 ? 1 : 0
                        , modeDb.isPowerSavingEnabled ? 1 : 0
                        , modeDb.timeDiffHours
                        , modeDb.timeRefreshTracking
                        , modeDb.trame == string.Empty ? string.Empty : ";" + modeDb.trame
                        , FOOTER_TRAME);
                }
                // Version 1.008
                else if (seekiosProductionDb.versionEmbedded_idversionEmbedded == 12)
                {
                    // Max location
                    var numberOfLocation = (from l in seekiosEntities.location
                                            where l.mode_idmode == modeDb.idmode
                                            select l.idlocation).Count();

                    var numberMaxLocation = 0;
                    if (numberOfLocation <= 0)
                    {
                        numberMaxLocation = modeDb.maxLocation;
                    }
                    else if (modeDb.maxLocation - numberOfLocation < 0)
                    {
                        numberMaxLocation = 0;
                    }
                    else numberMaxLocation = modeDb.maxLocation - numberOfLocation;

                    // Time activation / desactivation
                    var timeActivation = string.Empty;
                    if (modeDb.timeActivation == 0) timeActivation = "A0000";
                    else if (modeDb.timeActivation.ToString().Length == 3) timeActivation = "A0" + modeDb.timeActivation;
                    else if (modeDb.timeActivation.ToString().Length == 2) timeActivation = "A00" + modeDb.timeActivation;
                    else if (modeDb.timeActivation.ToString().Length == 1) timeActivation = "A000" + modeDb.timeActivation;
                    else timeActivation = "A" + modeDb.timeActivation;

                    var timeDesactivation = string.Empty;
                    if (modeDb.timeDesactivation == 0) timeDesactivation = "D0000";
                    else if (modeDb.timeDesactivation.ToString().Length == 3) timeDesactivation = "D0" + modeDb.timeDesactivation;
                    else if (modeDb.timeDesactivation.ToString().Length == 2) timeDesactivation = "D00" + modeDb.timeDesactivation;
                    else if (modeDb.timeDesactivation.ToString().Length == 1) timeDesactivation = "D000" + modeDb.timeDesactivation;
                    else timeDesactivation = "D" + modeDb.timeDesactivation;

                    // TODO : for the timedif 4.45 convert to 4.75

                    // Prepare instruction
                    if (modeDb.modeDefinition_idmodeDefinition == (int)ModeDefinitions.ModeTracking)
                    {
                        intructionTrame = string.Format("{0}{1};{2};{3};{4};{5};{6}{7}"
                            , string.Format(START_TRAME, modeDb.modeDefinition_idmodeDefinition)
                            , modeDb.idmode
                            , modeDb.statusDefinition_idstatusDefinition == 1 ? 1 : 0
                            , modeDb.isPowerSavingEnabled ? 1 : 0
                            , modeDb.timeDiffHours
                            , modeDb.timeRefreshTracking
                            , numberMaxLocation
                            , FOOTER_TRAME);
                    }
                    else
                    {
                        intructionTrame = string.Format("{0}{1};{2};{3};{4};{5};{6};{7};{8}{9}{10}"
                            , string.Format(START_TRAME, modeDb.modeDefinition_idmodeDefinition)
                            , modeDb.idmode
                            , modeDb.statusDefinition_idstatusDefinition == 1 ? 1 : 0
                            , modeDb.isPowerSavingEnabled ? 1 : 0
                            , modeDb.timeDiffHours
                            , timeActivation
                            , timeDesactivation
                            , modeDb.timeRefreshTracking
                            , numberMaxLocation
                            , modeDb.trame == string.Empty ? string.Empty : ";" + modeDb.trame
                            , FOOTER_TRAME);
                    }
                }
            }
            // If there is no mode, setup M01
            else
            {
                // waiting : 1
                intructionTrame = string.Format("{0}{1}"
                    , string.Format(START_TRAME, 1)
                    , FOOTER_TRAME);
            }
            // Prepare the instruction
            var instruction = new SeekiosInstruction
            {
                DateInstruction = DateTime.UtcNow,
                TrameInstruction = intructionTrame,
                TypeInstruction = InstructionType.ChangeMode
            };
            // WARNING: It's just for test
            var logInstruction = new logVodafoneAndOnesignal()
            {
                dateBegin = DateTime.UtcNow,
                dateEnd = DateTime.UtcNow,
                msgRef = "Log Trame",
                majorCode = intructionTrame,
                imsi = seekiosProductionDb.imsi
            };
            seekiosEntities.logVodafoneAndOnesignal.Add(logInstruction);
            seekiosEntities.SaveChanges();
            // Send (sms wakeup) and add the instruction in BDD
            AddSeekiosInstruction(seekiosEntities
                , seekiosDb
                , seekiosProductionDb
                , instruction
                , sendSMSWakeUp);
        }

        #endregion

        #region Send SMS

        /// <summary>
        /// Send an sms to the seekios through Vodafone API 
        /// With the sms, the seekios will check the last instruction
        /// </summary>
        /// <param name="seekiosEntities">seekios BbContext</param>
        /// <param name="imsi">the seekios imsi</param>
        private static void SendSMSWakeUp(seekios_dbEntities seekiosEntities, string imsi)
        {
            // send aon envoie un SMS wake up au seekios pour lui dire de venir chercher l'instruction
            var returnedValues = new Dictionary<string, string>();
            var output = new StringBuilder();
            var CustId = "100204520";
            var Password = "Mz_9@^Y+6C0O73BT";
            var UserId = "seekios-m2m-prd";
            DateTime beginDate = DateTime.UtcNow;
            try
            {
                if (imsi.Trim().Length != 0) //TODO : check is IMSI is correct
                {
                    VodafoneM2M.gdspHeader gdspHeader = new VodafoneM2M.gdspHeader();
                    VodafoneM2M.gdspCredentials credentials = new VodafoneM2M.gdspCredentials();
                    credentials.userId = UserId;
                    credentials.customerIdSpecified = true;
                    credentials.customerId = long.Parse(CustId);
                    credentials.password = Password;
                    gdspHeader.gdspCredentials = credentials;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    if (0 == VodafoneAPI.SubmitSimWUTrigger.Execute(ref returnedValues, ref output, ref gdspHeader, imsi))
                    {
                        returnedValues["outcome"] = "ok";
                    }
                    else returnedValues["outcome"] = "E" + output.ToString();

                }
                else returnedValues["outcome"] = "Mauvais IMSI : \"" + imsi + "\"";
            }
            catch (Exception e)
            {
                returnedValues["outcome"] = e.Message;
            }
            finally
            {
                // save the information in log table
                var log = new logVodafoneAndOnesignal()
                {
                    dateBegin = beginDate,
                    apiUser = CustId + "/" + UserId,
                    imsi = imsi,
                    majorCode = returnedValues.ContainsKey("MajorCode") ? returnedValues["MajorCode"] : "",
                    minorCode = returnedValues.ContainsKey("MinorCode") ? returnedValues["MinorCode"] : "",
                    msgRef = returnedValues.ContainsKey("messageReference") ? returnedValues["messageReference"] : "",
                    outcome = returnedValues.ContainsKey("outcome") ? returnedValues["outcome"] : "",
                    dateEnd = DateTime.UtcNow
                };
                seekiosEntities.logVodafoneAndOnesignal.Add(log);
                seekiosEntities.SaveChanges();
            }
        }

        #endregion

        #region Send Alert (email)

        /// <summary>
        /// Send the alerts associate with a mode
        /// </summary>
        public static void SendAlerts(seekios_dbEntities seekiosEntities
            , int idAlertOrMode
            , user userDb
            , string seekiosName
            , Tuple<double, double> latLong
            , LocationDefinition locationDefinition
            , string preferredLanguage
            , bool isGPS = true)
        {
            // ----- user section (send email to the user)
            var idcountryResources = ResourcesHelper.GetCountryResources(preferredLanguage);
            if (locationDefinition == LocationDefinition.SOS)
            {
                SendGridHelper.SendAlertSOSEmail(seekiosEntities
                    , userDb.email
                    , userDb.firstName
                    , userDb.lastName
                    , seekiosName
                    , latLong.Item1
                    , latLong.Item2
                    , null
                    , idcountryResources
                    , isGPS);
            }
            else if (locationDefinition == LocationDefinition.Zone)
            {
                SendGridHelper.SendZoneAlertEmail(seekiosEntities
                    , userDb.email
                    , userDb.firstName
                    , userDb.lastName
                    , seekiosName
                    , ResourcesHelper.GetLocalizedString("OutOfZone", preferredLanguage)
                    , string.Empty
                    , latLong.Item1
                    , latLong.Item2
                    , idcountryResources);
            }
            else if (locationDefinition == LocationDefinition.DontMove)
            {
                SendGridHelper.SendMoveAlertEmail(seekiosEntities
                    , userDb.email
                    , userDb.firstName
                    , userDb.lastName
                    , seekiosName
                    , ResourcesHelper.GetLocalizedString("SeekiosMoved", preferredLanguage)
                    , string.Empty
                    , idcountryResources);
            }
            if (idAlertOrMode == 0) return;

            // ----- alert section (send alert to the recipients belong to a alert)
            var lsEmailSent = new List<string>();
            lsEmailSent.Add(userDb.email);
            IEnumerable<IGrouping<int, alertAndAlertRecipient>> lsAlertAndAlertReciepients = null;
            if (locationDefinition == LocationDefinition.SOS)
            {
                // alert sos is linked to the idAlert
                lsAlertAndAlertReciepients = (from aar in seekiosEntities.alertAndAlertRecipient
                                              where aar.idalert == idAlertOrMode
                                              group aar by aar.idalert into g
                                              select g).ToArray();
            }
            else
            {
                // alerts mode / don't move are linked to the idMode
                lsAlertAndAlertReciepients = (from aar in seekiosEntities.alertAndAlertRecipient
                                              where aar.mode_idmode == idAlertOrMode
                                              group aar by aar.idalert into g
                                              select g).ToArray();
            }
            IEnumerable<string> lsEmailRecipient = null;
            alertAndAlertRecipient currentAlert = null;
            foreach (var alertAndAlertRecipient in lsAlertAndAlertReciepients)
            {
                // get the alert object
                currentAlert = alertAndAlertRecipient.First();
                if (!(currentAlert.alertDefinition_idalertType == (int)AlertDefinition.EMAIL || currentAlert.alertDefinition_idalertType == (int)AlertDefinition.SOS)) continue;
                // get all the recipients email (all the new one to process)
                lsEmailRecipient = alertAndAlertRecipient.Where(x => !lsEmailSent.Contains(x.email)).Select(x => x.email).ToArray();
                lsEmailSent.AddRange(lsEmailRecipient);
                // send email
                if (locationDefinition == LocationDefinition.SOS
                    && latLong != null)
                {
                    SendGridHelper.SendAlertSOSEmail(seekiosEntities
                       , lsEmailRecipient
                       , userDb.firstName
                       , userDb.lastName
                       , seekiosName
                       , latLong.Item1
                       , latLong.Item2
                       , currentAlert.content
                       , idcountryResources
                       , isGPS);
                }
                if (locationDefinition == LocationDefinition.Zone)
                {
                    SendGridHelper.SendZoneAlertEmail(seekiosEntities
                       , lsEmailRecipient
                       , userDb.firstName
                       , userDb.lastName
                       , seekiosName
                       , currentAlert.title
                       , currentAlert.content
                       , latLong.Item1
                       , latLong.Item2
                       , idcountryResources);
                }
                else if (locationDefinition == LocationDefinition.DontMove)
                {
                    SendGridHelper.SendMoveAlertEmail(seekiosEntities
                       , lsEmailRecipient
                       , userDb.firstName
                       , userDb.lastName
                       , seekiosName
                       , currentAlert.title
                       , currentAlert.content
                       , idcountryResources);
                }
            }
        }

        #endregion

        #endregion
    }
}