using System.Text.Json.Serialization;

namespace payload_size.json;

public class Telemetries
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }
    [JsonPropertyName("workingSet")]
    public double WorkingSet { get; set; }
}
