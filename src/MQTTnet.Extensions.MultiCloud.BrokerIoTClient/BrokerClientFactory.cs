using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient
{
    public static class BrokerClientFactory
    {
        public static async Task<IMqttClient> CreateFromConnectionSettingsAsync(string connectinString, bool lwt = true, CancellationToken cancellationToken = default) =>
            await CreateFromConnectionSettingsAsync(new ConnectionSettings(connectinString), lwt, cancellationToken);

        public static async Task<IMqttClient> CreateFromConnectionSettingsAsync(ConnectionSettings cs, bool lwt = true, CancellationToken cancellationToken = default)
        {
            MqttClient? mqtt = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
            var connAck = await mqtt!.ConnectAsync(new MqttClientOptionsBuilder().WithConnectionSettings(cs, lwt).Build());
            if (connAck.ResultCode != MqttClientConnectResultCode.Success)
            {
                throw new ApplicationException($"Cannot connect to {cs}");
            }
            return mqtt;
        }
    }
}
