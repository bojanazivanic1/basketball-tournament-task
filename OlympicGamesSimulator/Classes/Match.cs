using System;
using System.Collections.Generic;
using System.Linq;

namespace OlympicGamesSimulator.Classes
{
    public class Match
    {
        public Team Team1 { get; set; } = new Team();
        public Team Team2 { get; set; } = new Team();
        public int ScoreTeam1 { get; set; } = 0;
        public int ScoreTeam2 { get; set; } = 0;
        public int Round { get; set; } = 0;
        public string Group { get; set; } = string.Empty;

        public Team Winner => ScoreTeam1 > ScoreTeam2 ? Team1 : Team2;
        public Team Loser => ScoreTeam1 < ScoreTeam2 ? Team1 : Team2;

        public Match() { }
    
        public Match(Team team1, Team team2)
        {
            Team1 = team1;
            Team2 = team2;
        }
    }
}

