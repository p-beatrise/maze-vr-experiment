using Godot;
using System.Collections.Generic;

// PKlase labirinta pārvaldīšanai
public partial class MazeManager : Node3D
{
	private string        _mazeId          = "unknown";
	private string[]      _roomObjectNames  = [];
	private PackedScene[] _roomObjectScenes = [];
	private Color         _filterColor      = new(0f, 0f, 0f, 0f);

	private Marker3D      _spawnPoint;

	// Istabu secība, kādā spēlētājam tās jāapmeklē
	public int[] TargetSequence = [];

	private RoomInstance[] _rooms;

	private enum MazePhase { Exploration, Timed }
	private MazePhase _phase = MazePhase.Exploration;

	private int    _currentTargetIndex = 0;
	private double _explorationStart;
	private double _timedPhaseStart;

	private readonly MazeRunData _runData = new();
	private HUDManager  _hud;

	private Node3D  _playerBody;
	private Vector3 _lastPlayerPos;
	private double  _segmentDistance;

	public override void _Ready()
	{
		_hud = GetTree().GetFirstNodeInGroup("hud") as HUDManager;
		_playerBody = GetTree().GetFirstNodeInGroup("player_body") as Node3D;
		_explorationStart = Time.GetTicksMsec() / 1000.0;

		LoadCondition();
		SwapMazeLayout();
		TeleportPlayerToSpawn();
		DiscoverRooms();
		SpawnObjects();
		ConnectRoomTriggers();
		ApplyColorFilter();

		_hud?.ShowMessage("Izpētiet labirintu.\nAtceraties, kur atrodas katrs objekts.\nSpiediet pogu 'Sākt meklēt', lai sāktu.", 10);
	}

	// Nosaka krāsas filtru un objektu sarakstu no ExperimentManager
	private void LoadCondition()
	{
		var condition = ExperimentManager.Instance?.CurrentCondition;
		if (condition == null)
		{
			GD.PrintErr("[MazeManager] Nav pieejams nosacījums no ExperimentManager.");
			return;
		}
		_mazeId          = condition.MazeId;
		_roomObjectNames  = condition.RoomObjectNames  ?? [];
		_roomObjectScenes = condition.RoomObjectScenes ?? [];
		_filterColor      = condition.FilterColor;
	}

	// Noņem veco labirintu un ielādē jauno izkārtojumu
	private void SwapMazeLayout()
	{
		var em = ExperimentManager.Instance;
		if (em == null) return;

		GetNodeOrNull("World")?.Free();

		var layoutScene = GD.Load<PackedScene>(em.CurrentMazeLayoutPath);
		if (layoutScene == null)
		{
			GD.PrintErr($"[MazeManager] Neizdevās ielādēt izkārtojumu {em.CurrentMazeLayoutPath}");
			return;
		}

		var world = layoutScene.Instantiate<Node3D>();
		world.Name = "World";
		AddChild(world);
		_runData.LayoutId = em.CurrentMazeLayoutId;

		if (world is MazeLayoutConfig config)
		{
			TargetSequence = config.TargetSequence;
			GD.Print($"[MazeManager] Izkārtojums: {em.CurrentMazeLayoutId} | secība: [{string.Join(", ", TargetSequence)}]");
		}
		else
		{
			GD.PrintErr($"[MazeManager] Izkārtojumam nav MazeLayoutConfig {em.CurrentMazeLayoutPath}");
		}

		_spawnPoint = world.GetNodeOrNull<Marker3D>("spawn_point");
		if (_spawnPoint == null)
			GD.PrintErr($"[MazeManager] spawn_point nav atrasts izkārtojumā {em.CurrentMazeLayoutPath}");

		var button = world.GetNodeOrNull<VRButton>("StartTimerButton");
		if (button != null)
			button.Pressed += OnStartTimerPressed;
		else
			GD.PrintErr($"[MazeManager] StartTimerButton nav atrasts izkārtojumā {em.CurrentMazeLayoutPath}");
	}

	// Seko spēlētāja noietajam attālumam
	public override void _Process(double delta)
	{
		if (_phase != MazePhase.Timed || _playerBody == null) return;
		_segmentDistance += (_playerBody.GlobalPosition - _lastPlayerPos).Length();
		_lastPlayerPos = _playerBody.GlobalPosition;
	}

	// Atrod visas 9 istabas telpā un sakārto pēc indeksa
	private void DiscoverRooms()
	{
		var found = new List<RoomInstance>();
		foreach (Node node in GetTree().GetNodesInGroup("room_instance"))
			if (node is RoomInstance ri) found.Add(ri);

		found.Sort((a, b) => a.RoomIndex.CompareTo(b.RoomIndex));
		_rooms = found.ToArray();

		if (_rooms.Length != 9)
			GD.PrintErr($"[MazeManager] Sagaidītas 9 istabas, atrasto skaits: {_rooms.Length}.");
	}

	// Ievieto 3D objektu katrā istabā pēc indeksa
	private void SpawnObjects()
	{
		if (_rooms == null || _rooms.Length == 0) return;

		_runData.ObjectRoomAssignment = new();

		for (int roomIdx = 0; roomIdx < _rooms.Length; roomIdx++)
		{
			string name = roomIdx < _roomObjectNames.Length ? _roomObjectNames[roomIdx] : "";
			_runData.ObjectRoomAssignment[$"room_{roomIdx}"] = name;

			var spawnPoint = _rooms[roomIdx].ObjectSpawnPoint;
			if (spawnPoint == null) continue;

			if (roomIdx < _roomObjectScenes.Length && _roomObjectScenes[roomIdx] != null)
				spawnPoint.AddChild(_roomObjectScenes[roomIdx].Instantiate<Node3D>());
		}
	}

	// Pievieno klausītājus apmeklētas istabas detektēšanai
	private void ConnectRoomTriggers()
	{
		if (_rooms == null) return;
		foreach (var room in _rooms)
			room.PlayerEntered += OnPlayerEnteredRoom;
	}

	// Sāk uzņemt laiku, kad spēlētājs nospiež pogu "Sākt"
	public void OnStartTimerPressed()
	{
		if (_phase != MazePhase.Exploration) return;

		_runData.ExplorationDurationSeconds = Time.GetTicksMsec() / 1000.0 - _explorationStart;

		_runData.TargetSequence = new string[TargetSequence.Length];
		_runData.Targets        = new TargetVisit[TargetSequence.Length];
		for (int i = 0; i < TargetSequence.Length; i++)
		{
			int roomIdx = TargetSequence[i];
			_runData.TargetSequence[i] = roomIdx < _roomObjectNames.Length ? _roomObjectNames[roomIdx] : "";
		}

		FadeAndStartTimed();
	}

	// Aptumšo ekrānu, atiestata istabas un sāk laika uzņemšanu
	private void FadeAndStartTimed()
	{
		var bo  = FindBlackout();
		var seq = CreateTween();

		if (bo != null)
			seq.TweenProperty(bo, "fade", 1.0f, 0.5f);
		else
			seq.TweenInterval(0.2f);

		seq.TweenCallback(Callable.From(() =>
		{
			foreach (var room in _rooms) room.ResetPlayerState();
			_phase = MazePhase.Timed;
			_currentTargetIndex = 0;
			_segmentDistance = 0;
			if (_playerBody != null) _lastPlayerPos = _playerBody.GlobalPosition;
		}));

		if (bo != null)
			seq.TweenProperty(bo, "fade", 0.0f, 0.5f);

		seq.TweenCallback(Callable.From(() =>
		{
			_timedPhaseStart = Time.GetTicksMsec() / 1000.0;
			ShowCurrentTarget();
		}));
	}

	// Novieto cilvēku sākuma punktā telpā
	private void TeleportPlayerToSpawn()
	{
		if (_spawnPoint == null) return;

		if (GetTree().GetFirstNodeInGroup("player_body") is not Node3D playerBody) return;

		playerBody.Call("teleport", _spawnPoint.GlobalTransform);
	}

	// Telpas apmeklējuma izsaukums
	private void OnPlayerEnteredRoom(int roomIndex)
	{
		if (_phase != MazePhase.Timed) return;
		if (_currentTargetIndex >= TargetSequence.Length) return;

		double elapsed = Time.GetTicksMsec() / 1000.0 - _timedPhaseStart;

		if (roomIndex != TargetSequence[_currentTargetIndex])
		{
			// Saglabā nepareizas apmeklētas istabas datus
			int targetRoom = TargetSequence[_currentTargetIndex];
			_runData.WrongRoomVisits.Add(new WrongRoomVisit
			{
				RoomIndex        = roomIndex,
				ObjectName       = roomIndex   < _roomObjectNames.Length ? _roomObjectNames[roomIndex]   : "",
				TargetObjectName = targetRoom  < _roomObjectNames.Length ? _roomObjectNames[targetRoom]  : "",
				TimeSeconds      = elapsed
			});
			return;
		}

		_runData.Targets[_currentTargetIndex] = new TargetVisit
		{
			ObjectName           = roomIndex < _roomObjectNames.Length ? _roomObjectNames[roomIndex] : "",
			RoomIndex            = roomIndex,
			TimeSeconds          = elapsed,
			DistanceWalkedToHere = _segmentDistance
		};
		_segmentDistance = 0;

		GD.Print($"[MazeManager] Mērķis {_currentTargetIndex + 1} atrasts: istaba {roomIndex} pēc {elapsed:F2}s");

		_currentTargetIndex++;

		if (_currentTargetIndex >= TargetSequence.Length)
			CompleteMaze();
		else
			ShowCurrentTarget();
	}

	// Parāda nākamo meklējamo atrodamo kā tekstu
	private void ShowCurrentTarget()
	{
		int roomIdx = TargetSequence[_currentTargetIndex];
		string name = roomIdx < _roomObjectNames.Length ? _roomObjectNames[roomIdx] : "?";
		_hud?.ShowTarget($"Atrodiet: {name}", 5);
		_hud?.ShowMessage($"Objekts {_currentTargetIndex + 1} no {TargetSequence.Length}", 5);
	}

	// Pabeidz labirintu un nodod datus ExperimentManager apstrādei
	private void CompleteMaze()
	{
		_hud?.ShowTarget("");
		_hud?.ShowMessage("Visi objekti atrasti!\nJāizpilda aptauja...", 5f);

		_runData.MazeId = _mazeId;

		var timer = GetTree().CreateTimer(4.5);
		timer.Timeout += () => ExperimentManager.Instance?.OnMazeCompleted(_runData);
	}

	// Uzliek krāsas filtru spēlētāja kamerai
	private void ApplyColorFilter()
	{
		var players = GetTree().GetNodesInGroup("player");
		foreach (Node p in players)
		{
			var cf = p.GetNodeOrNull<ColorFilter>("XRCamera3D/ColorFilter");
			if (cf == null) continue;
			cf.FilterColor = _filterColor;
			return;
		}
		GD.PrintErr("[MazeManager] ColorFilter nav atrasts spēlētājā.");
	}

    // Aptumšošanas objekta atrašana
	private blackout FindBlackout()
	{
		var players = GetTree().GetNodesInGroup("player");
		foreach (Node p in players)
		{
			var bo = p.GetNodeOrNull<blackout>("XRCamera3D/BlackOut");
			if (bo != null) return bo;
		}
		return null;
	}
}
