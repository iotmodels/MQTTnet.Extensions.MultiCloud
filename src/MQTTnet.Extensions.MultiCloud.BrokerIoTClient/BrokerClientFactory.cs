using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient
{
    public static class BrokerClientFactory
    {
        public static string NuGetPackageVersion => $"{ThisAssembly.NuGetPackageVersion}";

        public static ConnectionSettings? ComputedSettings { get; private set; }
        public static async Task<IMqttClient> CreateFromConnectionSettingsAsync(string connectinString, bool withBirth = true, CancellationToken cancellationToken = default) =>
            await CreateFromConnectionSettingsAsync(new ConnectionSettings(connectinString), withBirth, cancellationToken);

        public static async Task<IMqttClient> CreateFromConnectionSettingsAsync(ConnectionSettings cs, bool withBirth = true, CancellationToken cancellationToken = default)
        {
            MqttClient? mqtt = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
            var connAck = await mqtt!.ConnectAsync(new MqttClientOptionsBuilder()
                .WithConnectionSettings(cs, withBirth)
                .WithProtocolVersion(Formatter.MqttProtocolVersion.V311)
                .Build());
            ComputedSettings = cs;
            if (connAck.ResultCode != MqttClientConnectResultCode.Success)
            {
                throw new ApplicationException($"Cannot connect to {cs}");
            }
            if (withBirth)
            {
                var birthPayload = new UTF8JsonSerializer().ToBytes(
                       new BirthConvention.BirthMessage(BirthConvention.ConnectionStatus.online)
                       {
                           ModelId = cs.ModelId
                       });

                var pubAck = await mqtt.PublishBinaryAsync(
                   BirthConvention.BirthTopic(mqtt.Options.ClientId),
                   birthPayload,
                   Protocol.MqttQualityOfServiceLevel.AtLeastOnce, true);
                if (pubAck.ReasonCode != MqttClientPublishReasonCode.Success)
                {
                    throw new ApplicationException($"Error publishing Birth {cs}");
                }
            }
            return mqtt;
        }
    }
}
