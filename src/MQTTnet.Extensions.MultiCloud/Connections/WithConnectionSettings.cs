using MQTTnet.Client;
using MQTTnet.Formatter;

namespace MQTTnet.Extensions.MultiCloud.Connections;

public static partial class MqttNetExtensions
{
    public static MqttClientOptionsBuilder WithConnectionSettings(this MqttClientOptionsBuilder builder, ConnectionSettings cs, bool withLWT = false)
    {
        builder
            .WithTimeout(TimeSpan.FromSeconds(30))
            .WithTcpServer(cs.HostName, cs.TcpPort)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(cs.KeepAliveInSeconds))
            .WithCleanSession(cs.CleanSession)
            .WithTlsSettings(cs);

        MqttProtocolVersion v = cs.MqttVersion switch
        {
            5 => MqttProtocolVersion.V500,
            3 => MqttProtocolVersion.V311,
            _ => MqttProtocolVersion.Unknown
        };

        builder.WithProtocolVersion(v);

        if (!string.IsNullOrEmpty(cs.Password))
        {
            builder.WithCredentials(cs.UserName, cs.Password);
        }

        if (cs.ClientId == "{machineName}")
        {
            cs.ClientId = Environment.MachineName;
        }

        builder.WithClientId(cs.ClientId);

        if (withLWT)
        {
            builder
            .WithWillTopic(BirthConvention.BirthTopic(cs.ClientId!))
            .WithWillQualityOfServiceLevel(Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .WithWillPayload(BirthConvention.LastWillPayload(cs.ModelId!))
            .WithWillRetain(true);
        }

        return builder;
    }
}

