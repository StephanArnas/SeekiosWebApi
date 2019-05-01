using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DTO
{
    [DataContract]
    public class PurchaseDTO
    {
        [DataMember]
        public int IdUser { get; set; }
        [DataMember]
        public string InnerData { get; set; }
        [DataMember]
        public int StoreId { get; set; }
        [DataMember]
        public String Signature { get; set; }
        [DataMember]
        public String VersionApp { get; set; }
        [DataMember]
        public string KeyProduct { get; set; }
    }
}