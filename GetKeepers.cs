using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace sleeper;
public static class GetKeepers
{
    private static readonly HttpClient httpClient = new();

    [Function("GetKeepers")]
    public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "keepers/{leagueId}")] HttpRequest req
      , string leagueId
      , FunctionContext context)
    {
        var corrolationId = context.TraceContext?.TraceParent?.ToString() ?? new Guid().ToString();
        var log = context.GetLogger("GetKeepers");
        log.LogInformation("[{corrolationId}] GetKeepers request received for leagueId: {leagueId}", corrolationId, leagueId);

        // Construct the API URL using the leagueId
        string url = $"https://api.sleeper.app/v1/league/{leagueId}/rosters";
        try
        {
            // Make an HTTP GET request to the Sleeper API
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // Read the response content as a string
            string json = await response.Content.ReadAsStringAsync();

            // Get our rosters
            var rosters = JsonConvert.DeserializeObject<List<Roster>>(json) ?? throw new Exception("Error deserializing rosters from Sleeper API response");

            // Get our owners
            url = $"https://api.sleeper.app/v1/league/{leagueId}/users";
            response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // Read the response content as a string
            json = await response.Content.ReadAsStringAsync();
            var owners = JsonConvert.DeserializeObject<List<Owner>>(json) ?? throw new Exception("Error deserializing owners from Sleeper API response");

            // Get all Rosters where Keeper count > 0
            rosters = rosters.Where(r => r.Keepers != null && r.Keepers.Count > 0).ToList();
            if (rosters == null || rosters.Count == 0)
            {
                string message = "No keepers found for leagueId: " + leagueId;
                log.LogInformation("[{corrolationId}] {message}", corrolationId, message);
                return new OkObjectResult(message);
            }

            // Get the players
            url = $"https://api.sleeper.app/v1/players/nfl";
            response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var players = JsonConvert.DeserializeObject<Players>(await response.Content.ReadAsStringAsync()) ?? throw new Exception("Error deserializing players from Sleeper API response");

            // Return KeeperResult from the keepers with the owner and player names
            var keeperResults = new List<KeeperResult>();
            foreach (var roster in rosters)
            {
                var owner = owners.FirstOrDefault(o => o.OwnerId == roster.Owner_Id) ?? throw new Exception("Owner Id not found: " + roster.Owner_Id);

                foreach (var playerId in roster.Keepers)
                {
                    if (!players.ContainsKey(playerId))
                    {
                        throw new Exception("Player Id not found: " + playerId);
                    }
                    var player = players[playerId];

                    keeperResults.Add(new KeeperResult
                    {
                        Owner = owner.DisplayName,
                        TeamName = owner.TeamName,
                        Player = player.FirstName + " " + player.LastName,
                        Position = player.Position,
                        Team = player.Team
                    });
                }
            }

            var results = new StringBuilder();
            foreach (var keeper in keeperResults.OrderBy(k => k.Owner).ThenBy(k => k.Player))
            {
                results.AppendLine($"{keeper.Owner} ({keeper.TeamName}): {keeper.Player} ({keeper.Position} - {keeper.Team})");
            }

            // Return the response
            return new OkObjectResult(results.ToString());
        }
        catch (Exception ex)
        {
            string message = $"Error fetching keeper data from Sleeper: {ex.Message}\n\nCorrolation Id: {corrolationId}";
            log.LogError("[{corrolationId}] {message}", corrolationId, message);
            return new ObjectResult(message) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }
}
