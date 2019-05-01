using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    public class DBSeekiosProduction
    {
        public string UIdSeekios { get; set; }
        public string Imei { get; set; }
        public bool LastUpdateConfirmed { get; set; }
        public int VersionEmbedded_idversionEmbedded { get; set; }
        public int FreeCredit { get; set; }
        public DateTime? DateFirstRegistration { get; set; }
    }
}