using MQTTnet.Client;
using System;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.GrpcBindings
{
    public abstract class RpcTopicBinder
    {
        public Func<byte[], Task<byte[]>>? OnCallbackDelegate { get; set; }
    }
}
