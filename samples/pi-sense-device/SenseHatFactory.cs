using dtmi_rido_pnp_sensehat;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace pi_sense_device
{
    public class SenseHatFactory
    {
        private static string nugetPackageVersion = string.Empty;
        public static string NuGetPackageVersion => nugetPackageVersion;

        private readonly IConfiguration _configuration;
        private readonly ILogger<SenseHatFactory> _logger;
        public SenseHatFactory(IConfiguration configuration, ILogger<SenseHatFactory> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        internal static string ComputeDeviceKey(string masterKey, string deviceId) =>
            Convert.ToBase64String(new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(masterKey)).ComputeHash(System.Text.Encoding.UTF8.GetBytes(deviceId)));

        internal static ConnectionSettings computedSettings = new ();

        public async Task<Isensehat> CreateSenseHatClientAsync(string connectionStringName, CancellationToken cancellationToken = default)
        {
            Isensehat client;
            string connectionString = _configuration.GetConnectionString(connectionStringName);
            var cs = new ConnectionSettings(connectionString);
            _logger.LogWarning("Connecting to .. {cs}", cs);

            if (connectionString.Contains("IdScope") || connectionString.Contains("SharedAccessKey"))
            {

                if (cs.IdScope != null && _configuration["masterKey"] != null)
                {
                    var deviceId = Environment.MachineName;
                    var masterKey = _configuration.GetValue<string>("masterKey");
                    var deviceKey = ComputeDeviceKey(masterKey, deviceId);
                    var newCs = $"IdScope={cs.IdScope};DeviceId={deviceId};SharedAccessKey={deviceKey};SasMinutes={cs.SasMinutes}";
                    client =  await CreateHubClientAsync(newCs, cancellationToken);
                }
                else
                {
                    client = await CreateHubClientAsync(connectionString, cancellationToken);
                }
            }
            else
            {
                client =  await CreateBrokerClientAsync(connectionString, cancellationToken);
            }
            _logger.LogWarning("Connected to {cs}", SenseHatFactory.computedSettings);
            return client;
        }

        private static async Task<dtmi_rido_pnp_sensehat.mqtt.sensehat> CreateBrokerClientAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            var cs = new ConnectionSettings(connectionString) { ModelId = Isensehat.ModelId };
            var mqtt = await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs, true, cancellationToken);
            var client = new dtmi_rido_pnp_sensehat.mqtt.sensehat(mqtt);
            nugetPackageVersion = BrokerClientFactory.NuGetPackageVersion;
            return client;
        }

        private static async Task<dtmi_rido_pnp_sensehat.hub.sensehat> CreateHubClientAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            var cs = connectionString + ";ModelId=" + Isensehat.ModelId;
            var hub = await HubDpsFactory.CreateFromConnectionSettingsAsync(cs, cancellationToken);
            var client = new dtmi_rido_pnp_sensehat.hub.sensehat(hub);
            computedSettings = HubDpsFactory.ComputedSettings!;
            nugetPackageVersion = HubDpsFactory.NuGetPackageVersion;
            //await client.InitState();
            return client;
        }

    }
}
