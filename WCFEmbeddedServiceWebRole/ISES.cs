using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace WCFServiceWebRoleEmbedded
{
    [ServiceContract]
    public interface ISES
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "GSI/{uidSeekios}/{battery}/{signal}/{isDateNeeded}/{timestamp}")]
        List<string> GetSeekiosInstructions(string uidSeekios, string battery, string signal, string isDateNeeded, string timestamp);
        
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "RODR/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}")]
        int RespondOnDemandRequest(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "RODRBCD/{uidSeekios}/{battery}/{signal}/{cellsData}/{timestamp}")]
        Task<int> RespondOnDemandRequestByCellsData(string uidSeekios, string battery, string signal, string cellsData, string timestamp);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "NSOOZ/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}/{modeId}")]
        int NotifySeekiosOutOfZone2(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp, string modeId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "NSM/{uidSeekios}/{battery}/{signal}/{timestamp}/{modeId}")]
        int NotifySeekiosMoved2(string uidSeekios, string battery, string signal, string timestamp, string modeId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "ANTL/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}/{modeId}")]
        int AddNewTrackingLocation2(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp, string modeId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "ANZTL/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}/{modeId}")]
        int AddNewZoneTrackingLocation2(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp, string modeId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "ANDMTL/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}/{modeId}")]
        int AddNewDontMoveTrackingLocation2(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp, string modeId);

        #region Optional Parameter (Version <= 1.006)

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "NSOOZ/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}")]
        int NotifySeekiosOutOfZone(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "NSM/{uidSeekios}/{battery}/{signal}/{timestamp}")]
        int NotifySeekiosMoved(string uidSeekios, string battery, string signal, string timestamp);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "ANTL/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}")]
        int AddNewTrackingLocation(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "ANZTL/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}")]
        int AddNewZoneTrackingLocation(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "ANDMTL/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}")]
        int AddNewDontMoveTrackingLocation(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp);
        
        #endregion

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "SSOS/{uidSeekios}/{battery}/{signal}/{timestamp}")]
        int SendSOS(string uidSeekios, string battery, string signal, string timestamp);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "SSOSL/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}")]
        int SendSOSLocation(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "SSOSLBCD/{uidSeekios}/{battery}/{signal}/{cellsData}/{timestamp}")]
        Task<int> SendSOSLocationByCellsData(string uidSeekios, string battery, string signal, string cellsData, string timestamp);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "SHR/{IMEI}/{UID}/{IMSI}/{MacAddress}/{BatteryLevel}/{Timestamp}/{BoolReport}/{OSVersion}")]
        int AddNewSeekiosHardwareReport(string IMEI, string UID, string IMSI, string MacAddress, string BatteryLevel, string Timestamp, string BoolReport, string OSVersion);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "USV/{UID}/{battery}/{signal}/{version}/{timestamp}")]
        int UpdateSeekiosVersion(string UID, string battery, string signal, string version, string timestamp);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "CBA/{uidSeekios}/{battery}/{signal}/{timestamp}")]
        int CriticalBatteryAlert(string uidSeekios, string battery, string signal, string timestamp);
        
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "PSD/{uidSeekios}/{battery}/{signal}/{timestamp}/{modeId}")]
        int PowerSavingDisabled(string uidSeekios, string battery, string signal, string timestamp, string modeId);

        //[OperationContract]
        //[WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "NSBIZ/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}")]
        //int NotifySeekiosBackInZone(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp);

        //[OperationContract]
        //[WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "ANFMTL/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}")]
        //int AddNewFollowMeTrackingLocation(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp);

        //[OperationContract]
        //[WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "Up")]
        //int PostSeekiosMessage(string uidSeekios, string message);

        //[OperationContract]
        //[WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "ADTL/{uidSeekios}/{battery}/{signal}/{latitude}/{longitude}/{altitude}/{accuracy}/{timestamp}")]
        //int AddDailyTrackLocation(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp);

        //[OperationContract]
        //[WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "ADTLBCD/{uidSeekios}/{battery}/{signal}/{cellsData}/{timestamp}")]
        //Task<int> AddDailyTrackLocationByCellsData(string uidSeekios, string battery, string signal, string cellsData, string timestamp);
    }
}
