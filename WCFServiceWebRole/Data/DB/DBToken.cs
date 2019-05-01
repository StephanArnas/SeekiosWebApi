using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    [DataContract]
    public class DBToken
    {
        [DataMember]
        public string AuthToken { get; set; }
        [DataMember]
        public DateTime DateCreation { get; set; }
        [DataMember]
        public DateTime DateExpires { get; set; }

        public static DBToken TokenToDBToken(token source)
        {
            if (source == null) return null;
            var toReturn = new DBToken()
            {
                AuthToken = source.authToken,
                DateCreation = source.dateCreationToken,
                DateExpires = source.dateExpiresToken
            };
            return toReturn;
        }
    }
}