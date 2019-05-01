using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    [DataContract]
    public class DBAlertRecipient
    {
        [DataMember]
        public int IdalertRecipient { get; set; }
        [DataMember]
        public string NameRecipient { get; set; }
        [DataMember]
        public int Alert_idalert { get; set; }
        [DataMember]
        public string PhoneNumber { get; set; }
        [DataMember]
        public string PhoneNumberType { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string EmailType { get; set; }

        public static DBAlertRecipient AlertRecipientToDbAlertRecipient(alertRecipient source)
        {
            if (source == null) return null;
            return new DBAlertRecipient()
            {
                IdalertRecipient = source.idalertRecipient,
                Alert_idalert = source.alert_idalert,
                NameRecipient = source.nameRecipient,
                PhoneNumber = source.phoneNumber,
                PhoneNumberType = source.phoneNumberType,
                Email = source.email,
                EmailType = source.emailType
            };
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var toCompare = obj as DBAlertRecipient;
            if (toCompare == null)
                return false;

            return CompareFields(this, toCompare);
        }

        public bool Equals(DBAlertRecipient toCompare)
        {
            if (toCompare == null)
                return false;

            return CompareFields(this, toCompare);
        }

        public static bool operator ==(DBAlertRecipient a, DBAlertRecipient b)
        {
            return CompareFields(a, b);
        }

        public static bool operator !=(DBAlertRecipient a, DBAlertRecipient b)
        {
            return !CompareFields(a, b);
        }

        private static bool CompareFields(DBAlertRecipient a, DBAlertRecipient b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return a.IdalertRecipient == b.IdalertRecipient &&
              a.NameRecipient == b.NameRecipient &&
              a.Alert_idalert == b.Alert_idalert &&
              a.PhoneNumber == b.PhoneNumber &&
              a.PhoneNumberType == b.PhoneNumberType &&
              a.Email == b.Email &&
              a.EmailType == b.EmailType;
        }
    }
}