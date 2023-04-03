using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Connections;
using MQTTnet.Extensions.MultiCloud.Serializers;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient
{
    public static class BrokerClientFactory
    {
        public static string NuGetPackageVersion => $"{ThisAssembly.AssemblyName} {ThisAssembly.NuGetPackageVersion}";

        public static ConnectionSettings? ComputedSettings { get; private set; }
        public static async Task<IMqttClient> CreateFromConnectionSettingsAsync(string connectionString, bool withBirth = false, CancellationToken cancellationToken = default) =>
            await CreateFromConnectionSettingsAsync(new ConnectionSettings(connectionString), withBirth, cancellationToken);

        public static async Task<IMqttClient> CreateFromConnectionSettingsAsync(ConnectionSettings cs, bool withBirth = false, CancellationToken cancellationToken = default)
        {
            MqttClient? mqtt = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
            var connAck = await mqtt!.ConnectAsync(new MqttClientOptionsBuilder()
                .WithConnectionSettings(cs, withBirth)
                //.WithProtocolVersion(Formatter.MqttProtocolVersion.V500)
                .Build(), cancellationToken);
            ComputedSettings = cs;
            if (connAck.ResultCode != MqttClientConnectResultCode.Success)
            {
                throw new ApplicationException($"Cannot connect to {cs}");
            }
            if (withBirth)
            {
                var birthPayload = new Utf8JsonSerializer().ToBytes(
                       new BirthConvention.BirthMessage(BirthConvention.ConnectionStatus.online)
                       {
                           ModelId = cs.ModelId
                       });

                var pubAck = await mqtt.PublishBinaryAsync(
                   BirthConvention.BirthTopic(mqtt.Options.ClientId),
                   birthPayload,
                   Protocol.MqttQualityOfServiceLevel.AtLeastOnce, true, cancellationToken); //hack to disable retained in registry
                if (pubAck.ReasonCode != MqttClientPublishReasonCode.Success)
                {
                    throw new ApplicationException($"Error publishing Birth {cs}");
                }
            }
            return mqtt;
        }
    }
}
