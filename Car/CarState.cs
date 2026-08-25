using System.Collections.Generic;
using Il2Cpp;
using Il2CppSpeed.Level;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WtcPractice;

public static class CarState
{
    public static CarSnapshot? TryCapture(string levelId, float time)
    {
        if (!TryFindCar(out ThisIsThePlayerVehicle vehicle, out Rigidbody body))
            return null;

        DampedSteering steering = vehicle.steering;
        Vector3 steerDir = steering == null ? body.transform.forward : steering.currDir;

        return new CarSnapshot(levelId, body.position, body.rotation, body.velocity, body.angularVelocity, steerDir, time, CaptureParts(vehicle, body));
    }

    public static bool TryRestore(CarSnapshot snapshot, out Vector3 warp)
    {
        warp = Vector3.zero;

        if (!TryFindCar(out ThisIsThePlayerVehicle vehicle, out Rigidbody body))
            return false;

        warp = snapshot.Position - body.position;

        List<Rigidbody> parts = FindParts(vehicle, body);
        using TeleportGuard teleport = new(body, parts);

        body.position = snapshot.Position;
        body.rotation = snapshot.Rotation;
        body.velocity = snapshot.Velocity;
        body.angularVelocity = snapshot.AngularVelocity;

        RestoreParts(snapshot, parts);

        Physics.SyncTransforms();

        DampedSteering steering = vehicle.steering;
        if (steering != null && snapshot.SteerDir.sqrMagnitude > 0.0001f)
            steering.SetOrientation(snapshot.SteerDir);

        return true;
    }

    private static CarPart[] CaptureParts(ThisIsThePlayerVehicle vehicle, Rigidbody main)
    {
        Quaternion inverse = Quaternion.Inverse(main.rotation);
        List<Rigidbody> parts = FindParts(vehicle, main);
        CarPart[] captured = new CarPart[parts.Count];

        for (int i = 0; i < parts.Count; i++)
        {
            Rigidbody part = parts[i];

            captured[i] = new CarPart(
                inverse * (part.position - main.position),
                inverse * part.rotation,
                part.velocity,
                part.angularVelocity);
        }

        return captured;
    }

    private static void RestoreParts(CarSnapshot snapshot, List<Rigidbody> parts)
    {
        if (snapshot.Parts == null)
            return;

        if (snapshot.Parts.Length != parts.Count)
        {
            PracticeCore.Log.Warning($"[car] car changed since the checkpoint ({snapshot.Parts.Length} parts, now {parts.Count}), moved the main body only");
            return;
        }

        for (int i = 0; i < parts.Count; i++)
        {
            CarPart part = snapshot.Parts[i];
            Rigidbody body = parts[i];

            body.position = snapshot.Position + snapshot.Rotation * part.LocalPosition;
            body.rotation = snapshot.Rotation * part.LocalRotation;
            body.velocity = part.Velocity;
            body.angularVelocity = part.AngularVelocity;
        }
    }

    private static List<Rigidbody> FindParts(ThisIsThePlayerVehicle vehicle, Rigidbody main)
    {
        List<Rigidbody> parts = new();

        foreach (Rigidbody part in vehicle.GetComponentsInChildren<Rigidbody>(true))
        {
            if (part == null || part.GetInstanceID() == main.GetInstanceID())
                continue;

            parts.Add(part);
        }

        return parts;
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
