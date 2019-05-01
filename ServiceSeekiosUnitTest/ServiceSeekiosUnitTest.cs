using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceSeekiosUnitTest.Helper;
using Newtonsoft.Json;
using WCFServiceWebRole.Data.DB;
using WCFServiceWebRole.Data.ERROR;
using System.Threading.Tasks;
using WCFServiceWebRole.Data.DTO;
using System.Linq;
using Z.EntityFramework.Plus;
using System.Collections.Generic;
using WCFServiceWebRole.Enum;

namespace ServiceSeekiosUnitTest
{
    [TestClass]
    public class ServiceSeekiosUnitTest
    {
        #region ----- PRIVATE VARIABLES -----------------------------------------------------------------------

        private const string BASE_URL_PROD = "http://seekios.cloudapp.net/SeekiosService.svc/";
        private const string BASE_URL_STAGING = "http://28e4f500d35c4a1fb9c1a50b7b47349b.cloudapp.net/SeekiosService.svc/";
        private const string BASE_URL_LOCAL = "";
        private static string BASE_URL = BASE_URL_PROD;

        private static string USER_ENVIRONMENT_URL = BASE_URL + "UserEnvironment";
        private static string USER_URL = BASE_URL + "User";
        private static string INSERT_USER_URL = BASE_URL + "InsertUser";
        private static string UPDATE_USER_URL = BASE_URL + "UpdateUser";
        private static string VALIDATE_USER_URL = BASE_URL + "ValidateUser";
        private static string USER_EXISTS_URL = BASE_URL + "UserExists";
        private static string ASK_FOR_NEW_PASSWORD_URL = BASE_URL + "AskForNewPassword";
        private static string SEND_NEW_PASSWORD_URL = BASE_URL + "SendNewPassword";

        private static string REGISTER_DEVICE_URL = BASE_URL + "RegisterDevice";
        private static string UNREGISTER_DEVICE_URL = BASE_URL + "UnregisterDevice";

        private static string SEEKIOS_URL = BASE_URL + "Seekios";
        private static string INSERT_SEEKIOS_URL = BASE_URL + "InsertSeekios";
        private static string UPDATE_SEEKIOS_URL = BASE_URL + "UpdateSeekios";
        private static string DELETE_SEEKIOS_URL = BASE_URL + "DeleteSeekios";
        private static string REFRESH_SEEKIOS_LOCATION_URL = BASE_URL + "RefreshSeekiosLocation";
        private static string REFRESH_SEEKIOS_BATTERY_LEVEL_URL = BASE_URL + "RefreshSeekiosBatteryLevel";

        private static string LOCATIONS_URL = BASE_URL + "Locations";
        private static string LOWER_DATE_AND_UPPER_DATE_URL = BASE_URL + "LowerDateAndUpperDate";

        private static string MODES_URL = BASE_URL + "Modes";
        private static string INSERT_MODE_TRACKING_URL = BASE_URL + "InsertModeTracking";
        private static string INSERT_MODE_ZONE_URL = BASE_URL + "InsertModeZone";
        private static string INSERT_MODE_DONT_MOVE_URL = BASE_URL + "InsertModeDontMove";
        private static string UPDATE_MODE_URL = BASE_URL + "UpdateMode";
        private static string DELETE_MODE_URL = BASE_URL + "DeleteMode";
        private static string RESTART_MODE_URL = BASE_URL + "RestartMode";

        private static string ALERTS_URL = BASE_URL + "Alerts";
        private static string ALERTS_BY_MODE_URL = BASE_URL + "AlertsByMode";
        private static string ALERTS_SOS_HAS_BEEN_READ_URL = BASE_URL + "AlertSOSHasBeenRead";
        private static string INSERT_ALERT_SOS_WITH_RECIPIENT_URL = BASE_URL + "InsertAlertSOSWithRecipient";
        private static string UPDATE_ALERT_SOS_WITH_RECIPIENT_URL = BASE_URL + "UpdateAlertSOSWithRecipient";
        private static string ALERT_RECIPIENTS_URL = BASE_URL + "AlertRecipients";

        private static string CREDIT_PACKS_BY_LANGUAGE_URL = BASE_URL + "CreditPacksByLanguage";
        private static string OPERATION_HISTORIC_URL = BASE_URL + "OperationHistoric";
        private static string OPERATION_FROM_STORE_HISTORIC_URL = BASE_URL + "OperationFromStoreHistoric";
        private static string INSERT_IN_APP_PURCHASE_URL = BASE_URL + "InsertInAppPurchase";

        private static string LAST_EMBEDDED_VERSION_URL = BASE_URL + "LastEmbeddedVersion";
        private static string SEEKIOS_VERSION_URL = BASE_URL + "SeekiosVersion";
        private static string SEEKIOS_NAME_URL = BASE_URL + "SeekiosName";
        private static string UPDATE_VERSION_EMBEDDED_URL = BASE_URL + "UpdateVersionEmbedded";

        private static string ACTIVATE_NOTIFICATION_URL = BASE_URL + "ActivateNotification";
        private static string DESACTIVATE_NOTIFICATION_URL = BASE_URL + "DesactivateNotification";

        private readonly string _DATE_STRING_FORMAT = "dd-MM-yyyy_HH!mm!ss";

        private DBUser _user = null;
        private DBSeekios _seekios = null;
        private DBMode _mode = null;
        private List<BDAlertWithRecipient> _alertRecipients = null;
        private const string SEEKIOS_IMEI = "123456789012345";
        private const string SEEKIOS_PIN_CODE = "7150";
        private const int ID_SEEKIOS = 1;
        private const int ID_SEEKIOS_WRONG = 76;

        #endregion

        #region ----- PUBLIC VARIABLES ------------------------------------------------------------------------

        public static string LOGIN_URL = BASE_URL + "Login";
        public static string EMAIL = "dev@thingsoftomorrow.com";
        public static string PASSWORD = "0c33bccf745e6f2616f8ca4964ff3f5c";

        public JsonSerializerSettings JsonSettings
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

        public ServiceSeekiosUnitTest()
        {
            _user = new DBUser()
            {
                FirstName = "UserTestFirstName",
                LastName = "UserTestLastName",
                IdCountryResource = 1,
                Email = EMAIL,
                Password = PASSWORD
            };
            _seekios = new DBSeekios()
            {
                SeekiosName = "SeekiosNameTest",
                Imei = SEEKIOS_IMEI,
                PinCode = SEEKIOS_PIN_CODE
            };
            _mode = new DBMode()
            {
                Seekios_idseekios = ID_SEEKIOS,
                Trame = "1",
            };
            _alertRecipients = new List<BDAlertWithRecipient>();
            _alertRecipients.Add(new BDAlertWithRecipient()
            {
                Title = "Alert Unit Test Title",
                Content = "Alert Unit Test Content",
                AlertDefinition_idalertType = (int)AlertDefinition.EMAIL,
                LsRecipients = new List<DBAlertRecipient>()
            });
            _alertRecipients.First().LsRecipients.Add(new DBAlertRecipient()
            {
                Email = "dev@thingsoftomorrow.com",
                EmailType = "Unit Test",
                NameRecipient = "dev"
            });
        }

        #endregion

        #region ----- PUBLIC METHODS --------------------------------------------------------------------------

        #region ----- LOGIN ----------------

        [TestMethod]
        public void Login()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                try
                {
                    await CheckOrInsertUser();
                    var result = await GetToken();
                    Assert.IsTrue(result);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        #endregion

        #region ----- NOTIFICATION ---------

        [TestMethod]
        public void RegisterDeviceForNotification()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                try
                {
                    await CheckOrInsertUser();
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}"
                        , REGISTER_DEVICE_URL
                        , "deviceUnitTest"
                        , "iOS"
                        , "1.0"
                        , Guid.NewGuid().ToString()
                        , "1"));
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
        public void UnregisterDeviceForNotification()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                try
                {
                    var uidDevice = await PrepareDataForUnregisterDevice();
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                    , UNREGISTER_DEVICE_URL
                    , uidDevice));
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

        #region ----- USER -----------------

        [TestMethod]
        public void UserEnvironment()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    await CheckOrInsertUser();
                    await GetToken();

                    using (var seekiosEntities = new seekios_dbEntities())
                    {
                        var versionApplicationAndroid = (from va in seekiosEntities.versionApplication
                                                         where va.isNeedUpdate == 1 && va.plateforme == (int)PlatformEnum.Android
                                                         orderby va.version_dateCreation descending
                                                         select va).Take(2).ToArray();
                        if (versionApplicationAndroid.Count() <= 0) Assert.IsTrue(false);
                        var firstVersionApplicationAndroid = versionApplicationAndroid[0];
                        var secondVersionApplicationAndroid = (versionApplicationAndroid.Count() >= 2) ? versionApplicationAndroid[1] : null;

                        var versionApplicationIOS = (from va in seekiosEntities.versionApplication
                                                     where va.isNeedUpdate == 1 && va.plateforme == (int)PlatformEnum.iOS
                                                     orderby va.version_dateCreation descending
                                                     select va).Take(2).ToArray();
                        if (versionApplicationIOS.Count() <= 0) Assert.IsTrue(false);
                        var firstVersionApplicationIOS = versionApplicationIOS[0];
                        var secondVersionApplicationIOS = (versionApplicationIOS.Count() >= 2) ? versionApplicationIOS[1] : null;

                        // ----- ERROR ID APP NOT A INT
                        result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}"
                            , USER_ENVIRONMENT_URL
                            , "azerty"
                            , "azerty"));
                        resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                        if (resultError?.ErrorCode != "0x0100") Assert.IsTrue(false);
                        // ----- ERROR ANDROID NEEDS UPDATE
                        result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}"
                            , USER_ENVIRONMENT_URL
                            , secondVersionApplicationAndroid.versionNumber
                            , secondVersionApplicationAndroid.plateforme));
                        resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                        if (resultError?.ErrorCode != "0x0102") Assert.IsTrue(false);
                        // ----- ERROR IOS NEEDS UPDATE
                        result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}"
                            , USER_ENVIRONMENT_URL
                            , secondVersionApplicationIOS.versionNumber
                            , secondVersionApplicationIOS.plateforme));
                        resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                        if (resultError?.ErrorCode != "0x0103") Assert.IsTrue(false);
                        // ------ SUCCESS
                        result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}"
                            , USER_ENVIRONMENT_URL
                            , firstVersionApplicationAndroid.versionNumber
                            , firstVersionApplicationAndroid.plateforme));
                        var userEnv = JsonConvert.DeserializeObject<UserEnvironment>(result);
                        if (userEnv != null) Assert.IsTrue(true);
                        else Assert.IsTrue(false);
                    }
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void User()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                try
                {
                    await CheckOrInsertUser();
                    await GetToken();
                    result = await HttpRequestHelper.GetRequestAsync(USER_URL);
                    var user = JsonConvert.DeserializeObject<DBUser>(result);
                    if (user != null) Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void InsertUser()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    RemoveUser();
                    // ----- ERROR EMPTY EMAIL
                    _user.Email = "";
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0120") Assert.IsTrue(false);
                    // ----- ERROR INVALID EMAIL
                    _user.Email = "dev@thingsoftomorrow.c";
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0121") Assert.IsTrue(false);
                    _user.Email = EMAIL;
                    // ----- ERROR EMPTY PASSWORD
                    _user.Password = "";
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0122") Assert.IsTrue(false);
                    _user.Password = PASSWORD;
                    // ----- ERROR EMPTY FIRST NAME
                    _user.FirstName = "";
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0124") Assert.IsTrue(false);
                    _user.FirstName = "UserTestFirstName";
                    // ----- ERROR EMPTY LAST NAME
                    _user.LastName = "";
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0125") Assert.IsTrue(false);
                    _user.LastName = "UserTestLastName";
                    // ----- ERROR COUNTRY CODE
                    _user.IdCountryResource = 0;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0126") Assert.IsTrue(false);
                    _user.IdCountryResource = 1;
                    // ----- SUCCESS
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    if (result.Contains("0x0127") || result == "1") Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void UpdateUser()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    await CheckOrInsertUser();
                    await GetToken();
                    // ----- ERROR EMPTY EMAIL
                    _user.Email = "";
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_USER_URL, JsonConvert.SerializeObject(_user));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0130") Assert.IsTrue(false);
                    // ----- ERROR INVALID EMAIL
                    _user.Email = "dev@thingsoftomorrow.c";
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_USER_URL, JsonConvert.SerializeObject(_user));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0131") Assert.IsTrue(false);
                    // ----- ERROR EMAIL DOES NOT MATCH WITH TOKEN EMAIL
                    _user.Email = "dev123@thingsoftomorrow.com";
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_USER_URL, JsonConvert.SerializeObject(_user));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0132") Assert.IsTrue(false);
                    _user.Email = EMAIL;
                    // ----- ERROR EMPTY PASSWORD
                    _user.Password = "";
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_USER_URL, JsonConvert.SerializeObject(_user));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0133") Assert.IsTrue(false);
                    _user.Password = PASSWORD;
                    // ----- ERROR EMPTY FIRST NAME
                    _user.FirstName = "";
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_USER_URL, JsonConvert.SerializeObject(_user));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0135") Assert.IsTrue(false);
                    _user.FirstName = "UserTestFirstName";
                    // ----- ERROR EMPTY LAST NAME
                    _user.LastName = "";
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_USER_URL, JsonConvert.SerializeObject(_user));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0136") Assert.IsTrue(false);
                    _user.LastName = "UserTestLastName2";
                    // ----- ERROR COUNTRY CODE
                    _user.IdCountryResource = 0;
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_USER_URL, JsonConvert.SerializeObject(_user));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0137") Assert.IsTrue(false);
                    _user.IdCountryResource = 1;
                    // ----- SUCCESS
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_USER_URL, JsonConvert.SerializeObject(_user));
                    if (result != "1") Assert.IsTrue(false);
                    else Assert.IsTrue(true);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void ValidateUser()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                var result = string.Empty;
                var validationToken = await PrepareDataForValidateUser();
                // ----- ERROR INVALID TOKEN
                result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                    , VALIDATE_USER_URL
                    , validationToken.Substring(0, validationToken.Length - 2))
                    , false);
                if (result != "-2") Assert.IsTrue(false);
                // ----- SUCCESS
                result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                    , VALIDATE_USER_URL
                    , validationToken)
                    , false);
                if (result != "1") Assert.IsTrue(false);
                // ----- SUCCESS
                result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                    , VALIDATE_USER_URL
                    , validationToken)
                    , false);
                if (result != "0") Assert.IsTrue(false);
                else Assert.IsTrue(true);
            });
        }

        [TestMethod]
        public void UserExists()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                await CheckOrInsertUser();
                var result = string.Empty;
                // ----- ERROR INVALID EMAIL
                result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                    , USER_EXISTS_URL
                    , "dev@thingsoftomorrow.c")
                    , false);
                if (result != "-1") Assert.IsTrue(false);
                // ----- ERROR NO USER FOUND
                result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                    , USER_EXISTS_URL
                    , "dev123456789@thingsoftomorrow.com")
                    , false);
                if (result != "0") Assert.IsTrue(false);
                // ----- SUCCESS
                result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                    , USER_EXISTS_URL
                    , "dev@thingsoftomorrow.com")
                    , false);
                if (result != "1") Assert.IsTrue(false);
                else Assert.IsTrue(true);
            });
        }

        [TestMethod]
        public void AskForNewPassword()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                await CheckOrInsertUser();

                var result = string.Empty;
                // ----- ERROR INVALID EMAIL
                result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                    , ASK_FOR_NEW_PASSWORD_URL
                    , "dev@thingsoftomorrow.c")
                    , false);
                if (result != "-2") Assert.IsTrue(false);
                // ----- ERROR NO USER FOUND
                result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                    , ASK_FOR_NEW_PASSWORD_URL
                    , "dev123456789@thingsoftomorrow.com")
                    , false);
                if (result != "-1") Assert.IsTrue(false);
                // ----- SUCCESS
                result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                    , ASK_FOR_NEW_PASSWORD_URL
                    , "dev@thingsoftomorrow.com"));
                if (result != "1") Assert.IsTrue(false);
                else Assert.IsTrue(true);
            });
        }

        [TestMethod]
        public void SendNewPassword()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                var tokenPassword = await PrepareDataForSendNewPassword();
                var result = string.Empty;

                // ----- ERROR INVALID EMAIL
                result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                    , SEND_NEW_PASSWORD_URL
                    , tokenPassword.Substring(0, tokenPassword.Length - 2))
                    , false);
                if (result != "-1") Assert.IsTrue(false);
                // ----- SUCCESS
                result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                    , SEND_NEW_PASSWORD_URL
                    , tokenPassword)
                    , false);
                if (result != "1") Assert.IsTrue(false);
                else Assert.IsTrue(true);
                // ----- SET THE PASSWORD TO THE OLD VALUE
                using (var seekiosEntities = new seekios_dbEntities())
                {
                    var userDb = seekiosEntities.user
                        .Where(x => x.email == EMAIL)
                        .Update(x => new user() { password = "0c33bccf745e6f2616f8ca4964ff3f5c" });
                }
            });
        }

        #endregion

        #region ----- SEEKIOS --------------

        [TestMethod]
        public void Seekios()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                try
                {
                    await CheckOrInsertSeekios();
                    result = await HttpRequestHelper.GetRequestAsync(SEEKIOS_URL);
                    var seekiosDb = JsonConvert.DeserializeObject<List<DBSeekios>>(result);
                    if (seekiosDb != null) Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void InsertSeekios()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    await PrepareDataForInsertSeekios();
                    // ----- ERROR EMPTY NAME
                    _seekios.SeekiosName = "";
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0220") Assert.IsTrue(false);
                    _seekios.SeekiosName = "SeekiosNameTest";
                    // ----- ERROR EMPTY IMEI
                    _seekios.Imei = "";
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0221") Assert.IsTrue(false);
                    // ----- ERROR IMEI 14 CARACT
                    _seekios.Imei = "1234567891234";
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0222") Assert.IsTrue(false);
                    _seekios.Imei = SEEKIOS_IMEI;
                    // ----- ERROR EMPTY PIN CODE
                    _seekios.PinCode = "";
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0223") Assert.IsTrue(false);
                    // ----- ERROR PIN CODE 3 CARACT
                    _seekios.PinCode = "123";
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0224") Assert.IsTrue(false);
                    _seekios.PinCode = SEEKIOS_PIN_CODE;
                    // ----- ERROR INVALID IMEI (NOT EXISTS)
                    _seekios.Imei = "000000000000000";
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0225") Assert.IsTrue(false);
                    _seekios.Imei = SEEKIOS_IMEI;
                    // ----- ERROR INVALID IMEI (NOT EXISTS)
                    _seekios.PinCode = "1234";
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0227") Assert.IsTrue(false);
                    // ----- SUCCESS
                    _seekios.PinCode = SEEKIOS_PIN_CODE;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings));
                    _seekios = JsonConvert.DeserializeObject<DBSeekios>(result);
                    if (_seekios?.Idseekios <= 0) Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS ALREADY IN BDD
                    _seekios.PinCode = SEEKIOS_PIN_CODE;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0226") Assert.IsTrue(false);
                    else Assert.IsTrue(true);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void UpdateSeekios()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    await CheckOrInsertSeekios();
                    // ----- ERROR EMPTY NAME
                    _seekios.Idseekios = 0;
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0230") Assert.IsTrue(false);
                    _seekios.Idseekios = ID_SEEKIOS;
                    // ----- ERROR EMPTY NAME
                    _seekios.SeekiosName = "";
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0231") Assert.IsTrue(false);
                    _seekios.SeekiosName = "SeekiosNameTest2";
                    // ----- ERROR EMPTY IMEI
                    _seekios.Idseekios = int.MaxValue;
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0232") Assert.IsTrue(false);
                    // ----- SUCCESS
                    _seekios.Idseekios = ID_SEEKIOS;
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings));
                    if (result != "1") Assert.IsTrue(false);
                    else Assert.IsTrue(true);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void DeleteSeekios()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    await CheckOrInsertSeekios();
                    // ----- ERROR INVLID ID SEEKIOS
                    result = await HttpRequestHelper.DeleteRequestAsync(string.Format("{0}/{1}"
                        , DELETE_SEEKIOS_URL
                        , "azeert"));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0240") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS NOT FOUND
                    result = await HttpRequestHelper.DeleteRequestAsync(string.Format("{0}/{1}"
                        , DELETE_SEEKIOS_URL
                        , int.MaxValue));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0241") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS DOES NOT BELONG TO THE USER
                    result = await HttpRequestHelper.DeleteRequestAsync(string.Format("{0}/{1}"
                        , DELETE_SEEKIOS_URL
                        , ID_SEEKIOS_WRONG));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0242") Assert.IsTrue(false);
                    // ----- SUCCESS
                    _seekios.Idseekios = ID_SEEKIOS;
                    result = await HttpRequestHelper.DeleteRequestAsync(string.Format("{0}/{1}"
                        , DELETE_SEEKIOS_URL
                        , ID_SEEKIOS));
                    if (result != "1") Assert.IsTrue(false);
                    else Assert.IsTrue(true);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void RefreshSeekiosLocation()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    await CheckOrInsertSeekios();
                    // ----- ERROR INVLID ID SEEKIOS
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                        , REFRESH_SEEKIOS_LOCATION_URL
                        , "azeert"));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0250") Assert.IsTrue(false);
                    using (var seekiosEntities = new seekios_dbEntities())
                    {
                        // put 0 free credits
                        seekiosEntities.seekiosProduction
                            .Where(x => x.idseekiosProduction == ID_SEEKIOS)
                            .Update(x => new seekiosProduction() { freeCredit = 0 });

                        // ----- ERROR NO CREDITS
                        result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                        , REFRESH_SEEKIOS_LOCATION_URL
                        , ID_SEEKIOS));
                        resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                        if (resultError?.ErrorCode != "0x0251") Assert.IsTrue(false);

                        // put 6000 free credits
                        seekiosEntities.seekiosProduction
                            .Where(x => x.idseekiosProduction == ID_SEEKIOS)
                            .Update(x => new seekiosProduction() { freeCredit = 6000 });
                    }
                    // ----- ERROR SEEKIOS NOT FOUND
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                    , REFRESH_SEEKIOS_LOCATION_URL
                    , int.MaxValue));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0252") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS DOES NOT BELONG TO THE USER
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                        , REFRESH_SEEKIOS_LOCATION_URL
                        , ID_SEEKIOS_WRONG));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0253") Assert.IsTrue(false);
                    // ----- SUCCESS
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                        , REFRESH_SEEKIOS_LOCATION_URL
                        , ID_SEEKIOS));
                    if (result != "1") Assert.IsTrue(false);
                    else Assert.IsTrue(true);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void RefreshSeekiosBatteryLevel()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    await CheckOrInsertSeekios();
                    // ----- ERROR INVLID ID SEEKIOS
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                        , REFRESH_SEEKIOS_BATTERY_LEVEL_URL
                        , "azeert"));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0260") Assert.IsTrue(false);
                    using (var seekiosEntities = new seekios_dbEntities())
                    {
                        // put 0 free credits
                        seekiosEntities.seekiosProduction
                            .Where(x => x.idseekiosProduction == ID_SEEKIOS)
                            .Update(x => new seekiosProduction() { freeCredit = 0 });

                        // ----- ERROR NO CREDITS
                        result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                        , REFRESH_SEEKIOS_BATTERY_LEVEL_URL
                        , ID_SEEKIOS));
                        resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                        if (resultError?.ErrorCode != "0x0261") Assert.IsTrue(false);

                        // put 6000 free credits
                        seekiosEntities.seekiosProduction
                            .Where(x => x.idseekiosProduction == ID_SEEKIOS)
                            .Update(x => new seekiosProduction() { freeCredit = 6000 });
                    }
                    // ----- ERROR SEEKIOS NOT FOUND
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                    , REFRESH_SEEKIOS_BATTERY_LEVEL_URL
                    , int.MaxValue));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0262") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS DOES NOT BELONG TO THE USER
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                        , REFRESH_SEEKIOS_BATTERY_LEVEL_URL
                        , ID_SEEKIOS_WRONG));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0263") Assert.IsTrue(false);
                    // ----- SUCCESS
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}"
                        , REFRESH_SEEKIOS_BATTERY_LEVEL_URL
                        , ID_SEEKIOS));
                    if (result != "1") Assert.IsTrue(false);
                    else Assert.IsTrue(true);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        #endregion

        #region ----- MODES ----------------

        [TestMethod]
        public void Modes()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                try
                {
                    await CheckOrInsertMode();
                    result = await HttpRequestHelper.GetRequestAsync(MODES_URL);
                    var modeDb = JsonConvert.DeserializeObject<List<DBMode>>(result);
                    if (modeDb != null) Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void InsertModeTracking()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    var idDevice = await PrepareDataForInsertMode();
                    // ----- ERROR MODE IS NULL
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_TRACKING_URL, JsonConvert.SerializeObject(null), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0460") Assert.IsTrue(false);
                    // ----- ERROR ID DEVICE INVALID
                    _mode.Device_iddevice = 0;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_TRACKING_URL, JsonConvert.SerializeObject(_mode, JsonSettings), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0461") Assert.IsTrue(false);
                    _mode.Device_iddevice = idDevice;
                    // ----- ERROR ID SEEKIOS INVALID
                    _mode.Seekios_idseekios = 0;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_TRACKING_URL, JsonConvert.SerializeObject(_mode, JsonSettings), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0462") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS NOT FOUND
                    _mode.Seekios_idseekios = int.MaxValue;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_TRACKING_URL, JsonConvert.SerializeObject(_mode, JsonSettings), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0463") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS DOES NOT BELONG TO THE USER
                    _mode.Seekios_idseekios = ID_SEEKIOS_WRONG;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_TRACKING_URL, JsonConvert.SerializeObject(_mode, JsonSettings), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0464") Assert.IsTrue(false);
                    _mode.Seekios_idseekios = ID_SEEKIOS;
                    // ----- ERROR ID DEVICE NOT FOUND
                    _mode.Device_iddevice = int.MaxValue;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_TRACKING_URL, JsonConvert.SerializeObject(_mode, JsonSettings), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0466") Assert.IsTrue(false);
                    _mode.Device_iddevice = idDevice;
                    using (var seekiosEntities = new seekios_dbEntities())
                    {
                        // put 0 free credits
                        seekiosEntities.seekiosProduction
                            .Where(x => x.idseekiosProduction == ID_SEEKIOS)
                            .Update(x => new seekiosProduction() { freeCredit = 0 });
                        // ----- ERROR NO CREDITS
                        result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_TRACKING_URL, JsonConvert.SerializeObject(_mode, JsonSettings), true);
                        resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                        if (resultError?.ErrorCode != "0x0467") Assert.IsTrue(false);
                        // put 6000 free credits
                        seekiosEntities.seekiosProduction
                            .Where(x => x.idseekiosProduction == ID_SEEKIOS)
                            .Update(x => new seekiosProduction() { freeCredit = 6000 });
                    }
                    // ----- SUCCESS
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_TRACKING_URL
                        , JsonConvert.SerializeObject(_mode, JsonSettings)
                        , true);
                    int idMode = 0;
                    if (int.TryParse(result, out idMode) && idMode > 0) Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void InsertModeZone()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    var idDevice = await PrepareDataForInsertMode();
                    // ----- ERROR MODE IS NULL
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_ZONE_URL, "{ \"modeToAdd\":null,\"alerts\":null}", true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0470") Assert.IsTrue(false);
                    // ----- ERROR ID DEVICE INVALID
                    _mode.Device_iddevice = 0;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_ZONE_URL, SerializeForModes(), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0471") Assert.IsTrue(false);
                    _mode.Device_iddevice = idDevice;
                    // ----- ERROR ID SEEKIOS INVALID
                    _mode.Seekios_idseekios = 0;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_ZONE_URL, SerializeForModes(), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0472") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS NOT FOUND
                    _mode.Seekios_idseekios = int.MaxValue;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_ZONE_URL, SerializeForModes(), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0473") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS DOES NOT BELONG TO THE USER
                    _mode.Seekios_idseekios = ID_SEEKIOS_WRONG;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_ZONE_URL, SerializeForModes(), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0474") Assert.IsTrue(false);
                    _mode.Seekios_idseekios = ID_SEEKIOS;
                    // ----- ERROR ID DEVICE NOT FOUND
                    _mode.Device_iddevice = int.MaxValue;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_ZONE_URL, SerializeForModes(), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0476") Assert.IsTrue(false);
                    _mode.Device_iddevice = idDevice;
                    using (var seekiosEntities = new seekios_dbEntities())
                    {
                        // put 0 free credits
                        seekiosEntities.seekiosProduction
                            .Where(x => x.idseekiosProduction == ID_SEEKIOS)
                            .Update(x => new seekiosProduction() { freeCredit = 0 });
                        // ----- ERROR NO CREDITS
                        result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_ZONE_URL, SerializeForModes(), true);
                        resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                        if (resultError?.ErrorCode != "0x0477") Assert.IsTrue(false);
                        // put 6000 free credits
                        seekiosEntities.seekiosProduction
                            .Where(x => x.idseekiosProduction == ID_SEEKIOS)
                            .Update(x => new seekiosProduction() { freeCredit = 6000 });
                    }
                    // ----- SUCCESS
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_ZONE_URL, SerializeForModes(), true);
                    int idMode = 0;
                    if (int.TryParse(result, out idMode) && idMode > 0) Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void InsertModeDontMove()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    var idDevice = await PrepareDataForInsertMode();
                    // ----- ERROR MODE IS NULL
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_DONT_MOVE_URL, "{ \"modeToAdd\":null,\"alerts\":null}", true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0480") Assert.IsTrue(false);
                    // ----- ERROR ID DEVICE INVALID
                    _mode.Device_iddevice = 0;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_DONT_MOVE_URL, SerializeForModes(), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0481") Assert.IsTrue(false);
                    _mode.Device_iddevice = idDevice;
                    // ----- ERROR ID SEEKIOS INVALID
                    _mode.Seekios_idseekios = 0;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_DONT_MOVE_URL, SerializeForModes(), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0482") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS NOT FOUND
                    _mode.Seekios_idseekios = int.MaxValue;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_DONT_MOVE_URL, SerializeForModes(), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0483") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS DOES NOT BELONG TO THE USER
                    _mode.Seekios_idseekios = ID_SEEKIOS_WRONG;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_DONT_MOVE_URL, SerializeForModes(), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0484") Assert.IsTrue(false);
                    _mode.Seekios_idseekios = ID_SEEKIOS;
                    // ----- ERROR ID DEVICE NOT FOUND
                    _mode.Device_iddevice = int.MaxValue;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_DONT_MOVE_URL, SerializeForModes(), true);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0486") Assert.IsTrue(false);
                    _mode.Device_iddevice = idDevice;
                    using (var seekiosEntities = new seekios_dbEntities())
                    {
                        // put 0 free credits
                        seekiosEntities.seekiosProduction
                            .Where(x => x.idseekiosProduction == ID_SEEKIOS)
                            .Update(x => new seekiosProduction() { freeCredit = 0 });
                        seekiosEntities.user
                            .Where(x => x.iduser == _user.Iduser)
                            .Update(x => new user() { remainingRequest = 0 });
                        // ----- ERROR NO CREDITS
                        result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_DONT_MOVE_URL, SerializeForModes(), true);
                        resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                        if (resultError?.ErrorCode != "0x0487") Assert.IsTrue(false);
                        // put 6000 free credits
                        seekiosEntities.seekiosProduction
                            .Where(x => x.idseekiosProduction == ID_SEEKIOS)
                            .Update(x => new seekiosProduction() { freeCredit = 6000 });
                    }
                    // ----- SUCCESS
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_MODE_DONT_MOVE_URL, SerializeForModes(), true);
                    int idMode = 0;
                    if (int.TryParse(result, out idMode) && idMode > 0) Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void DeleteMode()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    var idMode = await PrepareDataForDeleteMode();
                    // ----- ERROR INVLID ID MODE
                    result = await HttpRequestHelper.DeleteRequestAsync(string.Format("{0}/{1}", DELETE_MODE_URL, "azeert"));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0500") Assert.IsTrue(false);
                    // ----- ERROR MODE NOT FOUND
                    result = await HttpRequestHelper.DeleteRequestAsync(string.Format("{0}/{1}", DELETE_MODE_URL, int.MaxValue));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0501") Assert.IsTrue(false);
                    // ----- SUCCESS
                    result = await HttpRequestHelper.DeleteRequestAsync(string.Format("{0}/{1}", DELETE_MODE_URL, idMode));
                    if (result != "1") Assert.IsTrue(false);
                    else Assert.IsTrue(true);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void RestartMode()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    var idMode = await PrepareDataForDeleteMode();
                    // ----- ERROR INVLID ID MODE
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", RESTART_MODE_URL, "azeert"));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0510") Assert.IsTrue(false);
                    // ----- ERROR MODE NOT FOUND
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", RESTART_MODE_URL, int.MaxValue));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0511") Assert.IsTrue(false);
                    // ----- SUCCESS
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", RESTART_MODE_URL, idMode));
                    if (result != "1") Assert.IsTrue(false);
                    else Assert.IsTrue(true);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        #endregion

        #region ----- LOCATIONS ------------

        [TestMethod]
        public void Locations()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    await PrepareDataForLocations();
                    var lowerDate = DateTime.UtcNow.AddDays(-30);
                    var upperDate = DateTime.UtcNow;
                    // ----- ERROR INVALID ID SEEKIOS
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}", LOCATIONS_URL, "azeert", "azerty", "azerty"));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0350") Assert.IsTrue(false);
                    // ----- ERROR FORMAT LOWER DATE
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}", LOCATIONS_URL, ID_SEEKIOS, "azerty", "azerty"));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0351") Assert.IsTrue(false);
                    // ----- ERROR FORMAT UPPER DATE
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}", LOCATIONS_URL, ID_SEEKIOS, lowerDate.ToString(_DATE_STRING_FORMAT), "azerty"));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0352") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS DOES NOT BELONG TO YOU
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}", LOCATIONS_URL, int.MaxValue, lowerDate.ToString(_DATE_STRING_FORMAT), upperDate.ToString(_DATE_STRING_FORMAT)));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0353") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS DOES NOT EXIST
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}", LOCATIONS_URL, ID_SEEKIOS_WRONG, lowerDate.ToString(_DATE_STRING_FORMAT), upperDate.ToString(_DATE_STRING_FORMAT)));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0354") Assert.IsTrue(false);
                    // ----- SUCCESS
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}", LOCATIONS_URL, ID_SEEKIOS, lowerDate.ToString(_DATE_STRING_FORMAT), upperDate.ToString(_DATE_STRING_FORMAT)));
                    var locationUpperLowerDates = JsonConvert.DeserializeObject<IEnumerable<DBLocation>>(result);
                    if (locationUpperLowerDates == null) Assert.IsTrue(false);
                    else Assert.IsTrue(true);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void LowerDateAndUpperDate()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    await PrepareDataForLocations();
                    // ----- ERROR INVALID ID SEEKIOS
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", LOWER_DATE_AND_UPPER_DATE_URL, "azeert"));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0360") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS DOES NOT BELONG TO YOU
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", LOWER_DATE_AND_UPPER_DATE_URL, int.MaxValue));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0361") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS DOES NOT EXIST
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", LOWER_DATE_AND_UPPER_DATE_URL, ID_SEEKIOS_WRONG));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0362") Assert.IsTrue(false);
                    // ----- SUCCESS
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", LOWER_DATE_AND_UPPER_DATE_URL, ID_SEEKIOS));
                    var locationUpperLowerDates = JsonConvert.DeserializeObject<LocationUpperLowerDates>(result);
                    if (locationUpperLowerDates == null && locationUpperLowerDates.LowerDate != null && locationUpperLowerDates.UppderDate != null) Assert.IsTrue(false);
                    else Assert.IsTrue(true);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        #endregion

        #region ----- ALERTS ---------------

        [TestMethod]
        public void Alerts()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                try
                {
                    await CheckOrInsertAlert();
                    result = await HttpRequestHelper.GetRequestAsync(ALERTS_URL);
                    var alertsDb = JsonConvert.DeserializeObject<List<DBAlert>>(result);
                    if (alertsDb != null) Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void AlertsByMode()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    var idMode = await CheckOrInsertAlert();
                    // ----- ERROR INVALID ID MODE
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", ALERTS_BY_MODE_URL, "azerty"));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0660") Assert.IsTrue(false);
                    // ----- ERROR MODE DOES NOT EXIST
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", ALERTS_BY_MODE_URL, int.MaxValue));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0661") Assert.IsTrue(false);
                    // ----- SUCCESS
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", ALERTS_BY_MODE_URL, idMode));
                    var alertsDb = JsonConvert.DeserializeObject<List<DBAlert>>(result);
                    if (alertsDb != null) Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void AlertSOSHasBeenRead()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    var idSeekios = await CheckOrInsertAlertSOS();
                    // ----- ERROR INVALID ID MODE
                    result = await HttpRequestHelper.PutRequestAsync(string.Format("{0}/{1}", ALERTS_SOS_HAS_BEEN_READ_URL, "azerty"), string.Empty);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0670") Assert.IsTrue(false);
                    // ----- ERROR MODE DOES NOT EXIST
                    result = await HttpRequestHelper.PutRequestAsync(string.Format("{0}/{1}", ALERTS_SOS_HAS_BEEN_READ_URL, int.MaxValue), string.Empty);
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0671") Assert.IsTrue(false);
                    // ----- SUCCESS
                    result = await HttpRequestHelper.PutRequestAsync(string.Format("{0}/{1}", ALERTS_SOS_HAS_BEEN_READ_URL, idSeekios), string.Empty);
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
        public void InsertAlertSOSWithRecipient()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    var idAlert = await PrepareDataForInsertAlertSOSWithRecipients();
                    // ----- ERROR INVALID ID SEEKIOS
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_ALERT_SOS_WITH_RECIPIENT_URL, SerializeForAlertWithRecipients(-1));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0680") Assert.IsTrue(false);
                    // ----- ERROR TITLE IS EMPTY
                    _alertRecipients[0].Title = string.Empty;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_ALERT_SOS_WITH_RECIPIENT_URL, SerializeForAlertWithRecipients());
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0681") Assert.IsTrue(false);
                    _alertRecipients[0].Title = "Alert Unit Test Title";
                    // ----- ERROR CONTENT IS EMPTY
                    _alertRecipients[0].Content = string.Empty;
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_ALERT_SOS_WITH_RECIPIENT_URL, SerializeForAlertWithRecipients());
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0682") Assert.IsTrue(false);
                    _alertRecipients[0].Content = "Alert Unit Test Content";
                    // ----- ERROR LSRECIPIENT IS EMPTY
                    _alertRecipients[0].LsRecipients.Clear();
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_ALERT_SOS_WITH_RECIPIENT_URL, SerializeForAlertWithRecipients());
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0683") Assert.IsTrue(false);
                    _alertRecipients[0].LsRecipients.Add(new DBAlertRecipient()
                    {
                        Email = "dev@thingsoftomorrow.com",
                        EmailType = "Unit Test",
                        NameRecipient = "dev"
                    });
                    // ----- ERROR SEEKIOS NOT FOUND
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_ALERT_SOS_WITH_RECIPIENT_URL, SerializeForAlertWithRecipients(int.MaxValue));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0684") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS DOES NOT BELONG TO YOU
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_ALERT_SOS_WITH_RECIPIENT_URL, SerializeForAlertWithRecipients(ID_SEEKIOS_WRONG));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0685") Assert.IsTrue(false);
                    // ----- SUCCESS
                    result = await HttpRequestHelper.PostRequestAsync(INSERT_ALERT_SOS_WITH_RECIPIENT_URL, SerializeForAlertWithRecipients());
                    int idAlertSOS = 0;
                    int.TryParse(result, out idAlertSOS);
                    if (idAlertSOS > 0) Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void UpdateAlertSOSWithRecipient()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    var idAlert = await CheckOrInsertAlertSOS();
                    // ----- ERROR INVALID ID SEEKIOS
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_ALERT_SOS_WITH_RECIPIENT_URL, SerializeForAlertWithRecipients(-1));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0690") Assert.IsTrue(false);
                    // ----- ERROR TITLE IS EMPTY
                    _alertRecipients[0].Title = string.Empty;
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_ALERT_SOS_WITH_RECIPIENT_URL, SerializeForAlertWithRecipients());
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0691") Assert.IsTrue(false);
                    _alertRecipients[0].Title = "Alert Unit Test Title";
                    // ----- ERROR CONTENT IS EMPTY
                    _alertRecipients[0].Content = string.Empty;
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_ALERT_SOS_WITH_RECIPIENT_URL, SerializeForAlertWithRecipients());
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0692") Assert.IsTrue(false);
                    _alertRecipients[0].Content = "Alert Unit Test Content";
                    // ----- ERROR LSRECIPIENT IS EMPTY
                    _alertRecipients[0].LsRecipients.Clear();
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_ALERT_SOS_WITH_RECIPIENT_URL, SerializeForAlertWithRecipients());
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0693") Assert.IsTrue(false);
                    _alertRecipients[0].LsRecipients.Add(new DBAlertRecipient()
                    {
                        Email = "dev@thingsoftomorrow.com",
                        EmailType = "Unit Test",
                        NameRecipient = "dev"
                    });
                    // ----- ERROR SEEKIOS NOT FOUND
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_ALERT_SOS_WITH_RECIPIENT_URL, SerializeForAlertWithRecipients(int.MaxValue));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0694") Assert.IsTrue(false);
                    // ----- ERROR SEEKIOS DOES NOT BELONG TO YOU
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_ALERT_SOS_WITH_RECIPIENT_URL, SerializeForAlertWithRecipients(ID_SEEKIOS_WRONG));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0695") Assert.IsTrue(false);
                    // ----- SUCCESS
                    result = await HttpRequestHelper.PutRequestAsync(UPDATE_ALERT_SOS_WITH_RECIPIENT_URL, SerializeForAlertWithRecipients());
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
        public void AlertRecipients()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                try
                {
                    await CheckOrInsertAlertSOS();
                    result = await HttpRequestHelper.GetRequestAsync(ALERT_RECIPIENTS_URL);
                    var alertSOSDb = JsonConvert.DeserializeObject<IEnumerable<DBAlertRecipient>>(result);
                    if (alertSOSDb?.Count() > 0) Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        #endregion

        #region ----- PACKS CREDIT ---------

        [TestMethod]
        public void CreditPacksByLanguage()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    // ----- ERROR INVALID LANGUAGE
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", CREDIT_PACKS_BY_LANGUAGE_URL, "azerty"));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError?.ErrorCode != "0x0800") Assert.IsTrue(false);
                    // ----- SUCCESS
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", CREDIT_PACKS_BY_LANGUAGE_URL, "fr"));
                    var creditPacksDb = JsonConvert.DeserializeObject<IEnumerable<DBAlertRecipient>>(result);
                    if (creditPacksDb?.Count() > 0) Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void OperationHistoric()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                try
                {
                    var idoperation = await CheckOrInsertOperation();
                    // ----- SUCCESS
                    result = await HttpRequestHelper.GetRequestAsync(OPERATION_HISTORIC_URL);
                    using (var seekiosEntities = new seekios_dbEntities())
                    {
                        seekiosEntities.operation
                            .Where(x => x.idoperation == idoperation)
                            .Delete(x => x.BatchSize = 1000);
                    }
                    var operationDb = JsonConvert.DeserializeObject<IEnumerable<DBOperation>>(result);
                    if (operationDb?.Count() > 0) Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void OperationFromStoreHistoric()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                try
                {
                    var idoperationFromStore = await CheckOrInsertOperationFromStore();
                    // ----- SUCCESS
                    result = await HttpRequestHelper.GetRequestAsync(OPERATION_FROM_STORE_HISTORIC_URL);
                    using (var seekiosEntities = new seekios_dbEntities())
                    {
                        seekiosEntities.operationFromStore
                            .Where(x => x.idoperationFromStore == idoperationFromStore)
                            .Delete(x => x.BatchSize = 1000);
                    }
                    var operationDb = JsonConvert.DeserializeObject<IEnumerable<DBOperation>>(result);
                    if (operationDb?.Count() > 0) Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        public void InsertInAppPurchase()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                try
                {
                    var idoperationFromStore = await CheckOrInsertOperationFromStore();
                    // ----- SUCCESS
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        #endregion

        #region ----- EMBEDDED -------------

        [TestMethod]
        public void LastEmbeddedVersion()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                try
                {
                    var idversionEmbedded = CheckOrInsertVersionEmbedded();
                    // ----- SUCCESS
                    result = await HttpRequestHelper.GetRequestAsync(LAST_EMBEDDED_VERSION_URL);
                    var versionEmbeddedDb = JsonConvert.DeserializeObject<DBVersionEmbedded>(result);
                    if (versionEmbeddedDb != null) Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void SeekiosVersion()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                try
                {
                    var idversionEmbedded = CheckOrInsertVersionEmbedded();
                    // ----- SUCCESS
                    using (var seekiosEntities = new seekios_dbEntities())
                    {
                        var seekiosProductionDb = seekiosEntities.seekiosProduction.First(x => x.idseekiosProduction == ID_SEEKIOS);
                        result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", SEEKIOS_VERSION_URL, seekiosProductionDb.uidSeekios));
                        if (!string.IsNullOrEmpty(result)) Assert.IsTrue(true);
                        else Assert.IsTrue(false);
                    }
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void SeekiosName()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                try
                {
                    var idversionEmbedded = CheckOrInsertVersionEmbedded();
                    // ----- SUCCESS
                    using (var seekiosEntities = new seekios_dbEntities())
                    {
                        var seekiosProductionDb = seekiosEntities.seekiosProduction.First(x => x.idseekiosProduction == ID_SEEKIOS);
                        result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", SEEKIOS_NAME_URL, seekiosProductionDb.uidSeekios));
                        var seekiosVersionDb = JsonConvert.DeserializeObject<ShortSeekiosDTO>(result);
                        if (seekiosVersionDb != null) Assert.IsTrue(true);
                        else Assert.IsTrue(false);
                    }
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void UpdateVersionEmbedded()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                try
                {
                    var idversionEmbedded = CheckOrInsertVersionEmbedded();
                    using (var seekiosEntities = new seekios_dbEntities())
                    {
                        var seekiosProductionDb = seekiosEntities.seekiosProduction.First(x => x.idseekiosProduction == ID_SEEKIOS);
                        var versionEmbedded = seekiosEntities.versionEmbedded.OrderByDescending(x => x.idVersionEmbedded).First();
                        // ----- ERROR NO VERSION EMBEDDED FOUND
                        result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}", UPDATE_VERSION_EMBEDDED_URL, seekiosProductionDb.uidSeekios, "azerty"));
                        if (result != "-1") Assert.IsTrue(false);
                        // ----- ERROR NO SEEKIOS FOUND
                        result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}", UPDATE_VERSION_EMBEDDED_URL, "azerty", versionEmbedded.versionName));
                        if (result != "-2") Assert.IsTrue(false);
                        // ----- SUCCESS
                        result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}", UPDATE_VERSION_EMBEDDED_URL, seekiosProductionDb.uidSeekios, versionEmbedded.versionName));
                        if (result == "1") Assert.IsTrue(true);
                        else Assert.IsTrue(false);
                    }
                }
                catch (Exception)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        #region ----- NOTIFICATION ---------

        [TestMethod]
        public void ActivateNotification()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    var uidDevice = await CheckOrInsertDevice();
                    // ----- ERROR ID DEVICE DOES NOT EXIST
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", ACTIVATE_NOTIFICATION_URL, "azerty"));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError.ErrorCode != "0x1000") Assert.IsTrue(false);
                    // ----- SUCCESS
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", ACTIVATE_NOTIFICATION_URL, uidDevice));
                    if (result == "1") Assert.IsTrue(true);
                    else Assert.IsTrue(false);
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(false);
                }
            });
        }

        [TestMethod]
        public void DesactivateNotification()
        {
            GeneralThreadAffineContextHelper.Run(async () =>
            {
                string result = string.Empty;
                DefaultCustomError resultError = null;
                try
                {
                    var uidDevice = await CheckOrInsertDevice();
                    // ----- ERROR ID DEVICE DOES NOT EXIST
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", DESACTIVATE_NOTIFICATION_URL, "azerty"));
                    resultError = JsonConvert.DeserializeObject<DefaultCustomError>(result);
                    if (resultError.ErrorCode != "0x1010") Assert.IsTrue(false);
                    // ----- SUCCESS
                    result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}", DESACTIVATE_NOTIFICATION_URL, uidDevice));
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

        #endregion

        #endregion

        #region ----- PREPARE DATA ----------------------------------------------------------------------------

        private async Task<string> PrepareDataForValidateUser()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                }
                userDb.isValidate = false;
                seekiosEntities.SaveChanges();
                return userDb.validationToken;
            }
        }

        private async Task<string> PrepareDataForSendNewPassword()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                }
                string resetPasswordToken = string.Empty;
                if (string.IsNullOrEmpty(userDb.resetPasswordToken))
                {
                    resetPasswordToken = Guid.NewGuid().ToString();
                    userDb.resetPasswordToken = resetPasswordToken;
                    seekiosEntities.SaveChanges();
                }
                else resetPasswordToken = userDb.resetPasswordToken;
                return resetPasswordToken;
            }
        }

        private async Task<bool> PrepareDataForInsertSeekios()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                }

                await GetToken();

                var seekiosDb = seekiosEntities.seekios.FirstOrDefault(x => x.idseekios == ID_SEEKIOS);
                if (seekiosDb != null)
                {
                    var result = new System.Data.Entity.Core.Objects.ObjectParameter("Result", 0);
                    seekiosEntities.DeleteSeekiosById(seekiosDb.idseekios, result);
                    int integerResult = 0;
                    int.TryParse(result.Value.ToString(), out integerResult);
                    if (integerResult != 1) Assert.IsTrue(false);
                }
                return true;
            }
        }

        private async Task<string> PrepareDataForUnregisterDevice()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                }

                var deviceDb = seekiosEntities.device.FirstOrDefault(x => x.user_iduser == userDb.iduser);
                if (deviceDb == null)
                {
                    await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}"
                        , REGISTER_DEVICE_URL
                        , "deviceUnitTest"
                        , "iOS"
                        , "1.0"
                        , Guid.NewGuid().ToString()
                        , "1"));
                    deviceDb = seekiosEntities.device.FirstOrDefault(x => x.user_iduser == userDb.iduser);
                }
                return deviceDb.uidDevice;
            }
        }

        private async Task<int> PrepareDataForInsertMode()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                }
                else _user.Iduser = userDb.iduser;

                await GetToken();

                var seekiosDb = seekiosEntities.seekios.FirstOrDefault(x => x.idseekios == ID_SEEKIOS);
                if (seekiosDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings), true);
                }

                var deviceDb = seekiosEntities.device.FirstOrDefault(x => x.user_iduser == userDb.iduser);
                if (deviceDb == null)
                {
                    await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}"
                        , REGISTER_DEVICE_URL
                        , "deviceUnitTest"
                        , "iOS"
                        , "1.0"
                        , Guid.NewGuid().ToString()
                        , "1"));
                }
                _mode.Device_iddevice = deviceDb.iddevice;

                return deviceDb.iddevice;
            }
        }

        private async Task<int> PrepareDataForDeleteMode()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                }

                await GetToken();

                var seekiosDb = seekiosEntities.seekios.FirstOrDefault(x => x.idseekios == ID_SEEKIOS);
                if (seekiosDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings), true);
                }

                var deviceDb = seekiosEntities.device.FirstOrDefault(x => x.user_iduser == userDb.iduser);
                if (deviceDb == null)
                {
                    await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}"
                        , REGISTER_DEVICE_URL
                        , "deviceUnitTest"
                        , "iOS"
                        , "1.0"
                        , Guid.NewGuid().ToString()
                        , "1"));
                }
                _mode.Device_iddevice = deviceDb.iddevice;

                var modeDb = seekiosEntities.mode.FirstOrDefault(x => x.seekios_idseekios == ID_SEEKIOS);
                if (modeDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_MODE_TRACKING_URL, JsonConvert.SerializeObject(_mode, JsonSettings), true);
                    modeDb = seekiosEntities.mode.FirstOrDefault(x => x.seekios_idseekios == ID_SEEKIOS);
                }

                return modeDb.idmode;
            }
        }

        private async Task<int> PrepareDataForLocations()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                }

                await GetToken();

                var seekiosDb = seekiosEntities.seekios.FirstOrDefault(x => x.idseekios == ID_SEEKIOS);
                if (seekiosDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings), true);
                }

                var deviceDb = seekiosEntities.device.FirstOrDefault(x => x.user_iduser == userDb.iduser);
                if (deviceDb == null)
                {
                    await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}"
                        , REGISTER_DEVICE_URL
                        , "deviceUnitTest"
                        , "iOS"
                        , "1.0"
                        , Guid.NewGuid().ToString()
                        , "1"));
                }
                _mode.Device_iddevice = deviceDb.iddevice;

                var modeDb = seekiosEntities.mode.FirstOrDefault(x => x.seekios_idseekios == ID_SEEKIOS);
                if (modeDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_MODE_TRACKING_URL, JsonConvert.SerializeObject(_mode, JsonSettings), true);
                    modeDb = seekiosEntities.mode.FirstOrDefault(x => x.seekios_idseekios == ID_SEEKIOS);
                }

                var locationsDb = seekiosEntities.location.FirstOrDefault(x => x.seekios_idseekios == ID_SEEKIOS);
                if (locationsDb == null)
                {
                    locationsDb = new location()
                    {
                        accuracy = 0,
                        altitude = 0,
                        dateLocationCreation = DateTime.UtcNow,
                        latitude = 40.741895,
                        longitude = -73.989308,
                        mode_idmode = modeDb.idmode,
                        seekios_idseekios = ID_SEEKIOS,
                        //locationDefinition_idlocationDefinition = (int)
                    };
                    seekiosEntities.location.Add(locationsDb);
                    seekiosEntities.SaveChanges();
                }

                return locationsDb.idlocation;
            }
        }

        private async Task<bool> PrepareDataForInsertAlertSOSWithRecipients()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                }

                await GetToken();

                var seekiosDb = seekiosEntities.seekios.FirstOrDefault(x => x.idseekios == ID_SEEKIOS);
                if (seekiosDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings), true);
                }

                if (seekiosDb.alertSOS_idalert.HasValue)
                {
                    seekiosEntities.alert
                        .Where(x => x.idalert == seekiosDb.alertSOS_idalert)
                        .Delete(x => x.BatchSize = 1000);
                }

                return true;
            }
        }

        #endregion

        #region ----- PRIVATE METHODS -------------------------------------------------------------------------

        private async Task<bool> GetToken()
        {
            if (HttpRequestHelper.Token == null)
            {
                var result = await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}", LOGIN_URL, EMAIL, PASSWORD), false);
                HttpRequestHelper.Token = JsonConvert.DeserializeObject<DBToken>(result);
                return true;
            }
            return false;
        }

        private void RemoveUser()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb != null)
                {
                    seekiosEntities.DeleteUserByIDuser(userDb.iduser.ToString());
                }
            }
        }

        private async Task<bool> CheckOrInsertUser()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                }
                if (userDb.password != PASSWORD)
                {
                    seekiosEntities.user.Where(x => x.email == EMAIL).Update(x => new user { password = PASSWORD });
                }
                return true;
            }
        }

        private async Task<string> CheckOrInsertDevice()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                }

                var deviceDb = seekiosEntities.device.FirstOrDefault(x => x.user_iduser == userDb.iduser);
                if (deviceDb == null)
                {
                    await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}"
                        , REGISTER_DEVICE_URL
                        , "deviceUnitTest"
                        , "iOS"
                        , "1.0"
                        , Guid.NewGuid().ToString()
                        , "1"));
                    deviceDb = seekiosEntities.device.FirstOrDefault(x => x.user_iduser == userDb.iduser);
                }

                return deviceDb.uidDevice;
            }
        }

        private async Task<bool> CheckOrInsertSeekios()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                }

                await GetToken();

                var seekiosDb = seekiosEntities.seekios.FirstOrDefault(x => x.idseekios == ID_SEEKIOS);
                if (seekiosDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings), true);
                }
                return true;
            }
        }

        private async Task<bool> CheckOrInsertMode()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                }

                await GetToken();

                var seekiosDb = seekiosEntities.seekios.FirstOrDefault(x => x.idseekios == ID_SEEKIOS);
                if (seekiosDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings), true);
                }

                var modeDb = seekiosEntities.mode.FirstOrDefault(x => x.seekios_idseekios == ID_SEEKIOS);
                if (modeDb == null)
                {
                    var deviceDb = seekiosEntities.device.FirstOrDefault(x => x.user_iduser == userDb.iduser);
                    if (deviceDb == null)
                    {
                        await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}"
                            , REGISTER_DEVICE_URL
                            , "deviceUnitTest"
                            , "iOS"
                            , "1.0"
                            , Guid.NewGuid().ToString()
                            , "1"));
                    }
                    _mode.Device_iddevice = deviceDb.iddevice;
                    await HttpRequestHelper.PostRequestAsync(INSERT_MODE_TRACKING_URL, JsonConvert.SerializeObject(_mode, JsonSettings), true);
                }
                return true;
            }
        }

        private async Task<string> CheckOrInsertAlert()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                }

                await GetToken();

                var seekiosDb = seekiosEntities.seekios.FirstOrDefault(x => x.idseekios == ID_SEEKIOS);
                if (seekiosDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings), true);
                }

                var modeDb = seekiosEntities.mode.FirstOrDefault(x => x.seekios_idseekios == ID_SEEKIOS);
                if (modeDb == null
                    || modeDb.modeDefinition_idmodeDefinition != (int)ModeDefinitions.ModeZone
                    || modeDb.modeDefinition_idmodeDefinition != (int)ModeDefinitions.ModeDontMove)
                {
                    var deviceDb = seekiosEntities.device.FirstOrDefault(x => x.user_iduser == userDb.iduser);
                    if (deviceDb == null)
                    {
                        await HttpRequestHelper.GetRequestAsync(string.Format("{0}/{1}/{2}/{3}/{4}/{5}"
                            , REGISTER_DEVICE_URL
                            , "deviceUnitTest"
                            , "iOS"
                            , "1.0"
                            , Guid.NewGuid().ToString()
                            , "1"));
                    }
                    _mode.Device_iddevice = deviceDb.iddevice;
                    return await HttpRequestHelper.PostRequestAsync(INSERT_MODE_ZONE_URL, SerializeForModes(), true);
                }
                return modeDb.idmode.ToString();
            }
        }

        private async Task<string> CheckOrInsertAlertSOS()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                }

                await GetToken();

                var seekiosDb = seekiosEntities.seekios.FirstOrDefault(x => x.idseekios == ID_SEEKIOS);
                if (seekiosDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_SEEKIOS_URL, JsonConvert.SerializeObject(_seekios, JsonSettings), true);
                }

                if (!seekiosDb.alertSOS_idalert.HasValue)
                {
                    //var alertDb = seekiosEntities.alert.FirstOrDefault(x => x. == modeDb.idmode && x.alertDefinition_idalertType == (int)AlertDefinition.SOS);
                    //if (alertDb == null)
                    //{
                    //    alertDb = new alert()
                    //    {
                    //        alertDefinition_idalertType = (int)AlertDefinition.SOS,
                    //        title = "alert sos unit test title",
                    //        content = "alert sos unit test content",
                    //        dateAlertCreation = DateTime.UtcNow,
                    //        mode_idmode = modeDb.idmode
                    //    };
                    //    seekiosEntities.alert.Add(alertDb);
                    //    seekiosEntities.SaveChanges();
                    //}
                    //else if (seekiosDb.isLastSOSRead == 1)
                    //{
                    //    seekiosDb.isLastSOSRead = 0;
                    //    seekiosEntities.SaveChanges();
                    //}
                }
                return seekiosDb.idseekios.ToString();
            }
        }

        private async Task<int> CheckOrInsertOperation()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                }

                await GetToken();

                var operationDb = seekiosEntities.operation.FirstOrDefault(x => x.user_iduser == userDb.iduser);
                if (operationDb == null)
                {
                    operationDb = new operation()
                    {
                        amount = 1,
                        dateBeginOperation = DateTime.UtcNow,
                        dateEndOperation = DateTime.UtcNow,
                        isOnSeekios = true,
                        operationType_idoperationType = (int)OperationType.RefreshPosition,
                        seekios_idseekios = ID_SEEKIOS,
                        user_iduser = userDb.iduser
                    };
                    seekiosEntities.operation.Add(operationDb);
                    seekiosEntities.SaveChanges();
                }

                return operationDb.idoperation;
            }
        }

        private async Task<int> CheckOrInsertOperationFromStore()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                if (userDb == null)
                {
                    await HttpRequestHelper.PostRequestAsync(INSERT_USER_URL, JsonConvert.SerializeObject(_user), false);
                    userDb = seekiosEntities.user.FirstOrDefault(x => x.email == EMAIL);
                }

                await GetToken();

                var operationFromStoreDb = seekiosEntities.operationFromStore.FirstOrDefault(x => x.idUser == userDb.iduser);
                if (operationFromStoreDb == null)
                {
                    operationFromStoreDb = new operationFromStore()
                    {
                        creditsPurchased = 30,
                        dateTransaction = DateTime.UtcNow,
                        idPack = 1,
                        idUser = userDb.iduser,
                        isPackPremium = false,
                        refStore = "UNIT_TEST",
                        status = "UNIT_TEST",
                        versionApp = "UNIT_TEST"
                    };
                    seekiosEntities.operationFromStore.Add(operationFromStoreDb);
                    seekiosEntities.SaveChanges();
                }

                return operationFromStoreDb.idoperationFromStore;
            }
        }

        private int CheckOrInsertVersionEmbedded()
        {
            using (var seekiosEntities = new seekios_dbEntities())
            {
                var versionEmbeddedDb = seekiosEntities.versionEmbedded.FirstOrDefault();
                if (versionEmbeddedDb == null)
                {
                    versionEmbeddedDb = new versionEmbedded()
                    {
                        dateVersionCreation = DateTime.UtcNow,
                        releaseNotes = "UNIT TEST",
                        SHA1Hash = "UNIT TEST",
                        versionName = "UNIT TEST"
                    };
                    seekiosEntities.versionEmbedded.Add(versionEmbeddedDb);
                    seekiosEntities.SaveChanges();
                }

                return versionEmbeddedDb.idVersionEmbedded;
            }
        }

        private string SerializeForModes()
        {
            var jsonMode = JsonConvert.SerializeObject(_mode, JsonSettings);
            var jsonAlertWithRecipients = JsonConvert.SerializeObject(_alertRecipients, JsonSettings);
            return "{ \"modeToAdd\":" + jsonMode + ",\"alerts\":" + jsonAlertWithRecipients + "}";
        }

        private string SerializeForAlertWithRecipients(int idseekios = ID_SEEKIOS)
        {
            var jsonAlertWithRecipients = JsonConvert.SerializeObject(_alertRecipients[0], JsonSettings);
            if (idseekios == -1)
                return "{ \"idseekios\":\"azerty\",\"alertWithRecipient\":" + jsonAlertWithRecipients + "}";
            else
                return "{ \"idseekios\":" + idseekios + ",\"alertWithRecipient\":" + jsonAlertWithRecipients + "}";
        }

        #endregion
    }
}
