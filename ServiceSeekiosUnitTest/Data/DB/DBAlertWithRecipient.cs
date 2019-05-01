using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    public class BDAlertWithRecipient : DBAlert
    {
        public BDAlertWithRecipient()
        {
            LsRecipients = new List<DBAlertRecipient>();
        }

        public List<DBAlertRecipient> LsRecipients { get; set; }
    }
}