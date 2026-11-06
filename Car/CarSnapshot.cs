using UnityEngine;

namespace WtcPractice;

public readonly struct CarSnapshot
{
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;
    public readonly Vector3 Velocity;

    public readonly Vector3 AngularVelocity;

    public readonly Vector3 SteerDir;

    public readonly CarPart[] Parts;

    public readonly CarPower? Power;

    public CarSnapshot(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, Vector3 steerDir, CarPart[] parts, CarPower? power)
    {
        Position = position;
        Rotation = rotation;
        Velocity = velocity;
        AngularVelocity = angularVelocity;
        SteerDir = steerDir;
        Parts = parts;
        Power = power;
    }
}
