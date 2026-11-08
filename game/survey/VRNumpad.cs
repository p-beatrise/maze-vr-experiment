using Godot;
using System;

// VR ciparu ievade skaitliskām atbildēm
[GlobalClass]
public partial class VRNumpad : Node3D
{
    public event Action<string> ValueChanged;

    private const float BtnSize = 0.075f;
    private const float Gap     = 0.012f;

    private static readonly string[,] KeyGrid =
    {
        { "7", "8", "9" },
        { "4", "5", "6" },
        { "1", "2", "3" },
        { "←", "0", ""  },
    };

    private string  _current   = "";
    private int     _maxLength = 3;
    private Label3D _display;

    public string CurrentValue => _current;

    public override void _Ready()
    {
        BuildDisplay();
        BuildKeys();
    }

    // Iespējo vai atspējo ciparu ievades pogas
    public void SetEnabled(bool enabled)
    {
        foreach (var child in GetChildren())
            if (child is VRButton btn)
                btn.SetInteractable(enabled);
    }

    // Notīra ievadīto vērtību
    public void Reset()
    {
        _current = "";
        UpdateDisplay();
        ValueChanged?.Invoke(_current);
    }

    // Pogu teksta pievienošana
    private void BuildDisplay()
    {
        float step = BtnSize + Gap;
        int   rows = KeyGrid.GetLength(0);
        float topRowY = (rows - 1) * step / 2f;

        _display = new Label3D
        {
            FontSize  = 72,
            PixelSize = 0.0005f,
            Text      = "_",
            Modulate  = new Color(1f, 1f, 1f),
            Position  = new Vector3(0f, topRowY + step, 0f),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        AddChild(_display);
    }

    // Pogu izvietošana režģī
    private void BuildKeys()
    {
        int   rows   = KeyGrid.GetLength(0);
        int   cols   = KeyGrid.GetLength(1);
        float step   = BtnSize + Gap;
        float startX = -(cols - 1) * step / 2f;
        float startY = (rows - 1) * step / 2f;

        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            string key = KeyGrid[r, c];
            if (string.IsNullOrEmpty(key)) continue;

            var btn = new VRButton
            {
                ButtonText = key,
                Width      = BtnSize,
                Height     = BtnSize,
                FontSize   = 64,
                Position   = new Vector3(startX + c * step, startY - r * step, 0f),
            };

            string captured = key;
            btn.Pressed += () => OnKeyPressed(captured);
            AddChild(btn);
            btn.SetInteractable(false);
        }
    }

    // Taustiņu nospiešanas signāls
    private void OnKeyPressed(string key)
    {
        if (key == "←")
        {
            if (_current.Length > 0)
                _current = _current[..^1];
        }
        else if (_current.Length < _maxLength)
        {
            if (_current.Length == 0 && key == "0") return;
            _current += key;
        }

        UpdateDisplay();
        ValueChanged?.Invoke(_current);
    }

     private void UpdateDisplay()
    {
        _display.Text = _current.Length > 0 ? _current : "_";
    }
}
