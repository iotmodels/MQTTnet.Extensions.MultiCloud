using Avro.IO;
using Avro.Specific;

namespace MQTTnet.Extensions.MultiCloud.Serializers;

public class AvroSerializer : IMessageSerializer
{
    private readonly Avro.Schema schema;
    public AvroSerializer(Avro.Schema s)
    {
        schema = s;
    }

    public byte[] ToBytes<T>(T payload, string name = "")
    {
        using MemoryStream ms = new();
        BinaryEncoder encoder = new(ms);
        SpecificDefaultWriter writer = new(schema);
        writer.Write(payload, encoder);
        ms.Position = 0;
        byte[] bytes = ms.ToArray();
        return bytes;
    }

    public bool TryReadFromBytes<T>(byte[] payload, string name, out T result)
    {
        using MemoryStream mem = new(payload);
        BinaryDecoder decoder = new(mem);
        SpecificDefaultReader reader = new(schema, schema);
        T val = default!;
        reader.Read(val, decoder);
        result = val;
        return true;
    }
}
