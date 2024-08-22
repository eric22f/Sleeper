using Newtonsoft.Json;

namespace sleeper;
public class Player
{
    [JsonProperty("player_id")]
    public string PlayerId { get; set; } = string.Empty;

    [JsonProperty("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonProperty("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonProperty("position")]
    public string Position { get; set; } = string.Empty;

    [JsonProperty("team")]
    public string Team { get; set; } = string.Empty;
}

public class Players : Dictionary<string, Player>
{
}