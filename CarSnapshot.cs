using System;
using UnityEngine;

namespace WtcPractice;

public readonly struct CarSnapshot
{
    public readonly string LevelId;

    public readonly Vector3 Position;
    public readonly Quaternion Rotation;
    public readonly Vector3 Velocity;

    public readonly Vector3 AngularVelocity;

    public readonly Vector3 SteerDir;

    public CarSnapshot(string levelId, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, Vector3 steerDir)
    {
        LevelId = levelId;
        Position = position;
        Rotation = rotation;
        Velocity = velocity;
        AngularVelocity = angularVelocity;
        SteerDir = steerDir;
    }

    public float Speed => Velocity.magnitude;

    public bool Matches(string levelId) =>
        !string.IsNullOrEmpty(levelId) &&
        !string.IsNullOrEmpty(LevelId) &&
        string.Equals(LevelId, levelId, StringComparison.Ordinal);

    public string Describe() =>
        $"{LevelId} pos {Position.x:F1},{Position.y:F1},{Position.z:F1} " +
        $"speed {Speed:F1}m/s spin {AngularVelocity.magnitude:F1}rad/s " +
        $"dir {SteerDir.x:F2},{SteerDir.y:F2},{SteerDir.z:F2}";
}
