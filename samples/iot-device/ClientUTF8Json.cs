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
    public ICommand<int, string> EchoRepeater;
    public IWritableProperty<int> Interval;
    public IWritableProperty<bool> Enabled;

    public ClientUTF8Json(IMqttClient c)
    {
        Temperature = new TelemetryUTF8Json<double>(c, "temperature");
        SdkInfo = new ReadOnlyPropertyUTFJson<string>(c, "sdkInfo");
        EchoRepeater = new CommandUTF8Json<int, string>(c, "echoRepeater");
        Interval = new WritablePropertyUTFJson<int>(c, "interval");
        Enabled = new WritablePropertyUTFJson<bool>(c, "enabled");
    }
}
