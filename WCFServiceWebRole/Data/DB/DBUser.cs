using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    [DataContract]
    public class DBUser
    {
        [DataMember]
        public int Iduser { get; set; }
        [DataMember]
        public int IdCountryResource { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public int RemainingRequest { get; set; }
        [DataMember]
        public string UserPicture { get; set; }
        [DataMember]
        public bool IsValidate { get; set; }
        [DataMember]
        public DateTime? DateLastConnection { get; set; }

        public DBUser() { }

        public static DBUser UserToDBUser(user source)
        {
            if (source == null) return null;
            var toReturn = new DBUser()
            {
                Iduser = source.iduser,
                IdCountryResource = source.countryResources_idcountryResources,
                Email = source.email,
                Password = source.password,
                LastName = source.lastName,
                FirstName = source.firstName,
                RemainingRequest = source.remainingRequest ?? 0,
                UserPicture = source.userPicture == null ? string.Empty : Convert.ToBase64String(source.userPicture),
                IsValidate = source.isValidate ?? false,
                DateLastConnection = source.dateLastConnection,
            };
            return toReturn;
        }
    }
}