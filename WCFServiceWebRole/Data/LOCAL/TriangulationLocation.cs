using Newtonsoft.Json;

namespace WCFServiceWebRole.Data.LOCAL
{
    public class TriangulationLocation
    {
        [JsonProperty(PropertyName = "location")]
        public Coordinate Location { get; set; }
        [JsonProperty(PropertyName = "accuracy")]
        public double Accuracy { get; set; }
    }
}