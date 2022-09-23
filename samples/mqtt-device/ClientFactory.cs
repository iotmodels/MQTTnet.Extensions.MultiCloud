using dtmi_com_example_devicetemplate;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace mqtt_device
{
    internal class ClientFactory
    {
        static string nugetPackaveVersion = string.Empty;
        public static string NuGetPackageVersion => nugetPackaveVersion;
        internal static string ComputeDeviceKey(string masterKey, string deviceId) =>
            Convert.ToBase64String(new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(masterKey)).ComputeHash(System.Text.Encoding.UTF8.GetBytes(deviceId)));

        static internal ConnectionSettings computedSettings = new ConnectionSettings();
        IConfiguration _configuration;

        public ClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Idevicetemplate> CreateDeviceTemplateClientAsync(CancellationToken cancellationToken = default)
        {
            string connectionString = _configuration.GetConnectionString("cs");
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

        static async Task<dtmi_com_example_devicetemplate.mqtt.devicetemplate> CreateBrokerClientAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            var cs = new ConnectionSettings(connectionString) { ModelId = Idevicetemplate.ModelId };
            var mqtt = await BrokerClientFactory.CreateFromConnectionSettingsAsync(cs, true, cancellationToken);
            var client = new dtmi_com_example_devicetemplate.mqtt.devicetemplate(mqtt);
            computedSettings = cs;
            nugetPackaveVersion = BrokerClientFactory.NugetPackageVersion;
            return client;
        }

        static async Task<dtmi_com_example_devicetemplate.hub.devicetemplate> CreateHubClientAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            var cs = connectionString + ";ModelId=" + Idevicetemplate.ModelId;
            var hub = await HubDpsFactory.CreateFromConnectionSettingsAsync(cs);
            var client = new dtmi_com_example_devicetemplate.hub.devicetemplate(hub);
            computedSettings = HubDpsFactory.ComputedSettings;
            nugetPackaveVersion = HubDpsFactory.NugetPackageVersion;
            await client.InitState();
            return client;
        }
    }
}