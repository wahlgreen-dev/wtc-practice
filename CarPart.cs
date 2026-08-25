using UnityEngine;

namespace WtcPractice;

public readonly struct CarPart
{
    public readonly Vector3 LocalPosition;
    public readonly Quaternion LocalRotation;

    public readonly Vector3 Velocity;
    public readonly Vector3 AngularVelocity;

    public CarPart(Vector3 localPosition, Quaternion localRotation, Vector3 velocity, Vector3 angularVelocity)
    {
        LocalPosition = localPosition;
        LocalRotation = localRotation;
        Velocity = velocity;
        AngularVelocity = angularVelocity;
    }
}
