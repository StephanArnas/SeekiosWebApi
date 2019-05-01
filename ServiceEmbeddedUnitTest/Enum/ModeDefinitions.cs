using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WCFServiceWebRole.Enum
{
    public enum ModeDefinitions
    {
        ModeWaiting = 1,           //1 	Mode Attente
        //ModePosition = 2,          //2 	Mode Position
        ModeTracking = 3,          //3 	Mode Tracking
        ModeDontMove = 4,          //4 	Mode Don't Move
        ModeZone = 5,              //5 	Mode Zone
        //ModeInTime = 6,            //6 	Mode In Time
        //ModeFollowMe = 7,          //7 	Mode Follow Me
        //ModeKeepItClose = 8,       //8 	Mode Keep It Close
        //ModeDailyTrack = 9,        //9 	Mode Daily Track
        //ModeActivity = 10,         //10	Mode Activity
        //ModeRoad = 11,             //11	Mode Road
        //ModeSOS = 12               //12	SOS
    }
}