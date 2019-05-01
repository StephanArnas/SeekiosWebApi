using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using WCFServiceWebRole.Data.DB;

namespace WCFServiceWebRole.Data.DTO
{
    [DataContract]
    public class UserEnvironment
    {
        [DataMember]
        public DBUser User { get; set; }

        [DataMember]
        public DBDevice Device { get; set; }

        [DataMember]
        public IEnumerable<DBSeekios> LsSeekios { get; set; }

        [DataMember]
        public IEnumerable<DBMode> LsMode { get; set; }

        [DataMember]
        public IEnumerable<DBAlert> LsAlert { get; set; }

        [DataMember]
        public IEnumerable<DBAlertRecipient> LsAlertRecipient { get; set; }

        [DataMember]
        public IEnumerable<DBLocation> LsLocations { get; set; }

        [DataMember]
        public DateTime ServerSynchronisationDate { get; set; }

        [DataMember]
        public DBVersionEmbedded LastVersionEmbedded { get; set; }
    }
}