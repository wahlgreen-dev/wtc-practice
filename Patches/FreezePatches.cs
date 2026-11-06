using HarmonyLib;
using Il2CppSpeed;
using UnityEngine;

namespace WtcPractice;

[HarmonyPatch(typeof(TimeManager), nameof(TimeManager.Update))]
internal static class HoldFreezePatch
{
    private static void Postfix()
    {
        if (PracticeMod.Instance.Session.Frozen)
            Time.timeScale = 0f;
    }
}
