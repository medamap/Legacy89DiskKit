using System.Text.Json;

namespace Legacy89DiskKit.Fdc.Application.Hosts.Protocol;

public static class EmulatorHostProtocolCodec
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static string SerializeRequest(EmulatorHostRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return JsonSerializer.Serialize(request, SerializerOptions);
    }

    public static EmulatorHostRequest DeserializeRequest(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        return JsonSerializer.Deserialize<EmulatorHostRequest>(payload, SerializerOptions)
            ?? throw new InvalidOperationException("The emulator host request payload could not be deserialized.");
    }

    public static string SerializeResponse(EmulatorHostResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return JsonSerializer.Serialize(response, SerializerOptions);
    }

    public static EmulatorHostResponse DeserializeResponse(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        return JsonSerializer.Deserialize<EmulatorHostResponse>(payload, SerializerOptions)
            ?? throw new InvalidOperationException("The emulator host response payload could not be deserialized.");
    }

    public static string SerializeExchange(EmulatorHostExchange exchange)
    {
        ArgumentNullException.ThrowIfNull(exchange);
        return JsonSerializer.Serialize(exchange, SerializerOptions);
    }

    public static EmulatorHostExchange DeserializeExchange(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        return JsonSerializer.Deserialize<EmulatorHostExchange>(payload, SerializerOptions)
            ?? throw new InvalidOperationException("The emulator host exchange payload could not be deserialized.");
    }
}
