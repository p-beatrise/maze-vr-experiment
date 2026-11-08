using Godot;

// Kāsas filtrs un istabās izvietojamie objekti
[GlobalClass]
public partial class MazeConditionConfig : Resource
{
	[Export] public string        MazeId           = ""; 
	[Export] public Color         FilterColor       = new(0f, 0f, 0f, 0f);
	[Export] public string[]      RoomObjectNames   = [];
	[Export] public PackedScene[] RoomObjectScenes  = [];
}
