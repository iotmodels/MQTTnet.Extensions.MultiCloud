using dtmi_rido_pnp_memmon;
using MQTTnet.Extensions.MultiCloud.AwsIoTClient;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace memmon;

internal class MemMonFactory
{
    internal static string ComputeDeviceKey(string masterKey, string deviceId) =>
            Convert.ToBase64String(new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(masterKey)).ComputeHash(System.Text.Encoding.UTF8.GetBytes(deviceId)));

    private readonly IConfiguration _configuration;

    internal static ConnectionSettings computedSettings;

    public MemMonFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Imemmon> CreateMemMonClientAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connectionString, nameof(connectionString));
        if (connectionString.Contains("IdScope") || connectionString.Contains("SharedAccessKey"))
        {
            var tempSettings = new ConnectionSettings(connectionString);
            if (tempSettings.IdScope != null && _configuration["masterKey"] != null)
            {
                var deviceId = Environment.MachineName;
                var masterKey = _configuration.GetValue<string>("masterKey");
                var deviceKey = ComputeDeviceKey(masterKey, deviceId);
                var newCs = $"IdScope={tempSettings.IdScope};DeviceId={deviceId};SharedAccessKey={deviceKey};SasMinutes={tempSettings.SasMinutes}";
                return await CreateHubClientAsync(newCs, cancellationToken);
            }
            else
            {
                return await CreateHubClientAsync(connectionString, cancellationToken);
            }
        }
        else if (connectionString.Contains("amazonaws.com"))
        {
            return await CreateAwsClientAsync(connectionString, cancellationToken);
        }
        else if (connectionString.Contains("azure-devices.net"))
        {
            return await CreateHubClientAsync(connectionString, cancellationToken);
        }
        else
        {
            return await CreateBrokerClientAsync(connectionString, cancellationToken);
        }
    }

    private static async Task<dtmi_rido_pnp_memmon.mqtt.memmon> CreateBrokerClientAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var cs = new ConnectionSettings(connectionString) { ModelId = Imemmon.ModelId };
        var mqtt = await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs, true, cancellationToken);
        computedSettings = BrokerClientFactory.ComputedSettings;
        var client = new dtmi_rido_pnp_memmon.mqtt.memmon(mqtt);
        return client;
    }

    private static async Task<dtmi_rido_pnp_memmon.hub.memmon> CreateHubClientAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var cs = connectionString + ";ModelId=" + Imemmon.ModelId;
        var hub = await HubDpsFactory.CreateFromConnectionSettingsAsync(cs);
        computedSettings = HubDpsFactory.ComputedSettings;
        var client = new dtmi_rido_pnp_memmon.hub.memmon(hub);
        await client.InitState();
        return client;
    }

    private static async Task<dtmi_rido_pnp_memmon.aws.memmon> CreateAwsClientAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var mqtt = await AwsClientFactory.CreateFromConnectionSettingsAsync(connectionString, cancellationToken);
        computedSettings = AwsClientFactory.ComputedSettings;
        var client = new dtmi_rido_pnp_memmon.aws.memmon(mqtt);
        return client;
    }
}
