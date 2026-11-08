using Godot;

// Pievieno kā MeshInstance3D bērnu, lai "Sākt" poga nav pārklāta ar filtru
[GlobalClass]
public partial class FilterExempt : Node
{
	public override void _Ready()
	{
		var parent = GetParent<MeshInstance3D>();
		if (parent == null)
		{
			GD.PrintErr("[FilterExempt] Nav MeshInstance3D bērns");
			return;
		}

		var overlay = new MeshInstance3D();
		overlay.Mesh = parent.Mesh;
		overlay.Name = "FilterExemptOverlay";

		int surfaceCount = parent.Mesh?.GetSurfaceCount() ?? 0;

		for (int i = 0; i < surfaceCount; i++)
		{
			var original = parent.GetSurfaceOverrideMaterial(i)
			               ?? parent.Mesh.SurfaceGetMaterial(i);

			if (original == null) continue;

			if (original is BaseMaterial3D bm)
			{
				var copy = (BaseMaterial3D)bm.Duplicate();
				copy.NoDepthTest    = true;
				// Uzstādīta augstāka renderēšanas prioritāte kā filtram
				// Netiks pārklāts ar filtru
				copy.RenderPriority = 127;
				overlay.SetSurfaceOverrideMaterial(i, copy);
			}
			else
			{
				var copy = (Material)original.Duplicate();
				copy.RenderPriority = 127;
				overlay.SetSurfaceOverrideMaterial(i, copy);
				GD.PrintErr($"[FilterExempt] Virsma {i} izmanto ShaderMaterial — dziļuma tests nevar tikt atslēgts.");
			}
		}

		// Pievieno kā bērnu
		parent.AddChild(overlay);
		GD.Print($"[FilterExempt] Pārklājums izveidots priekš {parent.Name}");
	}
}
