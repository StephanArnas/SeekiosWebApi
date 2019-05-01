using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCFServiceWebRole.Enum
{
    public enum OperationType
    {
        ConfigureMode = 1,
        RefreshPosition = 2,
        AddTrackingPosition = 3,
        AddZoneTrackingPosition = 4,
        AddDontMoveTrackingPosition = 5,
        SeekiosMonthlyGift = 6,
        SendSOS = 7,
        RefreshBattery = 8,
        BoughtCredits1 = 9,
        BoughtCredits2 = 10,
        BoughtCredits3 = 11,
        BoughtCredits4 = 12,
        BoughtCredits5 = 13,
        BoughtCredits1Subscription = 14,
        BoughtCredits2Subscription = 15,
        BoughtCredits3Subscription = 16,
        BoughtCredits4Subscription = 17,
        BoughtCredits5Subscription = 18,
    }

    public static class OperationTypeExtensions
    {
        public static int GetAmount(this OperationType operationType)
        {
            switch (operationType)
            {
                case OperationType.ConfigureMode:
                    return -2;
                case OperationType.RefreshPosition:
                    return -1;
                case OperationType.AddTrackingPosition:
                    return -1;
                case OperationType.AddZoneTrackingPosition:
                    return -1;
                case OperationType.AddDontMoveTrackingPosition:
                    return -1;
                case OperationType.SeekiosMonthlyGift:
                    return 60;
                case OperationType.SendSOS:
                    return -1;
                case OperationType.RefreshBattery:
                    return -1;
                case OperationType.BoughtCredits1:
                    return 30;
                case OperationType.BoughtCredits2:
                    return 100;
                case OperationType.BoughtCredits3:
                    return 300;
                case OperationType.BoughtCredits4:
                    return 1000;
                case OperationType.BoughtCredits5:
                    return 2000;
                case OperationType.BoughtCredits1Subscription:
                    return 33;
                case OperationType.BoughtCredits2Subscription:
                    return 110;
                case OperationType.BoughtCredits3Subscription:
                    return 330;
                case OperationType.BoughtCredits4Subscription:
                    return 1100;
                case OperationType.BoughtCredits5Subscription:
                    return 2200;
                default:
                    return 0;
            }
        }
    }
}