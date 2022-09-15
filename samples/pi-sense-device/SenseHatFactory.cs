using dtmi_rido_pnp_sensehat;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace pi_sense_device
{
    internal class SenseHatFactory
    {
        IConfiguration _configuration;
        public SenseHatFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        internal static string ComputeDeviceKey(string masterKey, string deviceId) =>
            Convert.ToBase64String(new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(masterKey)).ComputeHash(System.Text.Encoding.UTF8.GetBytes(deviceId)));

        static internal ConnectionSettings computedSettings = new ConnectionSettings();

        public async Task<Isensehat> CreateSenseHatClientAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));

            if (connectionString.Contains("IdScope") || connectionString.Contains("SharedAccessKey"))
            {
                var cs = new ConnectionSettings(_configuration.GetConnectionString("cs"));

                if (cs.IdScope != null && _configuration["masterKey"] != null)
                {
                    var deviceId = Environment.MachineName;
                    var masterKey = _configuration.GetValue<string>("masterKey");
                    var deviceKey = ComputeDeviceKey(masterKey, deviceId);
                    var newCs = $"IdScope={cs.IdScope};DeviceId={deviceId};SharedAccessKey={deviceKey};SasMinutes={cs.SasMinutes}";
                    return await CreateHubClientAsync(newCs, cancellationToken);
                }
                else
                {
                    return await CreateHubClientAsync(connectionString, cancellationToken);
                }
            }
            else
            {
                return await CreateBrokerClientAsync(connectionString, cancellationToken);
            }
        }

        static async Task<dtmi_rido_pnp_sensehat.mqtt.sensehat> CreateBrokerClientAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            var cs = new ConnectionSettings(connectionString) { ModelId = Isensehat.ModelId };
            var mqtt = await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs, true, cancellationToken);
            var client = new dtmi_rido_pnp_sensehat.mqtt.sensehat(mqtt);
            return client;
        }

        static async Task<dtmi_rido_pnp_sensehat.hub.sensehat> CreateHubClientAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            var cs = connectionString + ";ModelId=" + Isensehat.ModelId;
            var hub = await HubDpsFactory.CreateFromConnectionSettingsAsync(cs);
            var client = new dtmi_rido_pnp_sensehat.hub.sensehat(hub);
            computedSettings = HubDpsFactory.ComputedSettings;
            await client.InitState();
            return client;
        }

    }
}
