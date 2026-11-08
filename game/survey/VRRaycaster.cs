using Godot;

// Pievieno bērnu katram XRController3D, lai uztvertui staru
public partial class VRRaycaster : Node3D
{
	private const int    UI_COLLISION_MASK = 4;
	private const float  RAY_LENGTH        = 2.0f;
	private const string TRIGGER_ACTION    = "trigger"; // jāsakrīt ar openxr_action_map.tres

	private static readonly Color PointerIdle    = new(1f,   1f, 1f, 0.45f);
	private static readonly Color PointerHovered = new(0.4f, 1f, 0.4f, 0.8f);

	private XRController3D              _controller;
	private VRButton                    _hoveredButton;
	private bool                        _triggerWasHeld;
	private MeshInstance3D              _pointerMesh;
	private CylinderMesh                _cylinderMesh;
	private StandardMaterial3D          _pointerMat;
	private PhysicsRayQueryParameters3D _rayQuery;
	private float                       _lastPointerDist = -1f;
	private bool                        _lastHovering;

	public override void _Ready()
	{
		_controller = GetParent<XRController3D>();
		BuildPointerVisual();

		_rayQuery = new PhysicsRayQueryParameters3D();
		_rayQuery.CollideWithAreas  = true;
		_rayQuery.CollideWithBodies = false;
		_rayQuery.CollisionMask     = UI_COLLISION_MASK;
	}

	// Nospiešanas apstrāde
	public override void _PhysicsProcess(double delta)
	{
		if (_controller == null) return;

		bool triggerHeld = _controller.GetFloat(TRIGGER_ACTION) > 0.5f;

		VRButton newHovered = FindHoveredButton(out Vector3 hitPoint);
		UpdatePointer(newHovered != null, hitPoint);

		if (newHovered != _hoveredButton)
		{
			_hoveredButton?.SetHovered(false);
			newHovered?.SetHovered(true);
			_hoveredButton = newHovered;
		}

		if (triggerHeld && !_triggerWasHeld && _hoveredButton != null)
			_hoveredButton.Press();

		_triggerWasHeld = triggerHeld;
	}

	// Raida staru no kontroliera un atgriež pogu, uz ko tas raidīts
	private VRButton FindHoveredButton(out Vector3 hitPoint)
	{
		var origin    = _controller.GlobalPosition;
		var direction = -_controller.GlobalTransform.Basis.Z;
		hitPoint = origin + direction * RAY_LENGTH;

		_rayQuery.From = origin;
		_rayQuery.To   = hitPoint;

		var result = GetWorld3D().DirectSpaceState.IntersectRay(_rayQuery);
		if (result.Count == 0) return null;

		hitPoint = result["position"].AsVector3();

		if (result["collider"].As<GodotObject>() is Area3D area)
			return area.GetParent() as VRButton;

		return null;
	}

	// Izveido stara vizuāli
	private void BuildPointerVisual()
	{
		var cylinder = new CylinderMesh();
		cylinder.TopRadius    = 0.003f;
		cylinder.BottomRadius = 0.003f;
		cylinder.Height       = RAY_LENGTH;

		var mat = new StandardMaterial3D();
		mat.AlbedoColor  = new Color(1f, 1f, 1f, 0.45f);
		mat.ShadingMode  = BaseMaterial3D.ShadingModeEnum.Unshaded;
		mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
		mat.RenderPriority = 127;

		_cylinderMesh = cylinder;
		_pointerMat   = mat;

		_pointerMesh = new MeshInstance3D();
		_pointerMesh.Mesh = cylinder;
		_pointerMesh.MaterialOverride = mat;

		_pointerMesh.RotateX(-Mathf.Pi / 2f);
		_pointerMesh.Position = new Vector3(0f, 0f, -RAY_LENGTH / 2f);

		AddChild(_pointerMesh);
	}

	// Atjaunina stara garumu un krāsu
	private void UpdatePointer(bool isHovering, Vector3 hitPoint)
	{
		if (_pointerMesh == null) return;

		float dist = isHovering
			? _controller.GlobalPosition.DistanceTo(hitPoint)
			: RAY_LENGTH;

		if (isHovering == _lastHovering && Mathf.Abs(dist - _lastPointerDist) < 0.001f) return;
		_lastHovering    = isHovering;
		_lastPointerDist = dist;

		_pointerMesh.Position = new Vector3(0f, 0f, -dist / 2f);
		_cylinderMesh.Height  = dist;
		_pointerMat.AlbedoColor = isHovering ? PointerHovered : PointerIdle;
	}
}
