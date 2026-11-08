using Godot;

// Pievieno istabas eksperimenta telpā. Nosaka istabu indeksus
public partial class RoomInstance : Node3D
{
	[Export] public int RoomIndex = 0;

	public Marker3D ObjectSpawnPoint { get; private set; }

	[Signal] public delegate void PlayerEnteredEventHandler(int roomIndex);

	private bool _playerInside = false;

	public override void _Ready()
	{
		AddToGroup("room_instance");

		ObjectSpawnPoint = FindChild<Marker3D>();
		if (ObjectSpawnPoint == null)
			GD.PrintErr($"[RoomInstance] Telpā nav atrasts Marker3D objekta izvietošanai {RoomIndex}");

		var area = FindChild<Area3D>();
		if (area != null)
		{
			area.Monitoring = true;
			if (!area.IsConnected(Area3D.SignalName.BodyEntered, Callable.From<Node3D>(OnBodyEntered)))
				area.BodyEntered += OnBodyEntered;
			if (!area.IsConnected(Area3D.SignalName.BodyExited, Callable.From<Node3D>(OnBodyExited)))
				area.BodyExited  += OnBodyExited;
			GD.Print($"[RoomInstance] Istaba {RoomIndex}: Area3D pievienots, monitoring={area.Monitoring}");
		}
		else
		{
			GD.PrintErr($"[RoomInstance] Istaba {RoomIndex}: nav atrasts Area3D.");
		}
	}

	public void ResetPlayerState()
	{
		_playerInside = false;
	}

	// Izdod signālu, ka spēlētājs iegāja istabā
	private void OnBodyEntered(Node3D body)
	{
		if (body is not CharacterBody3D) return;
		if (_playerInside) return;

		_playerInside = true;
		EmitSignal(SignalName.PlayerEntered, RoomIndex);
		GD.Print($"[RoomInstance] Spēlētājs iegāja istabā {RoomIndex}");
	}

	// Izdod signālu, ka spēlētājs izgāja no istabas
	private void OnBodyExited(Node3D body)
	{
		if (body is not CharacterBody3D) return;
		_playerInside = false;
	}

	private T FindChild<T>() where T : Node
	{
		foreach (Node n in GetChildren(true))
			if (n is T match) return match;
		return null;
	}
}
