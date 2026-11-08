using Godot;

// Pievieno filtru kā XRCamera3D objekta bērnu. Uzliek krāsas filtru priekšā kameras skatam.
[Tool]
public partial class ColorFilter : Node3D
{
	private Color _filterColor = new Color(0f, 0f, 0f, 0f);

	[Export]
	public Color FilterColor
	{
		get => _filterColor;
		set { _filterColor = value; ApplyColor(); }
	}

	private MeshInstance3D _mesh;
	private StandardMaterial3D _material;

	// Objekta inicializācija
	public override void _Ready()
	{
		var quad = new QuadMesh { Size = new Vector2(5f, 5f) };

		_material = new StandardMaterial3D
		{
			ShadingMode    = BaseMaterial3D.ShadingModeEnum.Unshaded,
			CullMode       = BaseMaterial3D.CullModeEnum.Back,
			SpecularMode   = BaseMaterial3D.SpecularModeEnum.Disabled,
			NoDepthTest    = true,
			BlendMode      = BaseMaterial3D.BlendModeEnum.Mul,
			RenderPriority = 100
		};

		_mesh = new MeshInstance3D
		{
			Mesh             = quad,
			Position         = new Vector3(0f, 0f, -0.5f), // Attālums 0.5m no kameras
			MaterialOverride = _material
		};
		AddChild(_mesh);

		ApplyColor();
	}

	// Aprēķina un uzliek filtra krāsu materiālam
	private void ApplyColor()
	{
		if (_material == null) return;

		// Ja filtrs ir caurspīdīgs, slēpj taisnstūri un pārtrauc
		if (_filterColor.A == 0f)
		{
			_mesh.Visible = false;
			return;
		}

		_mesh.Visible = true;

		float t = _filterColor.A;
		// Aprēķina filtra krāsu
		_material.AlbedoColor = new Color(
			Mathf.Lerp(1f, _filterColor.R, t),
			Mathf.Lerp(1f, _filterColor.G, t),
			Mathf.Lerp(1f, _filterColor.B, t),
			1f
		);
	}
}
