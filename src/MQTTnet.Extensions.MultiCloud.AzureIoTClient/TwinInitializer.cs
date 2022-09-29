using MQTTnet.Client;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient
{
    public class TwinInitializer
    {
        public static async Task InitPropertyValue<T>(IMqttClient client, string twin, IWritableProperty<T> prop, string propName, T defaultValue)
        {
            var ack = InitFromTwin(twin, propName, defaultValue);
            Ack<T> acceptedAck;
            if (prop.OnMessage != null)
            {
                acceptedAck = await prop.OnMessage.Invoke(ack.Value);
            }
            else
            {
                acceptedAck = ack;
            }
            acceptedAck.Status = 203;
            acceptedAck.Value = ack.Value;
            acceptedAck.Version = 0;
            acceptedAck.Description = "Init from default value";
            var roBinder = new ReadOnlyProperty<Ack<T>>(client, propName);
            await roBinder.SendMessageAsync(acceptedAck);
        }

        private static Ack<T> InitFromTwin<T>(string twinJson, string propName, T defaultValue)
        {
            if (string.IsNullOrEmpty(twinJson))
            {
                Trace.TraceWarning("InitFromTwin: Cannot initialize from empty twin");
                return new Ack<T>() { Value = defaultValue, Version = 0, Status = 203, Description = "Init from default value" };
            }

            JsonNode? root = JsonNode.Parse(twinJson);
            JsonNode? desired = root?["desired"];
            JsonNode? reported = root?["reported"];
            T desired_Prop = default!;
            int desiredVersion = desired!["$version"]!.GetValue<int>();
            Ack<T> result = new() { };

            bool desiredFound = false;
            if (desired[propName] != null)
            {
                desired_Prop = desired![propName]!.Deserialize<T>()!;
                desiredFound = true;
            }


            bool reportedFound = false;
            T reported_Prop = default!;
            int reported_Prop_version = 0;
            int reported_Prop_status = 001;
            string reported_Prop_description = string.Empty;

            if (reported![propName] != null)
            {
                reported_Prop = reported[propName]!["value"]!.Deserialize<T>()!;

                reported_Prop_version = reported[propName]!["av"]?.GetValue<int>() ?? -1;
                reported_Prop_status = reported![propName]!["ac"]!.GetValue<int>();
                reported_Prop_description = reported![propName]!["ad"]?.GetValue<string>()!;
                reportedFound = true;
            }


            if (!desiredFound && !reportedFound)
            {
                result = new Ack<T>()
                {
                    //DesiredVersion = desiredVersion,
                    Version = reported_Prop_version,
                    Value = defaultValue,
                    Status = 203,
                    Description = "Init from default value"
                };
            }

            if (!desiredFound && reportedFound)
            {
                result = new Ack<T>()
                {
                    Version = reported_Prop_version,
                    Value = reported_Prop,
                    Status = reported_Prop_status,
                    Description = reported_Prop_description,

                };
            }

            if (desiredFound && reportedFound)
            {
                if (desiredVersion >= reported_Prop_version)
                {
                    result = new Ack<T>()
                    {
                        Value = desired_Prop,
                        Version = desiredVersion,
                    };
                }
            }


            if (desiredFound && !reportedFound)
            {
                result = new Ack<T>()
                {
                    Version = desiredVersion,
                    Value = desired_Prop
                };
            }
            return result;
        }
    }
}
