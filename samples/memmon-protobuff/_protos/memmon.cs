using memmon_model_protos;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _protos
{
    internal class MemmonClient
    {
        public Properties Props = new Properties();
        public IReadOnlyProperty<Properties> AllProperties { get; set; }
        public IWritableProperty<Properties, ack> Property_interval { get; set; }
        public IWritableProperty<Properties, ack> Property_enabled { get; set; }
        public ITelemetry<Telemetries> AllTelemetry { get; set; }
        public ICommand<getRuntimeStatsRequest, getRuntimeStatsResponse> getRuntimeStats;

        public MemmonClient(IMqttClient c)
        {
            AllProperties = new ReadOnlyPropertyProtobuff<Properties>(c);
            AllTelemetry = new TelemetryProtobuff<Telemetries>(c);
            Property_interval = new WritablePropertyProtobuff<Properties, ack>(c, "interval", Properties.Parser);
            Property_enabled = new WritablePropertyProtobuff<Properties, ack>(c, "enabled", Properties.Parser);
            getRuntimeStats = new CommandProtobuff<getRuntimeStatsRequest, getRuntimeStatsResponse>(c, "getRuntimeStats", getRuntimeStatsRequest.Parser);
        }
    }
}
