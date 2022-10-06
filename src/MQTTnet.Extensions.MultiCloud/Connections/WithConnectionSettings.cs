using MQTTnet.Client;

namespace MQTTnet.Extensions.MultiCloud.Connections;

public static partial class MqttNetExtensions
{
    public static MqttClientOptionsBuilder WithConnectionSettings(this MqttClientOptionsBuilder builder, ConnectionSettings cs, bool withLWT = false)
    {
        if (cs.HostName != null && cs.HostName.Contains("azure-devices.net"))
        {
            builder.WithAzureIoTHubCredentials(cs);
        }
        else
        {
            builder
                .WithTimeout(TimeSpan.FromSeconds(30))
                .WithTcpServer(cs.HostName, cs.TcpPort)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(cs.KeepAliveInSeconds))
                .WithCleanSession(cs.CleanSession)
                .WithTlsSettings(cs);

            if (!string.IsNullOrEmpty(cs.Password))
            {
                builder.WithCredentials(cs.UserName, cs.Password);
            }

            if (cs.ClientId == "{machineName}")
            {
                cs.ClientId = Environment.MachineName;
            }
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

