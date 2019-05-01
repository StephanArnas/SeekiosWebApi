using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    public class DBSeekios : DBSeekiosProduction
    {
        public int Idseekios { get; set; }
        public string PinCode { get; set; }
        public string SeekiosName { get; set; }
        public string SeekiosPicture { get; set; }
        public DateTime SeekiosDateCreation { get; set; }
        public int BatteryLife { get; set; }
        public int SignalQuality { get; set; }
        public DateTime? DateLastCommunication { get; set; }
        public double LastKnownLocation_longitude { get; set; }
        public double LastKnownLocation_latitude { get; set; }
        public double LastKnownLocation_altitude { get; set; }
        public double LastKnownLocation_accuracy { get; set; }
        public DateTime? LastKnownLocation_dateLocationCreation { get; set; }
        public int LastKnowLocation_idlocationDefinition { get; set; }
        public int User_iduser { get; set; }
        public bool HasGetLastInstruction { get; set; }
        public bool IsAlertLowBattery { get; set; }
        public bool IsInPowerSaving { get; set; }
        public int PowerSaving_hourStart { get; set; }
        public int PowerSaving_hourEnd { get; set; }
        public int? AlertSOS_idalert { get; set; }
        public bool IsRefreshingBattery { get; set; }
        public DateTime? DateLastOnDemandRequest { get; set; }
        public bool IsLastSOSRead { get; set; }
        public DateTime? DateLastSOSSent { get; set; }
    }
}