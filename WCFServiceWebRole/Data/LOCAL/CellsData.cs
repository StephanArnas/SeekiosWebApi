using Newtonsoft.Json;
using System.Collections.Generic;

namespace WCFServiceWebRole.Data.LOCAL
{
    public class CellsData
    {
        public CellsData() { CellTowers = new List<CellTower>(); }

        [JsonProperty(PropertyName = "homeMobileCountryCode")]
        public int HomeMobileCountryCode { get; set; }
        [JsonProperty(PropertyName = "homeMobileNetworkCode")]
        public int HomeMobileNetworkCode { get; set; }
        [JsonProperty(PropertyName = "radioType")]
        public string RadioType { get; set; }
        [JsonProperty(PropertyName = "cellTowers")]
        public List<CellTower> CellTowers { get; set; }
    }
}