using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DTO
{
    public class SeekiosHardwareReportDTO : IEquatable<SeekiosHardwareReportDTO>
    {
        public string IMEI { get; set; }
        public int? SeekiosProdID { get; set; }
        public bool? GSMUSART { get; set; }
        public bool? GSM_GPRS { get; set; }
        public bool? GPSUSART { get; set; }
        public bool? GSPFrames { get; set; }
        public bool? GPSPosit { get; set; }
        public bool? IMUgetaccel { get; set; }
        public bool? BLEConfig { get; set; }
        public bool? IMUInterrupt { get; set; }
        public bool? BLEAdvertizing { get; set; }
        public bool? BLEConnection { get; set; }
        public bool? DataFlashRead { get; set; }
        public bool? DataFlashWrite { get; set; }
        public bool? LEDs { get; set; }
        public bool? Bouton { get; set; }
        public bool? CalendarTime { get; set; }
        public bool? CalendarInterrupt { get; set; }
        public int? DateDuTest { get; set; }
        public int SeekiosHardwareReportID { get; set; }
        public int? CalendarTimestamp { get; set; }
        public int? BatteryLife { get; set; }
        public bool? AdcUSB { get; set; }
        public bool? AdcBattery { get; set; }

        public bool Equals(SeekiosHardwareReportDTO other)
        {
            if (other == null) return false;
            else return this.SeekiosProdID.Equals(other.SeekiosProdID);
        }

        public override int GetHashCode()
        {
            return this.SeekiosProdID.GetHashCode();
        }
    }
}