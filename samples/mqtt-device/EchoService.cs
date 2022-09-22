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

    internal class GetRuntimeStatsService : Commands.CommandsBase
    {
        public override Task<getRuntimeStatsResponse> getRuntimeStats(getRuntimeStatsRequest req, ServerCallContext context)
        {
            var response = new getRuntimeStatsResponse();
            var mode = req.Mode;
            if (mode == getRuntimeStatsMode.Basic)
            {
                response.DiagResults.Add("machineName", Environment.MachineName);
            }
            if (mode == getRuntimeStatsMode.Normal)
            {
                response.DiagResults.Add("machineName", Environment.MachineName);
                response.DiagResults.Add("osVersion", Environment.OSVersion.ToString());
            }
            if (mode == getRuntimeStatsMode.Full)
            {
                response.DiagResults.Add("machineName", Environment.MachineName);
                response.DiagResults.Add("osVersion", Environment.OSVersion.ToString()); response.DiagResults.Add("f", "c");
                response.DiagResults.Add("runtimeVerison", ClientFactory.NuGetPackageVersion);
            }
            return Task.FromResult(response);
        }
    }
}
