using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    [DataContract]
    public class DBMode
    {
        [DataMember]
        public int Idmode { get; set; }
        [DataMember]
        public DateTime DateModeCreation { get; set; }
        [DataMember]
        public DateTime? DateModeActivation { get; set; }
        [DataMember]
        public string Trame { get; set; }
        [DataMember]
        public int CountOfTriggeredAlert { get; set; }
        [DataMember]
        public DateTime? LastTriggeredAlertDate { get; set; }
        [DataMember]
        public int Seekios_idseekios { get; set; }
        [DataMember]
        public int ModeDefinition_idmodeDefinition { get; set; }
        [DataMember]
        public int StatusDefinition_idstatusDefinition { get; set; }
        [DataMember]
        public int Device_iddevice { get; set; }
        [DataMember]
        public bool IsPowerSavingEnabled { get; set; }
        [DataMember]
        public int TimeRefreshTracking { get; set; }
        [DataMember]
        public int TimeDiffHours { get; set; }
        [DataMember]
        public int TimeActivation { get; set; }
        [DataMember]
        public int TimeDesactivation { get; set; }
        [DataMember]
        public int MaxLocation { get; set; }

        public static DBMode ModeToDBMode(mode source)
        {
            if (source == null) return null;
            return new DBMode()
            {
                Idmode = source.idmode,
                DateModeCreation = source.dateModeCreation ?? DateTime.MinValue,
                DateModeActivation = source.dateModeActivation,
                Trame = source.trame,
                CountOfTriggeredAlert = source.countOfTriggeredAlert ?? 0,
                LastTriggeredAlertDate = source.lastTriggeredAlertDate,
                Seekios_idseekios = source.seekios_idseekios,
                ModeDefinition_idmodeDefinition = source.modeDefinition_idmodeDefinition,
                StatusDefinition_idstatusDefinition = source.statusDefinition_idstatusDefinition,
                Device_iddevice = source.device_iddevice,
                IsPowerSavingEnabled = source.isPowerSavingEnabled,
                TimeRefreshTracking = source.timeRefreshTracking,
                TimeDiffHours = source.timeDiffHours,
                TimeActivation = source.timeActivation,
                TimeDesactivation = source.timeDesactivation,
                MaxLocation = source.maxLocation
            };
        }
    }
}