using Il2CppSpeed;
using Il2CppSpeed.Input;

namespace WtcPractice;

public static class GameInput
{
    private static bool lastPressWasGamepad;

    public static bool UsingGamepad()
    {
        if (TryAskGame(out bool gamepad))
            return gamepad;

        return lastPressWasGamepad;
    }

    public static void RememberKeyboard()
    {
        lastPressWasGamepad = false;
    }

    public static void RememberGamepad()
    {
        lastPressWasGamepad = true;
    }

    private static bool TryAskGame(out bool gamepad)
    {
        gamepad = false;

        if (!GameContext.exists)
            return false;

        GameInputManager manager = GameContext.gameInputManager;

        if (manager == null)
            return false;

        gamepad = manager.currentControllerType == EControllerType.Gamepad;
        return true;
    }
}
