using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCFServiceWebRole.Enum
{
    public enum AlertDefinition
    {
        NotificationPush = 1,
        SMS = 2,
        EMAIL = 3,
        VocalCall = 4,
        SOS = 5
    }
}