using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DTO
{
    public class PurchaseDTO
    {
        public int IdUser { get; set; }
        public string InnerData { get; set; }
        public int StoreId { get; set; }
        public String Signature { get; set; }
        public String VersionApp { get; set; }
        public string KeyProduct { get; set; }
    }
}