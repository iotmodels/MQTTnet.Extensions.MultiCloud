using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Connections;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient.Connections;
using System.Text;

namespace MQTTnet.Extensions.MultiCloud.IntegrationTests
{
    public class IoTHubConnectionFixture
    {
        private readonly MqttClient? client;
        public IoTHubConnectionFixture()
        {
            client = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
        }

        [Fact]
        public async Task DeviceSas()
        {
            var cs = new ConnectionSettings()
            {
                HostName = Environment.GetEnvironmentVariable("TestHubName"),
                DeviceId = "testdevice",
                SharedAccessKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.Empty.ToString("N")))

            };
            var connAck = await client!.ConnectAsync(new MqttClientOptionsBuilder()
                .WithAzureIoTHubCredentials(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }

        [Fact]
        public async Task ModuleSas()
        {
            var cs = new ConnectionSettings()
            {
                HostName = Environment.GetEnvironmentVariable("TestHubName"),
                DeviceId = "testdevice",
                ModuleId = "testmodule",
                SharedAccessKey = "py0vifQWT7QGK4AiDO3IlreGsvXrLet+sKZnErKMkAk="
            };
            var connAck = await client!.ConnectAsync(new MqttClientOptionsBuilder()
                .WithAzureIoTHubCredentials(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }

        [Fact]
        public async Task DeviceCert()
        {
            var cs = new ConnectionSettings()
            {
                HostName = Environment.GetEnvironmentVariable("TestHubName"),
                X509Key = "ca-device.pem|ca-device.key"
            };
            var connAck = await client!.ConnectAsync(new MqttClientOptionsBuilder()
                .WithAzureIoTHubCredentials(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }

        //[Fact]
        //public async Task DeviceCertFromIntermediate()
        //{
        //    var cs = new ConnectionSettings()
        //    {
        //        HostName = Environment.GetEnvironmentVariable("TestHubName"),
        //        X509Key = "dev03.pfx|"
        //    };
        //    var connAck = await client!.ConnectAsync(new MqttClientOptionsBuilder()
        //        .WithConnectionSettings(cs)
        //        .Build());
        //    Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
        //    Assert.True(client.IsConnected);
        //    await client.DisconnectAsync();
        //}

        [Fact]
        public async Task ModuleCert()
        {
            var cs = new ConnectionSettings()
            {
                HostName = Environment.GetEnvironmentVariable("TestHubName"),
                X509Key = "ca-module.pem|ca-module.key"
            };
            var connAck = await client!.ConnectAsync(new MqttClientOptionsBuilder()
                .WithAzureIoTHubCredentials(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }

        //Fact(Skip ="Required edgeHub-local")]
        [Fact]
        public async Task ModuleSasUsingEdgeHub()
        {
            var cs = new ConnectionSettings()
            {
                HostName = "rido-edges.azure-devices.net",
                DeviceId = "riduntu22",
                ModuleId = "ModuleOne",
                SharedAccessKey = "c+ZZs5grZizn/RHj7vWjTvboY+wQ5sWu9PFfchJVybk=",
                GatewayHostName = "localhost"
            };
            var connAck = await client!.ConnectAsync(new MqttClientOptionsBuilder()
                .WithAzureIoTHubCredentials(cs)
                .Build());
            Assert.Equal(MqttClientConnectResultCode.Success, connAck.ResultCode);
            Assert.True(client.IsConnected);
            await client.DisconnectAsync();
        }
    }
}