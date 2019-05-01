using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    public class DBUser
    {
        public int Iduser { get; set; }
        public int IdCountryResource { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int RemainingRequest { get; set; }
        public string UserPicture { get; set; }
        public bool IsValidate { get; set; }
        public DateTime? DateLastConnection { get; set; }

        public DBUser() { }
    }
}