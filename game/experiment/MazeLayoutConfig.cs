using Godot;

// Istabu secība, kādā tās jāapmeklē
[GlobalClass]
public partial class MazeLayoutConfig : Node3D
{
    [Export] public int[] TargetSequence = []; // Istabu indeksi
}
