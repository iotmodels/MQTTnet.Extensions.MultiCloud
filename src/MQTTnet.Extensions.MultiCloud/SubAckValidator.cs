using MQTTnet.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MQTTnet.Extensions.MultiCloud
{
    public static class SubAckExtensions
    {
        public static void TraceErrors(this MqttClientSubscribeResult subAck)
        {
            subAck.Items?
                .Where(s => (int)s.ResultCode > 0x02)
                .ToList()
                .ForEach( i => Trace.TraceWarning($"{i.TopicFilter.Topic} {i.ResultCode}"));
        }
    }
}
