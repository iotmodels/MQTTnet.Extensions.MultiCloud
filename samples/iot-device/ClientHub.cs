using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iot_device
{
    internal class ClientHub
    {
        RequestResponseBinder rr;

        public ITelemetry<double> Temperature;
        public IReadOnlyProperty<string> SdkInfo;
        public ICommand<string, string> Echo;
        public IWritableProperty<int> Interval;

        public ClientHub(IMqttClient c)
        {
            rr = new RequestResponseBinder(c);
            Temperature = new HubTelemetryUTF8Json<double>(c, "temp");
            SdkInfo = new HubReadOnlyPropertyUTFJson<string>(c, "sdkInfo");
            Echo = new HubCommandUTF8Json<string, string>(c, "echo");
            Interval = new HubWritablePropertyUTFJson<int>(c, "interval");
        }

        internal async Task<string> GetTwinAsync(CancellationToken cancellationToken = default) 
            => await rr.GetTwinAsync(cancellationToken);
    }
}
