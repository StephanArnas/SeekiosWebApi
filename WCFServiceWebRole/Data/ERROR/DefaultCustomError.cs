using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.ERROR
{
    [DataContract]
    public class DefaultCustomError
    {
        public DefaultCustomError(string errorCode, string errorInfo, string errorDetails)
        {
            ErrorCode = errorCode;
            ErrorInfo = errorInfo;
            ErrorDetails = errorDetails;
        }
        [DataMember]
        public string ErrorCode { get; private set; }
        [DataMember]
        public string ErrorInfo { get; private set; }
        [DataMember]
        public string ErrorDetails { get; private set; }
    }
}