using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.Converters;

public class IpAddressConverter : JsonConverter<IPAddress>
{
    public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var ipString = reader.GetString() ?? "0.0.0.0/0.0.0.0";
        
        return IPAddress.Parse(ipString);
    }

    public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}