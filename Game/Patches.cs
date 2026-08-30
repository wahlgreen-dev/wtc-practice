using System;
using HarmonyLib;
using Il2CppSpeed.GhostCar;
using Il2CppSpeed.Leaderboards;
using Il2CppSpeed;
using Il2CppSpeed.Level;
using UnityEngine;

namespace WtcPractice;

[HarmonyPatch(typeof(LevelInstance), nameof(LevelInstance.AddWonResult))]
internal static class PatchInvalidateWonResult
{
    private static void Prefix(ref bool cheated)
    {
        try
        {
            if (RunIntegrity.IsDirty)
                cheated = true;
        }
        catch (Exception e)
        {
            PracticeCore.Log?.Error($"invalidate-result prefix threw: {e}");
        }
    }
}

[HarmonyPatch(typeof(LevelLeaderboardsManager), nameof(LevelLeaderboardsManager.SubmitTime))]
internal static class PatchBlockLevelSubmit
{
    private static bool Prefix() => !RunIntegrity.Blocks("level leaderboard submit");
}

[HarmonyPatch(typeof(GlobalLeaderboardsManager), nameof(GlobalLeaderboardsManager.SubmitScore))]
internal static class PatchBlockGlobalSubmit
{
    private static bool Prefix() => !RunIntegrity.Blocks("global leaderboard submit");
}

[HarmonyPatch(typeof(GlobalLeaderboardsManager), nameof(GlobalLeaderboardsManager.SubmitScoreInMiliseconds))]
internal static class PatchBlockGlobalSubmitMs
{
    private static bool Prefix() => !RunIntegrity.Blocks("global leaderboard submit (ms)");
}

[HarmonyPatch(typeof(GhostCarFileManager), nameof(GhostCarFileManager.SaveGhostRunLocally))]
internal static class PatchBlockGhostSave
{
    private static bool Prefix() => !RunIntegrity.Blocks("personal-best ghost save");
}

[HarmonyPatch(typeof(LevelIntroState), nameof(LevelIntroState.OnEnterState))]
internal static class PatchAttemptFromIntro
{
    private static void Postfix() => Fresh.Begin("level intro");
}

[HarmonyPatch(typeof(LevelInstance), nameof(LevelInstance.PrepareForRestart))]
internal static class PatchAttemptFromRestart
{
    private static void Postfix() => Fresh.Begin("level restart");
}

[HarmonyPatch(typeof(TimeManager), nameof(TimeManager.Update))]
internal static class PatchHoldFreeze
{
    private static void Postfix()
    {
        try
        {
            if (PracticeMode.Frozen)
                Time.timeScale = 0f;
        }
        catch (Exception e)
        {
            PracticeCore.Log?.Error($"hold-freeze postfix threw: {e}");
        }
    }
}

internal static class Fresh
{
    internal static void Begin(string cause)
    {
        try
        {
            RunIntegrity.Reset(cause);
        }
        catch (Exception e)
        {
            PracticeCore.Log?.Error($"fresh-attempt hook threw: {e}");
        }
    }
}
