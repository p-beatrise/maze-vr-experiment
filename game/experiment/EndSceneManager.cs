using Godot;

// Noslēguma teksts eksperimenta beigās
public partial class EndSceneManager : Node3D
{
	[Export] public string EndMessage =
		"Paldies par dalību eksperimentā!\nEksperiments ir pabeigts.\n Varat noņemt brilles!";

	[Export] public Marker3D LabelAnchor;
	[Export] public int   FontSize  = 96;
	[Export] public float PixelSize = 0.001f;

	public override void _Ready()
	{
		var label = new Label3D
		{
			Text      = EndMessage,
			FontSize  = FontSize,
			PixelSize = PixelSize,
			Width     = 600,
			Billboard = BaseMaterial3D.BillboardModeEnum.FixedY,
			Modulate  = new Color(1f, 1f, 1f),
		};

		// Pievieno objektu
		if (LabelAnchor != null)
			LabelAnchor.AddChild(label);
		else
		{
			label.Position = new Vector3(0f, 1.5f, 2f);
			AddChild(label);
		}
	}
}
