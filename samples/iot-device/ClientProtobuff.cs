using device_template_protos;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Command;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.ReadOnlyProperty;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Telemetry;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient.WritableProperty;

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
