using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCFServiceWebRole.Enum
{
    public enum NotificationType
    {
        InstructionTaken,
        RefreshPosition,
        RefreshPositionByCellsData,
        NotifySeekiosOutOfZone,
        AddTrackingLocation,
        AddNewZoneTrackingLocation,
        AddNewDontMoveTrackingLocation,
        NotifySeekiosMoved,
        ChangeMode,
        SOSSent,
        SOSLocationSent,
        SOSLocationByCellsDataSent,
        RefreshCredits,
        CriticalBattery,
        PowerSavingDisabled
    }
}