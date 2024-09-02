using OlympicGamesSimulator.Classes;
using OlympicGamesSimulator.Helpers;

public class Program
{
    private const string GroupsDataPath = "Data/groups.json";
    private const string ExhibitionsDataPath = "Data/exhibitions.json";

    public static void Main(string[] args)
    {
        var groups = DataLoader.LoadData<Dictionary<string, List<Team>>>(GroupsDataPath);
        var exhibitions = DataLoader.LoadData<Dictionary<string, List<Exhibition>>>(ExhibitionsDataPath);

        var simulator = new Simulator(groups, exhibitions);

        RunGroupStage(simulator);
        RunDraw(simulator);
        RunEliminationStage(simulator);
    }

    private static void RunGroupStage(Simulator simulator)
    {
        simulator.GenerateGroupStageMatches();
        simulator.SimulateGroupMatches();
        simulator.RankTeamsWithinGroup();
        simulator.RankAllTeamsGroupStage();
        simulator.DisplayGroupStage();
    }

    private static void RunDraw(Simulator simulator)
    {
        simulator.PopulateEliminationPots();
        simulator.GenerateEliminationDraw();
        simulator.DisplayDraw();
    }

    private static void RunEliminationStage(Simulator simulator)
    {
        simulator.SimulateQuarterFinal();
        simulator.SimulateSemiFinal();
        simulator.SimulateMatchForThirdPlace();
        simulator.SimulateFinal();
        simulator.DisplayEliminationStage();
    }
}
