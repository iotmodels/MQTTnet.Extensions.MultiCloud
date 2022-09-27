using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.IoT.Binders.WritableProperty
{
    public class Ack<T> : DeviceToCloudBinder<T>, IReadOnlyProperty<T>
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

        public Ack(IMqttClient c, string name)
            : base(c, name, new UTF8JsonSerializer())
        {
            wrapMessage = true;
        }
    }
}
