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
        Log.Msg("ready");
    }
}
