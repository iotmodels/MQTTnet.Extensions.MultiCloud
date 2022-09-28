using System.IO;
using System.Text.Json;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped
{

    public class GenericPropertyAck
    {
        public int Version { get; set; }
        public int Status { get; set; }
        public string? Description { get; set; }
        public string? Value { get; set; }

        public string BuildAck()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (JsonDocument doc = JsonDocument.Parse(Value!))
                {
                    using (Utf8JsonWriter writer = new Utf8JsonWriter(ms))
                    {
                        writer.WriteStartObject();
                        //writer.WriteStartObject("properties");
                        //writer.WriteStartObject("reported");
                        foreach (var el in doc.RootElement.EnumerateObject())
                        {
                            if (!el.Name.StartsWith("$"))
                            {
                                writer.WritePropertyName(el.Name);
                                writer.WriteStartObject();
                                writer.WriteNumber("ac", Status);
                                writer.WriteNumber("av", Version);
                                writer.WriteString("ad", Description);
                                switch (el.Value.ValueKind)
                                {
                                    case JsonValueKind.String:
                                        writer.WriteString("value", el.Value.ToString());
                                        break;
                                    case JsonValueKind.Number:
                                        writer.WriteNumber("value", el.Value.GetDouble());
                                        break;
                                    case JsonValueKind.True:
                                    case JsonValueKind.False:
                                        writer.WriteBoolean("value", el.Value.GetBoolean());
                                        break;
                                    case JsonValueKind.Object:
                                        writer.WriteStartObject("value");
                                        foreach (var so in el.Value.EnumerateObject())
                                        {
                                            so.WriteTo(writer);
                                        }
                                        writer.WriteEndObject();
                                        break;
                                }
                                writer.WriteEndObject();
                            }
                        }
                        //writer.WriteEndObject();
                        //writer.WriteEndObject();
                        writer.WriteEndObject();
                        writer.Flush();
                        ms.Position = 0;
                        using (StreamReader sr = new StreamReader(ms))
                        {
                            string res = sr.ReadToEnd();
                            return res;
                        }
                    }
                }
            }
        }
    }
}