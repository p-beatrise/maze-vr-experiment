using Godot;

// Rāda tekstu spēlētājam kameras priekšā un uz rokas kontroliera
public partial class HUDManager : Node3D
{
	[Export] public bool UseLeftHand = true;

	private Label3D _messageLabel;
	private Label3D _targetLabel;
	private Label3D _handMessageLabel;
	private Label3D _handTargetLabel;

	// Taimeris teksta paslēpšanai
	private float _messageTimer = 5f;
	private float _targetTimer  = 5f;

	[Export] public Color MessageColor = new(1f, 1f, 1f);
	[Export] public Color TargetColor  = new(1f, 1f, 1f);
	private string _lastTimedMessage = "";
	private string _lastTimedTarget  = "";

	public override void _Ready()
	{
		AddToGroup("hud");
		SetProcess(false);

		_messageLabel = BuildLabel(new Vector3(0f, 0f, 0f),    40, MessageColor);
		_targetLabel  = BuildLabel(new Vector3(0f, -0.1f, 0f), 52, TargetColor);

		_messageLabel.Text = "";
		_targetLabel.Text  = "";

		SetupHandLabels();
		FadeInFromBlack();
	}

	// Pēc taimera beigām noslēpj meklējamā objketa nosaukumu no kameras priekšas un parāda to uz kreisās rokas kontroliera
	public override void _Process(double delta)
	{
		if (_messageTimer > 0f)
		{
			_messageTimer -= (float)delta;
			if (_messageTimer <= 0f)
			{
				_messageTimer = 0f;
				_messageLabel.Text = "";
				if (_handMessageLabel != null)
					_handMessageLabel.Text = _lastTimedMessage;
			}
		}

		if (_targetTimer > 0f)
		{
			_targetTimer -= (float)delta;
			if (_targetTimer <= 0f)
			{
				_targetTimer = 0f;
				_targetLabel.Text = "";
				if (_handTargetLabel != null)
					_handTargetLabel.Text = _lastTimedTarget;
			}
		}

		if (_messageTimer <= 0f && _targetTimer <= 0f)
			SetProcess(false);
	}

	// Rāda tekstu neierobežotu laiku
	public void ShowMessage(string text)
	{
		_messageLabel.Text = text;
		_messageTimer      = 0f;
		if (_handMessageLabel != null) _handMessageLabel.Text = "";
	}

	// Rāda tekstu uz ierobežotu laiku
	public void ShowMessage(string text, float seconds)
	{
		_messageLabel.Text = text;
		_messageTimer      = seconds;
		_lastTimedMessage  = text;
		if (_handMessageLabel != null) _handMessageLabel.Text = "";
		SetProcess(true);
	}

	// Rāda pastāvīgu meklējamā tekstu
	public void ShowTarget(string text)
	{
		_targetLabel.Text = text;
		_targetTimer      = 0f;
		if (_handTargetLabel != null) _handTargetLabel.Text = "";
	}

	// Rāda meklējamā objekta tekstu uz neierobežotu laiku
	public void ShowTarget(string text, float seconds)
	{
		_targetLabel.Text = text;
		_targetTimer      = seconds;
		_lastTimedTarget  = text;
		if (_handTargetLabel != null) _handTargetLabel.Text = "";
		SetProcess(true);
	}

	// Notīra meklējamā objekta tekstu
	public void ClearTarget()
	{
		_targetLabel.Text = "";
		_targetTimer      = 0f;
		if (_handTargetLabel != null) _handTargetLabel.Text = "";
	}

	// Paslēpj visus tekstus
	public void HideAll()
	{
		_messageLabel.Text = "";
		_targetLabel.Text  = "";
		_messageTimer = _targetTimer = 0f;
		if (_handMessageLabel != null) _handMessageLabel.Text = "";
		if (_handTargetLabel  != null) _handTargetLabel.Text  = "";
	}

	// Pievieno tekstus uz kreisā kontroliera
	private void SetupHandLabels()
	{
		var players = GetTree().GetNodesInGroup("player");
		foreach (Node p in players)
		{
			XRController3D controller = null;
			foreach (Node child in p.GetChildren(true))
			{
				if (child is not XRController3D xrc) continue;
				bool isLeft = xrc.Name.ToString().ToLower().Contains("left");
				if (UseLeftHand ? isLeft : !isLeft) { controller = xrc; break; }
				controller ??= xrc;
			}
			if (controller == null) continue;

			// Meklējamais objekts
			_handTargetLabel = new Label3D
			{
				FontSize    = 42,
				PixelSize   = 0.0005f,
				Width       = 350,
				Modulate    = TargetColor,
				Billboard   = BaseMaterial3D.BillboardModeEnum.Enabled,
				NoDepthTest = true,
				Position    = new Vector3(0f, 0.12f, 0f),
				RenderPriority = 127,
			};

			// Meklējamā objekta numurs
			_handMessageLabel = new Label3D
			{
				FontSize    = 36,
				PixelSize   = 0.0005f,
				Width       = 350,
				Modulate    = MessageColor,
				Billboard   = BaseMaterial3D.BillboardModeEnum.Enabled,
				NoDepthTest = true,
				Position    = new Vector3(0f, 0.05f, 0f),
				RenderPriority = 127,
			};

			controller.AddChild(_handTargetLabel);
			controller.AddChild(_handMessageLabel);
			return;
		}

		GD.PrintErr("[HUDManager] Nav atrasts XRController3D tekstam uz kontroliera");
	}

	// Izveido teksta etiķeti kamerā
	private Label3D BuildLabel(Vector3 localPos, int fontSize, Color color)
	{
		var label = new Label3D
		{
			Position    = localPos,
			FontSize    = fontSize,
			PixelSize   = 0.0008f,
			Width       = 500,
			Modulate    = color,
			Billboard   = BaseMaterial3D.BillboardModeEnum.Disabled,
			NoDepthTest = true,
			RenderPriority = 127,
		};
		AddChild(label);
		return label;
	}

	// Izgaismo ainu pēc aptumšošanas
	private void FadeInFromBlack()
	{
		var bo = FindBlackout();
		if (bo == null) return;

		bo.fade = 1.0f;
		var tween = CreateTween();
		tween.TweenProperty(bo, "fade", 0.0f, 0.5f);
	}

	// Aptumšo ainu
	private blackout FindBlackout()
	{
		var players = GetTree().GetNodesInGroup("player");
		foreach (Node p in players)
		{
			var bo = p.GetNodeOrNull<blackout>("XRCamera3D/BlackOut");
			if (bo != null) return bo;
		}
		return null;
	}
}
