using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DTO
{
    /// <summary>
    /// This class is only used inside PurchaseDTO.InnerData for Play Store data.
    /// We cannot change the json data, so we have to follow Google's naming conventions and not our own
    /// </summary>
    public class GoogleStorePurchaseDTO
    {
        public string packageName;
        public string orderId;
        public string productId;
        public string purchaseTime;
        public string purchaseState;
        public string developerPayload;
    }
}