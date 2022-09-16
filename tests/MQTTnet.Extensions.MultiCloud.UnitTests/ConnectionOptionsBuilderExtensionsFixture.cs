using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests
{
    public class ConnectionOptionsBuilderExtensionsFixture
    {
        [Fact]
        public void InferClientFromUserNameWhenNotSet()
        {
            MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder();
            var cs = new ConnectionSettings { UserName = "user", Password = "password" };
            builder.WithConnectionSettings(cs);
            Assert.Equal("user", cs.ClientId);
        }

        [Fact]
        public void BasicAuthRespectedClientId()
        {
            MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder();
            var cs = new ConnectionSettings { UserName = "user", Password = "password", ClientId = "client" };
            builder.WithConnectionSettings(cs);
            Assert.Equal("client", cs.ClientId);
        }

        [Fact]
        public void ClientIDMachineNanme()
        {
            MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder();
            var cs = new ConnectionSettings { UserName = "user", Password = "password", ClientId = "client" };
            builder.WithConnectionSettings(cs);
            Assert.Equal("client", cs.ClientId);

            cs = new ConnectionSettings { UserName = "user", Password = "password", ClientId = "{machineName}" };
            builder.WithConnectionSettings(cs);
            Assert.Equal(Environment.MachineName, cs.ClientId);

            cs = new ConnectionSettings { ClientId = "{machineName}" };
            builder.WithConnectionSettings(cs);
            Assert.Equal(Environment.MachineName, cs.ClientId);

            cs = new ConnectionSettings { X509Key="onething.pfx|1234", ClientId = "{machineName}" };
            builder.WithConnectionSettings(cs);
            Assert.Equal(Environment.MachineName, cs.ClientId);

            cs = new ConnectionSettings { X509Key = "onething.pfx|1234", };
            builder.WithConnectionSettings(cs);
            Assert.Equal("onething", cs.ClientId);

        }

        [Fact]
        public void InferClientFromCertWhenNotSet()
        {
            MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder();
            var cs = new ConnectionSettings { X509Key = "onething.pfx|1234" };
            builder.WithConnectionSettings(cs);
            Assert.Equal("onething", cs.ClientId);
        }

        [Fact]
        public void X509AuthRespectedClientId()
        {
            MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder();
            var cs = new ConnectionSettings { X509Key = "onething.pfx|1234", ClientId = "client" };
            builder.WithConnectionSettings(cs);
            Assert.Equal("client", cs.ClientId);
        }
    }
}
