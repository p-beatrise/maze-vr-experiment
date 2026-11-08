using Godot;

// OpenXR inicializācija
// Balstīts uz godot-xr-tools start_xr.gd.
public partial class startVR : Node3D
{
	[Signal]
	public delegate void FocusLostEventHandler();

	[Signal]
	public delegate void FocusGainedEventHandler();

	[Signal]
	public delegate void PoseRecenteredEventHandler();

	[Export]
	public int MaximumRefreshRate { get; set; } = 120;

	private OpenXRInterface _xrInterface;

	private bool _xrIsFocused;

	public override void _Ready()
	{
		_xrInterface = (OpenXRInterface)XRServer.FindInterface("OpenXR");
		if (_xrInterface != null && _xrInterface.IsInitialized())
		{
			GD.Print("OpenXR inicializēts");
			var vp = GetViewport();

			// Iespējo XR renderēšanu
			vp.UseXR = true;

			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);

			if (RenderingServer.GetRenderingDevice() != null)
			{
				vp.VrsMode = Viewport.VrsModeEnum.XR;
			}
			else if ((int)ProjectSettings.GetSetting("xr/openxr/foveation_level") == 0)
			{
				GD.PushWarning("OpenXR: Nekorekti Foveation līmeņe iestatījumi");
			}

			// Pievieno klausītājus OpenXR notikumiem
			_xrInterface.SessionBegun += OnOpenXRSessionBegun;
			_xrInterface.SessionVisible += OnOpenXRVisibleState;
			_xrInterface.SessionFocussed += OnOpenXRFocusedState;
			_xrInterface.SessionStopping += OnOpenXRStopping;
			_xrInterface.PoseRecentered += OnOpenXRPoseRecentered;
		}
		else
		{
			// Iziet no spēles, jo OpenXR nav inicializēts!
			GD.Print("OpenXR nav inicializēts!");
			GetTree().Quit();
		}
	}

	// Iestata augstāko pieejamo refresh rate intensitāti
	private void OnOpenXRSessionBegun()
	{
		var currentRefreshRate = _xrInterface.DisplayRefreshRate;
		GD.Print(currentRefreshRate > 0.0F
			? $"OpenXR: Refresh rate {currentRefreshRate}"
			: "OpenXR: OpenXR nav nepieciešamais refresh rate");

		var newRate = currentRefreshRate;
		var availableRates = _xrInterface.GetAvailableDisplayRefreshRates();
		if (availableRates.Count == 0)
		{
			GD.Print("OpenXR: Ierīce neatbalsta dažādus refresh rate");
		}
		else if (availableRates.Count == 1)
		{
			newRate = (float)availableRates[0];
		}
		else
		{
			GD.Print("OpenXR: Iespējamie refresh rates: ", availableRates);
			foreach (float rate in availableRates)
			{
				if (rate > newRate && rate <= MaximumRefreshRate)
				{
					newRate = rate;
				}
			}
		}

		if (currentRefreshRate != newRate)
		{
			GD.Print($"OpenXR: Refresh rate iestatīts uz {newRate}");
			_xrInterface.DisplayRefreshRate = newRate;
			currentRefreshRate = newRate;
		}

		Engine.PhysicsTicksPerSecond = (int)currentRefreshRate;
	}
	private void OnOpenXRVisibleState()
	{
		if (_xrIsFocused)
		{
			GD.Print("OpenXR zaudēja fokusu.");

			_xrIsFocused = false;

			GetTree().Paused = true;

			EmitSignal(SignalName.FocusLost);
		}
	}

	private void OnOpenXRFocusedState()
	{
		GD.Print("OpenXR ir fokuss");
		_xrIsFocused = true;

		GetTree().Paused = false;

		EmitSignal(SignalName.FocusGained);
	}

	private void OnOpenXRStopping()
	{
		GD.Print("OpenXR tiek apturēts.");
	}

	private void OnOpenXRPoseRecentered()
	{
		EmitSignal(SignalName.PoseRecentered);
	}
}
