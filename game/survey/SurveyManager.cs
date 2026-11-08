using Godot;
using System.Collections.Generic;

// Pārvalda aptaujas ainu - rāda jautājumus pa vienam un nosūta atbildes ExperimentManager
[GlobalClass]
public partial class SurveyManager : Node3D
{
	[Export] public float PanelDistance = 1.8f;

	[Export] public float OptionSpacing = 0.115f;

	private SurveyConfig _config;
	private int          _questionIndex = 0;
	private string       _selectedOption;
	private readonly Godot.Collections.Dictionary _responses = new Godot.Collections.Dictionary();

	private Label3D          _progressLabel;
	private Label3D          _questionLabel;
	private Node3D           _optionsRoot;
	private readonly List<VRButton>                    _optionButtons = [];
	private readonly List<VRButton.PressedEventHandler> _optionActions = [];
	private VRButton _nextButton;
	private VRNumpad _numpad;

	public override void _Ready()
	{
		LoadConfig();
		if (_config == null || _config.Questions.Length == 0)
		{
			GD.PrintErr("[SurveyManager] Nav atrasta aptauja, tiek izlaista");
			SubmitAndAdvance();
			return;
		}

		BuildPanelUI();
		ShowQuestion(0);
		FadeIn();
	}

	// Ielādē aptaujas konfigurāciju no ExperimentManager norādītā faila ceļa
	private void LoadConfig()
	{
		var path = ExperimentManager.Instance?.PendingSurveyConfigPath;
		if (!string.IsNullOrEmpty(path) && ResourceLoader.Exists(path))
			_config = ResourceLoader.Load<SurveyConfig>(path);
	}

	// Izveido aptaujas paneli
	private void BuildPanelUI()
	{
		GlobalPosition = new Vector3(0f, 1.0f, -PanelDistance);

		_progressLabel = MakeLabel(new Vector3(0f, 0.45f, 0f), 52, new Color(0.8f, 0.8f, 0.8f));
		_questionLabel = MakeLabel(new Vector3(0f, 0.20f, 0f), 52, new Color(1f, 1f, 1f));
		_questionLabel.Width = 600;

		_optionsRoot = new Node3D { Position = new Vector3(0f, -0.10f, 0f) };
		AddChild(_optionsRoot);

		_numpad = new VRNumpad { Position = new Vector3(0f, -0.25f, 0f), Visible = false };
		_numpad.ValueChanged += OnNumpadValueChanged;
		AddChild(_numpad);

		_nextButton = new VRButton
		{
			ButtonText = "Nākamais  →",
			Width      = 0.30f,
			Height     = 0.08f,
			Position   = new Vector3(0f, -0.70f, 0f),
			FontSize   = 52,
		};
		_nextButton.Pressed += OnNextPressed;
		AddChild(_nextButton);

		_nextButton.SetInteractable(false);
	}

	// Izveido tekstu jautājumiem
	private Label3D MakeLabel(Vector3 localPos, int fontSize, Color color)
	{
		var lbl = new Label3D
		{
			Position            = localPos,
			FontSize            = fontSize,
			PixelSize           = 0.0007f,
			Modulate            = color,
			HorizontalAlignment = HorizontalAlignment.Center,
		};
		AddChild(lbl);
		return lbl;
	}

	// Rāda jautājumus pēc kārtas
	private void ShowQuestion(int index)
	{
		var q = _config.Questions[index];

		_progressLabel.Text = $"Jautājums  {index + 1} no {_config.Questions.Length}";
		_questionLabel.Text = q.QuestionText;

		_selectedOption = null;
		_nextButton.SetInteractable(false);
		_nextButton.ButtonText = (index == _config.Questions.Length - 1) ? "Iesniegt" : "Nākamais";

		if (q.IsNumeric)
		{
			_optionsRoot.Visible = false;
			foreach (var btn in _optionButtons)
				btn.SetInteractable(false);
			_numpad.SetEnabled(true);
			_numpad.Visible = true;
			_numpad.Reset();
			return;
		}

		_numpad.SetEnabled(false);
		_numpad.Visible = false;
		_optionsRoot.Visible = true;

		for (int i = 0; i < _optionActions.Count; i++)
			_optionButtons[i].Pressed -= _optionActions[i];
		_optionActions.Clear();

		while (_optionButtons.Count < q.Options.Length)
		{
			var poolBtn = new VRButton { Width = 0.55f, Height = 0.08f };
			_optionsRoot.AddChild(poolBtn);
			_optionButtons.Add(poolBtn);
		}

		float totalHeight = (q.Options.Length - 1) * OptionSpacing;

		for (int i = 0; i < _optionButtons.Count; i++)
		{
			var btn = _optionButtons[i];

			if (i < q.Options.Length)
			{
				btn.ButtonText = q.Options[i];
				btn.SetSelected(false);
				btn.SetInteractable(true);
				float y = totalHeight / 2f - i * OptionSpacing;
				btn.Position = new Vector3(0f, y, 0f);
				btn.Visible = true;

				string capturedOption = q.Options[i];
				int    capturedIndex  = i;
				VRButton.PressedEventHandler handler = () => OnOptionSelected(capturedOption, capturedIndex);
				_optionActions.Add(handler);
				btn.Pressed += handler;
			}
			else
			{
				btn.Visible = false;
			}
		}
	}

	// Pārbauda vai ciparu ievade ir derīga
	private void OnNumpadValueChanged(string value)
	{
		var q = _config.Questions[_questionIndex];
		bool valid = value.Length > 0 &&
					 int.TryParse(value, out int n) &&
					 n >= q.MinValue &&
					 n <= q.MaxValue;
		_selectedOption = valid ? value : null;
		_nextButton.SetInteractable(valid);
	}

	// Atzīmē izvēlēto atbildi un iespējo pogu "Nākamais"
	private void OnOptionSelected(string option, int buttonIndex)
	{
		_selectedOption = option;

		for (int i = 0; i < _optionButtons.Count; i++)
			_optionButtons[i].SetSelected(i == buttonIndex);

		_nextButton.SetInteractable(true);
	}

	// Saglabā atbildi un pāriet uz nākamo jautājumu vai iesniedz aptauju
	private void OnNextPressed()
	{
		if (_selectedOption == null) return;

		var q = _config.Questions[_questionIndex];
		string key = string.IsNullOrEmpty(q.FieldName) ? q.QuestionText : q.FieldName;
		if (string.IsNullOrEmpty(q.FieldName))
			GD.PrintErr($"[SurveyManager] Jautājumam {_questionIndex + 1} nav FieldName - izmanto jautājuma tekstu kā atslēgu.");
		_responses[key] = _selectedOption;

		_questionIndex++;

		if (_questionIndex >= _config.Questions.Length)
		{
			_nextButton.SetInteractable(false);
			SubmitAndAdvance();
		}
		else
		{
			ShowQuestion(_questionIndex);
		}
	}

	// Nosūta atbildes ExperimentManager un pāriet uz nākamo ainu
	private void SubmitAndAdvance()
	{
		if (ExperimentManager.Instance == null) return;

		var phase = ExperimentManager.Instance.CurrentPhase;

		if (phase == ExperimentManager.Phase.PostMazeSurvey)
			ExperimentManager.Instance.OnPostMazeSurveyCompleted(_responses);
		else if (phase == ExperimentManager.Phase.Demographics)
			ExperimentManager.Instance.OnDemographicsCompleted(_responses);
	}

	// Pāreja uz jaunu ainu ar ekrāna aptumšošanu
	private void FadeIn()
	{
		var players = GetTree().GetNodesInGroup("player");
		foreach (Node p in players)
		{
			var bo = p.GetNodeOrNull<blackout>("XRCamera3D/BlackOut");
			if (bo == null) continue;
			bo.fade = 1.0f;
			var tween = CreateTween();
			tween.TweenProperty(bo, "fade", 0.0f, 0.5f);
			return;
		}
	}
}
