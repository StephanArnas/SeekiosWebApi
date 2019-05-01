using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DTO
{
    public class LocationUpperLowerDates
    {
        public DateTime? UppderDate { get; set; }
        public DateTime? LowerDate { get; set; }
    }
}