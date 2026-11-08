using System.Collections.Generic;
using Godot;
using GodotDict = Godot.Collections.Dictionary;
using GodotArray = Godot.Collections.Array;

// Pareiza objekta/istabas apmeklējums
public class TargetVisit
{
    public string ObjectName;
    public int RoomIndex;
    public double TimeSeconds;
    public double DistanceWalkedToHere;

    public GodotDict ToDict() => new GodotDict
    {
        { "object_name", ObjectName },
        { "room_index", RoomIndex },
        { "time_seconds", TimeSeconds },
        { "distance_walked_to_here", DistanceWalkedToHere }
    };
}

// Nepareiza objekta/istabas apmeklējums
public class WrongRoomVisit
{
    public int    RoomIndex;
    public string ObjectName;
    public string TargetObjectName;
    public double TimeSeconds;

    public GodotDict ToDict() => new GodotDict
    {
        { "room_index",          RoomIndex },
        { "object_name",         ObjectName },
        { "target_object_name",  TargetObjectName },
        { "time_seconds",        TimeSeconds }
    };
}

// Dati par eksperimenta telpas apmeklējumu
public class MazeRunData
{
    public string MazeId;
    public string LayoutId;
    public GodotDict ObjectRoomAssignment = new GodotDict();
    public string[] TargetSequence = new string[3];
    public double ExplorationDurationSeconds;
    public TargetVisit[] Targets = new TargetVisit[3];
    public List<WrongRoomVisit> WrongRoomVisits = new();
    public GodotDict PostMazeSurvey = new GodotDict();

    // Pārveido datus saglabāšanai JSON 
    public GodotDict ToDict()
    {
        var targetsArr = new GodotArray();
        foreach (var t in Targets)
            targetsArr.Add(t != null ? t.ToDict() : new GodotDict());

        var wrongArr = new GodotArray();
        foreach (var w in WrongRoomVisits)
            wrongArr.Add(w.ToDict());

        var seqArr = new GodotArray();
        foreach (var s in TargetSequence)
            seqArr.Add(s ?? "");

        return new GodotDict
        {
            { "maze_id", MazeId },
            { "layout_id", LayoutId },
            { "object_room_assignment", ObjectRoomAssignment },
            { "target_sequence", seqArr },
            { "exploration_duration_seconds", ExplorationDurationSeconds },
            { "targets", targetsArr },
            { "wrong_room_visits", wrongArr },
            { "post_maze_survey", PostMazeSurvey }
        };
    }
}

// Eksperimenta visu datu objektu kopums
public class ExperimentData
{
    public string ParticipantId;
    public string ExperimentStart;
    public string[] MazeOrder;
    public string[] LayoutOrder;
    public List<MazeRunData> Mazes = new List<MazeRunData>();
    public GodotDict DemographicSurvey = new GodotDict();

    // Pārveido datus saglabāšanai JSON 
    public GodotDict ToDict()
    {
        var mazesArr = new GodotArray();
        foreach (var m in Mazes)
            mazesArr.Add(m.ToDict());

        var orderArr = new GodotArray();
        foreach (var o in MazeOrder)
            orderArr.Add(o);

        var layoutArr = new GodotArray();
        foreach (var l in LayoutOrder)
            layoutArr.Add(l);

        return new GodotDict
        {
            { "participant_id", ParticipantId },
            { "experiment_start", ExperimentStart },
            { "maze_order", orderArr },
            { "layout_order", layoutArr },
            { "mazes", mazesArr },
            { "demographic_survey", DemographicSurvey }
        };
    }
}
