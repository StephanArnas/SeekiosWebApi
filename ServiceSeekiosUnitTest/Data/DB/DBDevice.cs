using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    public class DBDevice
    {
        public int Iddevice { get; set; }
        public string UidDevice { get; set; }
        public string DeviceName { get; set; }
        public string Os { get; set; }
        public string Plateform { get; set; }
        public string Password { get; set; }
        public string NotificationPlayerId { get; set; }
        public string MacAdress { get; set; }
        public DateTime LastUseDate { get; set; }
    }
}