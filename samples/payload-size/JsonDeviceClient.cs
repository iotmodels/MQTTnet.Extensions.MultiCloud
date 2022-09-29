using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using payload_size.json;

namespace payload_size;

internal class JsonDeviceClient
{
    internal ITelemetry<Telemetries> Telemetry{ get; set; }
    internal IReadOnlyProperty<Properties> Props { get; set; }
    internal IReadOnlyProperty<string> Prop_SdkInfo { get; set; }

    public JsonDeviceClient(IMqttClient mqtt)
    {
        Telemetry = new Telemetry<Telemetries>(mqtt) {  WrapMessage = false };
        Props = new ReadOnlyProperty<Properties>(mqtt);
        Prop_SdkInfo = new ReadOnlyProperty<string>(mqtt, "serialNumber") { WrapMessage = false, NameInTopic = true };
    }
}
