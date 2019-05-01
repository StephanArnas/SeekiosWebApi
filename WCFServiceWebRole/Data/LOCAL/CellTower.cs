using Newtonsoft.Json;

namespace WCFServiceWebRole.Data.LOCAL
{
    public class CellTower
    {
        [JsonProperty(PropertyName = "cellId")]
        public long CellId { get; set; }
        [JsonProperty(PropertyName = "locationAreaCode")]
        public int LocationAreaCode { get; set; }
        [JsonProperty(PropertyName = "mobileCountryCode")]
        public int MobileCountryCode { get; set; }
        [JsonProperty(PropertyName = "mobileNetworkCode")]
        public int MobileNetworkCode { get; set; }
    }
}