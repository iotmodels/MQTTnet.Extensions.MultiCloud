using System.Text.Json.Serialization;

namespace MQTTnet.Extensions.MultiCloud;

public class Ack<T>
{
    [JsonPropertyName("av")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Version { get; set; }

    [JsonPropertyName("ad")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    [JsonPropertyName("ac")]
    public int Status { get; set; }

    [JsonPropertyName("value")]
    public T Value { get; set; } = default!;
}
