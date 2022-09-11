using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
namespace MQTTnet.Extensions.MultiCloud.Clients
{
    public class PropertyAck<T>
    {
        public readonly string Name;
        public readonly string ComponentName;


        public PropertyAck(string name) : this(name, "") { }

        public PropertyAck(string name, string component)
        {
            Name = name;
            ComponentName = component;
            LastReported = default!;
        }

        [JsonIgnore]
        public int? DesiredVersion { get; set; }

        [JsonIgnore]
        public T LastReported { get; set; }

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

        public void SetDefault(T defaultValue)
        {
            Value = defaultValue;
            Version = 0;
            Status = 203;
            Description = "default value";
        }

        public Dictionary<string, object> ToAckDict()
        {
            if (string.IsNullOrEmpty(ComponentName))
            {
                return new Dictionary<string, object>() { { Name, this } };
            }
            else
            {
                Dictionary<string, Dictionary<string, object>> dict = new Dictionary<string, Dictionary<string, object>>
                {
                    { ComponentName, new Dictionary<string, object>() }
                };
                dict[ComponentName].Add("__t", "c");
                dict[ComponentName].Add(Name, this);
                return dict.ToDictionary(pair => pair.Key, pair => (object)pair.Value);
            }
        }
    }
}
