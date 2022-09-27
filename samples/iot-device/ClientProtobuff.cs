using iot_device_protos;
using MQTTnet.Client;
using MQTTnet.Extensions.IoT;
using MQTTnet.Extensions.IoT.Binders.Command;
using MQTTnet.Extensions.IoT.Binders.ReadOnlyProperty;
using MQTTnet.Extensions.IoT.Binders.Telemetry;

namespace iot_device;

internal class ClientProtobuff
{
    public ITelemetry<Telemetries> Temperature;
    public IReadOnlyProperty<Properties> SdkInfo;
    public ICommand<echoRequest, echoResponse> EchoRepeater;

    public ClientProtobuff(IMqttClient c)
    {
        Temperature = new TelemetryProtobuff<Telemetries>(c, "temperature");
        SdkInfo = new ReadOnlyPropertyProtobuff<Properties>(c, "sdkInfo");
        EchoRepeater = new CommandProtobuff<echoRequest, echoResponse>(c, "echoRepeater", echoRequest.Parser);
    }
}
