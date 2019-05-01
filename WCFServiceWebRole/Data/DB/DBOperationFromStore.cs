using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DTO
{
    [DataContract]
    public class DBOperationFromStore
    {
        [DataMember]
        public int IdOperationFromStore { get; set; }
        [DataMember]
        public int IdPack { get; set; }
        [DataMember]
        public int IdUser { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public DateTime DateTransaction { get; set; }
        [DataMember]
        public string RefStore { get; set; }
        [DataMember]
        public string VersionApp { get; set; }
        [DataMember]
        public int CreditsPurchased { get; set; }
        [DataMember]
        public bool IsPackPremium { get; set; }
    }
}