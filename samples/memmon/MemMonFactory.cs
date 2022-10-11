using dtmi_rido_memmon;
using MQTTnet.Extensions.MultiCloud.AwsIoTClient;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace memmon;

public class MemMonFactory
{
    static string nugetPackageVersion = string.Empty;
    public static string NuGetPackageVersion => nugetPackageVersion;
    internal static string ComputeDeviceKey(string masterKey, string deviceId) =>
            Convert.ToBase64String(new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(masterKey)).ComputeHash(System.Text.Encoding.UTF8.GetBytes(deviceId)));

    readonly IConfiguration _configuration;
    readonly ILogger<MemMonFactory> _logger;

    internal static ConnectionSettings connectionSettings;

    public MemMonFactory(IConfiguration configuration, ILogger<MemMonFactory> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Imemmon> CreateMemMonClientAsync(string connectinStringName, CancellationToken cancellationToken = default)
    {
        Imemmon client;
        string connectionString = _configuration.GetConnectionString(connectinStringName);
        connectionSettings = new ConnectionSettings(connectionString);
        _logger.LogWarning("Connecting to..{cs}", connectionSettings);
        if (connectionString.Contains("IdScope") || connectionString.Contains("SharedAccessKey"))
        {
            if (connectionSettings.IdScope != null && _configuration["masterKey"] != null)
            {
                var deviceId = Environment.MachineName;
                var masterKey = _configuration.GetValue<string>("masterKey");
                var deviceKey = ComputeDeviceKey(masterKey, deviceId);
                var newCs = $"IdScope={connectionSettings.IdScope};DeviceId={deviceId};SharedAccessKey={deviceKey};SasMinutes={connectionSettings.SasMinutes}";
                client =  await CreateHubClientAsync(newCs, cancellationToken);
            }
            else
            {
                client = await CreateHubClientAsync(connectionString, cancellationToken);
            }
        }
        else if (connectionSettings.HostName.Contains("amazonaws.com"))
        {
            client = await CreateAwsClientAsync(connectionString, cancellationToken);
        }
        else if (connectionSettings.HostName.Contains("azure-devices.net"))
        {
            client =  await CreateHubClientAsync(connectionString, cancellationToken);
        }
        else
        {
            client = await CreateBrokerClientAsync(connectionString, cancellationToken);
        }

        _logger.LogWarning("Connected");
        return client;
    }

    static async Task<dtmi_rido_memmon.mqtt.memmon> CreateBrokerClientAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var cs = new ConnectionSettings(connectionString) { ModelId = Imemmon.ModelId };
        var mqtt = await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs, true, cancellationToken);
        connectionSettings = BrokerClientFactory.ComputedSettings;
        var client = new dtmi_rido_memmon.mqtt.memmon(mqtt)
        {
            InitialState = String.Empty
        };
        nugetPackageVersion = BrokerClientFactory.NuGetPackageVersion;
        return client;
    }

    static async Task<dtmi_rido_memmon.hub.memmon> CreateHubClientAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var cs = new ConnectionSettings(connectionString) { ModelId = Imemmon.ModelId };
        var hub = await HubDpsFactory.CreateFromConnectionSettingsAsync(cs, cancellationToken);
        connectionSettings = HubDpsFactory.ComputedSettings;
        var client = new dtmi_rido_memmon.hub.memmon(hub);
        nugetPackageVersion = HubDpsFactory.NuGetPackageVersion;
        client.InitialState = await client.GetTwinAsync(cancellationToken);
        return client;
    }

    static async Task<dtmi_rido_memmon.aws.memmon> CreateAwsClientAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var mqtt = await AwsClientFactory.CreateFromConnectionSettingsAsync(connectionString, cancellationToken);
        var client = new dtmi_rido_memmon.aws.memmon(mqtt);
        nugetPackageVersion = AwsClientFactory.NuGetPackageVersion;
        return client;
    }
}
