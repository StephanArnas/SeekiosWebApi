using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using WCFServiceWebRole.Data.DB;
using WCFServiceWebRole.Data.DTO;

namespace WCFServiceWebRole
{
    [ServiceContract]
    public interface ISeekiosService
    {
        #region (Ex 0x0000 -> 0x0049) Login

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "Login/{email}/{password}")]
        DBToken Login(string email, string password);

        #endregion

        #region (Ex 0x0050 -> 0x0099) Device

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "RegisterDevice/{deviceModel}/{platform}/{version}/{uidDevice}/{countryCode}")]
        int RegisterDevice(string deviceModel, string platform, string version, string uidDevice, string countryCode);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "UnregisterDevice/{uidDevice}")]
        int UnregisterDevice(string uidDevice);

        #endregion

        #region (Ex 0x0100 -> 0x0199) User

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "UserEnvironment/{idapp}/{platform}/{deviceModel}/{version}/{uidDevice}/{countryCode}")]
        UserEnvironment UserEnvironment(string idapp, string platform, string deviceModel, string version, string uidDevice, string countryCode);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "User")]
        DBUser User();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "InsertUser")]
        int InsertUser(DBUser seekios);

        [OperationContract]
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "UpdateUser/{uidDevice}")]
        int UpdateUser(string uidDevice, DBUser seekios);

        //[OperationContract]
        //[WebInvoke(Method = "DELETE", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "DeleteUser")]
        //int DeleteUser();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "ValidateUser/{token}")]
        int ValidateUser(string token);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "UserExists/{email}")]
        int UserExists(string email);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "AskForNewPassword/{email}")]
        int AskForNewPassword(string email);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "SendNewPassword/{token}")]
        int SendNewPassword(string token);

        #endregion

        #region (Ex 0x0200 -> 0x0299) Seekios

        //[OperationContract]
        //[WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "IsIMEIAndPinValid/{imei}/{pin}")]
        //int IsIMEIAndPinValid(string imei, string pin);

        //WARNING : not use in plc
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "Seekios")]
        IEnumerable<DBSeekios> Seekios();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "InsertSeekios/{uidDevice}")]
        DBSeekios InsertSeekios(string uidDevice, DBSeekios seekios);

        [OperationContract]
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "UpdateSeekios/{uidDevice}")]
        int UpdateSeekios(string uidDevice, DBSeekios seekios);

        [OperationContract]
        [WebInvoke(Method = "DELETE", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "DeleteSeekios/{uidDevice}/{idseekios}")]
        int DeleteSeekios(string uidDevice, string idseekios);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "RefreshSeekiosLocation/{uidDevice}/{idseekios}")]
        int RefreshSeekiosLocation(string uidDevice, string idseekios);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "RefreshSeekiosBatteryLevel/{uidDevice}/{idseekios}")]
        int RefreshSeekiosBatteryLevel(string uidDevice, string idseekios);

        //[OperationContract]
        //[WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "EnablePowerSaving/{idSeekios}/{hourInDay}")]
        //int EnablePowerSaving(string idSeekios, string hourInDay);

        //[OperationContract]
        //[WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "DisablePowerSaving/{idSeekios}")]
        //int DisablePowerSaving(string idSeekios);

        #endregion

        #region (Ex 0x0300 -> 0x0349) Seekios (Methods for mass production)

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetSeekiosHardwareReport")]
        IEnumerable<SeekiosHardwareReportDTO> GetSeekiosHardwareReport();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetSeekiosIMEIAndPIN")]
        IEnumerable<SeekiosIMEIAndPinDTO> GetSeekiosIMEIAndPIN();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "IMEI2PIN/{imei}/{qui}")]
        string Imei2Pin(string imei, string qui);

        #endregion

        #region (Ex 0x0350 -> 0x0449) Locations

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "Locations/{idSeekios}/{lowerDate}/{upperDate}")]
        IEnumerable<DBLocation> Locations(string idSeekios, string lowerDate, string upperDate);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "LowerDateAndUpperDate/{idSeekios}")]
        LocationUpperLowerDates LowerDateAndUpperDate(string idSeekios);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "LocationsByMode/{idmode}")]
        IEnumerable<DBLocation> LocationsByMode(string idmode);

        #endregion

        #region (Ex 0x0450 -> 0x0549) Modes

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "Modes")]
        IEnumerable<DBMode> Modes();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "InsertModeTracking/{uidDevice}")]
        int InsertModeTracking(string uidDevice, DBMode mode);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "InsertModeZone/{uidDevice}", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        int InsertModeZone(string uidDevice, DBMode modeToAdd, List<BDAlertWithRecipient> alerts);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "InsertModeDontMove/{uidDevice}", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        int InsertModeDontMove(string uidDevice, DBMode modeToAdd, List<BDAlertWithRecipient> alerts);

        //[OperationContract]
        //[WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "UpdateMode")]
        //int UpdateMode(DBMode mode);

        [OperationContract]
        [WebInvoke(Method = "DELETE", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "DeleteMode/{uidDevice}/{idmode}")]
        int DeleteMode(string uidDevice, string idmode);

        //[OperationContract]
        //[WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "RestartMode/{idmode}")]
        //int RestartMode(string idmode);

        #endregion

        #region (Ex 0x0550 -> 0x0649) Alerts

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "Alerts")]
        IEnumerable<DBAlert> Alerts();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "AlertsByMode/{idmode}")]
        IEnumerable<DBAlert> AlertsByMode(string idmode);

        [OperationContract]
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "AlertSOSHasBeenRead/{uidDevice}/{idseekios}")]
        int AlertSOSHasBeenRead(string uidDevice, string idseekios);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "InsertAlertSOSWithRecipient/{uidDevice}/{idseekios}")]
        int InsertAlertSOSWithRecipient(string uidDevice, string idseekios, BDAlertWithRecipient alertWithRecipient);

        [OperationContract]
        [WebInvoke(Method = "PUT", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "UpdateAlertSOSWithRecipient/{uidDevice}/{idseekios}")]
        int UpdateAlertSOSWithRecipient(string uidDevice, string idseekios, BDAlertWithRecipient alertWithRecipient);

        #endregion

        #region (Ex 0x0800 -> 0x0899) AlertRecipient

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "AlertRecipients")]
        IEnumerable<DBAlertRecipient> AlertRecipients();

        #endregion

        #region (Ex 0x0800 -> 0x0899) PackCredit

        //[OperationContract]
        //[WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "CreditPacksByLanguage/{language}")]
        //IEnumerable<DBPackCredit> CreditPacksByLanguage(string language);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "OperationHistoric")]
        IEnumerable<DBOperation> OperationHistoric();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "OperationFromStoreHistoric")]
        IEnumerable<DBOperationFromStore> OperationFromStoreHistoric();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "InsertInAppPurchase")]
        int InsertInAppPurchase(PurchaseDTO purchase);

        #endregion

        #region (Ex 0x0900 -> 0x0949) Version Application

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "IsSeekiosVersionApplicationNeedForceUpdate/{id}/{plateforme}")]
        int IsSeekiosVersionApplicationNeedForceUpdate(string id, string plateforme);

        #endregion

        #region (Ex 0x0950 -> 0x0999) Version Embedded (use for the update software)

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "LastEmbeddedVersion")]
        DBVersionEmbedded LastEmbeddedVersion();

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "SeekiosVersion/{uidSeekios}")]
        string SeekiosVersion(string uidSeekios);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "SeekiosName/{uidSeekios}")]
        ShortSeekiosDTO SeekiosName(string uidSeekios);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "UpdateVersionEmbedded/{uidSeekios}/{versionName}")]
        int UpdateVersionEmbedded(string uidSeekios, string versionName);

        #endregion

        #region (Ex 0x1000 -> 0x1099) Notifications

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "UpdateNotificationSetting/{idSeekiosStr}/{uidDevice}/{SendNotificationOnNewTrackingLocationStr}/{SendNotificationOnNewOutOfZoneLocationStr}/{SendNotificationOnNewDontMoveLocationStr}")]
        int UpdateNotificationSetting(string idSeekiosStr
            , string uidDevice
            , string SendNotificationOnNewTrackingLocationStr
            , string SendNotificationOnNewOutOfZoneLocationStr
            , string SendNotificationOnNewDontMoveLocationStr);

        #endregion

        #region Test with Vodafone

        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "VodafonTestAPI/{imsi}/{token}")]
        int VodafonTestAPI(string imsi, string token);

        #endregion
    }
}
