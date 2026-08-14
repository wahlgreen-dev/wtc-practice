using System;

namespace WtcPractice;

public static class RunIntegrity
{
    public static bool IsDirty { get; private set; }

    public static string Reason { get; private set; }

    public static void MarkDirty(string reason)
    {
        if (IsDirty)
            return;

        IsDirty = true;
        Reason = reason;
        PracticeCore.Log?.Msg($"[integrity] run invalidated — {reason}");
    }

    public static void BeginFreshAttempt(string cause)
    {
        if (!IsDirty)
            return;

        IsDirty = false;
        Reason = null;
        PracticeCore.Log?.Msg($"[integrity] clean again — {cause}");
    }

    public static bool Blocks(string what)
    {
        try
        {
            if (!IsDirty)
                return false;

            PracticeCore.Log?.Msg($"[integrity] blocked {what} — {Reason}");
            return true;
        }
        catch (Exception e)
        {
            PracticeCore.Log?.Error($"integrity guard threw: {e}");
            return false;
        }
    }
}
