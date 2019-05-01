using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    [DataContract]
    public class DBSeekios : DBSeekiosProduction
    {
        [DataMember]
        public int Idseekios { get; set; }
        [DataMember]
        public string PinCode { get; set; }
        [DataMember]
        public string SeekiosName { get; set; }
        [DataMember]
        public string SeekiosPicture { get; set; }
        [DataMember]
        public DateTime SeekiosDateCreation { get; set; }
        [DataMember]
        public int BatteryLife { get; set; }
        [DataMember]
        public int SignalQuality { get; set; }
        [DataMember]
        public DateTime? DateLastCommunication { get; set; }
        [DataMember]
        public double LastKnownLocation_longitude { get; set; }
        [DataMember]
        public double LastKnownLocation_latitude { get; set; }
        [DataMember]
        public double LastKnownLocation_altitude { get; set; }
        [DataMember]
        public double LastKnownLocation_accuracy { get; set; }
        [DataMember]
        public DateTime? LastKnownLocation_dateLocationCreation { get; set; }
        [DataMember]
        public int LastKnowLocation_idlocationDefinition { get; set; }
        [DataMember]
        public int User_iduser { get; set; }
        [DataMember]
        public bool HasGetLastInstruction { get; set; }
        [DataMember]
        public bool IsAlertLowBattery { get; set; }
        [DataMember]
        public bool IsInPowerSaving { get; set; }
        [DataMember]
        public int PowerSaving_hourStart { get; set; }
        [DataMember]
        public int PowerSaving_hourEnd { get; set; }
        [DataMember]
        public int? AlertSOS_idalert { get; set; }
        [DataMember]
        public bool IsRefreshingBattery { get; set; }
        [DataMember]
        public DateTime? DateLastOnDemandRequest { get; set; }
        [DataMember]
        public bool IsLastSOSRead { get; set; }
        [DataMember]
        public DateTime? DateLastSOSSent { get; set; }
        [DataMember]
        public bool SendNotificationOnNewTrackingLocation { get; set; }
        [DataMember]
        public bool SendNotificationOnNewOutOfZoneLocation { get; set; }
        [DataMember]
        public bool SendNotificationOnNewDontMoveLocation { get; set; }

        public static DBSeekios SeekiosToDBSeekios(seekios source1)
        {
            if (source1 == null) return null;
            return new DBSeekios()
            {
                Idseekios = source1.idseekios,
                SeekiosName = source1.seekiosName,
                SeekiosPicture = source1.seekiosPicture == null ? string.Empty : Convert.ToBase64String(source1.seekiosPicture),
                SeekiosDateCreation = source1.seekios_dateCretaion,
                BatteryLife = source1.batteryLife ?? 0,
                SignalQuality = source1.signalQuality ?? 0,
                DateLastCommunication = source1.dateLastCommunication,
                LastKnownLocation_longitude = source1.lastKnownLocation_longitude,
                LastKnownLocation_latitude = source1.lastKnownLocation_latitude,
                LastKnownLocation_altitude = source1.lastKnownLocation_altitude ?? 0.0,
                LastKnownLocation_accuracy = source1.lastKnownLocation_accuracy ?? 0.0,
                LastKnownLocation_dateLocationCreation = source1.lastKnownLocation_dateLocationCreation,
                LastKnowLocation_idlocationDefinition = source1.lastKnowLocation_idlocationDefinition,
                User_iduser = source1.user_iduser,
                HasGetLastInstruction = source1.hasGetLastInstruction == 1,
                IsAlertLowBattery = source1.isAlertLowBattery == 1,
                IsInPowerSaving = source1.isInPowerSaving == 1,
                PowerSaving_hourStart = source1.powerSaving_hourStart,
                PowerSaving_hourEnd = source1.powerSaving_hourEnd,
                AlertSOS_idalert = source1.alertSOS_idalert,
                IsRefreshingBattery = source1.isRefreshingBattery == 1,
                DateLastOnDemandRequest = source1.dateLastOnDemandRequest,
                IsLastSOSRead = source1.isLastSOSRead == 1,
                DateLastSOSSent = source1.dateLastSOSSent,
                SendNotificationOnNewTrackingLocation = source1.sendNotificationOnNewTrackingLocation == 1,
                SendNotificationOnNewOutOfZoneLocation = source1.sendNotificationOnNewOutOfZoneLocation == 1,
                SendNotificationOnNewDontMoveLocation = source1.sendNotificationOnNewDontMoveLocation == 1
            };
        }

        public static DBSeekios SeekiosToDBSeekios(seekios source1, seekiosAndSeekiosProduction source2)
        {
            if (source1 == null) return null;
            if (source2 == null) return null;
            return new DBSeekios()
            {
                UIdSeekios = source2.uidSeekios,
                Imei = source2.imei,
                LastUpdateConfirmed = source2.lastUpdateConfirmed == 1,
                VersionEmbedded_idversionEmbedded = source2.versionEmbedded_idversionEmbedded,
                DateFirstRegistration = source2.dateFirstRegistration,
                FreeCredit = source2.freeCredit,

                Idseekios = source1.idseekios,
                SeekiosName = source1.seekiosName,
                SeekiosPicture = source1.seekiosPicture == null ? string.Empty : Convert.ToBase64String(source1.seekiosPicture),
                SeekiosDateCreation = source1.seekios_dateCretaion,
                BatteryLife = source1.batteryLife ?? 0,
                SignalQuality = source1.signalQuality ?? 0,
                DateLastCommunication = source1.dateLastCommunication,
                LastKnownLocation_longitude = source1.lastKnownLocation_longitude,
                LastKnownLocation_latitude = source1.lastKnownLocation_latitude,
                LastKnownLocation_altitude = source1.lastKnownLocation_altitude ?? 0.0,
                LastKnownLocation_accuracy = source1.lastKnownLocation_accuracy ?? 0.0,
                LastKnownLocation_dateLocationCreation = source1.lastKnownLocation_dateLocationCreation,
                LastKnowLocation_idlocationDefinition = source1.lastKnowLocation_idlocationDefinition,
                User_iduser = source1.user_iduser,
                HasGetLastInstruction = source1.hasGetLastInstruction == 1,
                IsAlertLowBattery = source1.isAlertLowBattery == 1,
                IsInPowerSaving = source1.isInPowerSaving == 1,
                PowerSaving_hourStart = source1.powerSaving_hourStart,
                PowerSaving_hourEnd = source1.powerSaving_hourEnd,
                AlertSOS_idalert = source1.alertSOS_idalert,
                IsRefreshingBattery = source1.isRefreshingBattery == 1,
                DateLastOnDemandRequest = source1.dateLastOnDemandRequest,
                IsLastSOSRead = source1.isLastSOSRead == 1,
                DateLastSOSSent = source1.dateLastSOSSent,
                SendNotificationOnNewTrackingLocation = source1.sendNotificationOnNewTrackingLocation == 1,
                SendNotificationOnNewOutOfZoneLocation = source1.sendNotificationOnNewOutOfZoneLocation == 1,
                SendNotificationOnNewDontMoveLocation = source1.sendNotificationOnNewDontMoveLocation == 1
            };
        }

        public static DBSeekios SeekiosAndSeekiosProductionToDBSeekios(seekiosAndSeekiosProduction source)
        {
            if (source == null) return null;
            if (!source.idseekios.HasValue) throw new Exception("SeekiosAndSeekiosProductionToDBSeekios : idseekios is require");
            return new DBSeekios()
            {
                UIdSeekios = source.uidSeekios,
                Imei = source.imei,
                LastUpdateConfirmed = source.lastUpdateConfirmed == 1,
                VersionEmbedded_idversionEmbedded = source.versionEmbedded_idversionEmbedded,
                DateFirstRegistration = source.dateFirstRegistration,
                FreeCredit = source.freeCredit,

                Idseekios = source.idseekios.Value,
                SeekiosName = source.seekiosName,
                SeekiosPicture = source.seekiosPicture == null ? string.Empty : Convert.ToBase64String(source.seekiosPicture),
                SeekiosDateCreation = source.seekios_dateCretaion.Value,
                BatteryLife = source.batteryLife ?? 0,
                SignalQuality = source.signalQuality ?? 0,
                DateLastCommunication = source.dateLastCommunication,
                LastKnownLocation_longitude = source.lastKnownLocation_longitude.Value,
                LastKnownLocation_latitude = source.lastKnownLocation_latitude.Value,
                LastKnownLocation_altitude = source.lastKnownLocation_altitude ?? 0.0,
                LastKnownLocation_accuracy = source.lastKnownLocation_accuracy ?? 0.0,
                LastKnownLocation_dateLocationCreation = source.lastKnownLocation_dateLocationCreation,
                User_iduser = source.user_iduser.Value,
                HasGetLastInstruction = source.hasGetLastInstruction == 1,
                IsAlertLowBattery = source.isAlertLowBattery == 1,
                IsInPowerSaving = source.isInPowerSaving == 1,
                PowerSaving_hourStart = source.powerSaving_hourStart ?? 0,
                PowerSaving_hourEnd = source.powerSaving_hourEnd ?? 0,
                AlertSOS_idalert = source.alertSOS_idalert,
                IsRefreshingBattery = source.isRefreshingBattery == 1,
                DateLastOnDemandRequest = source.dateLastOnDemandRequest,
                IsLastSOSRead = source.isLastSOSRead == 1,
                DateLastSOSSent = source.dateLastSOSSent,
                SendNotificationOnNewTrackingLocation = source.sendNotificationOnNewTrackingLocation == 1,
                SendNotificationOnNewOutOfZoneLocation = source.sendNotificationOnNewOutOfZoneLocation == 1,
                SendNotificationOnNewDontMoveLocation = source.sendNotificationOnNewDontMoveLocation == 1
            };
        }
    }
}