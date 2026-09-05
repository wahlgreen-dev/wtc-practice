using UnityEngine;

namespace WtcPractice;

public readonly struct WorldBody
{
    public readonly Rigidbody Body;

    public readonly Vector3 Position;
    public readonly Quaternion Rotation;

    public readonly Vector3 Velocity;
    public readonly Vector3 AngularVelocity;

    public readonly bool Kinematic;
    public readonly bool Sleeping;
    public readonly bool Active;

    public WorldBody(Rigidbody body)
    {
        Body = body;

        Position = body.position;
        Rotation = body.rotation;

        Velocity = body.velocity;
        AngularVelocity = body.angularVelocity;

        Kinematic = body.isKinematic;
        Sleeping = body.IsSleeping();
        Active = body.gameObject.activeSelf;
    }
}
