using MelonLoader;


[assembly: MelonInfo(typeof(WtcPractice.PracticeCore), "WTC Practice", "0.2.0", "wahlgreen.dev")]
[assembly: MelonGame("Triband", "WHATTHECAR")]

namespace WtcPractice;

public class PracticeCore : MelonMod
{
    public static MelonLogger.Instance Log { get; private set; }

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        Log.Msg($"[practice] {PracticeMode.SaveKey} or {PracticeMode.SaveButton} sets a checkpoint, {PracticeMode.RestoreKey} or {PracticeMode.RestoreButton} teleports back, {PracticeMode.ToggleHudKey} toggles the HUD");
    }

    public override void OnUpdate()
    {
        PracticeMode.Tick();
    }

    public override void OnLateUpdate()
    {
        PracticeMode.LateTick();
    }

    public override void OnGUI()
    {
        PracticeHud.Draw(PracticeMode.CanPractice);
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        PracticeMode.Forget();
        RunIntegrity.Reset("scene load");
    }
}
