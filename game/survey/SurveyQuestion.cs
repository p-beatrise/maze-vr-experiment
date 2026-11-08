using Godot;

// Aptaujas jautājums ar atbilžu variantiem vai ciparu ievadi
[GlobalClass]
public partial class SurveyQuestion : Resource
{
    // Jautājuma teksts, kas tiek rādīts spēlētājam
    [Export(PropertyHint.MultilineText)]
    public string QuestionText = "";

    // Jautājuma nosaukums JSON failā
    [Export]
    public string FieldName = "";

    // Atbilžu varianti
    [Export]
    public string[] Options = System.Array.Empty<string>();

    [Export] public bool IsNumeric = false;
    [Export] public int  MinValue  = 1;
    [Export] public int  MaxValue  = 120;
}
