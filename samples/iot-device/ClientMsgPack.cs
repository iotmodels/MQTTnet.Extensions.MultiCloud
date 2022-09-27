using MQTTnet.Client;
using MQTTnet.Extensions.IoT;
using MQTTnet.Extensions.IoT.Binders.Command;
using MQTTnet.Extensions.IoT.Binders.ReadOnlyProperty;
using MQTTnet.Extensions.IoT.Binders.Telemetry;

namespace iot_device;

internal class ClientMsgPack
{
    public ITelemetry<double> Temperature;
    public IReadOnlyProperty<string> SdkInfo;
    public ICommand<int, string> EchoRepeater;

    public ClientMsgPack(IMqttClient c)
    {
        Temperature = new TelemetryMsgPack<double>(c, "temperature");
        SdkInfo = new ReadOnlyPropertyMessagePack<string>(c, "sdkInfo");
        EchoRepeater = new CommandMsgPack<int, string>(c, "echoRepeater");
    }
}
