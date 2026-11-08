using Godot;

// Aptaujas konfigurācija
[GlobalClass]
public partial class SurveyConfig : Resource
{
    // Aptaujas id reģistrēšanai, piem., post_maze, demographic
    [Export]
    public string SurveyId = "";

    [Export]
    public SurveyQuestion[] Questions = System.Array.Empty<SurveyQuestion>();
}
