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
    private static readonly HttpClient httpClient = new HttpClient();

    [Function("GetKeepers")]
    public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "keepers/{leagueId}")] HttpRequest req
      , string leagueId
      , FunctionContext context)
    {
        var log = context.GetLogger("GetKeepers");
        log.LogInformation("GetKeepers request received for leagueId: {leagueId}", leagueId);

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
            var rosters = JsonConvert.DeserializeObject<List<Roster>>(json);
            if (rosters == null)
            {
                log.LogError("Error deserializing rosters from Sleeper API response");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            // Get our owners
            url = $"https://api.sleeper.app/v1/league/{leagueId}/users";
            response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // Read the response content as a string
            json = await response.Content.ReadAsStringAsync();
            var owners = JsonConvert.DeserializeObject<List<Owner>>(json);
            if (owners == null)
            {
                log.LogError("Error deserializing owners from Sleeper API response");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            // Get all Rosters where Keeper count > 0
            rosters = rosters.Where(r => r.Keepers != null && r.Keepers.Count > 0).ToList();
            if (rosters == null || rosters.Count == 0)
            {
                log.LogInformation("No keepers found for leagueId: {leagueId}", leagueId);
                return new OkObjectResult(new List<Roster>());
            }

            // Get the players
            url = $"https://api.sleeper.app/v1/players/nfl";
            response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var players = JsonConvert.DeserializeObject<Players>(await response.Content.ReadAsStringAsync());
            if (players == null)
            {
                log.LogError("Error deserializing players from Sleeper API response");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            // Return KeeperResult from the keepers with the owner and player names
            var keeperResults = new List<KeeperResult>();
            foreach (var roster in rosters)
            {
                var owner = owners.FirstOrDefault(o => o.OwnerId == roster.Owner_Id);
                if (owner == null)
                {
                    log.LogError("Owner Id not found: {ownerId}", roster.Owner_Id);
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                foreach (var playerId in roster.Keepers)
                {
                    if (!players.ContainsKey(playerId))
                    {
                        log.LogError("Player Id not found: {playerId}", playerId);
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
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
        catch (HttpRequestException ex)
        {
            log.LogError($"Error fetching data from Sleeper API: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
