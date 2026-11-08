using Godot;

// Saglabā eksperimenta datus JSON failā maze_experiment mapē
public static class DataLogger
{
    public static void Save(ExperimentData data, string participantId)
    {
        // Izveido folderi, ja tā nav
        string baseDir = OS.GetSystemDir(OS.SystemDir.Documents);
        string dataDir = baseDir + "/maze_experiment";
        DirAccess.MakeDirRecursiveAbsolute(dataDir);

        string filePath = $"{dataDir}/{participantId}.json";
        string json = Json.Stringify(data.ToDict(), "\t");

        // Raksta JSON failu
        using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(json);
            GD.Print($"[DataLogger] Fails saglabāts: {filePath}");
        }
        else
        {
            GD.PrintErr($"[DataLogger] Neizdevās atvērt failu: {FileAccess.GetOpenError()}");
        }
    }
}
