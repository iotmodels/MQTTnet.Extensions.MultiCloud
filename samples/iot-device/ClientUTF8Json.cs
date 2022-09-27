using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Command;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.ReadOnlyProperty;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Telemetry;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.WritableProperty;

namespace iot_device;

internal class ClientUTF8Json
{
    public ITelemetry<double> Temperature;
    public IReadOnlyProperty<string> SdkInfo;
    public ICommand<string, string> Echo;
    public IWritableProperty<int> Interval;

    public ClientUTF8Json(IMqttClient c)
    {
        Temperature = new TelemetryUTF8Json<double>(c, "temp");
        SdkInfo = new ReadOnlyPropertyUTFJson<string>(c, "sdkInfo");
        Echo = new CommandUTF8Json<string, string>(c, "echo");
        Interval = new WritablePropertyUTFJson<int>(c, "interval");
    }
}
