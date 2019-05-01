using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using WCFServiceWebRole.Data.DB;

namespace WCFServiceWebRole.Data.DTO
{
    public class UserEnvironment
    {
        public DBUser User { get; set; }
        public DBDevice Device { get; set; }
        public List<DBSeekios> LsSeekios { get; set; }
        public List<DBMode> LsMode { get; set; }
        public List<DBAlert> LsAlert { get; set; }
        public List<DBAlertRecipient> LsAlertRecipient { get; set; }
        public List<DBLocation> LsLocations { get; set; }
        public DateTime ServerSynchronisationDate { get; set; }
    }
}