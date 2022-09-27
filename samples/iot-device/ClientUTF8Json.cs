using MQTTnet.Client;
using MQTTnet.Extensions.IoT;
using MQTTnet.Extensions.IoT.Binders.Command;
using MQTTnet.Extensions.IoT.Binders.ReadOnlyProperty;
using MQTTnet.Extensions.IoT.Binders.Telemetry;
using MQTTnet.Extensions.IoT.Binders.WritableProperty;

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
