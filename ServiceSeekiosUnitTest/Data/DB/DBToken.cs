using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    public class DBToken
    {
        public string AuthToken { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime DateExpires { get; set; }
    }
}