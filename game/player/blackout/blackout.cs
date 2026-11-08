using Godot;

// Pārvalda aptumšošanu
[Tool]
public partial class blackout : Node3D
{
	private float _fade = 0.0f;

	[Export(PropertyHint.Range, "0,1,0.1")]
	public float fade
	{
		get => _fade;
		set
		{
			_fade = value;
			if (IsInsideTree())
			{
				UpdateFade();
			}
		}
	}

	private ShaderMaterial _material;
	private MeshInstance3D _mesh;

	public override void _Ready()
	{
		_mesh = GetNode<MeshInstance3D>("MeshInstance3D");
		_material = _mesh.MaterialOverride as ShaderMaterial;

		UpdateFade();
	}

	// Atjaunina aptumšojuma stiprumu
	private void UpdateFade()
	{
		if (_mesh == null)
			return;

		if (_fade == 0.0f)
		{
			_mesh.Visible = false;
		}
		else
		{
			if (_material != null)
			{
				_material.SetShaderParameter("albedo", new Color(0f, 0f, 0f, _fade));
			}

			_mesh.Visible = true;
		}
	}
}
