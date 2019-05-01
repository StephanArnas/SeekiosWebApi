using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCFServiceWebRole.Enum
{
    public enum InstructionType
    {
        Unknown = 0,
        OnDemand = 1,
        ChangeMode = 2,
        ChangePowerSavingConfig = 4,
        SendBatteryLevel = 5,
        SendSOS = 6
    }
}