using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests.BrokerJsonBindings
{
    public class CommandBinderFixture
    {
        [Fact]
        public void CommandWithReqResp()
        {
            MockMqttClient mockMqtt = new();
            Command<int, string> cmd = new(mockMqtt, "aCmdReqResp");
            bool cmdReceived = false;
            cmd.OnMessage = async req =>
            {
                cmdReceived = true;
                return await Task.FromResult(req.ToString());
            };
            mockMqtt.SimulateNewBinaryMessage("device/mock/commands/aCmdReqResp",new UTF8JsonSerializer().ToBytes(2));
            Assert.True(cmdReceived);
            Assert.Equal("device/mock/commands/aCmdReqResp/resp", mockMqtt.topicRecceived);
            Assert.Equal("2", mockMqtt.payloadReceived);
        }

        [Fact]
        public void CommandWithReq()
        {
            MockMqttClient mockMqtt = new();
            Command<int> cmd = new(mockMqtt, "aCmdReq");
            bool cmdReceived = false;
            cmd.OnMessage = async req =>
            {
                cmdReceived = true;
                return await Task.FromResult(string.Empty);
            };
            mockMqtt.SimulateNewBinaryMessage("device/mock/commands/aCmdReq", new UTF8JsonSerializer().ToBytes(2));
            Assert.True(cmdReceived);
            Assert.Equal("device/mock/commands/aCmdReq/resp", mockMqtt.topicRecceived);
            Assert.Empty(mockMqtt.payloadReceived);
        }

        [Fact]
        public void CommandWithRes()
        {
            MockMqttClient mockMqtt = new();
            Command<string, int> cmd = new(mockMqtt, "aCmdRes");
            bool cmdReceived = false;
            cmd.OnMessage = async req =>
            {
                cmdReceived = true;
                return await Task.FromResult(1);
            };
            mockMqtt.SimulateNewBinaryMessage("device/mock/commands/aCmdRes", new UTF8JsonSerializer().ToBytes(""));
            Assert.True(cmdReceived);
            Assert.Equal("device/mock/commands/aCmdRes/resp", mockMqtt.topicRecceived);
            Assert.Equal("1", mockMqtt.payloadReceived);
        }

        [Fact]
        public void CommandEmpty()
        {
            MockMqttClient mockMqtt = new();
            Command cmd = new(mockMqtt, "aCmd");
            bool cmdReceived = false;
            cmd.OnMessage = async req =>
            {
                cmdReceived = true;
                return await Task.FromResult(string.Empty);
            };
            mockMqtt.SimulateNewBinaryMessage("device/mock/commands/aCmd", new UTF8JsonSerializer().ToBytes(""));
            Assert.True(cmdReceived);
            Assert.Equal("device/mock/commands/aCmd/resp", mockMqtt.topicRecceived);
            Assert.Empty(mockMqtt.payloadReceived);
        }
    }
}
