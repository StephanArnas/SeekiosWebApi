using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    [DataContract]
    public class DBAlert
    {
        [DataMember]
        public int Idalert { get; set; }
        [DataMember]
        public string Content { get; set; }
        [DataMember]
        public int? Mode_idmode { get; set; }
        [DataMember]
        public int AlertDefinition_idalertType { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public DateTime CreationDate { get; set; }

        public static DBAlert AlertToDbAlert(alert source)
        {
            if (source == null) return null;
            return new DBAlert()
            {
                Idalert = source.idalert,
                Content = source.content,
                Mode_idmode = source.mode_idmode,
                AlertDefinition_idalertType = source.alertDefinition_idalertType,
                Title = source.title,
                CreationDate = source.dateAlertCreation
            };
        }
    }
}