using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient.Dps;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient
{
    public class HubDpsFactory
    {
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
            var connAck = await mqtt.ConnectAsync(new MqttClientOptionsBuilder().WithAzureIoTHubCredentials(cs).Build());
            ConnectionSettings = cs;
            if (connAck.ResultCode != MqttClientConnectResultCode.Success)
            {
                throw new ApplicationException($"Cannot connect to {cs}");
            }
            return mqtt;
        }


    }
}
