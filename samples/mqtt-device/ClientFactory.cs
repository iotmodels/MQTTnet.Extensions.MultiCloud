using dtmi_com_example_devicetemplate;
using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;
using System.Reflection;

namespace mqtt_device
{
    internal class ClientFactory
    {
        //internal static string ComputeDeviceKey(string masterKey, string deviceId) =>
        //    Convert.ToBase64String(new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(masterKey)).ComputeHash(System.Text.Encoding.UTF8.GetBytes(deviceId)));
        static string nuGetPackageVersion = String.Empty;
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
                //var cs = new ConnectionSettings(_configuration.GetConnectionString("cs"));

                //if (cs.IdScope != null && _configuration["masterKey"] != null)
                //{
                //    var deviceId = Environment.MachineName;
                //    var masterKey = _configuration.GetValue<string>("masterKey");
                //    var deviceKey = ComputeDeviceKey(masterKey, deviceId);
                //    var newCs = $"IdScope={cs.IdScope};DeviceId={deviceId};SharedAccessKey={deviceKey};SasMinutes={cs.SasMinutes}";
                //    return await CreateHubClientAsync(newCs, cancellationToken);
                //}
                //else
                //{
                //    return await CreateHubClientAsync(connectionString, cancellationToken);
                //}
                throw new NotImplementedException();
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

            System.Type baseClient = client.GetType().BaseType!;
            string nugetVersion = baseClient.Assembly
                .GetType("ThisAssembly")!
                .GetField("NuGetPackageVersion", BindingFlags.NonPublic | BindingFlags.Static)!
                .GetValue(null)!
                .ToString()!;
            nuGetPackageVersion = $"{baseClient.Namespace} {nugetVersion}";
            return client;
        }

        static internal string NuGetPackageVersion
        {
           get => nuGetPackageVersion;
        }

        //static async Task<dtmi_com_example_devicetemplate.hub.devicetemplate> CreateHubClientAsync(string connectionString, CancellationToken cancellationToken = default)
        //{
        //    var cs = connectionString + ";ModelId=" + Idevicetemplate.ModelId;
        //    var hub = await HubDpsFactory.CreateFromConnectionSettingsAsync(cs);
        //    var client = new dtmi_com_example_devicetemplate.hub.devicetemplate(hub);
        //    computedSettings = HubDpsFactory.ComputedSettings;
        //    await client.InitState();
        //    return client;
        //}
    }
}