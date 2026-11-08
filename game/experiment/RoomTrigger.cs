using Godot;

// Apstrādā istabas apmeklējumu
public partial class RoomTrigger : Node3D
{
    [Export] public int RoomIndex = 0;

    [Signal] public delegate void PlayerEnteredEventHandler(int roomIndex);

    public override void _Ready()
    {
        var area = GetNodeOrNull<Area3D>("Area3D");
        if (area == null)
        {
            GD.PrintErr($"[RoomTrigger] Istabā nav Area3D objekta{RoomIndex}");
            return;
        }

        area.BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is CharacterBody3D)
            EmitSignal(SignalName.PlayerEntered, RoomIndex);
    }
}
