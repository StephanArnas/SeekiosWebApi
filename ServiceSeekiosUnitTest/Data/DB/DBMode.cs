using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    public class DBMode
    {
        public int Idmode { get; set; }
        public DateTime DateModeCreation { get; set; }
        public string Trame { get; set; }
        public int NotificationPush { get; set; }
        public int CountOfTriggeredAlert { get; set; }
        public DateTime? LastTriggeredAlertDate { get; set; }
        public int Seekios_idseekios { get; set; }
        public int ModeDefinition_idmodeDefinition { get; set; }
        public int StatusDefinition_idstatusDefinition { get; set; }
        public int Device_iddevice { get; set; }
    }
}