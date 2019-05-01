using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCFServiceWebRole.Helper
{
    public class SignalRHelper
    {
        #region ===== Attributs ===================================================================

        private const string _SIGNALR_URL = "http://seekiossignalr.azurewebsites.net";
        private const string _HEADER_USERID_KEY = "userId";
        private const string _HEADER_USERID_VALUE = "SeekiosCloud";

        private static HubConnection _hubConnection = new HubConnection(_SIGNALR_URL);
        private static Dictionary<HubProxyEnum, IHubProxy> _hubProxiesDictionary = new Dictionary<HubProxyEnum, IHubProxy>();
        private static bool _isInitialized = false;

        #region SES
        public const string METHOD_REFRESH_CREDIT= "RefreshCredits";
        public const string METHOD_INSTRUCTION_TAKEN = "InstructionTaken";
        public const string METHOD_REFRESH_POSITION = "RefreshPosition";
        public const string METHOD_NOTIFY_SEEKIOS_OUT_OF_ZONE = "NotifySeekiosOutOfZone";
        public const string METHOD_NOTIFY_SEEKIOS_MOVED = "NotifySeekiosMoved";
        public const string METHOD_ADD_TRACKING_LOCATION = "AddTrackingLocation";
        public const string METHOD_ADD_NEW_ZONE_TRACKING_LOCATION = "AddNewZoneTrackingLocation";
        public const string METHOD_ADD_NEW_DONT_MOVE_TRACKING_LOCATION = "AddNewDontMoveTrackingLocation";
        public const string METHOD_SOS_SENT = "SOSSent";
        public const string METHOD_SOS_LOCATION_SENT = "SOSLocationSent";
        public const string METHOD_CRITICAL_BATTERY = "CriticalBattery";
        public const string METHOD_POWER_SAVING_DISABLED = "PowerSavingDisabled";
        #endregion

        #region SEEKIOS SERVICE
        public const string METHOD_INSERT_SEEKIOS = "InsertSeekios";
        public const string METHOD_UPDATE_SEEKIOS = "UpdateSeekios";
        public const string METHOD_DELETE_SEEKIOS = "DeleteSeekios";
        public const string METHOD_REFRESH_SEEKIOS_LOCATION = "RefreshSeekiosLocation";
        public const string METHOD_REFRESH_SEEKIOS_BATTERY_LEVEL = "RefreshSeekiosBatteryLevel";
        public const string METHOD_DELETE_MODE = "DeleteMode";
        public const string METHOD_INSERT_MODE_TRACKING = "InsertModeTracking";
        public const string METHOD_INSERT_MODE_ZONE = "InsertModeZone";
        public const string METHOD_INSERT_MODE_DONT_MOVE = "InsertModeDontMove";
        public const string METHOD_ALERT_HAS_BEEN_READ = "AlertSOSHasBeenRead"; 
        public const string METHOD_INSERT_ALERT_SOS_WITH_RECIPIENT = "InsertAlertSOSWithRecipient"; 
        public const string METHOD_UPDATE_USER = "UpdateUser";
        #endregion
        #endregion

        #region ===== Properties ==================================================================

        /// <summary>
        /// True if the connection is active
        /// </summary>
        public static bool IsSignalRConnected
        {
            get
            {
                return _hubConnection.State == ConnectionState.Connected;
            }
        }

        #endregion

        #region ===== Public Methods ==============================================================

        /// <summary>
        /// Close the SignalR connection
        /// </summary>
        public static bool CloseConnection()
        {
            if (!IsSignalRConnected) return true;

            try
            {
                _hubConnection.Stop();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Raised a hub on SignalR
        /// </summary>
        public static bool BroadcastUser(HubProxyEnum hubProxy, string methodName, object[] parameters)
        {
            // If we are not connected, try to reconnect
            if (!IsSignalRConnected)
            {
                if (!Connect()) return false;
            }

            // If the proxy has not been reset, return false
            if (!_hubProxiesDictionary.ContainsKey(hubProxy)) return false;

            // Call the method
            try
            {
                _hubProxiesDictionary[hubProxy].Invoke(methodName, parameters).Wait();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #endregion

        #region ===== Private Methods =============================================================

        /// <summary>
        /// Initialize the header and his hubProxies
        /// </summary>
        private static void Initialize()
        {
            // Initialize header
            _hubConnection.Headers.Add(_HEADER_USERID_KEY, _HEADER_USERID_VALUE);
            // Initialize hubProxies
            _hubProxiesDictionary.Add(HubProxyEnum.SeekiosHub, _hubConnection.CreateHubProxy("SeekiosHub"));
            _hubProxiesDictionary.Add(HubProxyEnum.CreditsHub, _hubConnection.CreateHubProxy("CreditsHub"));
            _hubProxiesDictionary.Add(HubProxyEnum.TrackingHub, _hubConnection.CreateHubProxy("TrackingHub"));
            _hubProxiesDictionary.Add(HubProxyEnum.DontMoveHub, _hubConnection.CreateHubProxy("DontMoveHub"));
            _hubProxiesDictionary.Add(HubProxyEnum.ZoneHub, _hubConnection.CreateHubProxy("ZoneHub"));
            _hubProxiesDictionary.Add(HubProxyEnum.UserHub, _hubConnection.CreateHubProxy("UserHub"));
            _isInitialized = true;
        }

        /// <summary>
        /// Connect to SignalR
        /// </summary>
        private static bool Connect()
        {
            if (IsSignalRConnected) return true;

            if (!_isInitialized) Initialize();

            try
            {
                _hubConnection.Start().Wait();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #endregion
    }

    public enum HubProxyEnum
    {
        TrackingHub,
        DontMoveHub,
        ZoneHub,
        CreditsHub,
        SeekiosHub,
        UserHub
    }
}
