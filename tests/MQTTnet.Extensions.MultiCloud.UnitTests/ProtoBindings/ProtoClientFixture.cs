using mqtt_grpc_device;
using mqtt_grpc_device_protos;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests.ProtoBindings
{
    public class ProtoClientFixture
    {
        [Fact]
        public void ClientReceivesCommand()
        {
            var mockMqtt = new MockMqttClient();
            var client = new mqtt_grpc_sample_device(mockMqtt);
            bool commandReceived = false;
            client.Echo.OnMessage = async m =>
            {
                commandReceived = true;
                return await Task.FromResult(
                    new mqtt_grpc_device_protos.echoResponse() 
                    { 
                        OutEcho = m.InEcho + m.InEcho
                    });
            };
            mockMqtt.SimulateNewBinaryMessage("device/mock/cmd/echo", 
                new ProtobufSerializer().ToBytes(new echoRequest { InEcho = "hi"}));
            Assert.True(commandReceived);
            Assert.Equal("device/mock/cmd/echo/resp", mockMqtt.topicRecceived);
        }

        [Fact]
        public void ClientReceivesProp()
        {
            var mockMqtt = new MockMqttClient();
            var client = new mqtt_grpc_sample_device(mockMqtt);
            bool propReceived = false;
            client.Interval.OnMessage = async m =>
            {
                propReceived = true;
                return await Task.FromResult(
                    new mqtt_grpc_device_protos.ack()
                    {
                       Status = 200,
                       Description = "prop accepted",
                       Value = Google.Protobuf.WellKnownTypes.Any.Pack(m)
                    });
            };
            mockMqtt.SimulateNewBinaryMessage("device/mock/props/interval/set", 
                new ProtobufSerializer().ToBytes(new Properties() { Interval = 5}));
            Assert.True(propReceived);
            Assert.Equal("device/mock/props/interval/ack", mockMqtt.topicRecceived);
        }
    }
}
