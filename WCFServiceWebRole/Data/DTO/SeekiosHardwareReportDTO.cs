using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DTO
{
    [DataContract]
    public class SeekiosHardwareReportDTO : IEquatable<SeekiosHardwareReportDTO>
    {
        [DataMember]
        public string IMEI { get; set; }
        [DataMember]
        public int? SeekiosProdID { get; set; }
        [DataMember]
        public bool? GSMUSART { get; set; }
        [DataMember]
        public bool? GSM_GPRS { get; set; }
        [DataMember]
        public bool? GPSUSART { get; set; }
        [DataMember]
        public bool? GSPFrames { get; set; }
        [DataMember]
        public bool? GPSPosit { get; set; }
        [DataMember]
        public bool? IMUgetaccel { get; set; }
        [DataMember]
        public bool? BLEConfig { get; set; }
        [DataMember]
        public bool? IMUInterrupt { get; set; }
        [DataMember]
        public bool? BLEAdvertizing { get; set; }
        [DataMember]
        public bool? BLEConnection { get; set; }
        [DataMember]
        public bool? DataFlashRead { get; set; }
        [DataMember]
        public bool? DataFlashWrite { get; set; }
        [DataMember]
        public bool? LEDs { get; set; }
        [DataMember]
        public bool? Bouton { get; set; }
        [DataMember]
        public bool? CalendarTime { get; set; }
        [DataMember]
        public bool? CalendarInterrupt { get; set; }
        [DataMember]
        public int? DateDuTest { get; set; }
        [DataMember]
        public int SeekiosHardwareReportID { get; set; }
        [DataMember]
        public int? CalendarTimestamp { get; set; }
        [DataMember]
        public int? BatteryLife { get; set; }
        [DataMember]
        public bool? AdcUSB { get; set; }
        [DataMember]
        public bool? AdcBattery { get; set; }

        public static SeekiosHardwareReportDTO SeekiosHardwareReportTOSeekiosHardwareReportDTO(seekiosHardwareReport source)
        {
            if (source == null) return null;
            return new SeekiosHardwareReportDTO()
            {
                SeekiosProdID = source.SeekiosProdID,
                GSMUSART = source.GSMUSART,
                AdcBattery = source.AdcBattery,
                AdcUSB = source.AdcUSB,
                BatteryLife = source.BatteryLife,
                BLEAdvertizing = source.BLEAdvertizing,
                BLEConfig = source.BLEConfig,
                BLEConnection = source.BLEConnection,
                Bouton = source.Bouton,
                CalendarInterrupt = source.CalendarInterrupt,
                CalendarTime = source.CalendarTime,
                CalendarTimestamp = source.CalendarTimestamp,
                DataFlashRead = source.DataFlashRead,
                DataFlashWrite = source.DataFlashWrite,
                DateDuTest = source.DateDuTest,
                GPSPosit = source.GPSPosit,
                GPSUSART = source.GPSUSART,
                GSM_GPRS = source.GSM_GPRS,
                GSPFrames = source.GSPFrames,
                IMUgetaccel = source.IMUgetaccel,
                IMUInterrupt = source.IMUInterrupt,
                LEDs = source.LEDs,
                SeekiosHardwareReportID = source.SeekiosHardwareReportID
            };
        }

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