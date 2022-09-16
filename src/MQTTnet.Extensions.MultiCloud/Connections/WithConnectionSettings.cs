using MQTTnet.Client;
using System;

namespace MQTTnet.Extensions.MultiCloud.Connections
{
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

            if (!string.IsNullOrEmpty(cs.Password))
            {
                builder.WithCredentials(cs.UserName, cs.Password);
                if (string.IsNullOrEmpty(cs.ClientId))
                {
                    cs.ClientId = cs.UserName;
                }
            }

            if (string.IsNullOrEmpty(cs.ClientId))
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
}

