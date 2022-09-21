using MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests.HubClient
{
    public class CommandBinderFixture
    {
        private class CmdRequest : IBaseCommandRequest<CmdRequest>
        {

            public CmdRequest DeserializeBody(string payload) => new CmdRequest();

            public CmdRequest DeserializeBody(byte[] payload)
            {
                throw new System.NotImplementedException();
            }
        }

        private class CmdResponse : IBaseCommandResponse
        {
            public CmdResponse()
            {
                ReponsePayload= "result";
            }

            public int Status { get; set; }
            public object ReponsePayload { get; set; }
            public byte[] ResponseBytes { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        }


        [Fact]
        public void ReceiveCommand()
        {
            var mqttClient = new MockMqttClient();
            var command = new Command<CmdRequest, CmdResponse>(mqttClient, "myCmd");
            bool cmdCalled = false;
            command.OnCmdDelegate = m =>
            {
                cmdCalled = true;
                return new CmdResponse();
            };
            mqttClient.SimulateNewMessage("$iothub/methods/POST/myCmd", "{}");
            Assert.True(cmdCalled);
        }

        [Fact]
        public void ReceiveCommand_Component()
        {
            var mqttClient = new MockMqttClient();
            var command = new Command<CmdRequest, CmdResponse>(mqttClient, "myCmd", "myComp");
            bool cmdCalled = false;
            command.OnCmdDelegate = m =>
            {
                cmdCalled = true;
                return new CmdResponse();
            };
            mqttClient.SimulateNewMessage("$iothub/methods/POST/myComp*myCmd", "{}");
            Assert.True(cmdCalled);
        }
    }
}
