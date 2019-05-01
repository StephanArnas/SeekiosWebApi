using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    [DataContract]
    public class DBDevice
    {
        [DataMember]
        public int Iddevice { get; set; }
        [DataMember]
        public string UidDevice { get; set; }
        [DataMember]
        public string DeviceName { get; set; }
        [DataMember]
        public string Os { get; set; }
        [DataMember]
        public string Plateform { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string NotificationPlayerId { get; set; }
        [DataMember]
        public string MacAdress { get; set; }
        [DataMember]
        public DateTime LastUseDate { get; set; }

        public static DBDevice DeviceToDBDevice(device source)
        {
            if (source == null) return null;
            var toReturn = new DBDevice()
            {
                Iddevice = source.iddevice,
                UidDevice = source.uidDevice,
                DeviceName = source.deviceName,
                Os = source.os,
                Plateform = source.plateform,
                NotificationPlayerId = source.notificationPlayerId,
                MacAdress = source.macAdress,
                LastUseDate = source.lastUseDate,
            };
            return toReturn;
        }
    }
}