using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.ERROR
{
    public class DefaultCustomError
    {
        public string ErrorCode { get; set; }
        public string ErrorInfo { get; set; }
        public string ErrorDetails { get; set; }
    }
}