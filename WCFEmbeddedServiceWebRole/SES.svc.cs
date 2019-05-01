using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Activation;
using System.Threading.Tasks;
using WCFServiceWebRole;
using WCFServiceWebRole.Enum;
using WCFServiceWebRole.Helper;
using System.Data.Entity;
using WCFEmbeddedServiceWebRole.ExceptionLog;
using System.ServiceModel.Web;
using System.Net;
using System.ServiceModel;

namespace WCFServiceWebRoleEmbedded
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall
        , ConcurrencyMode = ConcurrencyMode.Multiple)
        , AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
        , ErrorHandlerAttribute(typeof(ErrorHandler))]
    public class SES : ISES
    {

        #region ----- VARIABLES -------------------------------------------------------------------------------

        public static string START_TRAME = "#M{0:00}";
        public static string ADMIN_TRAME = "#A02{0}";
        public static string START_STATUS_TRAME = "#S{0:00}";
        public static string START_DATE_TRAME = "#D";
        public static string FOOTER_TRAME = "&";

        private static SeekiosService _webService = new SeekiosService();
        private CultureInfo _enProvider = new CultureInfo("en-US");
        private static TimeSpan _onDemandGSITimeout = new TimeSpan(0, 1, 15);
        private static TimeSpan _onDemandResponseTimeout = new TimeSpan(0, 4, 0);
        private const string SEEKIOS_CNX_STR = "seekios";
        private const string SEEKIOS_DB_NAME_PROD = "u2b7kcodrq";

        #endregion

        #region ----- METHODS FROM THE INTERFACE --------------------------------------------------------------

        /// <summary>
        /// Return the instruction waiting for the seekios
        /// Error -1 => -50
        /// </summary>
        public List<string> GetSeekiosInstructions(string uidSeekios
            , string batteryStr
            , string signalStr
            , string isDateNeededStr
            , string timestampStr)
        {
            int batteryLife = 0, signalQuality = 0, timestamp = 0, isDateNeeded = 0;
            if (!int.TryParse(batteryStr, out batteryLife)) throw new WebFaultException<int>(-1, HttpStatusCode.Forbidden);
            if (!int.TryParse(signalStr, out signalQuality)) throw new WebFaultException<int>(-2, HttpStatusCode.Forbidden);
            if (!int.TryParse(timestampStr, out timestamp)) throw new WebFaultException<int>(-3, HttpStatusCode.Forbidden);
            if (!int.TryParse(isDateNeededStr, out isDateNeeded)) throw new WebFaultException<int>(-4, HttpStatusCode.Forbidden);

            var lsInstructions = new List<string>();
            var modeWaitingTrame = string.Format(START_TRAME, (int)ModeDefinitions.ModeWaiting) + FOOTER_TRAME;
            var modeInstructionExists = false;

            // If the date is asking, we send it (the date is require if the seekios has been reset or it's the first start)
            if (isDateNeeded == 1)
            {
                var unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                lsInstructions.Add(START_DATE_TRAME + unixTimestamp.ToString() + FOOTER_TRAME);
            }

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Generate admin trame to verify if a user still can send SOS alerts
                lsInstructions.Add(GenerateAdminInstruction(seekiosEntities, uidSeekios));
                // Get the seekios
                var seekiosProductionDb = GetSeekiosProductionByUidSeekios(seekiosEntities, uidSeekios, -5);
                int idSeekios = seekiosProductionDb.idseekiosProduction;
                var seekiosDb = (from s in seekiosEntities.seekios
                                 where s.idseekios == idSeekios
                                 select s).Take(1).FirstOrDefault();
                if (seekiosDb == null) return lsInstructions; // <- Need to return that string if we just delete a seekios and we want to remove the current mode
                // Get all the seekios instructions
                var seekiosInstructionsDb = (from si in seekiosEntities.seekiosInstruction
                                             where si.seekiosProduction_idseekiosProduction == idSeekios
                                             orderby si.idseekiosInstruction descending
                                             select si).ToList();
                //var sendDate = isDateNeeded == 1 ? DateTime.UtcNow : UnixTimeStampToDateTime(timestamp);
                var sendDate = DateTime.UtcNow;

                foreach (var seekiosInstructionDb in seekiosInstructionsDb)
                {
                    // Instruction is a mode configuration
                    if (seekiosInstructionDb.instructionType == (int)InstructionType.ChangeMode && seekiosInstructionDb.instruction != modeWaitingTrame)
                    {
                        var modeDb = (from m in seekiosEntities.mode
                                      where m.seekios_idseekios == idSeekios
                                      orderby m.idmode descending
                                      select m).FirstOrDefault();
                        if (modeDb == null)
                        {
                            //seekiosEntities.seekiosInstruction.Remove(seekiosInstructionDb);
                            continue;
                        }

                        if (!modeDb.dateModeActivation.HasValue)
                        {
                            modeDb.dateModeActivation = DateTime.UtcNow;
                            seekiosEntities.SaveChanges();
                        }

                        var operationDb = (from o in seekiosEntities.operation
                                           where o.mode_idmode == modeDb.idmode
                                               && o.operationType_idoperationType == (int)OperationType.ConfigureMode
                                           orderby o.idoperation descending
                                           select o).FirstOrDefault();
                        // If mode payement operation is not already done
                        if (operationDb == null)
                        {
                            // Try to pay the credit cost
                            // If payment failed do not send this instruction to seekios
                            if (CreditBillingHelper.PayOperationCost(seekiosEntities
                                , OperationType.ConfigureMode
                                , seekiosDb.user_iduser
                                , modeDb != null ? (int?)modeDb.idmode : null
                                , idSeekios) != 1)
                            {
                                continue;
                            }
                            else
                            {
                                // Send notification change mode
                                var preferredLanguage = ResourcesHelper.GetPreferredLanguage(seekiosEntities, seekiosDb.user_iduser);
                                var displayMessage = string.Empty;
                                if (modeDb.modeDefinition_idmodeDefinition == (int)ModeDefinitions.ModeTracking)
                                {
                                    displayMessage = string.Format(NotificationHelper.GetContent(NotificationType.ChangeMode, preferredLanguage)
                                        , ResourcesHelper.GetLocalizedString("ModeTracking", preferredLanguage));
                                }
                                else if (modeDb.modeDefinition_idmodeDefinition == (int)ModeDefinitions.ModeZone)
                                {
                                    displayMessage = string.Format(NotificationHelper.GetContent(NotificationType.ChangeMode, preferredLanguage)
                                        , ResourcesHelper.GetLocalizedString("ModeZone", preferredLanguage));
                                }
                                else if (modeDb.modeDefinition_idmodeDefinition == (int)ModeDefinitions.ModeDontMove)
                                {
                                    displayMessage = string.Format(NotificationHelper.GetContent(NotificationType.ChangeMode, preferredLanguage)
                                        , ResourcesHelper.GetLocalizedString("ModeDontMove", preferredLanguage));
                                }
                                NotificationHelper.SendNotifications(seekiosEntities
                                    , seekiosDb.user_iduser
                                    , seekiosDb.seekiosName
                                    , null
                                    , displayMessage
                                    , preferredLanguage
                                    , false);
                            }
                        }
                        modeInstructionExists = true;
                    }
                    // Instruction is a refresh position
                    else if (seekiosInstructionDb.instructionType == (int)InstructionType.OnDemand)
                    {
                        // If on demand instruction has expired // 1:15 timeout
                        if (seekiosInstructionDb.dateCreation.Add(_onDemandGSITimeout) < DateTime.UtcNow)
                        {
                            seekiosEntities.DeleteSeekiosInstructionByIdSeekios(idSeekios, (int)InstructionType.OnDemand);
                            continue;
                        }
                    }
                    // Instruction is a refresh battery
                    else if (seekiosInstructionDb.instructionType == (int)InstructionType.SendBatteryLevel)
                    {
                        seekiosEntities.DeleteSeekiosInstructionByIdSeekios(idSeekios, (int)InstructionType.SendBatteryLevel);
                        // Refresh battery is a free action
                        CreditBillingHelper.AddOperationInDatabase(seekiosEntities
                            , DateTime.UtcNow
                            , null
                            , seekiosDb.idseekios
                            , seekiosDb.user_iduser
                            , OperationType.RefreshBattery
                            , 0
                            , false);
                    }
                    lsInstructions.Add(seekiosInstructionDb.instruction);
                }
                seekiosDb.hasGetLastInstruction = 1;
                AddSeekiosCommunication(seekiosEntities, seekiosDb, batteryLife, signalQuality, sendDate);
                // Broadcast user devices
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_INSTRUCTION_TAKEN, new object[]
                {
                    seekiosDb.user_iduser,
                    uidSeekios,
                    new Tuple<int, int>(batteryLife, signalQuality),
                    sendDate
                });
            }
            // If there is no instruction mode 
            if (!modeInstructionExists && !lsInstructions.Contains(modeWaitingTrame))
            {
                lsInstructions.Add(modeWaitingTrame);
            }
            return lsInstructions;
        }

        /// <summary>
        /// Get the answer from the seekios for the instruction "on demand"
        /// Error -51 => -100
        /// </summary>
        public int RespondOnDemandRequest(string uidSeekios
            , string batteryStr
            , string signalStr
            , string latitudeStr
            , string longitudeStr
            , string altitudeStr
            , string accuracyStr
            , string timestampStr)
        {
            int timestamp = 0, batteryLife = 0, signalQuality = 0;
            double latitude = 0.0, longitude = 0.0, altitude = 0.0, accuracy = 0.0;

            if (!int.TryParse(timestampStr, out timestamp)) throw new WebFaultException<int>(-51, HttpStatusCode.Forbidden);
            if (!int.TryParse(batteryStr, out batteryLife)) throw new WebFaultException<int>(-52, HttpStatusCode.Forbidden);
            if (!int.TryParse(signalStr, out signalQuality)) throw new WebFaultException<int>(-53, HttpStatusCode.Forbidden);
            if (!double.TryParse(latitudeStr, NumberStyles.Any, _enProvider, out latitude)) throw new WebFaultException<int>(-54, HttpStatusCode.Forbidden);
            if (!double.TryParse(longitudeStr, NumberStyles.Any, _enProvider, out longitude)) throw new WebFaultException<int>(-55, HttpStatusCode.Forbidden);
            if (!double.TryParse(altitudeStr, NumberStyles.Any, _enProvider, out altitude)) throw new WebFaultException<int>(-56, HttpStatusCode.Forbidden);
            if (!double.TryParse(accuracyStr, NumberStyles.Any, _enProvider, out accuracy)) throw new WebFaultException<int>(-57, HttpStatusCode.Forbidden);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the seekios
                var seekiosProductionDb = GetSeekiosProductionByUidSeekios(seekiosEntities, uidSeekios, -58);
                // Parse send date timestamp to DateTime
                var sendDate = UnixTimeStampToDateTime(timestamp);
                //var sendDate = DateTime.UtcNow;
                // Protection againt duplicate location rows
                if (IsDuplicateLocation(seekiosEntities, sendDate, seekiosProductionDb.idseekiosProduction)) return 0;
                var seekiosDb = GetSeekiosById(seekiosEntities, seekiosProductionDb.idseekiosProduction, -59);
                seekiosDb.dateLastOnDemandRequest = null;
                seekiosDb.lastKnowLocation_idlocationDefinition = (int)LocationDefinition.OnDemand;
                // Add a new communication in the database
                AddSeekiosCommunication(seekiosEntities
                    , seekiosDb
                    , batteryLife
                    , signalQuality
                    , sendDate);
                // Add a new location in the database
                if (AddSeekiosLocation(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb
                    , latitude
                    , longitude
                    , altitude
                    , 0.0
                    , sendDate
                    , LocationDefinition.OnDemand) != 1) throw new WebFaultException<int>(-60, HttpStatusCode.Forbidden);
                // Broadcast user devices
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_REFRESH_POSITION, new object[]
                {
                    seekiosDb.user_iduser,
                    uidSeekios,
                    new Tuple<int, int>(batteryLife, signalQuality),
                    new Tuple<double, double, double, double>(latitude, longitude, altitude, 0.0),
                    sendDate
                });
                // Send notification, get a new location
                var preferredLanguage = ResourcesHelper.GetPreferredLanguage(seekiosEntities, seekiosDb.user_iduser);
                NotificationHelper.SendNotifications(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb.seekiosName
                    , null
                    , NotificationHelper.GetContent(NotificationType.RefreshPosition, preferredLanguage)
                    , preferredLanguage
                    , false);
            }
            return 1;
        }

        /// <summary>
        /// Get the answer from the seekios for the instruction "on demand"
        /// Error -101 => -150
        /// </summary>
        public async Task<int> RespondOnDemandRequestByCellsData(string uidSeekios
            , string batteryStr
            , string signalStr
            , string cellsDataStr
            , string timestampStr)
        {
            SeekiosService.Telemetry.TrackEvent("Google Triangulation");

            var cellsData = TriangulationHelper.DeserializeCellsData(cellsDataStr);
            var triangulation = await TriangulationHelper.GetTriangulationLocation(cellsData);

            int timestamp = 0, batteryLife = 0, signalQuality = 0;

            if (triangulation.Location == null) throw new WebFaultException<int>(-101, HttpStatusCode.Forbidden);
            if (!int.TryParse(timestampStr, out timestamp)) throw new WebFaultException<int>(-102, HttpStatusCode.Forbidden);
            if (!int.TryParse(batteryStr, out batteryLife)) throw new WebFaultException<int>(-103, HttpStatusCode.Forbidden);
            if (!int.TryParse(signalStr, out signalQuality)) throw new WebFaultException<int>(-104, HttpStatusCode.Forbidden);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the seekios
                var seekiosProductionDb = GetSeekiosProductionByUidSeekios(seekiosEntities, uidSeekios, -105);
                // Parse send date timestamp to DateTime
                var sendDate = UnixTimeStampToDateTime(timestamp);
                //var sendDate = DateTime.UtcNow;
                // Protection againt duplicate location rows
                if (IsDuplicateLocation(seekiosEntities, sendDate, seekiosProductionDb.idseekiosProduction)) return 0;
                var seekiosDb = GetSeekiosById(seekiosEntities, seekiosProductionDb.idseekiosProduction, -106);
                seekiosDb.dateLastOnDemandRequest = null;
                seekiosDb.lastKnowLocation_idlocationDefinition = (int)LocationDefinition.OnDemand;
                // Add a new communication in the database
                AddSeekiosCommunication(seekiosEntities
                    , seekiosDb
                    , batteryLife
                    , signalQuality
                    , sendDate);
                // Add a new location in the database
                if (AddSeekiosLocation(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb
                    , triangulation.Location.Lat
                    , triangulation.Location.Lon
                    , 0.0
                    , triangulation.Accuracy
                    , sendDate
                    , LocationDefinition.OnDemand
                    , false) != 1) throw new WebFaultException<int>(-107, HttpStatusCode.Forbidden);
                // GSM position is a free action
                CreditBillingHelper.AddOperationInDatabase(seekiosEntities
                    , DateTime.UtcNow
                    , null
                    , seekiosDb.idseekios
                    , seekiosDb.user_iduser
                    , OperationType.RefreshPosition
                    , 0
                    , false);
                // Broadcast user devices
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_REFRESH_POSITION, new object[]
                {
                    seekiosDb.user_iduser,
                    uidSeekios,
                    new Tuple<int, int>(batteryLife, signalQuality),
                    new Tuple<double, double, double, double>(triangulation.Location.Lat, triangulation.Location.Lon, 0.0, triangulation.Accuracy),
                    sendDate
                });
                // Send notification, get a new location
                var preferredLanguage = ResourcesHelper.GetPreferredLanguage(seekiosEntities, seekiosDb.user_iduser);
                NotificationHelper.SendNotifications(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb.seekiosName
                    , null
                    , NotificationHelper.GetContent(NotificationType.RefreshPositionByCellsData, preferredLanguage)
                    , preferredLanguage
                    , false);
            }
            return 1;
        }

        /// <summary>
        /// Notify the seekios is out of the zone
        /// Error -201 => -250
        /// </summary>
        public int NotifySeekiosOutOfZone2(string uidSeekios
            , string batteryStr
            , string signalStr
            , string latitudeStr
            , string longitudeStr
            , string altitudeStr
            , string accuracyStr
            , string timestampStr
            , string modeIdStr)
        {
            int timestamp = 0, batteryLife = 0, signalQuality = 0, modeId = 0;
            double latitude = 0.0, longitude = 0.0, altitude = 0.0, accuracy = 0.0;

            if (!int.TryParse(timestampStr, out timestamp)) throw new WebFaultException<int>(-201, HttpStatusCode.Forbidden);
            if (!int.TryParse(batteryStr, out batteryLife)) throw new WebFaultException<int>(-202, HttpStatusCode.Forbidden);
            if (!int.TryParse(signalStr, out signalQuality)) throw new WebFaultException<int>(-203, HttpStatusCode.Forbidden);
            if (!double.TryParse(latitudeStr, NumberStyles.Any, _enProvider, out latitude)) throw new WebFaultException<int>(-204, HttpStatusCode.Forbidden);
            if (!double.TryParse(longitudeStr, NumberStyles.Any, _enProvider, out longitude)) throw new WebFaultException<int>(-205, HttpStatusCode.Forbidden);
            if (!double.TryParse(altitudeStr, NumberStyles.Any, _enProvider, out altitude)) throw new WebFaultException<int>(-206, HttpStatusCode.Forbidden);
            if (!double.TryParse(accuracyStr, NumberStyles.Any, _enProvider, out accuracy)) throw new WebFaultException<int>(-207, HttpStatusCode.Forbidden);
            if (!int.TryParse(modeIdStr, out modeId)) throw new WebFaultException<int>(-208, HttpStatusCode.Forbidden);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the seekios
                var seekiosProductionDb = GetSeekiosProductionByUidSeekios(seekiosEntities, uidSeekios, -209);
                // Parse send date timestamp to DateTime
                //var sendDate = UnixTimeStampToDateTime(timestamp);
                var sendDate = DateTime.UtcNow;
                // Protection againt duplicate location rows
                if (IsDuplicateLocation(seekiosEntities, sendDate, seekiosProductionDb.idseekiosProduction)) return 0;
                var seekiosDb = GetSeekiosById(seekiosEntities, seekiosProductionDb.idseekiosProduction, -210);
                // Add a new communication in the database
                AddSeekiosCommunication(seekiosEntities
                    , seekiosDb
                    , batteryLife
                    , signalQuality
                    , sendDate);
                // Add a new location in the database
                if (AddSeekiosLocation(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb
                    , latitude
                    , longitude
                    , altitude
                    , 0.0
                    , sendDate
                    , LocationDefinition.Zone) != 1) throw new WebFaultException<int>(-211, HttpStatusCode.Forbidden);
                // Update the mode
                mode modeDb = null;
                if (modeId == 0)
                {
                    modeDb = GetModeByIdSeekios(seekiosEntities, seekiosDb.idseekios, ModeDefinitions.ModeZone, -212);
                }
                else modeDb = GetModeByIdSeekiosAndIdMode(seekiosEntities, seekiosDb.idseekios, modeId, -213);
                modeDb.statusDefinition_idstatusDefinition = (int)StatusDefinition.SeekiosOutOfZone;
                modeDb.countOfTriggeredAlert++; // Increments the number of alerts launched
                modeDb.lastTriggeredAlertDate = sendDate;
                seekiosEntities.SaveChanges();
                // Change the mode instruction with the correct state
                SeekiosService.PrepareInstructionForNewMode(seekiosEntities
                    , modeDb
                    , seekiosDb
                    , seekiosProductionDb
                    , false);
                // Send alerts (emails)
                var preferredLanguage = ResourcesHelper.GetPreferredLanguage(seekiosEntities, seekiosDb.user_iduser);
                SeekiosService.SendAlerts(seekiosEntities
                    , modeDb.idmode
                    , (from u in seekiosEntities.user where u.iduser == seekiosDb.user_iduser select u).FirstOrDefault()
                    , seekiosDb.seekiosName
                    , new Tuple<double, double>(latitude, longitude)
                    , LocationDefinition.Zone
                    , preferredLanguage);
                // Broadcast user devices
                SignalRHelper.BroadcastUser(HubProxyEnum.ZoneHub, SignalRHelper.METHOD_NOTIFY_SEEKIOS_OUT_OF_ZONE, new object[]
                {
                    seekiosDb.user_iduser,
                    uidSeekios,
                    new Tuple<int, int>(batteryLife, signalQuality),
                    new Tuple<double, double, double, double>(latitude, longitude, altitude, accuracy),
                    sendDate
                });
                // Send notification out of zone
                NotificationHelper.SendNotifications(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb.seekiosName
                    , null
                    , NotificationHelper.GetContent(NotificationType.NotifySeekiosOutOfZone, preferredLanguage)
                    , preferredLanguage
                    , true);
            }
            return 1;
        }

        /// <summary>
        /// Notify the seekios has moved
        /// Error -251 => -300
        /// </summary>
        public int NotifySeekiosMoved2(string uidSeekios
            , string batteryStr
            , string signalStr
            , string timestampStr
            , string modeIdStr)
        {
            int timestamp = 0, batteryLife = 0, signalQuality = 0, modeId = 0;

            if (!int.TryParse(batteryStr, out batteryLife)) throw new WebFaultException<int>(-251, HttpStatusCode.Forbidden);
            if (!int.TryParse(signalStr, out signalQuality)) throw new WebFaultException<int>(-252, HttpStatusCode.Forbidden);
            if (!int.TryParse(timestampStr, out timestamp)) throw new WebFaultException<int>(-253, HttpStatusCode.Forbidden);
            if (!int.TryParse(modeIdStr, out modeId)) throw new WebFaultException<int>(-254, HttpStatusCode.Forbidden);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the seekios
                var seekiosProductionDb = GetSeekiosProductionByUidSeekios(seekiosEntities, uidSeekios, -255);
                var seekiosDb = GetSeekiosById(seekiosEntities, seekiosProductionDb.idseekiosProduction, -256);
                // Parse send date timestamp to DateTime
                //var sendDate = UnixTimeStampToDateTime(timestamp);
                var sendDate = DateTime.UtcNow;
                // Add a new communication in the database
                AddSeekiosCommunication(seekiosEntities
                    , seekiosDb
                    , batteryLife
                    , signalQuality
                    , sendDate);
                // Update the mode
                mode modeDb = null;
                if (modeId == 0)
                {
                    modeDb = GetModeByIdSeekios(seekiosEntities, seekiosDb.idseekios, ModeDefinitions.ModeDontMove, -257);
                }
                else modeDb = GetModeByIdSeekiosAndIdMode(seekiosEntities, seekiosDb.idseekios, modeId, -258);
                modeDb.statusDefinition_idstatusDefinition = (int)StatusDefinition.SeekiosMoved;
                modeDb.countOfTriggeredAlert++; // Increments the number of alerts launched
                modeDb.lastTriggeredAlertDate = sendDate;
                seekiosEntities.SaveChanges();
                // Change the mode instruction with the correct state
                SeekiosService.PrepareInstructionForNewMode(seekiosEntities
                    , modeDb
                    , seekiosDb
                    , seekiosProductionDb
                    , false);
                // Send alerts (emails)
                var preferredLanguage = ResourcesHelper.GetPreferredLanguage(seekiosEntities, seekiosDb.user_iduser);
                SeekiosService.SendAlerts(seekiosEntities
                    , modeDb.idmode
                    , (from u in seekiosEntities.user where u.iduser == seekiosDb.user_iduser select u).FirstOrDefault()
                    , seekiosDb.seekiosName
                    , null
                    , LocationDefinition.DontMove
                    , preferredLanguage);
                // Broadcast user devices
                SignalRHelper.BroadcastUser(HubProxyEnum.DontMoveHub, SignalRHelper.METHOD_NOTIFY_SEEKIOS_MOVED, new object[]
                {
                    seekiosDb.user_iduser,
                    uidSeekios,
                    new Tuple<int, int>(batteryLife, signalQuality),
                    sendDate
                });
                // Send notification moved
                NotificationHelper.SendNotifications(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb.seekiosName
                    , null
                    , NotificationHelper.GetContent(NotificationType.NotifySeekiosMoved, preferredLanguage)
                    , preferredLanguage
                    , true);
            }
            return 1;
        }

        /// <summary>
        /// Add a new tracking location and notify the client
        /// Error -301 => -350
        /// </summary>
        public int AddNewTrackingLocation2(string uidSeekios
            , string batteryStr
            , string signalStr
            , string latitudeStr
            , string longitudeStr
            , string altitudeStr
            , string accuracyStr
            , string timestampStr
            , string modeIdStr)
        {
            int timestamp = 0, batteryLife = 0, signalQuality = 0, modeId = 0;
            double latitude = 0.0, longitude = 0.0, altitude = 0.0, accuracy = 0.0;

            if (!int.TryParse(timestampStr, out timestamp)) throw new WebFaultException<int>(-301, HttpStatusCode.Forbidden);
            if (!int.TryParse(batteryStr, out batteryLife)) throw new WebFaultException<int>(-302, HttpStatusCode.Forbidden);
            if (!int.TryParse(signalStr, out signalQuality)) throw new WebFaultException<int>(-303, HttpStatusCode.Forbidden);
            if (!double.TryParse(latitudeStr, NumberStyles.Any, _enProvider, out latitude)) throw new WebFaultException<int>(-304, HttpStatusCode.Forbidden);
            if (!double.TryParse(longitudeStr, NumberStyles.Any, _enProvider, out longitude)) throw new WebFaultException<int>(-305, HttpStatusCode.Forbidden);
            if (!double.TryParse(altitudeStr, NumberStyles.Any, _enProvider, out altitude)) throw new WebFaultException<int>(-306, HttpStatusCode.Forbidden);
            if (!double.TryParse(accuracyStr, NumberStyles.Any, _enProvider, out accuracy)) throw new WebFaultException<int>(-307, HttpStatusCode.Forbidden);
            if (!int.TryParse(modeIdStr, out modeId)) throw new WebFaultException<int>(-308, HttpStatusCode.Forbidden);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the seekios and mode
                var seekiosProductionDb = GetSeekiosProductionByUidSeekios(seekiosEntities, uidSeekios, -309);
                // Parse send date timestamp to DateTime
                //var sendDate = UnixTimeStampToDateTime(timestamp);
                var sendDate = DateTime.UtcNow;
                // Protection against duplicate location rows
                if (IsDuplicateLocation(seekiosEntities, sendDate, seekiosProductionDb.idseekiosProduction)) return 0;
                var seekiosDb = GetSeekiosById(seekiosEntities, seekiosProductionDb.idseekiosProduction, -310);
                seekiosDb.lastKnowLocation_idlocationDefinition = (int)LocationDefinition.Tracking;
                // Get the mode
                mode modeDb = null;
                if (modeId == 0)
                {
                    modeDb = GetModeByIdSeekios(seekiosEntities, seekiosDb.idseekios, ModeDefinitions.ModeTracking, -311);
                }
                else modeDb = GetModeByIdSeekiosAndIdMode(seekiosEntities, seekiosDb.idseekios, modeId, -312);
                // Pay the transaction
                if (CreditBillingHelper.PayOperationCost(seekiosEntities
                    , OperationType.AddTrackingPosition
                    , seekiosDb.user_iduser
                    , modeDb.idmode
                    , seekiosProductionDb.idseekiosProduction) != 1) throw new WebFaultException<int>(-313, HttpStatusCode.Forbidden);
                // Update the mode
                modeDb.countOfTriggeredAlert++;
                modeDb.lastTriggeredAlertDate = sendDate;
                // Add a new communication in the database
                AddSeekiosCommunication(seekiosEntities
                    , seekiosDb
                    , batteryLife
                    , signalQuality
                    , sendDate);
                // Add a new location in the database
                if (AddSeekiosLocation(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb, latitude
                    , longitude
                    , altitude
                    , 0.0
                    , sendDate
                    , LocationDefinition.Tracking) != 1) throw new WebFaultException<int>(-314, HttpStatusCode.Forbidden);
                // Broadcast user devices
                SignalRHelper.BroadcastUser(HubProxyEnum.TrackingHub, SignalRHelper.METHOD_ADD_TRACKING_LOCATION, new object[]
                {
                    seekiosDb.user_iduser,
                    uidSeekios,
                    new Tuple<int, int>(batteryLife, signalQuality),
                    new Tuple<double, double, double, double>(latitude, longitude, altitude, 0.0),
                    sendDate
                });
                if (seekiosDb.sendNotificationOnNewTrackingLocation == 1)
                {
                    // Send notification for the new tracking location
                    var preferredLanguage = ResourcesHelper.GetPreferredLanguage(seekiosEntities, seekiosDb.user_iduser);
                    NotificationHelper.SendNotifications(seekiosEntities
                        , seekiosDb.user_iduser
                        , seekiosDb.seekiosName
                        , null
                        , NotificationHelper.GetContent(NotificationType.AddTrackingLocation, preferredLanguage)
                        , preferredLanguage
                        , false);
                }
            }
            return 1;
        }

        /// <summary>
        /// Add a new tracking location when the seekios is out of zone and notify the client
        /// Error -401 => -450
        /// </summary>
        public int AddNewZoneTrackingLocation2(string uidSeekios
            , string batteryStr
            , string signalStr
            , string latitudeStr
            , string longitudeStr
            , string altitudeStr
            , string accuracyStr
            , string timestampStr
            , string modeIdStr)
        {
            int timestamp = 0, batteryLife = 0, signalQuality = 0, modeId = 0;
            double latitude = 0.0, longitude = 0.0, altitude = 0.0, accuracy = 0.0;

            if (!int.TryParse(timestampStr, out timestamp)) throw new WebFaultException<int>(-401, HttpStatusCode.Forbidden);
            if (!int.TryParse(batteryStr, out batteryLife)) throw new WebFaultException<int>(-402, HttpStatusCode.Forbidden);
            if (!int.TryParse(signalStr, out signalQuality)) throw new WebFaultException<int>(-403, HttpStatusCode.Forbidden);
            if (!double.TryParse(latitudeStr, NumberStyles.Any, _enProvider, out latitude)) throw new WebFaultException<int>(-404, HttpStatusCode.Forbidden);
            if (!double.TryParse(longitudeStr, NumberStyles.Any, _enProvider, out longitude)) throw new WebFaultException<int>(-405, HttpStatusCode.Forbidden);
            if (!double.TryParse(altitudeStr, NumberStyles.Any, _enProvider, out altitude)) throw new WebFaultException<int>(-406, HttpStatusCode.Forbidden);
            if (!double.TryParse(accuracyStr, NumberStyles.Any, _enProvider, out accuracy)) throw new WebFaultException<int>(-407, HttpStatusCode.Forbidden);
            if (!int.TryParse(modeIdStr, out modeId)) throw new WebFaultException<int>(-408, HttpStatusCode.Forbidden);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the seekios and mode
                var seekiosProductionDb = GetSeekiosProductionByUidSeekios(seekiosEntities, uidSeekios, -409);
                // Parse send date timestamp to DateTime
                //var sendDate = UnixTimeStampToDateTime(timestamp);
                var sendDate = DateTime.UtcNow;
                // Protection againt duplicate location rows
                if (IsDuplicateLocation(seekiosEntities, sendDate, seekiosProductionDb.idseekiosProduction)) return 0;
                var seekiosDb = GetSeekiosById(seekiosEntities, seekiosProductionDb.idseekiosProduction, -410);
                seekiosDb.lastKnowLocation_idlocationDefinition = (int)LocationDefinition.Zone;
                // Get the current idmode
                mode modeDb = null;
                if (modeId == 0)
                {
                    modeDb = GetModeByIdSeekios(seekiosEntities, seekiosDb.idseekios, ModeDefinitions.ModeZone, -411);
                }
                else modeDb = GetModeByIdSeekiosAndIdMode(seekiosEntities, seekiosDb.idseekios, modeId, -412);
                // Pay the transaction
                if (CreditBillingHelper.PayOperationCost(seekiosEntities
                    , OperationType.AddZoneTrackingPosition
                    , seekiosDb.user_iduser
                    , modeDb.idmode
                    , seekiosProductionDb.idseekiosProduction) != 1) throw new WebFaultException<int>(-413, HttpStatusCode.Forbidden);
                // Add a new communication in the database
                AddSeekiosCommunication(seekiosEntities
                    , seekiosDb
                    , batteryLife
                    , signalQuality
                    , sendDate);
                // Add a new location in the database
                if (AddSeekiosLocation(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb, latitude
                    , longitude
                    , altitude
                    , 0.0
                    , sendDate
                    , LocationDefinition.Zone) != 1) throw new WebFaultException<int>(-414, HttpStatusCode.Forbidden);
                // Broadcast user devices
                SignalRHelper.BroadcastUser(HubProxyEnum.ZoneHub, SignalRHelper.METHOD_ADD_NEW_ZONE_TRACKING_LOCATION, new object[]
                {
                    seekiosDb.user_iduser,
                    uidSeekios,
                    new Tuple<int, int>(batteryLife, signalQuality),
                    new Tuple<double, double, double, double>(latitude, longitude, altitude, 0.0),
                    sendDate
                });
                if (seekiosDb.sendNotificationOnNewOutOfZoneLocation == 1)
                {
                    // Send notification for the new tracking location when the seekios is out of zone
                    var preferredLanguage = ResourcesHelper.GetPreferredLanguage(seekiosEntities, seekiosDb.user_iduser);
                    NotificationHelper.SendNotifications(seekiosEntities
                        , seekiosDb.user_iduser
                        , seekiosDb.seekiosName
                        , null
                        , NotificationHelper.GetContent(NotificationType.AddNewZoneTrackingLocation, preferredLanguage)
                        , preferredLanguage
                        , false);
                }
            }
            return 1;
        }

        /// <summary>
        /// Add a new tracking location when the seekios moved and notify the client
        /// Error -451 => -500
        /// </summary>
        public int AddNewDontMoveTrackingLocation2(string uidSeekios
            , string batteryStr
            , string signalStr
            , string latitudeStr
            , string longitudeStr
            , string altitudeStr
            , string accuracyStr
            , string timestampStr
            , string modeIdStr)
        {
            int timestamp = 0, batteryLife = 0, signalQuality = 0, modeId = 0;
            double latitude = 0.0, longitude = 0.0, altitude = 0.0, accuracy = 0.0;

            if (!int.TryParse(timestampStr, out timestamp)) throw new WebFaultException<int>(-451, HttpStatusCode.Forbidden);
            if (!int.TryParse(batteryStr, out batteryLife)) throw new WebFaultException<int>(-452, HttpStatusCode.Forbidden);
            if (!int.TryParse(signalStr, out signalQuality)) throw new WebFaultException<int>(-453, HttpStatusCode.Forbidden);
            if (!double.TryParse(latitudeStr, NumberStyles.Any, _enProvider, out latitude)) throw new WebFaultException<int>(-454, HttpStatusCode.Forbidden);
            if (!double.TryParse(longitudeStr, NumberStyles.Any, _enProvider, out longitude)) throw new WebFaultException<int>(-455, HttpStatusCode.Forbidden);
            if (!double.TryParse(altitudeStr, NumberStyles.Any, _enProvider, out altitude)) throw new WebFaultException<int>(-456, HttpStatusCode.Forbidden);
            if (!double.TryParse(accuracyStr, NumberStyles.Any, _enProvider, out accuracy)) throw new WebFaultException<int>(-457, HttpStatusCode.Forbidden);
            if (!int.TryParse(modeIdStr, out modeId)) throw new WebFaultException<int>(-458, HttpStatusCode.Forbidden);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get seekios
                var seekiosProductionDb = GetSeekiosProductionByUidSeekios(seekiosEntities, uidSeekios, -459);
                // Parse send date timestamp to DateTime
                //var sendDate = UnixTimeStampToDateTime(timestamp);
                var sendDate = DateTime.UtcNow;
                // Protection againt duplicate location rows
                if (IsDuplicateLocation(seekiosEntities, sendDate, seekiosProductionDb.idseekiosProduction)) return 0;
                var seekiosDb = GetSeekiosById(seekiosEntities, seekiosProductionDb.idseekiosProduction, -460);
                seekiosDb.lastKnowLocation_idlocationDefinition = (int)LocationDefinition.DontMove;
                // Get the mode
                mode modeDb = null;
                if (modeId == 0)
                {
                    modeDb = GetModeByIdSeekios(seekiosEntities, seekiosDb.idseekios, ModeDefinitions.ModeDontMove, -461);
                }
                else modeDb = GetModeByIdSeekiosAndIdMode(seekiosEntities, seekiosDb.idseekios, modeId, -462);
                // Pay the transaction
                if (CreditBillingHelper.PayOperationCost(seekiosEntities
                    , OperationType.AddDontMoveTrackingPosition
                    , seekiosDb.user_iduser
                    , modeDb.idmode
                    , seekiosProductionDb.idseekiosProduction) != 1) throw new WebFaultException<int>(-463, HttpStatusCode.Forbidden);
                // Add a new communication in the database
                AddSeekiosCommunication(seekiosEntities
                    , seekiosDb
                    , batteryLife
                    , signalQuality
                    , sendDate);
                // Add a new location in the database
                if (AddSeekiosLocation(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb, latitude
                    , longitude
                    , altitude
                    , 0.0
                    , sendDate
                    , LocationDefinition.DontMove) != 1) throw new WebFaultException<int>(-464, HttpStatusCode.Forbidden);
                // Broadcast user devices
                SignalRHelper.BroadcastUser(HubProxyEnum.DontMoveHub, SignalRHelper.METHOD_ADD_NEW_DONT_MOVE_TRACKING_LOCATION, new object[]
                {
                    seekiosDb.user_iduser,
                    uidSeekios,
                    new Tuple<int, int>(batteryLife, signalQuality),
                    new Tuple<double, double, double, double>(latitude, longitude, altitude, 0.0),
                    sendDate
                });
                if (seekiosDb.sendNotificationOnNewDontMoveLocation == 1)
                {
                    // Send notification for the new tracking location when the seekios has moved
                    var preferredLanguage = ResourcesHelper.GetPreferredLanguage(seekiosEntities, seekiosDb.user_iduser);
                    NotificationHelper.SendNotifications(seekiosEntities
                        , seekiosDb.user_iduser
                        , seekiosDb.seekiosName
                        , null
                        , NotificationHelper.GetContent(NotificationType.AddNewDontMoveTrackingLocation, preferredLanguage)
                        , preferredLanguage
                        , false);
                }
            }
            return 1;
        }

        /// <summary>
        /// Send a SOS notification
        /// Error -501 => -550
        /// </summary>
        public int SendSOS(string uidSeekios
            , string batteryStr
            , string signalStr
            , string timestampStr)
        {
            int timestamp = 0, batteryLife = 0, signalQuality = 0;

            if (!int.TryParse(timestampStr, out timestamp)) throw new WebFaultException<int>(-551, HttpStatusCode.Forbidden);
            if (!int.TryParse(batteryStr, out batteryLife)) throw new WebFaultException<int>(-552, HttpStatusCode.Forbidden);
            if (!int.TryParse(signalStr, out signalQuality)) throw new WebFaultException<int>(-553, HttpStatusCode.Forbidden);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get seekios and mode
                var seekiosProductionDb = GetSeekiosProductionByUidSeekios(seekiosEntities, uidSeekios, -554);
                var seekiosDb = GetSeekiosById(seekiosEntities, seekiosProductionDb.idseekiosProduction, -555);
                // Get the last mode of seekios
                var lastMode = (from m in seekiosEntities.mode
                                where m.seekios_idseekios == seekiosDb.idseekios
                                select m).OrderByDescending(m => m.idmode).Take(1).FirstOrDefault();
                // Get the user's device (each mode is linked to a user-device WHICH IS USELESS but cannot be NULLABLE cause its in a table constraint)
                var deviceDb = GetDeviceByIdUser(seekiosEntities, seekiosDb.user_iduser, -556);
                // Pay the transaction
                if (CreditBillingHelper.PayOperationCost(seekiosEntities
                    , OperationType.SendSOS
                    , seekiosDb.user_iduser
                    , lastMode != null ? (int?)lastMode.idmode : null
                    , seekiosProductionDb.idseekiosProduction) != 1) throw new WebFaultException<int>(-6, HttpStatusCode.Forbidden);
                // Parse send date timestamp to DateTime
                //var sendDate = UnixTimeStampToDateTime(timestamp);
                var sendDate = DateTime.UtcNow;
                // Add a new location in the database
                AddSeekiosCommunication(seekiosEntities
                    , seekiosDb
                    , batteryLife
                    , signalQuality
                    , sendDate);
                // Update the sos values
                seekiosDb.isLastSOSRead = 0;
                seekiosDb.dateLastSOSSent = sendDate;
                seekiosEntities.SaveChanges();
                // Broadcast user devices
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_SOS_SENT, new object[]
                {
                    seekiosDb.user_iduser,
                    uidSeekios,
                    new Tuple<int, int>(batteryLife, signalQuality),
                    sendDate
                });
                // Send notification to inform the client a alert sos is comming
                var preferredLanguage = ResourcesHelper.GetPreferredLanguage(seekiosEntities, seekiosDb.user_iduser);
                NotificationHelper.SendNotifications(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb.seekiosName
                    , null
                    , NotificationHelper.GetContent(NotificationType.SOSSent)
                    , preferredLanguage
                    , true);
            }
            return 1;
        }

        /// <summary>
        /// Send the new seekios location in a SOS case
        /// Error -551 => -600
        /// </summary>
        public int SendSOSLocation(string uidSeekios
            , string batteryStr
            , string signalStr
            , string latitudeStr
            , string longitudeStr
            , string altitudeStr
            , string accuracyStr
            , string timestampStr)
        {
            int timestamp = 0, batteryLife = 0, signalQuality = 0;
            double latitude = 0.0, longitude = 0.0, altitude = 0.0, accuracy = 0.0;

            if (!int.TryParse(timestampStr, out timestamp)) throw new WebFaultException<int>(-551, HttpStatusCode.Forbidden);
            if (!int.TryParse(batteryStr, out batteryLife)) throw new WebFaultException<int>(-552, HttpStatusCode.Forbidden);
            if (!int.TryParse(signalStr, out signalQuality)) throw new WebFaultException<int>(-553, HttpStatusCode.Forbidden);
            if (!double.TryParse(latitudeStr, NumberStyles.Any, _enProvider, out latitude)) throw new WebFaultException<int>(-554, HttpStatusCode.Forbidden);
            if (!double.TryParse(longitudeStr, NumberStyles.Any, _enProvider, out longitude)) throw new WebFaultException<int>(-555, HttpStatusCode.Forbidden);
            if (!double.TryParse(altitudeStr, NumberStyles.Any, _enProvider, out altitude)) throw new WebFaultException<int>(-556, HttpStatusCode.Forbidden);
            if (!double.TryParse(accuracyStr, NumberStyles.Any, _enProvider, out accuracy)) throw new WebFaultException<int>(-557, HttpStatusCode.Forbidden);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get seekios and mode
                var seekiosProductionDb = GetSeekiosProductionByUidSeekios(seekiosEntities, uidSeekios, -558);
                // Parse send date timestamp to DateTime
                //var sendDate = UnixTimeStampToDateTime(timestamp);
                var sendDate = DateTime.UtcNow;
                // Protection againt duplicate location rows
                if (IsDuplicateLocation(seekiosEntities, sendDate, seekiosProductionDb.idseekiosProduction)) return 0;
                var seekiosDb = GetSeekiosById(seekiosEntities, seekiosProductionDb.idseekiosProduction, -559);
                seekiosDb.lastKnowLocation_idlocationDefinition = (int)LocationDefinition.SOS;
                // Add a new communication in the database
                AddSeekiosCommunication(seekiosEntities
                    , seekiosDb
                    , batteryLife
                    , signalQuality
                    , sendDate);
                // Add a new location in the database
                if (AddSeekiosLocation(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb
                    , latitude
                    , longitude
                    , altitude
                    , 0.0
                    , sendDate
                    , LocationDefinition.SOS) != 1) throw new WebFaultException<int>(-560, HttpStatusCode.Forbidden);
                // Send alerts (emails)
                var preferredLanguage = ResourcesHelper.GetPreferredLanguage(seekiosEntities, seekiosDb.user_iduser);
                SeekiosService.SendAlerts(seekiosEntities
                    , seekiosDb.alertSOS_idalert ?? 0
                    , (from u in seekiosEntities.user where u.iduser == seekiosDb.user_iduser select u).FirstOrDefault()
                    , seekiosDb.seekiosName
                    , new Tuple<double, double>(latitude, longitude)
                    , LocationDefinition.SOS
                    , preferredLanguage);
                // Broadcast user devices
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_SOS_LOCATION_SENT, new object[]
                {
                    seekiosDb.user_iduser,
                    uidSeekios,
                    new Tuple<int, int>(batteryLife, signalQuality),
                    new Tuple<double, double, double, double>(latitude, longitude, altitude, accuracy),
                    sendDate
                });
                // Send notification for the new tracking location when the seekios has moved
                NotificationHelper.SendNotifications(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb.seekiosName
                    , null
                    , NotificationHelper.GetContent(NotificationType.SOSLocationSent, preferredLanguage)
                    , preferredLanguage
                    , false);
            }
            return 1;
        }

        /// <summary>
        /// Send the new seekios triangulation location in a SOS case
        /// Error -651 => -700
        /// </summary>
        public async Task<int> SendSOSLocationByCellsData(string uidSeekios
            , string batteryStr
            , string signalStr
            , string cellsDataStr
            , string timestampStr)
        {
            SeekiosService.Telemetry.TrackEvent("Google Triangulation");

            var cellsData = TriangulationHelper.DeserializeCellsData(cellsDataStr);
            var triangulation = await TriangulationHelper.GetTriangulationLocation(cellsData);

            int timestamp = 0, batteryLife = 0, signalQuality = 0;

            if (triangulation.Location == null) throw new WebFaultException<int>(-651, HttpStatusCode.Forbidden);
            if (!int.TryParse(timestampStr, out timestamp)) throw new WebFaultException<int>(-652, HttpStatusCode.Forbidden);
            if (!int.TryParse(batteryStr, out batteryLife)) throw new WebFaultException<int>(-653, HttpStatusCode.Forbidden);
            if (!int.TryParse(signalStr, out signalQuality)) throw new WebFaultException<int>(-654, HttpStatusCode.Forbidden);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the seekios
                var seekiosProductionDb = GetSeekiosProductionByUidSeekios(seekiosEntities, uidSeekios, -655);
                // Parse send date timestamp to DateTime
                //var sendDate = UnixTimeStampToDateTime(timestamp);
                var sendDate = DateTime.UtcNow;
                // Protection againt duplicate location rows
                if (IsDuplicateLocation(seekiosEntities, sendDate, seekiosProductionDb.idseekiosProduction)) return 0;
                var seekiosDb = GetSeekiosById(seekiosEntities, seekiosProductionDb.idseekiosProduction, -656);
                seekiosDb.lastKnowLocation_idlocationDefinition = (int)LocationDefinition.SOS;
                // Add a new communication in the database
                AddSeekiosCommunication(seekiosEntities
                    , seekiosDb
                    , batteryLife
                    , signalQuality
                    , sendDate);
                // Add a new location in the database
                if (AddSeekiosLocation(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb
                    , triangulation.Location.Lat
                    , triangulation.Location.Lon
                    , 0.0
                    , triangulation.Accuracy
                    , sendDate
                    , LocationDefinition.SOS) != 1) throw new WebFaultException<int>(-657, HttpStatusCode.Forbidden);
                // Send alerts (emails)
                var preferredLanguage = ResourcesHelper.GetPreferredLanguage(seekiosEntities, seekiosDb.user_iduser);
                SeekiosService.SendAlerts(seekiosEntities
                    , seekiosDb.alertSOS_idalert ?? 0
                    , (from u in seekiosEntities.user where u.iduser == seekiosDb.user_iduser select u).FirstOrDefault()
                    , seekiosDb.seekiosName
                    , new Tuple<double, double>(triangulation.Location.Lat, triangulation.Location.Lon)
                    , LocationDefinition.SOS
                    , preferredLanguage
                    , false);
                // Broadcast user devices
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_SOS_LOCATION_SENT, new object[]
                {
                    seekiosDb.user_iduser,
                    uidSeekios,
                    new Tuple<int, int>(batteryLife, signalQuality),
                    new Tuple<double, double, double, double>(triangulation.Location.Lat, triangulation.Location.Lon, 0.0, triangulation.Accuracy),
                    sendDate
                });
                // Send notification for the new tracking location when the seekios has moved
                NotificationHelper.SendNotifications(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb.seekiosName
                    , null
                    , NotificationHelper.GetContent(NotificationType.SOSLocationByCellsDataSent)
                    , preferredLanguage
                    , false);
            }
            return 1;
        }

        /// <summary>
        /// Send an alert to the user when the seekios battery reach 20%
        /// Error -701 => -750
        /// </summary>
        public int CriticalBatteryAlert(string uidSeekios
            , string batteryStr
            , string signalStr
            , string timestampStr)
        {
            int batteryLife = 0, signalQuality = 0;
            long timestamp = 0;

            if (!long.TryParse(timestampStr, out timestamp)) throw new WebFaultException<int>(-701, HttpStatusCode.Forbidden);
            if (!int.TryParse(batteryStr, out batteryLife)) throw new WebFaultException<int>(-702, HttpStatusCode.Forbidden);
            if (!int.TryParse(signalStr, out signalQuality)) throw new WebFaultException<int>(-703, HttpStatusCode.Forbidden);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the seekios
                var seekiosProductionDb = GetSeekiosProductionByUidSeekios(seekiosEntities, uidSeekios, -704);
                var seekiosDb = GetSeekiosById(seekiosEntities, seekiosProductionDb.idseekiosProduction, -705);
                // Update seekios in bdd
                seekiosDb.batteryLife = batteryLife;
                seekiosDb.signalQuality = signalQuality;
                seekiosDb.dateLastCommunication = /*timestamp > 0 ? UnixTimeStampToDateTime(timestamp) : */DateTime.UtcNow;
                seekiosEntities.SaveChanges();
                // Communication date
                //var sendDate = UnixTimeStampToDateTime(timestamp);
                var sendDate = DateTime.UtcNow;
                // Add a new communication in the database
                AddSeekiosCommunication(seekiosEntities
                    , seekiosDb
                    , batteryLife
                    , signalQuality
                    , sendDate);
                // Broadcast user devices
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_CRITICAL_BATTERY, new object[]
                {
                    seekiosDb.user_iduser,
                    uidSeekios,
                    new Tuple<int, int>(batteryLife, signalQuality),
                    sendDate
                });
                // Get language and send notification
                var preferredLanguage = ResourcesHelper.GetPreferredLanguage(seekiosEntities, seekiosDb.user_iduser);
                NotificationHelper.SendNotifications(seekiosEntities
                    , seekiosDb.user_iduser
                    , seekiosDb.seekiosName
                    , null
                    , NotificationHelper.GetContent(NotificationType.CriticalBattery)
                    , preferredLanguage
                    , true);
            }
            return 1;
        }

        /// <summary>
        /// Called when the seekios disables the power saving on its current mode (most likely after turning it off then on again).
        /// </summary>
        public int PowerSavingDisabled(string uidSeekios
        , string batteryStr
        , string signalStr
        , string timestampStr
        , string modeIdStr)
        {
            int timestamp = 0, batteryLife = 0, signalQuality = 0, modeId = 0;

            if (!int.TryParse(batteryStr, out batteryLife)) throw new WebFaultException<int>(-1, HttpStatusCode.Forbidden);
            if (!int.TryParse(signalStr, out signalQuality)) throw new WebFaultException<int>(-2, HttpStatusCode.Forbidden);
            if (!int.TryParse(timestampStr, out timestamp)) throw new WebFaultException<int>(-3, HttpStatusCode.Forbidden);
            if (!int.TryParse(modeIdStr, out modeId)) throw new WebFaultException<int>(-7, HttpStatusCode.Forbidden);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the seekios
                var seekiosProductionDb = GetSeekiosProductionByUidSeekios(seekiosEntities, uidSeekios, -4);
                var seekiosDb = GetSeekiosById(seekiosEntities, seekiosProductionDb.idseekiosProduction, -5);
                // Parse send date timestamp to DateTime
                //var sendDate = UnixTimeStampToDateTime(timestamp);
                var sendDate = DateTime.UtcNow;
                // Add a new communication in the database
                AddSeekiosCommunication(seekiosEntities
                    , seekiosDb
                    , batteryLife
                    , signalQuality
                    , sendDate);

                // If the mode is 0 (no mode), we don't check the mode
                mode modeDb = null;
                if (modeId > 0)
                {
                    modeDb = (from m in seekiosEntities.mode
                              where m.seekios_idseekios == seekiosDb.idseekios
                              && m.idmode == modeId
                              select m).FirstOrDefault();
                }
                // Update the power saving state

                if (modeDb != null)
                {
                    modeDb.isPowerSavingEnabled = false;
                    // Change the mode instruction with the correct state
                    SeekiosService.PrepareInstructionForNewMode(seekiosEntities
                        , modeDb
                        , seekiosDb
                        , seekiosProductionDb
                        , false);
                    seekiosDb.isInPowerSaving = 0;
                }
                else // If no mode was found, we check the current mode on the seekios. We will send a power saving disabled is the current mode has no powersaving
                {
                    modeDb = (from m in seekiosEntities.mode
                              where m.seekios_idseekios == seekiosDb.idseekios
                              select m).FirstOrDefault();
                    if (modeDb == null)
                    {
                        seekiosDb.isInPowerSaving = 0;
                    }
                    else if (modeDb.isPowerSavingEnabled == false)
                    {
                        seekiosDb.isInPowerSaving = 0;
                    }
                }
                seekiosEntities.SaveChanges();
                // Broadcast user devices
                SignalRHelper.BroadcastUser(HubProxyEnum.SeekiosHub, SignalRHelper.METHOD_POWER_SAVING_DISABLED, new object[]
                {
                        seekiosDb.user_iduser,
                        uidSeekios,
                        new Tuple<int, int>(batteryLife, signalQuality),
                        sendDate
                });
            }
            return 1;
        }

        #region MASS PRODUCTION

        /// <summary>
        /// Add new seekios hardware report (for mass production)
        /// </summary>
        public int AddNewSeekiosHardwareReport(string IMEI
        , string UID
        , string IMSI
        , string MacAddress
        , string batteryStr
        , string Timestamp
        , string BoolReport
        , string OSVersion)
        {
            int batteryLife = 0;

            if (!int.TryParse(batteryStr, out batteryLife)) throw new WebFaultException<int>(-2, HttpStatusCode.Forbidden);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                int idSeekios = -1;
                // Get the seekios production
                var seekiosProductionDb = (from m in seekiosEntities.seekiosProduction
                                           where m.imei == IMEI
                                           select m).FirstOrDefault();
                // Get the firmware version 
                var versionDb = (from v in seekiosEntities.versionEmbedded
                                 where v.versionName == OSVersion
                                 select v).FirstOrDefault();
                if (seekiosProductionDb == null)
                {
                    // Create a new seekios production
                    var seekiosProd = new seekiosProduction()
                    {
                        imei = IMEI,
                        imsi = IMSI,
                        macAddress = MacAddress,
                        versionEmbedded_idversionEmbedded = versionDb == null ? 1 : versionDb.idVersionEmbedded,
                        uidSeekios = UID,
                        lastUpdateConfirmed = 1
                    };
                    seekiosEntities.seekiosProduction.Add(seekiosProd);
                    seekiosEntities.SaveChanges();
                    idSeekios = seekiosProd.idseekiosProduction;
                }
                else
                {
                    // Update a seekios production
                    idSeekios = seekiosProductionDb.idseekiosProduction;
                    seekiosProductionDb.imei = IMEI;
                    seekiosProductionDb.imsi = IMSI;
                    seekiosProductionDb.macAddress = MacAddress;
                    seekiosProductionDb.versionEmbedded_idversionEmbedded = versionDb == null ? 1 : versionDb.idVersionEmbedded;
                    seekiosProductionDb.uidSeekios = UID;
                    seekiosProductionDb.lastUpdateConfirmed = 1;
                    seekiosEntities.SaveChanges();
                }
                var bitvect = BoolReport.ToCharArray();
                if (bitvect.Length != 18) throw new WebFaultException<int>(-3, HttpStatusCode.Forbidden);
                var hardwareReport = new seekiosHardwareReport()
                {
                    GSMUSART = bitvect[0] == '1' ? true : false,
                    GSM_GPRS = bitvect[1] == '1' ? true : false,
                    GPSUSART = bitvect[2] == '1' ? true : false,
                    GSPFrames = bitvect[3] == '1' ? true : false,
                    GPSPosit = bitvect[4] == '1' ? true : false,
                    BLEConfig = bitvect[5] == '1' ? true : false,
                    BLEAdvertizing = bitvect[6] == '1' ? true : false,
                    BLEConnection = bitvect[7] == '1' ? true : false,
                    IMUgetaccel = bitvect[8] == '1' ? true : false,
                    IMUInterrupt = bitvect[9] == '1' ? true : false,
                    DataFlashRead = bitvect[10] == '1' ? true : false,
                    DataFlashWrite = bitvect[11] == '1' ? true : false,
                    LEDs = bitvect[12] == '1' ? true : false,
                    Bouton = bitvect[13] == '1' ? true : false,
                    CalendarTime = bitvect[14] == '1' ? true : false,
                    CalendarInterrupt = bitvect[15] == '1' ? true : false,
                    AdcUSB = bitvect[16] == '1' ? true : false,
                    AdcBattery = bitvect[17] == '1' ? true : false,
                    DateDuTest = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds, //TODO: mettre ca propre...
                    BatteryLife = batteryLife,
                    SeekiosProdID = idSeekios,
                };
                seekiosEntities.seekiosHardwareReport.Add(hardwareReport);
                seekiosEntities.SaveChanges();
            }
            return 1;
        }

        #endregion

        #region UPDATE SEEKIOS SOFTWARE

        /// <summary>
        /// Update the seekios version
        /// </summary>
        public int UpdateSeekiosVersion(string uidSeekios
            , string batteryStr
            , string signalStr
            , string OSVersion
            , string timestampStr)
        {
            int batteryLife = 0, signalQuality = 0;
            long timestamp = 0;

            if (!long.TryParse(timestampStr, out timestamp)) throw new WebFaultException<int>(-1, HttpStatusCode.Forbidden);
            if (!int.TryParse(batteryStr, out batteryLife)) throw new WebFaultException<int>(-2, HttpStatusCode.Forbidden);
            if (!int.TryParse(signalStr, out signalQuality)) throw new WebFaultException<int>(-3, HttpStatusCode.Forbidden);

            using (seekios_dbEntities seekiosEntities = new seekios_dbEntities())
            {
                // Get the seekios
                var seekiosProductionDb = GetSeekiosProductionByUidSeekios(seekiosEntities, uidSeekios, -4);
                var seekiosDb = (from s in seekiosEntities.seekios
                                 where s.idseekios == seekiosProductionDb.idseekiosProduction
                                 select s).Take(1).FirstOrDefault();
                if (seekiosDb != null)
                {
                    // Update seekios informations
                    seekiosDb.batteryLife = batteryLife;
                    seekiosDb.signalQuality = signalQuality;
                    seekiosDb.dateLastCommunication = /*timestamp > 0 ? UnixTimeStampToDateTime(timestamp) : */DateTime.UtcNow;
                    seekiosEntities.SaveChanges();
                }
                // Get the version embedded
                var versionDb = GetVersionEmbedded(seekiosEntities, OSVersion, -5);
                // If actual seekios version is version sended by seekios
                seekiosProductionDb.lastUpdateConfirmed = (byte)(seekiosProductionDb.versionEmbedded_idversionEmbedded == versionDb.idVersionEmbedded ? 1 : 0);
                seekiosEntities.SaveChanges();
            }
            return 1;
        }

        #endregion

        #region Optional Parameter (Version <= 1.006)

        public int NotifySeekiosOutOfZone(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp)
        {
            return NotifySeekiosOutOfZone2(uidSeekios
                , battery
                , signal
                , latitude
                , longitude
                , altitude
                , accuracy
                , timestamp
                , "0");
        }

        public int NotifySeekiosMoved(string uidSeekios, string battery, string signal, string timestamp)
        {
            return NotifySeekiosMoved2(uidSeekios
                , battery
                , signal
                , timestamp
                , "0");
        }

        public int AddNewTrackingLocation(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp)
        {
            return AddNewTrackingLocation2(uidSeekios
                , battery
                , signal
                , latitude
                , longitude
                , altitude
                , accuracy
                , timestamp
                , "0");
        }

        public int AddNewZoneTrackingLocation(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp)
        {
            return AddNewZoneTrackingLocation2(uidSeekios
                , battery
                , signal
                , latitude
                , longitude
                , altitude
                , accuracy
                , timestamp
                , "0");
        }

        public int AddNewDontMoveTrackingLocation(string uidSeekios, string battery, string signal, string latitude, string longitude, string altitude, string accuracy, string timestamp)
        {
            return AddNewDontMoveTrackingLocation2(uidSeekios
                , battery
                , signal
                , latitude
                , longitude
                , altitude
                , accuracy
                , timestamp
                , "0");
        }

        #endregion

        #endregion

        #region ----- PRIVATES METHODS ----------------------------------------------------- (no telemetry) ---

        /// <summary>
        /// NOTA : .net >= 4.6, there are native methods which do that... 
        /// ex: DateTimeOffset.FromUnixTimeMilliseconds and dateTimeOffset.ToUnixTimeMilliseconds()
        /// </summary>
        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        /// <summary>
        /// Add a new seekios communication in database
        /// </summary>
        private void AddSeekiosCommunication(seekios_dbEntities seekiosEntities
            , seekios seekiosDb
            , int batteryLife
            , int signalQuality
            , DateTime sendDate)
        {
            seekiosDb.batteryLife = batteryLife;
            seekiosDb.signalQuality = signalQuality;
            seekiosDb.dateLastCommunication = sendDate;
            seekiosDb.isRefreshingBattery = 0;
            seekiosEntities.SaveChanges();
            var communication = new seekiosCommunication()
            {
                receptionDate = DateTime.UtcNow,
                sendDate = sendDate,
                seekiosBatteryLife = batteryLife,
                seekiosSignalQuality = signalQuality,
                seekios_idseekios = seekiosDb.idseekios
            };
            seekiosEntities.seekiosCommunication.Add(communication);
            seekiosEntities.SaveChanges();
        }

        /// <summary>
        /// Add a seekios location in the database and update seekios location
        /// </summary>
        private int AddSeekiosLocation(seekios_dbEntities seekiosEntities
            , int idUser
            , seekios seekiosDb
            , double latitude
            , double longitude
            , double altitude
            , double accuracy
            , DateTime sendDate
            , LocationDefinition locationDefinition
            , bool needToPay = true)
        {
            // We handle the seekios delay response. too long ? we ignore it
            if (locationDefinition == LocationDefinition.OnDemand)
            {
                //TODO : Get the real date of SeekiosInstruction in SeekiosInstruction table
                var onDemandInstructionDate = (from si in seekiosEntities.seekiosInstruction
                                               where si.seekiosProduction_idseekiosProduction == seekiosDb.idseekios
                                               && si.instructionType == (int)InstructionType.OnDemand
                                               orderby si.idseekiosInstruction descending
                                               select si.dateCreation).Take(1).FirstOrDefault();
                // Get rid of the ondemand instruction
                seekiosEntities.DeleteSeekiosInstructionByIdSeekios(seekiosDb.idseekios, (int)InstructionType.OnDemand);
                if (onDemandInstructionDate == null || onDemandInstructionDate.Add(_onDemandResponseTimeout) < DateTime.UtcNow)
                {
                    return -1;
                }
                if (needToPay)
                {
                    if (CreditBillingHelper.PayOperationCost(seekiosEntities
                       , OperationType.RefreshPosition
                       , idUser
                       , null
                       , seekiosDb.idseekios) != 1)
                    {
                        return -1;
                    }
                }
            }
            // Update the seekios informations
            seekiosDb.lastKnownLocation_latitude = latitude;
            seekiosDb.lastKnownLocation_longitude = longitude;
            seekiosDb.lastKnownLocation_altitude = altitude;
            seekiosDb.lastKnownLocation_accuracy = accuracy;
            seekiosDb.lastKnownLocation_dateLocationCreation = sendDate;
            //seekiosEntities.SaveChanges();
            // Get the last mode of seekios
            var lastMode = (from m in seekiosEntities.mode
                            where m.seekios_idseekios == seekiosDb.idseekios
                            orderby m.idmode descending
                            select m).Take(1).FirstOrDefault();
            // Insert the location linked to a mode
            var location = new location
            {
                dateLocationCreation = sendDate,
                latitude = seekiosDb.lastKnownLocation_latitude,
                longitude = seekiosDb.lastKnownLocation_longitude,
                altitude = seekiosDb.lastKnownLocation_altitude,
                accuracy = seekiosDb.lastKnownLocation_accuracy,
                seekios_idseekios = seekiosDb.idseekios,
                mode_idmode = lastMode != null ? (int?)lastMode.idmode : null,
                locationDefinition_idlocationDefinition = (int)locationDefinition
            };
            seekiosEntities.location.Add(location);
            seekiosEntities.SaveChanges();
            return 1;
        }

        /// <summary>
        /// Returns #A201& if the seekios can send SOS alerts, returns #A200& otherwise
        /// </summary>
        private string GenerateAdminInstruction(seekios_dbEntities seekiosEntities, string uidSeekios)
        {
            return string.Format(ADMIN_TRAME, (CreditBillingHelper.SeekiosCanAffordAction(seekiosEntities, uidSeekios, -4) ? 1 : 0) + FOOTER_TRAME);
        }

        private seekios GetSeekiosById(seekios_dbEntities seekiosEntities, int idSeekios, int errorCode)
        {
            var seekiosDb = (from s in seekiosEntities.seekios
                             where s.idseekios == idSeekios
                             select s).Take(1).FirstOrDefault();
            if (seekiosDb == null) throw new WebFaultException<int>(errorCode, HttpStatusCode.Forbidden);
            return seekiosDb;
        }

        private seekiosProduction GetSeekiosProductionByUidSeekios(seekios_dbEntities seekiosEntities, string uidSeekios, int errorCode)
        {
            var seekiosProdDb = (from sp in seekiosEntities.seekiosProduction
                                 where sp.uidSeekios == uidSeekios
                                 select sp).Take(1).FirstOrDefault();
            if (seekiosProdDb == null) throw new WebFaultException<int>(errorCode, HttpStatusCode.Forbidden);
            return seekiosProdDb;
        }

        private mode GetModeByIdSeekiosAndIdMode(seekios_dbEntities seekiosEntities, int idSeekios, int idMode, int errorCode)
        {
            var modeDb = (from m in seekiosEntities.mode
                          where m.seekios_idseekios == idSeekios
                          && m.idmode == idMode
                          select m).FirstOrDefault();
            if (modeDb == null) throw new WebFaultException<int>(errorCode, HttpStatusCode.Forbidden);
            return modeDb;
        }

        private mode GetModeByIdSeekios(seekios_dbEntities seekiosEntities, int idSeekios, ModeDefinitions modeDefinition, int errorCode)
        {
            var modeDb = (from m in seekiosEntities.mode
                          where m.seekios_idseekios == idSeekios
                          && m.modeDefinition_idmodeDefinition == (int)modeDefinition
                          select m).FirstOrDefault();
            if (modeDb == null) throw new WebFaultException<int>(errorCode, HttpStatusCode.Forbidden);
            return modeDb;
        }

        private mode GetModeByIdSeekios(seekios_dbEntities seekiosEntities, int idSeekios, int errorCode)
        {
            var modeDb = (from m in seekiosEntities.mode
                          where m.seekios_idseekios == idSeekios
                          select m).FirstOrDefault();
            if (modeDb == null) throw new WebFaultException<int>(errorCode, HttpStatusCode.Forbidden);
            return modeDb;
        }

        private device GetDeviceByIdUser(seekios_dbEntities seekiosEntities, int idUser, int errorCode)
        {
            var deviceDb = (from d in seekiosEntities.device
                            where d.user_iduser == idUser
                            select d).Take(1).FirstOrDefault();
            if (deviceDb == null) throw new WebFaultException<int>(errorCode, HttpStatusCode.Forbidden);
            return deviceDb;
        }

        private versionEmbedded GetVersionEmbedded(seekios_dbEntities seekiosEntities, string version, int errorCode)
        {
            var versionDb = (from v in seekiosEntities.versionEmbedded
                             where v.versionName == version
                             select v).FirstOrDefault();
            if (versionDb == null) throw new WebFaultException<int>(errorCode, HttpStatusCode.Forbidden);
            return versionDb;
        }

        private bool IsDuplicateLocation(seekios_dbEntities seekiosEntities, DateTime createDate, int idSeekios)
        {
            var result = (from l in seekiosEntities.location
                          where l.dateLocationCreation == createDate && l.seekios_idseekios == idSeekios
                          select l).Take(1).FirstOrDefault();
            if (result != null)
            {
                SeekiosService.Telemetry.TrackEvent("Protection Againt Duplicated Location Activate");
                return true;
            }
            else return false;
        }

        #endregion
    }
}