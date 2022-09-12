using dtmi_rido_pnp_memmon.hub;
using Microsoft.Azure.Devices;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using pnp_memmon;
using System.Text.Json;
using Xunit.Abstractions;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests.e2e
{

    public class HubEndToEndFixture : IDisposable
    {
        string hubConnectionString = Environment.GetEnvironmentVariable("E2EHubConnectionString")!;
        string hubName = Environment.GetEnvironmentVariable("TestHubName")!;
        readonly RegistryManager rm;
        readonly string deviceId = string.Empty;
        readonly Device device;

        const int defaultInterval = 23;

        private readonly ITestOutputHelper output;

        public HubEndToEndFixture(ITestOutputHelper output)
        {
            this.output = output;
            rm = RegistryManager.CreateFromConnectionString(hubConnectionString);
            deviceId = "memmon-test" + new Random().Next(100);
            output.WriteLine(deviceId);
            device = GetOrCreateDeviceAsync(deviceId).Result;
        }

        [Fact]
        public async Task NewDeviceSendDefaults()
        {

            var td = new memmon(await HubDpsFactory.CreateFromConnectionSettingsAsync($"HostName={hubName};DeviceId={deviceId};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}"));
            await td.Property_interval.InitPropertyAsync(td.InitialState, defaultInterval);
            await Task.Delay(200);
            var serviceTwin = await rm.GetTwinAsync(deviceId);
            var intervalTwin = serviceTwin.Properties.Reported["interval"];
            Assert.NotNull(intervalTwin);
            Assert.Equal(defaultInterval, Convert.ToInt32(intervalTwin["value"]));
            Assert.Equal(0, Convert.ToInt32(intervalTwin["av"]));
            Assert.Equal(203, Convert.ToInt32(intervalTwin["ac"]));
            Assert.Equal("Init from default value", Convert.ToString(intervalTwin["ad"]));
            Assert.Equal(defaultInterval, td.Property_interval.PropertyValue.Value);
        }

        [Fact(Skip = "Async issues")]
        public async Task DeviceReadsSettingsAtStartup()
        {
            var twin = await rm.GetTwinAsync(deviceId);
            int interval = 5;
            var patch = new
            {
                properties = new
                {
                    desired = new
                    {
                        interval
                    }
                }
            };
            await rm.UpdateTwinAsync(deviceId, JsonSerializer.Serialize(patch), twin.ETag);

            var td = new memmon(await HubDpsFactory.CreateFromConnectionSettingsAsync($"HostName={hubName};DeviceId={deviceId};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}"));
            td.Property_interval.OnProperty_Updated = async m =>
            {
                return await Task.FromResult(new PropertyAck<int>("interval")
                {
                    Version = m.Version,
                    Value = m.Value,
                    Status = 200,
                    Description = "accepted from device"
                });
            };
            await td.Property_interval.InitPropertyAsync(td.InitialState, defaultInterval);
            await Task.Delay(200);
            var serviceTwin = await rm.GetTwinAsync(deviceId);
            var intervalTwin = serviceTwin.Properties.Reported["interval"];
            Assert.NotNull(intervalTwin);
            Assert.Equal(interval, Convert.ToInt32(intervalTwin["value"]));
            Assert.Equal(serviceTwin.Properties.Desired.Version, Convert.ToInt32(intervalTwin["av"]));
            Assert.Equal(200, Convert.ToInt32(intervalTwin["ac"]));
            Assert.Equal("accepted from device", Convert.ToString(intervalTwin["ad"]));
            Assert.Equal("accepted from device", td.Property_interval.PropertyValue.Description);
            Assert.Equal(interval, td.Property_interval.PropertyValue.Value);
        }

        [Fact(Skip = "Async issues")]
        public async Task UpdatesDesiredPropertyWhenOnline()
        {
            var td = new memmon(await HubDpsFactory.CreateFromConnectionSettingsAsync($"HostName={hubName};DeviceId={deviceId};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}"));
            td.Property_interval.OnProperty_Updated = async m =>
            {
                var ack = new PropertyAck<int>(m.Name)
                {
                    Version = m.Version,
                    Value = m.Value,
                    Status = 200,
                    Description = "accepted from device"
                };
                td.Property_interval.PropertyValue = ack;
                return await Task.FromResult(ack);
            };

            var twin = await rm.GetTwinAsync(deviceId);
            int interval = 9;
            var patch = new
            {
                properties = new
                {
                    desired = new
                    {
                        interval
                    }
                }
            };
            await rm.UpdateTwinAsync(deviceId, JsonSerializer.Serialize(patch), twin.ETag);

            await Task.Delay(500);

            var serviceTwin = await rm.GetTwinAsync(deviceId);
            var intervalTwin = serviceTwin.Properties.Reported["interval"];
            Assert.NotNull(intervalTwin);
            Assert.Equal(interval, Convert.ToInt32(intervalTwin["value"]));
            Assert.Equal(serviceTwin.Properties.Desired.Version, Convert.ToInt32(intervalTwin["av"]));
            Assert.Equal(200, Convert.ToInt32(intervalTwin["ac"]));
            Assert.Equal("accepted from device", Convert.ToString(intervalTwin["ad"]));
            Assert.Equal("accepted from device", td.Property_interval.PropertyValue.Description);
            Assert.Equal(interval, td.Property_interval.PropertyValue.Value);
        }

        [Fact]
        public async Task CommandsGetCalled()
        {
            bool commandInvoked = false;
            var td = new memmon(await HubDpsFactory.CreateFromConnectionSettingsAsync($"HostName={hubName};DeviceId={deviceId};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}"));
            td.Command_getRuntimeStats.OnCmdDelegate = async m =>
            {
                commandInvoked = true;
                var result = new Cmd_getRuntimeStats_Response()
                {
                    Status = 200
                };

                result.diagnosticResults.Add("test", "ok");
                return await Task.FromResult(result);
            };
            var sc = ServiceClient.CreateFromConnectionString(hubConnectionString);
            CloudToDeviceMethod c2dMethod = new CloudToDeviceMethod("getRuntimeStats");
            c2dMethod.SetPayloadJson(JsonSerializer.Serialize(1));
            var dmRes = await sc.InvokeDeviceMethodAsync(deviceId, c2dMethod);
            Assert.True(commandInvoked);
            Assert.Equal("{\"diagnosticResults\":{\"test\":\"ok\"}}", dmRes.GetPayloadAsJson());

        }

        private async Task<Device> GetOrCreateDeviceAsync(string deviceId, bool x509 = false)
        {
            var device = await rm.GetDeviceAsync(deviceId);
            if (device == null)
            {
                var d = new Device(deviceId);
                if (x509)
                {
                    d.Authentication = new AuthenticationMechanism()
                    {
                        Type = AuthenticationType.CertificateAuthority
                    };
                }
                device = await rm.AddDeviceAsync(d);
            }
            output.WriteLine($"Test Device Created: {hubName} {device.Id}");
            return device;
        }

        public void Dispose()
        {
            rm.RemoveDeviceAsync(deviceId).Wait();
        }
    }
}
