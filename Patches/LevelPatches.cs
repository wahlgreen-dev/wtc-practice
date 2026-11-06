using System;
using HarmonyLib;
using Il2CppSpeed.Level;

namespace WtcPractice;

[HarmonyPatch(typeof(LevelIntroState), nameof(LevelIntroState.OnEnterState))]
internal static class LevelIntroPatch
{
    private static void Postfix()
    {
        PracticeMod.Instance.Session.OnNewAttempt();
    }
}

[HarmonyPatch(typeof(LevelInstance), nameof(LevelInstance.PrepareForRestart))]
internal static class LevelRestartPatch
{
    private static void Postfix()
    {
        PracticeMod.Instance.Session.OnNewAttempt();
    }
}

[HarmonyPatch(typeof(LevelManager), nameof(LevelManager.RestartLevel), new Type[0])]
internal static class ManagerRestartPatch
{
    private static void Postfix()
    {
        PracticeMod.Instance.Session.OnNewAttempt();
    }
}

[HarmonyPatch(typeof(LevelManager), nameof(LevelManager.RestartLevel), new[] { typeof(LevelInstance) })]
internal static class ManagerRestartLevelPatch
{
    private static void Postfix()
    {
        PracticeMod.Instance.Session.OnNewAttempt();
    }
}

[HarmonyPatch(typeof(LevelManager), nameof(LevelManager.RestartLevelAfterFail))]
internal static class RestartAfterFailPatch
{
    private static void Postfix()
    {
        PracticeMod.Instance.Session.OnNewAttempt();
    }
}

[HarmonyPatch(typeof(LevelManager), nameof(LevelManager.ResetWithoutReload))]
internal static class ResetWithoutReloadPatch
{
    private static void Postfix()
    {
        PracticeMod.Instance.Session.OnNewAttempt();
    }
}
