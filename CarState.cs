using Il2Cpp;
using Il2CppSpeed.Level;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WtcPractice;

public static class CarState
{
    public static CarSnapshot? TryCapture(string levelId)
    {
        if (!TryFindCar(out ThisIsThePlayerVehicle vehicle, out Rigidbody body))
            return null;

        DampedSteering steering = vehicle.steering;
        Vector3 steerDir = steering == null ? body.transform.forward : steering.currDir;

        var snapshot = new CarSnapshot(levelId, body.position, body.rotation, body.velocity, body.angularVelocity, steerDir);

        PracticeCore.Log.Msg($"[capture] {snapshot.Describe()} (rb {body.GetInstanceID()})");
        return snapshot;
    }

    public static bool TryRestore(CarSnapshot snapshot)
    {
        if (!TryFindCar(out ThisIsThePlayerVehicle vehicle, out Rigidbody body))
            return false;

        body.position = snapshot.Position;
        body.rotation = snapshot.Rotation;
        body.velocity = snapshot.Velocity;
        body.angularVelocity = snapshot.AngularVelocity;

        DampedSteering steering = vehicle.steering;
        if (steering != null && snapshot.SteerDir.sqrMagnitude > 0.0001f)
            steering.SetOrientation(snapshot.SteerDir);

        PracticeCore.Log.Msg($"[restore] {snapshot.Describe()} (rb {body.GetInstanceID()}, steering {(steering == null ? "none" : "set")})");
        return true;
    }

    private static bool TryFindCar(out ThisIsThePlayerVehicle vehicle, out Rigidbody body)
    {
        body = null;
        vehicle = Object.FindObjectOfType<ThisIsThePlayerVehicle>();
        if (vehicle == null)
        {
            PracticeCore.Log.Warning("[car] no player vehicle in this scene");
            return false;
        }

        body = vehicle._mainRigidbody;
        if (body == null)
        {
            PracticeCore.Log.Warning("[car] vehicle has no main rigidbody yet");
            return false;
        }

        return true;
    }
}
