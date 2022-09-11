using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace MQTTnet.Extensions.MultiCloud.Connections
{
    public static partial class MqttNetExtensions
    {
        public static MqttClientOptionsBuilder WithConnectionSettings(this MqttClientOptionsBuilder builder, ConnectionSettings cs, bool withLWT = false)
        {
            builder
                .WithTimeout(TimeSpan.FromSeconds(30))
                .WithTcpServer(cs.HostName, cs.TcpPort)
                .WithClientId(cs.ClientId)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(cs.KeepAliveInSeconds))
                .WithCleanSession(cs.CleanSession)
                .WithTlsSettings(cs);

            if (!string.IsNullOrEmpty(cs.Password))
            {
                builder.WithCredentials(cs.UserName, cs.Password);
            }

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

