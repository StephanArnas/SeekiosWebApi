using System.Text;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using WCFServiceWebRole.Enum;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;

namespace WCFServiceWebRole.Helper
{
    public class NotificationHelper
    {
        #region ----- VARIABLES -------------------------------------------------------------------------------

        private static bool _isRefreshingCredits = false;

        private const string ONESIGNAL_URL = "https://onesignal.com/api/v1/notifications";
        private const string ONESIGNAL_PRIVATE_KEY_ANDROID = "ee8851b0-f171-4de0-b86b-74ef18eefa02";
        private const string ONESIGNAL_PRIVATE_KEY_IOS = "4dbbcd4b-8108-4711-a923-92ac93cb48b4";
        private const string ONESIGNAL_PRIVATE_KEY_WEB = "766b16a6-9730-4570-8222-fae9dcbd21a9";

        #endregion

        #region ----- PUBLIC METHODS --------------------------------------------------------------------------

        public static void SendNotifications(seekios_dbEntities seekiosEntities
            , int idUser
            , string seekiosName
            , object parameters
            , string messageSent
            , string language
            , bool isNotificationWithBadge = false)
        {
            // get the platform list that the user id targets
            bool containAndroid = false;
            bool containiOS = false;
            bool containWeb = false;
            SeekiosService.GetPlatformsByUser(seekiosEntities, idUser, out containAndroid, out containiOS, out containWeb);

            string tag = GetTagToAdd(idUser.ToString());

            if (containAndroid) SendNotificationWithDataToAndroid(seekiosEntities, tag, seekiosName, messageSent, parameters,  language);
            if (containiOS) SendNotificationWithDataToiOS(seekiosEntities, tag, seekiosName, messageSent, parameters, isNotificationWithBadge);
            if (containWeb) SendNotificationWithDataToWeb(seekiosEntities, tag, seekiosName, messageSent, parameters);
        }

        public static bool SendNotificationWithDataToAndroid(seekios_dbEntities seekiosEntities
            , string tag
            , string seekiosName
            , string methodName
            , object parameters
            , string language)
        {
            var request = CreateHttpRequest(PlatformEnum.Android);
            var obj = new
            {
                app_id = ONESIGNAL_PRIVATE_KEY_ANDROID,
                contents = new { en = methodName },
                headings = new { en = seekiosName },
                filters = new object[] { new { field = "tag", key = tag, relation = "exists" } },//filters,
                data = parameters,
                android_group = _isRefreshingCredits ? "2" : "1",
                android_group_message = new { en = ResourcesHelper.GetLocalizedString("NotifCount", language) },
            };
            var param = new JavaScriptSerializer().Serialize(obj);
            var byteArray = Encoding.UTF8.GetBytes(param);
            var responseContent = string.Empty;
            var exception = string.Empty;
            var returnCode = true;
            try
            {
                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                exception = ex.Message;
                returnCode = !returnCode;
            }
            LogOneSignalRequest(seekiosEntities, tag, seekiosName, methodName, exception, responseContent, returnCode);
            return returnCode;
        }

        public static bool SendNotificationWithDataToiOS(seekios_dbEntities seekiosEntities
            , string tag
            , string seekiosName
            , string methodName
            , object parameters
            , bool isNotificationWithBadge = true)
        {
            var request = CreateHttpRequest(PlatformEnum.iOS);
            var serializer = new JavaScriptSerializer();
            var obj = new object();
            if (isNotificationWithBadge)
            {
                obj = new
                {
                    app_id = ONESIGNAL_PRIVATE_KEY_IOS,
                    contents = new { en = methodName },
                    headings = new { en = seekiosName },
                    filters = new object[] { new { field = "tag", key = tag, relation = "exists" } },//filters,
                    data = parameters,
                    collapse_id = new Guid().ToString(),
                    ios_badgeType = "Increase",
                    ios_badgeCount = 1,
                    //ios_sound = "nil",
                    content_available = 1
                };
            }
            else
            {
                obj = new
                {
                    app_id = ONESIGNAL_PRIVATE_KEY_IOS,
                    contents = new { en = methodName },
                    headings = new { en = seekiosName },
                    filters = new object[] { new { field = "tag", key = tag, relation = "exists" } },
                    data = parameters,
                    collapse_id = new Guid().ToString(),
                    //ios_sound = "nil",
                    content_available = 1
                };
            }
            var param = serializer.Serialize(obj);
            var byteArray = Encoding.UTF8.GetBytes(param);
            var responseContent = string.Empty;
            var exception = string.Empty;
            var returnCode = true;
            try
            {
                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                exception = ex.Message + " | " + new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                returnCode = !returnCode;
            }
            LogOneSignalRequest(seekiosEntities, tag, seekiosName, methodName, exception, responseContent, returnCode);
            return returnCode;
        }

        public static bool SendNotificationWithDataToWeb(seekios_dbEntities seekiosEntities
            , string tag
            , string seekiosName
            , string methodName
            , object parameters)
        {
            var request = CreateHttpRequest(PlatformEnum.Web);
            var serializer = new JavaScriptSerializer();
            var obj = new
            {
                app_id = ONESIGNAL_PRIVATE_KEY_WEB,
                contents = new { en = methodName },
                headings = new { en = seekiosName },
                filters = new object[] { new { field = "tag", key = tag, relation = "exists" } },
                data = parameters,
            };
            var param = serializer.Serialize(obj);
            var byteArray = Encoding.UTF8.GetBytes(param);
            var responseContent = string.Empty;
            var exception = string.Empty;
            var returnCode = true;
            try
            {
                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                exception = ex.Message + " | " + new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                returnCode = !returnCode;
            }
            LogOneSignalRequest(seekiosEntities, tag, seekiosName, methodName, exception, responseContent, returnCode);
            return returnCode;
        }

        /// <summary>
        /// Get notification content by NotificationType
        /// </summary>
        /// <param name="notification">notification type</param>
        /// <returns></returns>
        public static string GetContent(NotificationType notification, string countryLanguage = "fr")
        {
            switch (notification)
            {
                case NotificationType.InstructionTaken:
                    return ResourcesHelper.GetLocalizedString("InstructionTaken", countryLanguage);
                case NotificationType.RefreshPosition:
                    return ResourcesHelper.GetLocalizedString("GPSPositionReceived", countryLanguage);
                case NotificationType.RefreshPositionByCellsData:
                    return ResourcesHelper.GetLocalizedString("ApproximativePositionReceived", countryLanguage);
                case NotificationType.NotifySeekiosOutOfZone:
                    return ResourcesHelper.GetLocalizedString("OutOfZone", countryLanguage);
                case NotificationType.AddTrackingLocation:
                    return ResourcesHelper.GetLocalizedString("TrackingPositionReveived", countryLanguage);
                case NotificationType.AddNewZoneTrackingLocation:
                    return ResourcesHelper.GetLocalizedString("ZoneTrackingPositionReceived", countryLanguage);
                case NotificationType.AddNewDontMoveTrackingLocation:
                    return ResourcesHelper.GetLocalizedString("DMTrackingPositionReceived", countryLanguage);
                case NotificationType.NotifySeekiosMoved:
                    return ResourcesHelper.GetLocalizedString("SeekiosMoved", countryLanguage);
                case NotificationType.SOSSent:
                    return ResourcesHelper.GetLocalizedString("SOSAlertReceived", countryLanguage);
                case NotificationType.SOSLocationSent:
                    return ResourcesHelper.GetLocalizedString("SOSPositionGPSReceived", countryLanguage);
                case NotificationType.SOSLocationByCellsDataSent:
                    return ResourcesHelper.GetLocalizedString("SOSPositionApproximativeReceived", countryLanguage);
                case NotificationType.RefreshCredits:
                    return ResourcesHelper.GetLocalizedString("RefreshCredits", countryLanguage);
                case NotificationType.CriticalBattery:
                    return ResourcesHelper.GetLocalizedString("CriticalBattery", countryLanguage);
                case NotificationType.PowerSavingDisabled:
                    return ResourcesHelper.GetLocalizedString("PowerSavingDisabled", countryLanguage);
                case NotificationType.ChangeMode:
                    return ResourcesHelper.GetLocalizedString("ChangeMode", countryLanguage);
                default:
                    return string.Empty;
            }
        }

        #endregion

        #region ----- PRIVATES METHODS ------------------------------------------------------------------------

        /// <summary>
        /// Get tag : staging : idUSer+"s" ; prod : idUser+"p"
        /// </summary>
        private static string GetTagToAdd(string idUser)
        {
            if (SeekiosService.IsStaging) return (idUser + "s");
            else return (idUser + "p");
        }

        /// <summary>
        /// Create and format HTTP request
        /// </summary>
        /// <param name="platform">need different HTTP header depend on the platform</param>
        /// <returns>HttpWebRequest initializing</returns>
        private static HttpWebRequest CreateHttpRequest(PlatformEnum platform)
        {
            var request = WebRequest.Create(ONESIGNAL_URL) as HttpWebRequest;
            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            switch (platform)
            {
                case PlatformEnum.Android:
                    request.Headers.Add("authorization", "Basic YTBiNDlhY2EtMDBjOC00ZjBmLWFmNjQtNDMzMzlkYTczMDQ3");
                    break;
                case PlatformEnum.iOS:
                    request.Headers.Add("authorization", "Basic OTJlY2UzNDktMGQ2Yi00NmY1LWI2MjgtYTVkMzQ4NDE4MGM0");
                    break;
                case PlatformEnum.Web:
                    request.Headers.Add("authorization", "Basic MGRiMmVhMWUtMGNlMC00ZTgyLWIxMDAtMDk2ZDIzMGY2MjNm");
                    break;
            }
            return request;
        }

        /// <summary>
        /// Trace in the table LogVodafoneAndOnesignal
        /// </summary>
        private static void LogOneSignalRequest(seekios_dbEntities seekiosEntities
            , string tag
            , string seekiosName
            , string methodName
            , string exception
            , string responseContent
            , bool returnCode)
        {
            seekiosEntities.logVodafoneAndOnesignal.Add(new logVodafoneAndOnesignal()
            {
                dateBegin = DateTime.UtcNow,
                apiUser = "onesignal/android/" + ONESIGNAL_PRIVATE_KEY_ANDROID,
                imsi = methodName + "/" + seekiosName,
                majorCode = responseContent,
                minorCode = exception.Length < 50 ? exception : exception.Substring(0, 50),
                msgRef = tag,
                outcome = returnCode ? "ok" : "bad",
                dateEnd = DateTime.UtcNow
            });
            seekiosEntities.SaveChanges();
        }

        #endregion
    }
}