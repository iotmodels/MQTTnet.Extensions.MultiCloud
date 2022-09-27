using device_template_protos;
using MQTTnet.Client;
using MQTTnet.Extensions.IoT;
using MQTTnet.Extensions.IoT.Binders.Command;
using MQTTnet.Extensions.IoT.Binders.ReadOnlyProperty;
using MQTTnet.Extensions.IoT.Binders.Telemetry;
using MQTTnet.Extensions.IoT.Binders.WritableProperty;

namespace iot_device;

internal class ClientProtobuff
{
    public ITelemetry<Telemetries> Temperature;
    public IReadOnlyProperty<Properties> SdkInfo;
    public ICommand<echoRequest, echoResponse> Echo;
    public IWritableProperty<Properties, ack> Interval;

    public ClientProtobuff(IMqttClient c)
    {
        Temperature = new TelemetryProtobuff<Telemetries>(c, "temp");
        SdkInfo = new ReadOnlyPropertyProtobuff<Properties>(c, "sdkInfo");
        Echo = new CommandProtobuff<echoRequest, echoResponse>(c, "echo", echoRequest.Parser);
        Interval = new WritablePropertyProtobuff<Properties, ack>(c, "interval", Properties.Parser);
    }
}
