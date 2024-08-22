using Newtonsoft.Json;

namespace sleeper;

public class Owner
{
    [JsonProperty("display_name")]
    public string DisplayName { get; set; } = string.Empty;
    public OwnerMetadata metadata { get; set; } = new OwnerMetadata();
    [JsonProperty("user_id")]
    public string OwnerId { get; set; } = string.Empty;
    public string TeamName
    {
        get
        {
            return metadata.Team_Name;
        }
    }
}

public class OwnerMetadata
{
    [JsonProperty("team_name")]
    public string Team_Name { get; set; } = string.Empty;
}