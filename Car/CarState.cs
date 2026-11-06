using System.Collections.Generic;
using Il2Cpp;
using Il2CppSpeed.Level;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WtcPractice;

public class CarState
{
    private readonly MelonLogger.Instance logger;

    public CarState(MelonLogger.Instance logger)
    {
        this.logger = logger;
    }

    public CarSnapshot? TryCapture()
    {
        if (!TryFindCar(out ThisIsThePlayerVehicle vehicle, out Rigidbody body))
            return null;

        DampedSteering steering = vehicle.steering;
        Vector3 steerDir = steering == null ? body.transform.forward : steering.currDir;

        return new CarSnapshot(body.position, body.rotation, body.velocity, body.angularVelocity, steerDir, CaptureParts(vehicle, body), CapturePower(vehicle));
    }

    public bool TryRestore(CarSnapshot snapshot, out Vector3 warp)
    {
        warp = Vector3.zero;

        if (!TryFindCar(out ThisIsThePlayerVehicle vehicle, out Rigidbody body))
            return false;

        warp = snapshot.Position - body.position;

        List<Rigidbody> parts = FindParts(vehicle, body);
        using TeleportGuard teleport = new(body, parts);

        Place(body, snapshot.Position, snapshot.Rotation, snapshot.Velocity, snapshot.AngularVelocity);

        RestoreParts(snapshot, parts);

        RestorePower(vehicle, snapshot.Power);

        Physics.SyncTransforms();

        DampedSteering steering = vehicle.steering;
        if (steering != null && snapshot.SteerDir.sqrMagnitude > 0.0001f)
            steering.SetOrientation(snapshot.SteerDir);

        return true;
    }

    private CarPower? CapturePower(ThisIsThePlayerVehicle vehicle)
    {
        ActionPowerState actionPower = FindPower(vehicle);
        if (actionPower == null)
            return null;

        return new CarPower(actionPower, Time.time);
    }

    private void RestorePower(ThisIsThePlayerVehicle vehicle, CarPower? snapshot)
    {
        if (!snapshot.HasValue)
            return;

        ActionPowerState actionPower = FindPower(vehicle);
        if (actionPower == null)
        {
            logger.Warning("The checkpoint had a power bar, this car has none");
            return;
        }

        snapshot.Value.RestoreTo(actionPower, Time.time);
    }

    private ActionPowerState FindPower(ThisIsThePlayerVehicle vehicle)
    {
        ControllerState state = vehicle.state;
        return state == null ? null : state.actionPower;
    }

    private CarPart[] CaptureParts(ThisIsThePlayerVehicle vehicle, Rigidbody main)
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

    private void RestoreParts(CarSnapshot snapshot, List<Rigidbody> parts)
    {
        if (snapshot.Parts == null)
            return;

        if (snapshot.Parts.Length != parts.Count)
        {
            logger.Warning($"Car changed since the checkpoint ({snapshot.Parts.Length} parts, now {parts.Count}), moved the main body only");
            return;
        }

        for (int i = 0; i < parts.Count; i++)
        {
            CarPart part = snapshot.Parts[i];
            Vector3 position = snapshot.Position + snapshot.Rotation * part.LocalPosition;
            Quaternion rotation = snapshot.Rotation * part.LocalRotation;

            Place(parts[i], position, rotation, part.Velocity, part.AngularVelocity);
        }
    }

    private void Place(Rigidbody body, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
    {
        body.position = position;
        body.rotation = rotation;
        body.velocity = velocity;
        body.angularVelocity = angularVelocity;
        body.transform.SetPositionAndRotation(position, rotation);
    }

    private List<Rigidbody> FindParts(ThisIsThePlayerVehicle vehicle, Rigidbody main)
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

    private bool TryFindCar(out ThisIsThePlayerVehicle vehicle, out Rigidbody body)
    {
        body = null;
        vehicle = Object.FindObjectOfType<ThisIsThePlayerVehicle>();
        if (vehicle == null)
        {
            logger.Warning("No player vehicle in this scene");
            return false;
        }

        body = vehicle._mainRigidbody;
        if (body == null)
        {
            logger.Warning("Vehicle has no main rigidbody yet");
            return false;
        }

        return true;
    }
}
