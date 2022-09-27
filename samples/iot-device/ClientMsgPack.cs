using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Command;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.ReadOnlyProperty;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Telemetry;

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
