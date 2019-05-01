using Newtonsoft.Json;

namespace WCFServiceWebRole.Data.LOCAL
{
    public class Coordinate
    {
        [JsonProperty(PropertyName = "lat")]
        public double Lat { get; set; }
        [JsonProperty(PropertyName = "lng")]
        public double Lon { get; set; }
    }
}