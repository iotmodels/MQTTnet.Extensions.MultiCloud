﻿using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests
{
    public class ConnectionSettingsFixture
    {
        [Fact]
        public void DefaultValues()
        {
            var dcs = new ConnectionSettings();
            Assert.Equal(60, dcs.SasMinutes);
            Assert.Equal(60, dcs.KeepAliveInSeconds);
            Assert.Equal(AuthType.Basic, dcs.Auth);
            Assert.Equal(8883, dcs.TcpPort);
            Assert.False(dcs.DisableCrl);
            Assert.True(dcs.UseTls);
            Assert.Equal("TcpPort=8883;Auth=Basic;MqttVersion=5", dcs.ToString());
        }

        [Fact]
        public void GetAuthType()
        {
            Assert.Equal(AuthType.X509, new ConnectionSettings { X509Key = "key" }.Auth);
            Assert.Equal(AuthType.Sas, new ConnectionSettings { SharedAccessKey = "key" }.Auth);
            Assert.Equal(AuthType.Basic, new ConnectionSettings { }.Auth);
        }

        //[Fact]
        //public void InferClientIdFromUserName()
        //{
        //    Assert.Equal("user", new ConnectionSettings { UserName = "user" }.ClientId);
        //    Assert.Equal("client", new ConnectionSettings { UserName = "user", ClientId = "client" }.ClientId);
        //}

        //[Fact]
        //public void InferClientIdFromCert()
        //{
        //    Assert.Equal("onething", new ConnectionSettings { X509Key = "onething.pem|onething.key" }.ClientId);
        //    Assert.Equal("client", new ConnectionSettings { X509Key = "onething.pem|onething.key",  ClientId = "client" }.ClientId);
        //}


        [Fact]
        public void ParseConnectionString()
        {
            string cs = "HostName=<hubname>.azure-devices.net;DeviceId=<deviceId>;SharedAccessKey=<SasKey>";
            ConnectionSettings dcs = ConnectionSettings.FromConnectionString(cs);
            Assert.Equal("<hubname>.azure-devices.net", dcs.HostName);
            Assert.Equal("<deviceId>", dcs.DeviceId);
            Assert.Equal("<SasKey>", dcs.SharedAccessKey);
            Assert.Empty(dcs.ClientId!);
        }

        [Fact]
        public void InvalidValuesDontUseDefaults()
        {
            string cs = "HostName=<hubname>.azure-devices.net;DeviceId=<deviceId>;SharedAccessKey=<SasKey>;MaxRetries=-2;SasMinutes=aa;RetryInterval=4.3";
            ConnectionSettings dcs = new(cs);
            Assert.Equal("<hubname>.azure-devices.net", dcs.HostName);
            Assert.Equal("<deviceId>", dcs.DeviceId);
            Assert.Equal("<SasKey>", dcs.SharedAccessKey);
            Assert.Equal(60, dcs.SasMinutes);
        }


        [Fact]
        public void ParseConnectionStringWithDefaultValues()
        {
            string cs = "HostName=<hubname>.azure-devices.net;DeviceId=<deviceId>;ModuleId=<moduleId>;SharedAccessKey=<SasKey>";
            ConnectionSettings dcs = ConnectionSettings.FromConnectionString(cs);
            Assert.Equal("<hubname>.azure-devices.net", dcs.HostName);
            Assert.Equal("<deviceId>", dcs.DeviceId);
            Assert.Equal("<moduleId>", dcs.ModuleId);
            Assert.Equal("<SasKey>", dcs.SharedAccessKey);
            Assert.Equal(60, dcs.SasMinutes);
            Assert.Equal(60, dcs.KeepAliveInSeconds);
            Assert.Equal(8883, dcs.TcpPort);
            Assert.Empty(dcs.ClientId!);
            Assert.True(dcs.UseTls);
            Assert.False(dcs.DisableCrl);
        }

        [Fact]
        public void ParseConnectionStringWithAllValues()
        {
            string cs = "HostName=<hubname>.azure-devices.net;DeviceId=<deviceId>;ClientId=<ClientId>;ModuleId=<moduleId>;SharedAccessKey=<SasKey>;SasMinutes=2;TcpPort=1234;UseTls=false;CaFile=<path>;DisableCrl=true;UserName=<usr>;Password=<pwd>;MqttVersion=3;MqttGatewayHostName=mqtt";

            ConnectionSettings dcs = ConnectionSettings.FromConnectionString(cs);
            Assert.Equal("<hubname>.azure-devices.net", dcs.HostName);
            Assert.Equal("<deviceId>", dcs.DeviceId);
            Assert.Equal("<moduleId>", dcs.ModuleId);
            Assert.Equal("<SasKey>", dcs.SharedAccessKey);
            Assert.Equal("<ClientId>", dcs.ClientId);
            Assert.Equal("<usr>", dcs.UserName);
            Assert.Equal("<pwd>", dcs.Password);
            Assert.Equal(2, dcs.SasMinutes);
            Assert.Equal(1234, dcs.TcpPort);
            Assert.False(dcs.UseTls);
            Assert.Equal("<path>", dcs.CaFile);
            Assert.True(dcs.DisableCrl);
            Assert.Equal(3, dcs.MqttVersion);
            Assert.Equal("mqtt", dcs.MqttGatewayHostName);
        }

        [Fact]
        public void ToStringReturnConnectionString()
        {
            ConnectionSettings dcs = new()
            {
                HostName = "h",
                DeviceId = "d",
                SharedAccessKey = "sas",
                ModelId = "dtmi",
                MqttVersion = 3
            };
            string expected = "HostName=h;TcpPort=8883;DeviceId=d;SharedAccessKey=***;ModelId=dtmi;Auth=Sas;MqttVersion=3";
            Assert.Equal(expected, dcs.ToString());
        }

        [Fact]
        public void ToStringReturnConnectionStringWithModule()
        {
            ConnectionSettings dcs = new()
            {
                HostName = "h",
                DeviceId = "d",
                ModuleId = "m",
                SharedAccessKey = "sas"
            };
            string expected = "HostName=h;TcpPort=8883;DeviceId=d;ModuleId=m;SharedAccessKey=***;Auth=Sas;MqttVersion=5";
            Assert.Equal(expected, dcs.ToString());
        }

        [Fact]
        public void InvalidMqttVersionThrowsException()
        {
            string cs = "HostName=<hubname>.azure-devices.net;DeviceId=<deviceId>;SharedAccessKey=<SasKey>;MaxRetries=-2;SasMinutes=aa;MqttVersion=4";
            try
            {
                ConnectionSettings dcs = new(cs);
                Assert.Fail("should throw");
            }
            catch (ApplicationException ex)
            {
                Assert.Equal("Invalid Mqtt Version 4", ex.Message);
                
            }
            
        }
    }
}
