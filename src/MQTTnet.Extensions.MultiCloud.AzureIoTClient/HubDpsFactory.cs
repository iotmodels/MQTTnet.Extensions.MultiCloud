using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient.Dps;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient
{
    public class HubDpsFactory
    {
        private static Timer reconnectTimer;
        public static ConnectionSettings ConnectionSettings;
        public static async Task<IMqttClient> CreateFromConnectionSettingsAsync(string connectionString, CancellationToken cancellationToken = default) =>
            await CreateFromConnectionSettingsAsync(new ConnectionSettings(connectionString), cancellationToken);

        public static async Task<IMqttClient> CreateFromConnectionSettingsAsync(ConnectionSettings cs, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(cs.HostName) && !string.IsNullOrEmpty(cs.IdScope))
            {
                var dpsMqtt = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
                await dpsMqtt.ConnectAsync(new MqttClientOptionsBuilder().WithAzureDpsCredentials(cs).Build());
                var dpsClient = new MqttDpsClient(dpsMqtt, cs.ModelId);
                var dpsRes = await dpsClient.ProvisionDeviceIdentity();
                cs.HostName = dpsRes.RegistrationState.AssignedHub;
                cs.ClientId = dpsRes.RegistrationState.DeviceId;
                ConnectionSettings = cs;
                await dpsMqtt.DisconnectAsync(new MqttClientDisconnectOptions() { Reason = MqttClientDisconnectReason.NormalDisconnection }, cancellationToken);
            }
            var mqtt = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
            MqttClientConnectResult connAck;
            if (cs.Auth == AuthType.Sas)
            {
                connAck = ConnectWithTimer(mqtt, cs, cancellationToken);
            }
            else
            {
                connAck = await mqtt.ConnectAsync(new MqttClientOptionsBuilder().WithAzureIoTHubCredentials(cs).Build());
            }
            ConnectionSettings = cs;
            if (connAck.ResultCode != MqttClientConnectResultCode.Success)
            {
                throw new ApplicationException($"Cannot connect to {cs}");
            }
            return mqtt;
        }

        private static MqttClientConnectResult ConnectWithTimer(IMqttClient mqtt, ConnectionSettings connectionSettings, CancellationToken cancellationToken = default)
        {
            if (mqtt.IsConnected)
            {
                mqtt.DisconnectAsync().Wait();
            }

            Trace.TraceInformation("Reconnecting before SasToken expires");
            var connAck = mqtt.ConnectAsync(
                new MqttClientOptionsBuilder()
                    .WithAzureIoTHubCredentials(connectionSettings)
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(connectionSettings.KeepAliveInSeconds))
                    .Build(),
                cancellationToken).Result;

            ReplySubscriptions.Get().ForEach(async t =>
            {
                Trace.TraceInformation($"Re-Subscribing to {t}");
                var subAck = await mqtt.SubscribeAsync(t);
                subAck.TraceErrors();
            });

            reconnectTimer = new Timer(o =>
            {
                ConnectWithTimer(mqtt, connectionSettings, cancellationToken);
            }, null, (connectionSettings.SasMinutes * 60 * 1000) - 10, 0);
            return connAck;
        }


    }
}
