using dtmi_rido_pnp_memmon.hub;
using Microsoft.Azure.Devices;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using pnp_memmon;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests.e2e
{

    public class HubEndToEndFixture
    {
        private static readonly string hubConnectionString = Environment.GetEnvironmentVariable("E2EHubConnectionString")!;
        private static readonly string hubName = Environment.GetEnvironmentVariable("TestHubName")!;
        private const int defaultInterval = 23;
        private readonly RegistryManager rm = RegistryManager.CreateFromConnectionString(hubConnectionString);

        [Fact, Trait("e2e", "hub")]
        public async Task NewDeviceSendDefaults()
        {
            var deviceId = "memmon-test" + new Random().Next(100);
            var device = await GetOrCreateDeviceAsync(deviceId);

            var td = new memmon(await HubDpsFactory.CreateFromConnectionSettingsAsync($"HostName={hubName};DeviceId={deviceId};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}"));
            td.Property_interval.OnMessage = async p =>
            {
                td.Property_interval.Value = p;
                return await Task.FromResult(new Ack<int> 
                { 
                    Value = p
                });
            };


            var twin = await td.GetTwinAsync();
            await TwinInitializer.InitPropertyAsync(td.Connection, twin, td.Property_interval, "interval", defaultInterval);
            await Task.Delay(500);
            var serviceTwin = await rm.GetTwinAsync(deviceId);
            var intervalTwin = serviceTwin.Properties.Reported["interval"];
            Assert.NotNull(intervalTwin);
            Assert.Equal(defaultInterval, Convert.ToInt32(intervalTwin["value"]));
            Assert.Equal(0, Convert.ToInt32(intervalTwin["av"]));
            Assert.Equal(203, Convert.ToInt32(intervalTwin["ac"]));
            Assert.Equal("Init from default value", Convert.ToString(intervalTwin["ad"]));
            Assert.Equal(defaultInterval, td.Property_interval.Value);

            await rm.RemoveDeviceAsync(deviceId);
        }

        [Fact(Skip = "investigate timeout")]
        public async Task DeviceReadsSettingsAtStartup()
        {

            var deviceId = "memmon-test" + new Random().Next(100);
            var device = await GetOrCreateDeviceAsync(deviceId);
            while (device.Status != DeviceStatus.Enabled)
            {
                device = await GetOrCreateDeviceAsync(deviceId);
            }
            
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

            await Task.Delay(1000);

            var td = new memmon(await HubDpsFactory.CreateFromConnectionSettingsAsync($"HostName={hubName};DeviceId={deviceId};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}"));
           // await td.InitState();

            td.Property_interval.OnMessage = async m =>
            {
                return await Task.FromResult(new Ack<int>
                {
                    Version = 1,
                    Value = m,
                    Status = 200,
                    Description = "accepted from device"
                });
            };

           

            //await td.Property_interval.InitPropertyAsync(td.InitialState, defaultInterval);

            await Task.Delay(500);

            var serviceTwin = await rm.GetTwinAsync(deviceId);
            var intervalTwin = serviceTwin.Properties.Reported["interval"];
            Assert.NotNull(intervalTwin);
            Assert.Equal(interval, Convert.ToInt32(intervalTwin["value"]));
            Assert.Equal(serviceTwin.Properties.Desired.Version, Convert.ToInt32(intervalTwin["av"]));
            Assert.Equal(200, Convert.ToInt32(intervalTwin["ac"]));
            Assert.Equal("accepted from device", Convert.ToString(intervalTwin["ad"]));
            //Assert.Equal("accepted from device", td.Property_interval.Description);
            Assert.Equal(interval, td.Property_interval.Value);

            await rm.RemoveDeviceAsync(deviceId);
        }

        [Fact(Skip = "Threading issues"), Trait("e2e", "hub")]
        public async Task UpdatesDesiredPropertyWhenOnline()
        {

            var deviceId = "memmon-test" + new Random().Next(100);
            var device = await GetOrCreateDeviceAsync(deviceId);

            var td = new memmon(await HubDpsFactory.CreateFromConnectionSettingsAsync($"HostName={hubName};DeviceId={deviceId};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}"));
            td.Property_interval.OnMessage = m =>
            {
                var ack = new Ack<int>()
                {
                    Version = 2,
                    Value = m,
                    Status = 200,
                    Description = "accepted from device"
                };
                td.Property_interval.Value = m;
                return Task.FromResult(ack);
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

            Thread.Sleep(1000);

            var serviceTwin = await rm.GetTwinAsync(deviceId);
            var intervalTwin = serviceTwin.Properties.Reported["interval"];
            Assert.NotNull(intervalTwin);
            Assert.Equal(interval, Convert.ToInt32(intervalTwin["value"]));
            Assert.Equal(serviceTwin.Properties.Desired.Version, Convert.ToInt32(intervalTwin["av"]));
            Assert.Equal(200, Convert.ToInt32(intervalTwin["ac"]));
            Assert.Equal("accepted from device", Convert.ToString(intervalTwin["ad"]));
            //Assert.Equal("accepted from device", td.Property_interval.Description);
            Assert.Equal(interval, td.Property_interval.Value);

            rm.RemoveDeviceAsync(deviceId).Wait();
        }

        [Fact, Trait("e2e", "hub")]
        public async Task CommandsGetCalled()
        {
            var deviceId = "memmon-test" + new Random().Next(100);
            var device = await GetOrCreateDeviceAsync(deviceId);

            bool commandInvoked = false;
            var td = new memmon(await HubDpsFactory.CreateFromConnectionSettingsAsync($"HostName={hubName};DeviceId={deviceId};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}"));
            td.Command_getRuntimeStats.OnMessage = async m =>
            {

                Dictionary<string, string> diagResults = new()
                {
                    { "test", "ok" }
                };
                commandInvoked = true;
                return await Task.FromResult(diagResults);
            };
            await Task.Delay(200);
            var sc = ServiceClient.CreateFromConnectionString(hubConnectionString);
            CloudToDeviceMethod c2dMethod = new("getRuntimeStats");
            c2dMethod.SetPayloadJson(JsonSerializer.Serialize("1"));
            var dmRes = await sc.InvokeDeviceMethodAsync(deviceId, c2dMethod);
            Assert.True(commandInvoked);
            string expectedJson = Json.Stringify(new { test = "ok" });
            Assert.Equal(expectedJson, dmRes.GetPayloadAsJson());
            //Assert.Equal("\"{\\\"test\\\":\\\"ok\\\"}\"", dmRes.GetPayloadAsJson());

            await rm.RemoveDeviceAsync(deviceId);

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
            return device;
        }


    }
}
