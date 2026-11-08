using Godot;
using System;

// Klase eksperimenta pārvaldīšanai. Pievienota kā autoload fails projekta iestatījumos.
public partial class ExperimentManager : Node
{
	public static ExperimentManager Instance { get; private set; }

	public enum Phase { Tutorial, MazeExploration, MazeTimed, PostMazeSurvey, Demographics, End }

	public Phase CurrentPhase { get; private set; } = Phase.Tutorial;
	public int CurrentMazeIndex { get; private set; } = -1;
	public string ParticipantId { get; private set; }

	// Parādāmās aptaujas ceļš, ko nolasa SurveyManager pirms ainas ielādes
	public string PendingSurveyConfigPath { get; private set; }

	private ExperimentData _data = new ExperimentData();

	// Ainu faili
	private const string MazeScenePath              = "res://scenes/maze_scene.tscn";
	private const string StartScenePath             = "res://scenes/start_scene.tscn";
	private const string PostMazeSurveyScenePath    = "res://scenes/survey_post_maze.tscn";
	private const string DemographicSurveyScenePath = "res://scenes/survey_demographic.tscn";
	private const string EndScenePath               = "res://scenes/end_scene.tscn";
	private const string PostMazeSurveyConfigPath    = "res://surveys/post_maze_survey.tres";
	private const string DemographicSurveyConfigPath = "res://surveys/demographic_survey.tres";

	// Noklusētie istabu izkārtojumi
	private static readonly string[] DefaultMazeLayoutPaths =
	[
		"res://scenes/components/maze_1.tscn",
		"res://scenes/components/maze_2.tscn",
		"res://scenes/components/maze_3.tscn"
	];
	private static readonly string[] DefaultMazeLayoutIds = ["layout_1", "layout_2", "layout_3"];

	private MazeConditionConfig[] _mazeConditions; // Filtru un objektu iestatījumu failu ceļi
	private string[]              _mazeIds;
	private string[]              _mazeLayoutPaths;  // Eksperimenta telpas istabu faila ceļš
	private string[]              _mazeLayoutIds;

	// Atgriež pašreizējā labirinta datus
	public MazeConditionConfig CurrentCondition    => _mazeConditions[CurrentMazeIndex];
	public string CurrentMazeLayoutPath            => _mazeLayoutPaths[CurrentMazeIndex];
	public string CurrentMazeLayoutId              => _mazeLayoutIds[CurrentMazeIndex];

	[Signal] public delegate void PhaseChangedEventHandler(int phase);

	// Inicializācija
	public override void _Ready()
	{
		if (Instance != null && Instance != this)
		{
			QueueFree();
			return;
		}
		Instance = this;

		ParticipantId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
		_data.ParticipantId   = ParticipantId;
		_data.ExperimentStart = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

		ShuffleMazeOrder();
		_data.MazeOrder   = _mazeIds;
		_data.LayoutOrder = _mazeLayoutIds;

		GD.Print($"[ExperimentManager] Sesija sākta {ParticipantId} |  filtri/objekti: {string.Join(" -> ", _mazeIds)} | izkārtojums: {string.Join(" ->", _mazeLayoutIds)}");
	}

	// Sāk eksperimentu no pirmā labirinta
	public void StartExperiment()
	{
		CurrentMazeIndex = 0;
		SetPhase(Phase.MazeExploration);
		TransitionToScene(MazeScenePath);
	}

	// Izsauc MazeManager, kad apmeklētas visas nepieciešamās istabas
	public void OnMazeCompleted(MazeRunData mazeData)
	{
		_data.Mazes.Add(mazeData);
		SetPhase(Phase.PostMazeSurvey);
		PendingSurveyConfigPath = PostMazeSurveyConfigPath;
		TransitionToScene(PostMazeSurveyScenePath);
	}

	// Izsauc SurveyManager pēc labirinta aptaujas iesniegšanas
	public void OnPostMazeSurveyCompleted(Godot.Collections.Dictionary responses)
	{
		if (_data.Mazes.Count > 0)
			_data.Mazes[_data.Mazes.Count - 1].PostMazeSurvey = responses;

		CurrentMazeIndex++;

		if (CurrentMazeIndex < _mazeConditions.Length)
		{
			SetPhase(Phase.MazeExploration);
			TransitionToScene(MazeScenePath);
		}
		else
		{
			SetPhase(Phase.Demographics);
			PendingSurveyConfigPath = DemographicSurveyConfigPath;
			TransitionToScene(DemographicSurveyScenePath);
		}
	}

	// Izsauc SurveyManager pēc demogrāfiskās aptaujas iesniegšanas
	public void OnDemographicsCompleted(Godot.Collections.Dictionary responses)
	{
		_data.DemographicSurvey = responses;
		DataLogger.Save(_data, ParticipantId);
		SetPhase(Phase.End);
		TransitionToScene(EndScenePath);
	}

	// Atjauno eksperimentu no sākuma
	public void RestartToStart()
	{
		CurrentMazeIndex = -1;
		CurrentPhase     = Phase.Tutorial;
		ParticipantId    = DateTime.Now.ToString("yyyyMMdd_HHmmss");
		_data = new ExperimentData
		{
			ParticipantId   = ParticipantId,
			ExperimentStart = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
		};
		ShuffleMazeOrder();
		_data.MazeOrder   = _mazeIds;
		_data.LayoutOrder = _mazeLayoutIds;
		GD.Print($"[ExperimentManager] Eksperiments restartēts: {ParticipantId}");
		TransitionToScene(StartScenePath);
	}

	private static readonly string[] ConditionPaths =
	[
		"res://conditions/blue.tres",
		"res://conditions/red.tres",
		"res://conditions/neutral.tres"
	];

	// Ielādē visus filtrus+objektus
	private static MazeConditionConfig[] BuildConditions()
	{
		var configs = new MazeConditionConfig[ConditionPaths.Length];
		for (int i = 0; i < ConditionPaths.Length; i++)
		{
			var raw = ResourceLoader.Load<MazeConditionConfig>(ConditionPaths[i]);
			if (raw is MazeConditionConfig config)
				configs[i] = config;
			else
				GD.PrintErr($"[ExperimentManager] Neizdevās ielādēt filtrus/objektus");
		}
		return configs;
	}

	// Randomizē filtrus/objektus un istabu izkārtojumu secību neatkarīgi
	private void ShuffleMazeOrder()
	{
		var rng = new RandomNumberGenerator();
		rng.Randomize();

		// Filtra secības randomizācija
		_mazeConditions = BuildConditions();
		for (int i = _mazeConditions.Length - 1; i > 0; i--)
		{
			int j = rng.RandiRange(0, i);
			(_mazeConditions[i], _mazeConditions[j]) = (_mazeConditions[j], _mazeConditions[i]);
		}
		_mazeIds = Array.ConvertAll(_mazeConditions, c => c.MazeId);

		// Isepējamo apmeklējamo istabu secību randomizācija
		// Definēti ir trīs iespējami veidi, kā jāapmeklē dalībniekam istabas
		_mazeLayoutPaths = (string[])DefaultMazeLayoutPaths.Clone();
		_mazeLayoutIds   = (string[])DefaultMazeLayoutIds.Clone();
		for (int i = _mazeLayoutPaths.Length - 1; i > 0; i--)
		{
			int j = rng.RandiRange(0, i);
			(_mazeLayoutPaths[i], _mazeLayoutPaths[j]) = (_mazeLayoutPaths[j], _mazeLayoutPaths[i]);
			(_mazeLayoutIds[i],   _mazeLayoutIds[j])   = (_mazeLayoutIds[j],   _mazeLayoutIds[i]);
		}
	}

	//  Eksperimenta fāzes maiņa
	//  Iespējamas: Tutorial, MazeExploration, MazeTimed, PostMazeSurvey, Demographics, End
	private void SetPhase(Phase phase)
	{
		CurrentPhase = phase;
		EmitSignal(SignalName.PhaseChanged, (int)phase);
		GD.Print($"[ExperimentManager] Fāze: {phase}");
	}

	// Pāreja uz jaunu ainu ar caur ekrāna aptumšošanu
	private async void TransitionToScene(string path)
	{
		var bo = FindBlackout();
		if (bo != null)
		{
			var tween = CreateTween();
			tween.TweenProperty(bo, "fade", 1.0f, 0.3f);
			await ToSignal(tween, Tween.SignalName.Finished);
		}
		else
		{
			GD.PrintErr("[ExperimentManager] Aptumšošana nestrādā");
		}

		GetTree().ChangeSceneToFile(path);
	}

	// Aptumšošanas objekta atrašana
	private blackout FindBlackout()
	{
		var players = GetTree().GetNodesInGroup("player");
		foreach (Node p in players)
		{
			var bo = p.GetNodeOrNull<blackout>("XRCamera3D/BlackOut");
			if (bo != null)
			{
				GD.Print($"Aptumšošanas nostrādā");
				return bo;
			}
		}
		GD.PrintErr("Aptumšošana nav atrasta");
		return null;
	}
}
