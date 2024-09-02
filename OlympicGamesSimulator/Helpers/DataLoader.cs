using System.Text.Json;

namespace OlympicGamesSimulator.Helpers
{
    public static class DataLoader
    {
        public static T LoadData<T>(string path)
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path))!;
        }
    }
}
