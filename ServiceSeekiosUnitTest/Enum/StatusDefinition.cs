using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCFServiceWebRole.Enum
{
    public enum StatusDefinition
    {
        RAS = 1,
        SeekiosOutOfZone = 2,
        SeekiosMoved = 3,
        BleConnectionLost = 4
    }
}