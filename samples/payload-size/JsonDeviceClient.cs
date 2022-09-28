using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace payload_size
{



    internal class JsonDeviceClient
    {
        public class Telemetries
        {
            [JsonPropertyName("temperature")]
            public double Temperature { get; set; }
            [JsonPropertyName("workingSet")]
            public double WorkingSet { get; set; }

        }

        public class Properties
        {
            [JsonPropertyName("sdkInfo")]
            public string? SdkInfo{ get; set; }
            [JsonPropertyName("started")]
            public DateTime Started { get; set; }

        }

        internal Telemetry<Telemetries> Telemetry{ get; set; }
        internal ReadOnlyProperty<Properties> Props { get; set; }

        public JsonDeviceClient(IMqttClient mqtt)
        {
            Telemetry = new Telemetry<Telemetries>(mqtt) {  WrapMessage = false };
            Props = new ReadOnlyProperty<Properties>(mqtt);
        }
    }
}
