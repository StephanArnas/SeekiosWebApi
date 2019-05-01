using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    [DataContract]
    public class DBVersionEmbedded
    {
        [DataMember]
        private int IdVersionEmbedded { get; set; }

        [DataMember]
        private string VersionName { get; set; }

        [DataMember]
        private DateTime DateVersionCreation { get; set; }

        [DataMember]
        private string ReleaseNotes { get; set; }

        [DataMember]
        private bool IsBetaVersion { get; set; }

        [DataMember]
        private string SHA1Hash { get; set; }

        public static DBVersionEmbedded VersionEmbeddedToDBVersionEmbedded(versionEmbedded source)
        {
            if (source == null) return null;
            return new DBVersionEmbedded()
            {
                IdVersionEmbedded = source.idVersionEmbedded,
                VersionName = source.versionName,
                DateVersionCreation = source.dateVersionCreation,
                ReleaseNotes = source.releaseNotes,
                SHA1Hash = source.SHA1Hash,
                IsBetaVersion = source.isBetaVersion
            };
        }
    }
}