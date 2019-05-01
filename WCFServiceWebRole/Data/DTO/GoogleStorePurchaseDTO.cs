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
    [DataContract]
    public class GoogleStorePurchaseDTO
    {
        [DataMember]
        public string packageName;
        [DataMember]
        public string orderId;
        [DataMember]
        public string productId;
        [DataMember]
        public string purchaseTime;
        [DataMember]
        public string purchaseState;
        [DataMember]
        public string developerPayload;
    }
}