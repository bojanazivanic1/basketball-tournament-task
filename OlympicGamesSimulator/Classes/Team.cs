using System.Text.Json.Serialization;

namespace OlympicGamesSimulator.Classes
{
    public class Team
    {
        [JsonPropertyName("Team")]
        public string Country { get; set; } = string.Empty;
        [JsonPropertyName("ISOCode")]
        public string IsoCode { get; set; } = string.Empty;
        [JsonPropertyName("FIBARanking")]
        public int FibaRank { get; set; } = 0;

        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int Points { get; set; } = 0;
        public int PointsFor { get; set; } = 0;
        public int PointsAgainst { get; set; } = 0;
        public string Group { get; set; } = string.Empty;
        public int Rank {  get; set; } = 0;
        public int Form {  get; set; } = 0;

        public int PointDifference => PointsFor - PointsAgainst;

        public int CalculateTeamForm(Dictionary<string, List<Exhibition>> exhibitions)
        {
            int form = 0;
            if (exhibitions.TryGetValue(IsoCode, out var results))
            {
                foreach (var result in results)
                {
                    var scores = result.Result.Split('-');
                    int teamScore = int.Parse(scores[0]);
                    int opponentScore = int.Parse(scores[1]);
                    form += (teamScore - opponentScore); 
                }
            }
            return form;
        }
    }
}
