using HarmonyLib;
using Il2CppSpeed.GhostCar;
using Il2CppSpeed.Leaderboards;
using Il2CppSpeed.Level;

namespace WtcPractice;

[HarmonyPatch(typeof(LevelInstance), nameof(LevelInstance.AddWonResult))]
internal static class InvalidateWonResultPatch
{
    private static void Prefix(ref bool cheated)
    {
        if (PracticeMod.Instance.Session.RunInvalidated)
            cheated = true;
    }
}

[HarmonyPatch(typeof(LevelLeaderboardsManager), nameof(LevelLeaderboardsManager.SubmitTime))]
internal static class BlockLevelSubmitPatch
{
    private static bool Prefix() => !PracticeMod.Instance.Session.RunInvalidated;
}

[HarmonyPatch(typeof(GlobalLeaderboardsManager), nameof(GlobalLeaderboardsManager.SubmitScore))]
internal static class BlockGlobalSubmitPatch
{
    private static bool Prefix() => !PracticeMod.Instance.Session.RunInvalidated;
}

[HarmonyPatch(typeof(GlobalLeaderboardsManager), nameof(GlobalLeaderboardsManager.SubmitScoreInMiliseconds))]
internal static class BlockGlobalSubmitMsPatch
{
    private static bool Prefix() => !PracticeMod.Instance.Session.RunInvalidated;
}

[HarmonyPatch(typeof(GhostCarFileManager), nameof(GhostCarFileManager.SaveGhostRunLocally))]
internal static class BlockGhostSavePatch
{
    private static bool Prefix() => !PracticeMod.Instance.Session.RunInvalidated;
}
