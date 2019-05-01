using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    [DataContract]
    public class DBSeekiosProduction
    {
        [DataMember]
        public string UIdSeekios { get; set; }
        [DataMember]
        public string Imei { get; set; }
        [DataMember]
        public bool LastUpdateConfirmed { get; set; }
        [DataMember]
        public int VersionEmbedded_idversionEmbedded { get; set; }
        [DataMember]
        public int FreeCredit { get; set; }
        [DataMember]
        public DateTime? DateFirstRegistration { get; set; }
    }
}