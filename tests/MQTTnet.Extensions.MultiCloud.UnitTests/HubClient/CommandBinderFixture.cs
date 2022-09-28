using MQTTnet.Extensions.MultiCloud.AzureIoTClient;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests.HubClient
{
    public class HubCommandUTF8JsoFixture
    {
        [Fact]
        public void ReceiveCommand()
        {
            var mqttClient = new MockMqttClient();
            var command = new HubCommandUTF8Json<string, string>(mqttClient, "myCmd");
            bool cmdCalled = false;
            command.OnMessage = async m =>
            {
                cmdCalled = true;
                return await Task.FromResult("response");
            };
            mqttClient.SimulateNewMessage("$iothub/methods/POST/myCmd", "\"in\"");
            Assert.True(cmdCalled);
        }
    }
}
