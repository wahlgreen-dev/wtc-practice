using Il2CppSpeed;
using Il2CppSpeed.Level;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;


[assembly: MelonInfo(typeof(WtcPractice.PracticeCore), "WTC Practice", "0.1.0", "wahlgreen.dev")]
[assembly: MelonGame("Triband", "WHATTHECAR")]

namespace WtcPractice;

public class PracticeCore : MelonMod
{
    private CarSnapshot snapshot;
    private Rigidbody playerBody;
    public static MelonLogger.Instance Log { get; private set; }

    public override void OnInitializeMelon()
    {
        Log = LoggerInstance;

        MelonPreferences_Category cfg = MelonPreferences.CreateCategory("WtcPractice");
    }

    public override void OnUpdate()
    {
        if (!GameContext.gameFlow.isInLevel)
            return;
        
        if (playerBody == null)
        {
            ThisIsThePlayerVehicle vehicle = Object.FindObjectOfType<ThisIsThePlayerVehicle>();
            playerBody = vehicle._mainRigidbody;
        }
        
        Keyboard keyboard = Keyboard.current;
        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            snapshot = new(playerBody.position, playerBody.rotation, playerBody.velocity, playerBody.angularVelocity);
        }

        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            playerBody.position = snapshot.Position;
            playerBody.rotation = snapshot.Rotation;
            playerBody.velocity = snapshot.Velocity;
            playerBody.angularVelocity = snapshot.AngularVelocity;
        }
        
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        Log.Msg($"[scene] {sceneName}");
    }
}
