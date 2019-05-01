using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    public class DBVersionEmbedded
    {
        private string VersionName { get; set; }
        private DateTime DateVersionCreation { get; set; }
        private string ReleaseNotes { get; set; }
        private string SHA1Hash { get; set; }
    }
}