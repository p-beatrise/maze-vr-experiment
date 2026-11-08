using Godot;

// Pārvalda sākuma ainu
public partial class StartSceneManager : Node3D
{
	[Export] public string WelcomeMessage =
		"Sveicināti! \nJums būs jāizpēta trīs labirinti un jāatrod norādītie objekti istabās. \n" +
		"Lai kustētos, lietojiet kreisā kontroliera kursorsviru. \nLai skatītos apkārt, izmantojiet labā kontroliera kursorsviru, vai arī grozot galvu. \n" +
		"Katra labirinta sākumā ir dots neierobežots laiks iepazīties ar istabu izvietojumu. \nPēc tam ir jādodas uz sākuma punktu un jānospiez 'Sākt' poga. Tad tiks parādīts objekts, kas ir jāatrod. \n" +
		"Lai nokļūtu eksperimenta telpā, spiediet pogu 'Sākt'";

	[Export] public Marker3D LabelAnchor;
	[Export] public int   FontSize  = 48;
	[Export] public float PixelSize = 0.001f;

	public override void _Ready()
	{
		var label = new Label3D
		{
			Text      = WelcomeMessage,
			FontSize  = FontSize,
			PixelSize = PixelSize,
			Width     = 600,
			Billboard = BaseMaterial3D.BillboardModeEnum.FixedY,
			Modulate  = new Color(1f, 1f, 1f),
		};

		if (LabelAnchor != null)
			LabelAnchor.AddChild(label);
		else
		{
			label.Position = new Vector3(0f, 1.5f, -2f);
			AddChild(label);
		}
	}

	// Izsauc, kad spēlētājs nospiež pogu "Sākt"
	// Pārvieto uz eksperimenta telpu
	public void OnStartButtonPressed()
	{
		ExperimentManager.Instance?.StartExperiment();
	}
}
