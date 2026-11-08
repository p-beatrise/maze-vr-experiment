using Godot;

// VR poga ar 3D izskatu. Mijiedarbību nodrošina VRRaycaster
[GlobalClass]
public partial class VRButton : Node3D
{
	private const uint UI_COLLISION_LAYER = 4u;

	// Pogas krāsas dažādos stāvokļos
	[Export] private Color ColNormal   = new Color(0.15f, 0.20f, 0.70f);
	[Export] private Color ColHovered  = new Color(0.30f, 0.40f, 1.00f);
	[Export] private Color ColSelected = new Color(0.10f, 0.70f, 0.20f);
	[Export] private Color ColDisabled = new Color(0.25f, 0.25f, 0.25f);

	private string _buttonText = "";
	[Export]
	public string ButtonText
	{
		get => _buttonText;
		set { _buttonText = value; if (_label != null) _label.Text = value; }
	}

	[Export] public float Width  = 0.45f;
	[Export] public float Height = 0.09f;
	[Export] public float Depth  = 0.025f;
	[Export] public int FontSize = 36;

	[Signal] public delegate void PressedEventHandler();

	private MeshInstance3D       _mesh;
	private Label3D              _label;
	private Area3D               _area;
	private StandardMaterial3D  _material;

	private bool _isSelected    = false;
	private bool _isInteractable = true;

	public override void _Ready()
	{
		BuildMesh();
		BuildLabel();
		BuildArea();
	}

	// Maina krāsu, kad ir novietots stars uz pogu
	public void SetHovered(bool hovered)
	{
		if (!_isInteractable) return;
		_material.AlbedoColor = hovered ? ColHovered : (_isSelected ? ColSelected : ColNormal);
	}

	// Pogas nospiešana ar kontroliera palīdzību
	public void SetSelected(bool selected)
	{
		_isSelected = selected;
		_material.AlbedoColor = selected ? ColSelected : ColNormal;
	}

	// Pogas atspējošana
	public void SetInteractable(bool interactable)
	{
		_isInteractable = interactable;
		_material.AlbedoColor = interactable ? (_isSelected ? ColSelected : ColNormal) : ColDisabled;
		_area.CollisionLayer = interactable ? UI_COLLISION_LAYER : 0u;
		_area.Monitorable    = interactable;
	}

	// Pogas nospiešanas signāls
	public void Press()
	{
		if (!_isInteractable) return;
		EmitSignal(SignalName.Pressed);
	}

	public Area3D GetArea() => _area;

	// Pogas formas izveide
	private void BuildMesh()
	{
		var box = new BoxMesh();
		box.Size = new Vector3(Width, Height, Depth);

		_material = new StandardMaterial3D();
		_material.AlbedoColor    = ColNormal;
		_material.NoDepthTest    = false;
		_material.Transparency   = BaseMaterial3D.TransparencyEnum.Alpha;
		_material.DepthDrawMode  = BaseMaterial3D.DepthDrawModeEnum.Always;
		_material.RenderPriority = 126;

		_mesh = new MeshInstance3D();
		_mesh.Mesh = box;
		_mesh.MaterialOverride = _material;
		AddChild(_mesh);
	}

	// Teksta pievienošana pogai
	private void BuildLabel()
	{
		_label = new Label3D();
		_label.Text = _buttonText;
		_label.FontSize = FontSize;
		_label.PixelSize = 0.0005f;
		_label.Position = new Vector3(0f, 0f, Depth / 2f + 0.001f);
		_label.NoDepthTest = false;
		_label.RenderPriority = 127;
		AddChild(_label);
	}

	// Objekts staru uztveršanai
	private void BuildArea()
	{
		_area = new Area3D();
		_area.CollisionLayer = UI_COLLISION_LAYER;
		_area.CollisionMask  = 0; // Nereaģē uz citiem objektiem, tikai stariem

		var shape = new BoxShape3D();
		shape.Size = new Vector3(Width, Height, Depth + 0.01f);

		var collision = new CollisionShape3D();
		collision.Shape = shape;
		_area.AddChild(collision);
		AddChild(_area);
	}
}
