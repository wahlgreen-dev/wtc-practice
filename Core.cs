using MelonLoader;


[assembly: MelonInfo(typeof(WtcPractice.PracticeCore), "WTC Practice", "0.1.0", "wahlgreen.dev")]
[assembly: MelonGame("Triband", "WHATTHECAR")]

namespace WtcPractice;

public class PracticeCore : MelonMod
{
    public static MelonLogger.Instance Log { get; private set; }

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;
        Log.Msg($"[practice] {PracticeMode.SaveKey} saves, {PracticeMode.RestoreKey} restores");
    }

    public override void OnUpdate()
    {
        PracticeMode.Tick();
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        PracticeMode.Forget();
        Log.Msg($"[scene] {sceneName}");
    }
}
