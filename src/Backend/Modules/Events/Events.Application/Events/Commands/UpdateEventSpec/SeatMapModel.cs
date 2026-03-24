using System.Text.Json;
using System.Text.Json.Serialization;

namespace Events.Application.Events.Commands.UpdateEventSpec;

public sealed class SeatMapModel
{
    [JsonPropertyName("areas")]
    public List<SeatMapArea> Areas { get; set; } = [];

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public sealed class SeatMapArea
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SeatMapAreaType Type { get; set; }

    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    [JsonPropertyName("width")]
    public float Width { get; set; }

    [JsonPropertyName("height")]
    public float Height { get; set; }

    [JsonPropertyName("rotation")]
    public float Rotation { get; set; }

    [JsonPropertyName("stroke")]
    public string? Stroke { get; set; }

    [JsonPropertyName("ticketTypeId")]
    public string? TicketTypeId { get; set; }

    [JsonPropertyName("price")]
    public int Price { get; set; }

    [JsonPropertyName("labelFontSize")]
    public int LabelFontSize { get; set; }

    [JsonPropertyName("draggable")]
    public bool Draggable { get; set; }

    [JsonPropertyName("isAreaType")]
    public bool IsAreaType { get; set; }

    [JsonPropertyName("fill")]
    public string? Fill { get; set; }

    [JsonPropertyName("points")]
    public List<int>? Points { get; set; }

    [JsonPropertyName("seats")]
    public List<SeatMapSeat>? Seats { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

public sealed class SeatMapSeat
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("sectionId")]
    public string SectionId { get; set; } = string.Empty;

    [JsonPropertyName("row")]
    public string Row { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    [JsonPropertyName("width")]
    public float Width { get; set; }

    [JsonPropertyName("height")]
    public float Height { get; set; }

    [JsonPropertyName("rotation")]
    public float Rotation { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "available";

    [JsonPropertyName("fill")]
    public string? Fill { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    // Helper to convert status string → SeatMapSeatStatus
    [JsonIgnore]
    public SeatMapSeatStatus ParsedStatus => Status switch
    {
        "blocked" => SeatMapSeatStatus.blocked,
        _ => SeatMapSeatStatus.available
    };
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SeatMapSeatStatus
{
    available,
    blocked
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SeatMapAreaType
{
    rect,
    square,
    circle,
    triagle,
    parallelogram,
    trapezoid,
    polygon
}
