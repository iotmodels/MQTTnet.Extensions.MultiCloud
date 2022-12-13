using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace mqtt_connection;
internal class ClientFactory
{
    internal static ConnectionSettings? ConnectionSettings;
    public static async Task<IMqttClient> CreateFromConnectionStringAsync(string connectionString)
    {
        IMqttClient mqttClient;
        if (connectionString.Contains("IdScope") || connectionString.Contains("azure-devices.net"))
        {
            mqttClient = await HubDpsFactory.CreateFromConnectionSettingsAsync(connectionString);
            ConnectionSettings = HubDpsFactory.ComputedSettings!;
        }
        else
        {
            mqttClient = await BrokerClientFactory.CreateFromConnectionSettingsAsync(connectionString);
            ConnectionSettings = BrokerClientFactory.ComputedSettings!;
        }
        return mqttClient;
    }
}
