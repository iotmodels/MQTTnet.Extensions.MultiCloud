using Grpc.Core;
using mqttdevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mqtt_device
{
    internal class EchoService : Commands.CommandsBase
    {
        public override Task<echoResponse> echo(echoRequest req, ServerCallContext context)
        {
            return Task.FromResult(new echoResponse
            {
                OutEcho = req.InEcho + req.InEcho
            });
        }
    }
}
