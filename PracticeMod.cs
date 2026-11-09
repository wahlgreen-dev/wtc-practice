using MelonLoader;

[assembly: MelonInfo(typeof(WtcPractice.PracticeMod), "WTC Practice", "0.3.0", "wahlgreen-dev")]
[assembly: MelonGame("Triband", "WHATTHECAR")]

namespace WtcPractice;

public class PracticeMod : MelonMod
{
    public static PracticeMod Instance { get; private set; }

    public LevelSession Session { get; private set; }

    public override void OnInitializeMelon()
    {
        Instance = this;
        Session = new LevelSession(LoggerInstance);
    }

    public override void OnUpdate()
    {
        Session.Update();
    }

    public override void OnLateUpdate()
    {
        Session.LateUpdate();
    }

    public override void OnGUI()
    {
        Session.DrawHud();
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        Session.OnSceneInitialized(sceneName);
    }
}
