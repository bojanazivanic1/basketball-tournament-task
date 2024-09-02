
namespace OlympicGamesSimulator.Classes
{
    public class Simulator
    {
        private Dictionary<string, Team> _allTeams = new();
        private Dictionary<string, List<Team>> _groups = new();
        private Dictionary<string, List<Team>> _pots = new();
        private List<Match> _groupStageMatches = new();
        private List<Match> _quarterFinalMatches = new();
        private List<Match> _semiFinalMatches = new();
        private Match _finalMatch = new();
        private Match _matchForThirdPlace = new();
        private Dictionary<string, List<Exhibition>> _exhibitions = new();

        public Simulator(Dictionary<string, List<Team>> groups, Dictionary<string, List<Exhibition>> exhibitions)
        {
            _groups = groups;
            _exhibitions = exhibitions;

            foreach (var group in groups)
            {
                foreach (var team in group.Value)
                {
                    _allTeams[team.Country] = team;
                    _allTeams[team.Country].Group = group.Key;
                    _allTeams[team.Country].Form = _allTeams[team.Country].CalculateTeamForm(exhibitions);

                    if (team.Country == "Srbija")
                    {
                        _allTeams[team.Country].Form += 100;
                    }
                }
            }
        }

        public void GenerateGroupStageMatches()
        {
            foreach (var group in _groups)
            {
                List<Team> teams = group.Value;

                if (teams.Count != 4)
                {
                    throw new InvalidOperationException("Grupa mora imati tačno 4 tima.");
                }

                int[,] schedule = new int[,]
                {
                    { 0, 1, 2, 3 },
                    { 0, 2, 1, 3 },
                    { 0, 3, 1, 2 }
                };

                for (int round = 0; round < 3; round++)
                {
                    CreateMatchPair(group.Key, round + 1, teams, schedule[round, 0], schedule[round, 1]);
                    CreateMatchPair(group.Key, round + 1, teams, schedule[round, 2], schedule[round, 3]);
                }
            }
        }

        private void CreateMatchPair(string groupKey, int round, List<Team> teams, int team1Index, int team2Index)
        {
            _groupStageMatches.Add(new Match
            {
                Round = round,
                Team1 = teams[team1Index],
                Team2 = teams[team2Index]
            });
        }

        public void SimulateGroupMatches()
        {
            foreach (var match in _groupStageMatches)
            {
                SimulateAndAssignScore(match);

                UpdateTeamStatus(match.Team1, match.Team2, match.ScoreTeam1, match.ScoreTeam2);
            }
        }

        public (int scoreTeam1, int scoreTeam2) SimulateMatch(Team team1, Team team2)
        {
            Random random = new Random();

            double rankDifference = team1.FibaRank - team2.FibaRank;

            double winProbabilityTeam1 = 1 / (1 + Math.Exp(-0.1 * rankDifference));

            bool team1Wins = 0.5 < winProbabilityTeam1;

            int scoreTeam1 = random.Next(70, 100);
            int scoreTeam2 = random.Next(70, 100);

            while (scoreTeam1 == scoreTeam2)
            {
                if (team1Wins)
                {
                    scoreTeam1 += 5;
                }
                else
                {
                    scoreTeam2 += 5;
                }

                if (team1.Form > team2.Form)
                {
                    scoreTeam1 += 5;
                }
                else if (team2.Form > team1.Form)
                {
                    scoreTeam2 += 5;
                }
            }

            team1.Form += scoreTeam1 - scoreTeam2;
            team2.Form += scoreTeam2 - scoreTeam1;

            return (scoreTeam1, scoreTeam2);
        }

        private void UpdateTeamStatus(Team team1, Team team2, int scoreTeam1, int scoreTeam2)
        {
            if (scoreTeam1 > scoreTeam2)
            {
                team1.Points += 2;
                team2.Points += 1;
                team1.Wins++;
                team2.Losses++;
            }
            else if (scoreTeam2 > scoreTeam1)
            {
                team1.Points += 1;
                team2.Points += 2;
                team1.Losses++;
                team2.Wins++;
            }
            else
            {
                team1.Points += 1;
                team2.Points += 1;
            }

            team1.PointsFor += scoreTeam1;
            team1.PointsAgainst += scoreTeam2;
            team2.PointsFor += scoreTeam2;
            team2.PointsAgainst += scoreTeam1;
        }

        public void RankTeamsWithinGroup()
        {
            foreach (var group in _groups)
            {
                var sortedTeams = group.Value
                    .OrderByDescending(team => team.Points)
                    .ToList();

                for (int i = 0; i < sortedTeams.Count; i++)
                {
                    sortedTeams[i].Rank = i + 1;
                }

                ResolveTiesInGroup(sortedTeams);
            }
        }

        private void ResolveTiesInGroup(List<Team> sortedTeams)
        {
            var teamsGroupedByPoints = sortedTeams
                .GroupBy(team => team.Points)
                .OrderByDescending(group => group.Key)
                .ToList();

            foreach (var groupOfTeams in teamsGroupedByPoints)
            {
                if (groupOfTeams.Count() == 1) { continue; }

                if (groupOfTeams.Count() == 2)
                {
                    var tiedTeams = groupOfTeams.ToList();

                    var match = _groupStageMatches
                        .FirstOrDefault(m => m.Team1 == tiedTeams[0] && m.Team2 == tiedTeams[1] ||
                                              m.Team1 == tiedTeams[1] && m.Team2 == tiedTeams[0])!;

                    var (winner, loser) = (match.Winner, match.Loser);

                    if (winner.Rank > loser.Rank)
                    {
                        (winner.Rank, loser.Rank) = (loser.Rank, winner.Rank);
                    }
                }

                else if (groupOfTeams.Count() == 3)
                {
                    var tiedTeams = groupOfTeams.ToList();

                    Dictionary<Team, int> scoreDifferences = new Dictionary<Team, int>();

                    foreach (var team in tiedTeams)
                    {
                        scoreDifferences[team] = 0;
                    }

                    foreach (var team1 in tiedTeams)
                    {
                        foreach (var team2 in tiedTeams)
                        {
                            if (team1 == team2) { continue; }

                            var match = _groupStageMatches
                               .FirstOrDefault(m => m.Team1 == team1 && m.Team2 == team2 ||
                                                     m.Team1 == team2 && m.Team2 == team1)!;

                            scoreDifferences[team1] += match.ScoreTeam1 - match.ScoreTeam2;
                        }
                    }

                    tiedTeams = tiedTeams
                        .OrderByDescending(t => scoreDifferences[t])
                        .ToList();

                    var currentRanks = new Dictionary<Team, int>();
                    foreach (var team in tiedTeams)
                    {
                        currentRanks[team] = team.Rank;
                    }

                    int rank = currentRanks.Values.Min();
                    foreach (var team in tiedTeams)
                    {
                        if (currentRanks[team] != rank)
                        {
                            team.Rank = rank;
                        }
                        rank++;
                    }
                }
            }
        }

        public void RankAllTeamsGroupStage()
        {
            var teamsGroupedByRank = _allTeams.Values
                .GroupBy(t => t.Rank)
                .Where(g => g.Key != 4)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var team in _allTeams.Values)
            {
                if (!teamsGroupedByRank.Values.Any(group => group.Contains(team)))
                {
                    team.Rank = 0;
                }
            }

            int currentRank = 1;

            foreach (var teamsInTheGroup in teamsGroupedByRank)
            {
                var sortedTeams = teamsInTheGroup.Value
                    .OrderByDescending(t => t.Points)
                    .ThenByDescending(t => t.PointDifference)
                    .ThenByDescending(t => t.PointsFor)
                    .ToList();

                foreach (var team in sortedTeams)
                {
                    team.Rank = currentRank++;
                }
            }
        }

        public void DisplayGroupStage()
        {
            var resultsByRoundAndGroup = _groupStageMatches
                .GroupBy(m => new { m.Round, m.Team1.Group })
                .OrderBy(g => g.Key.Round)
                .ThenBy(g => g.Key.Group);

            Console.WriteLine("Rezultati utakmica:");
            foreach (var roundGroup in resultsByRoundAndGroup)
            {
                Console.WriteLine($"Kolo {roundGroup.Key.Round}, Grupa {roundGroup.Key.Group}:");
                foreach (var match in roundGroup)
                {
                    Console.WriteLine($"{match.Team1.Country} - {match.Team2.Country} ({match.ScoreTeam1}:{match.ScoreTeam2})");
                }
                Console.WriteLine();
            }

            var groupedTeams = _allTeams.Values
                .GroupBy(t => t.Group)
                .OrderBy(g => g.Key);

            Console.WriteLine("Konačan plasman u grupama:");
            foreach (var group in groupedTeams)
            {
                Console.WriteLine($"Grupa {group.Key}:");
                var sortedTeams = group.OrderByDescending(t => t.Points)
                                       .ThenByDescending(t => t.PointDifference)
                                       .ThenByDescending(t => t.PointsFor);

                int rank = 1;
                foreach (var team in sortedTeams)
                {
                    Console.WriteLine($"{rank}. {team.Country} - {team.Wins} / {team.Losses} / {team.Points} / {team.PointsFor} / {team.PointsAgainst} / {team.PointDifference}");
                    rank++;
                }
                Console.WriteLine();
            }

            var topTeams = _allTeams.Values
                .Where(t => t.Rank >= 1 && t.Rank <= 8)
                .OrderBy(t => t.Rank)
                .Select(t => new { t.Rank, t.Country })
                .ToList();

            Console.WriteLine("Konačan plasman u eliminacionu fazu:");
            foreach (var team in topTeams)
            {
                Console.WriteLine($"{team.Rank}. {team.Country}");
            }
        }

        public void PopulateEliminationPots()
        {
            var eliminationStageTeams = _allTeams
                .Where(kvp => kvp.Value.Rank >= 1 && kvp.Value.Rank <= 8)
                .OrderBy(kvp => kvp.Value.Rank)
                .Select(kvp => kvp.Value)
                .ToList();

            _pots = new Dictionary<string, List<Team>>
            {
                { "D", new List<Team>() { eliminationStageTeams[0], eliminationStageTeams[1] } },
                { "E", new List<Team>() { eliminationStageTeams[2], eliminationStageTeams[3] } },
                { "F", new List<Team>() { eliminationStageTeams[4], eliminationStageTeams[5] } },
                { "G", new List<Team>() { eliminationStageTeams[6], eliminationStageTeams[7] } }
            };
        }

        public void GenerateEliminationDraw()
        {
            List<Match> temporaryQuarterFinalMatches = new List<Match>();

            GenerateQuarterFinalMatches("D", "G", temporaryQuarterFinalMatches);
            GenerateQuarterFinalMatches("E", "F", temporaryQuarterFinalMatches);

            _quarterFinalMatches.Add(temporaryQuarterFinalMatches[0]);
            _quarterFinalMatches.Add(temporaryQuarterFinalMatches[2]);
            _quarterFinalMatches.Add(temporaryQuarterFinalMatches[1]);
            _quarterFinalMatches.Add(temporaryQuarterFinalMatches[3]);
        }

        private void GenerateQuarterFinalMatches(string pot1Key, string pot2Key, List<Match> temporaryQuarterFinalMatches)
        {
            if (HavePlayedBefore(_pots[pot1Key][0], _pots[pot2Key][0]) || HavePlayedBefore(_pots[pot1Key][1], _pots[pot2Key][1]))
            {
                temporaryQuarterFinalMatches.Add(new Match(_pots[pot1Key][0], _pots[pot2Key][1]));
                temporaryQuarterFinalMatches.Add(new Match(_pots[pot1Key][1], _pots[pot2Key][0]));
            }
            else
            {
                temporaryQuarterFinalMatches.Add(new Match(_pots[pot1Key][0], _pots[pot2Key][0]));
                temporaryQuarterFinalMatches.Add(new Match(_pots[pot1Key][1], _pots[pot2Key][1]));
            }
        }

        private bool TryGenerateDraw(Dictionary<string, List<Team>> potsCopy, List<Match> temporaryMatches)
        {
            if (temporaryMatches.Count == 4)
            {
                _quarterFinalMatches.AddRange(temporaryMatches);
                return true;
            }

            var (potA, potB) = temporaryMatches.Count < 2 ? ("D", "G") : ("E", "F");
            var currentPotA = potsCopy[potA];
            var currentPotB = potsCopy[potB];

            foreach (var teamA in currentPotA.ToList())
            {
                var possibleOpponents = currentPotB.Where(t => !HavePlayedBefore(teamA, t)).ToList();
                foreach (var opponent in possibleOpponents)
                {
                    var match = new Match(teamA, opponent);
                    temporaryMatches.Add(match);
                    currentPotA.Remove(teamA);
                    currentPotB.Remove(opponent);

                    if (TryGenerateDraw(potsCopy, temporaryMatches))
                    {
                        return true;
                    }

                    temporaryMatches.Remove(match);
                    currentPotA.Add(teamA);
                    currentPotB.Add(opponent);
                }
            }

            return false;
        }

        private bool HavePlayedBefore(Team team1, Team team2) =>
             team1.Group == team2.Group;

        public void DisplayDraw()
        {
            Console.WriteLine("\nŠeširi:");
            foreach (var pot in _pots)
            {
                Console.WriteLine($"    Šešir {pot.Key}");
                foreach (var team in pot.Value)
                {
                    Console.WriteLine($"        {team.Country}");
                }
            }

            Console.WriteLine("\nEliminaciona faza:");
            for (int i = 0; i < _quarterFinalMatches.Count; i++)
            {
                Console.WriteLine($"    {_quarterFinalMatches[i].Team1.Country} - {_quarterFinalMatches[i].Team2.Country}");

                if (i == 1)
                {
                    Console.WriteLine();
                }
            }
        }

        public void SimulateAndAssignScore(Match match)
        {
            var (scoreTeam1, scoreTeam2) = SimulateMatch(match.Team1, match.Team2);
            match.ScoreTeam1 = scoreTeam1;
            match.ScoreTeam2 = scoreTeam2;
        }

        public void SimulateQuarterFinal()
        {
            SimulateAndAssignScore(_quarterFinalMatches[0]);
            SimulateAndAssignScore(_quarterFinalMatches[1]);

            _semiFinalMatches.Add(new Match(_quarterFinalMatches[0].Winner, _quarterFinalMatches[1].Winner));

            SimulateAndAssignScore(_quarterFinalMatches[2]);
            SimulateAndAssignScore(_quarterFinalMatches[3]);

            _semiFinalMatches.Add(new Match(_quarterFinalMatches[2].Winner, _quarterFinalMatches[3].Winner));
        }

        public void SimulateSemiFinal()
        {
            SimulateAndAssignScore(_semiFinalMatches[0]);
            SimulateAndAssignScore(_semiFinalMatches[1]);

            _finalMatch = new Match(_semiFinalMatches[0].Winner, _semiFinalMatches[1].Winner);
            _matchForThirdPlace = new Match(_semiFinalMatches[0].Loser, _semiFinalMatches[1].Loser);
        }

        public void SimulateFinal()
        {
            SimulateAndAssignScore(_finalMatch);
        }

        public void SimulateMatchForThirdPlace()
        {
            SimulateAndAssignScore(_matchForThirdPlace);
        }

        public void DisplayEliminationStage()
        {
            Console.WriteLine("\nČetvrtfinale:");
            Console.WriteLine($"{_quarterFinalMatches[0].Team1.Country} - {_quarterFinalMatches[0].Team2.Country} ({_quarterFinalMatches[0].ScoreTeam1}: {_quarterFinalMatches[0].ScoreTeam2})");
            Console.WriteLine($"{_quarterFinalMatches[1].Team1.Country} - {_quarterFinalMatches[1].Team2.Country} ({_quarterFinalMatches[1].ScoreTeam1}: {_quarterFinalMatches[1].ScoreTeam2})");
            Console.WriteLine($"{_quarterFinalMatches[2].Team1.Country} - {_quarterFinalMatches[2].Team2.Country} ({_quarterFinalMatches[2].ScoreTeam1}: {_quarterFinalMatches[2].ScoreTeam2})");
            Console.WriteLine($"{_quarterFinalMatches[3].Team1.Country} - {_quarterFinalMatches[3].Team2.Country} ({_quarterFinalMatches[3].ScoreTeam1}: {_quarterFinalMatches[3].ScoreTeam2})");

            Console.WriteLine("\nPolufinale:");
            Console.WriteLine($"{_semiFinalMatches[0].Team1.Country} - {_semiFinalMatches[0].Team2.Country} ({_semiFinalMatches[0].ScoreTeam1}: {_semiFinalMatches[0].ScoreTeam2})");
            Console.WriteLine($"{_semiFinalMatches[1].Team1.Country} - {_semiFinalMatches[1].Team2.Country} ({_semiFinalMatches[1].ScoreTeam1}: {_semiFinalMatches[1].ScoreTeam2})");

            Console.WriteLine("\nUtakmica za treće mjesto:");
            Console.WriteLine($"{_matchForThirdPlace.Team1.Country} - {_matchForThirdPlace.Team2.Country} ({_matchForThirdPlace.ScoreTeam1}: {_matchForThirdPlace.ScoreTeam2})");

            Console.WriteLine("\nFinale:");
            Console.WriteLine($"{_finalMatch.Team1.Country} - {_finalMatch.Team2.Country} ({_finalMatch.ScoreTeam1}: {_finalMatch.ScoreTeam2})");

            Console.WriteLine("\nMedalje:");
            Console.WriteLine($"1. {_finalMatch.Winner.Country}");
            Console.WriteLine($"2. {_finalMatch.Loser.Country}");
            Console.WriteLine($"3. {_matchForThirdPlace.Winner.Country}");
        }
    }
}
