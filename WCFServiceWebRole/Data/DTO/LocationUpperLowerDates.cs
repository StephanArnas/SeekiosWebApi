using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DTO
{
    [DataContract]
    public class LocationUpperLowerDates
    {
        [DataMember]
        public DateTime? UppderDate { get; set; }
        [DataMember]
        public DateTime? LowerDate { get; set; }
    }
}