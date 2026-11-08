using Godot;

// Seko spēlētāja stāvoklim - restartē eksperimentu, ja uz kreisā kontoliera tiek Y poga turēta nospiestas vairāk kā 5 sekundes
[GlobalClass]
public partial class PlayerManager : Node
{
    [Export] private float FallThreshold = -10f;
    [Export] private float ResetHoldTime = 5f;

    private static readonly StringName ByButton = "by_button";

    private XROrigin3D _origin;
    private XRController3D _leftController;
    private float _holdAccum = 0f;

    public override void _Ready()
    {
        _origin = GetParent<XROrigin3D>();
        _leftController = _origin?.GetNodeOrNull<XRController3D>("LeftHand");
    }

    public override void _Process(double delta)
    {
        if (_origin != null && _origin.GlobalPosition.Y < FallThreshold)
            _origin.GlobalPosition = Vector3.Zero;

        // Restartē eksperimenta sesiju, ja tiek turēta Y poga
        if (_leftController != null && _leftController.IsButtonPressed(ByButton))
        {
            _holdAccum += (float)delta;
            if (_holdAccum >= ResetHoldTime)
            {
                _holdAccum = 0f;
                ExperimentManager.Instance?.RestartToStart();
            }
        }
        else
        {
            _holdAccum = 0f;
        }
    }
}
