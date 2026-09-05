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

    public readonly float Time;

    public readonly CarPart[] Parts;

    public readonly CarPower? Power;

    public CarSnapshot(string levelId, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity, Vector3 steerDir, float time, CarPart[] parts, CarPower? power)
    {
        LevelId = levelId;
        Position = position;
        Rotation = rotation;
        Velocity = velocity;
        AngularVelocity = angularVelocity;
        SteerDir = steerDir;
        Time = time;
        Parts = parts;
        Power = power;
    }

    public bool Matches(string levelId) =>
        !string.IsNullOrEmpty(levelId) &&
        !string.IsNullOrEmpty(LevelId) &&
        string.Equals(LevelId, levelId, StringComparison.Ordinal);
}
