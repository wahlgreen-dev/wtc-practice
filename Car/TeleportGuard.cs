using System;
using System.Collections.Generic;
using UnityEngine;

namespace WtcPractice;

public readonly struct TeleportGuard : IDisposable
{
    private readonly List<Held> held;

    public TeleportGuard(IReadOnlyList<Rigidbody> bodies)
    {
        held = new List<Held>(bodies.Count);

        foreach (Rigidbody body in bodies)
        {
            held.Add(new Held(body));

            body.interpolation = RigidbodyInterpolation.None;
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }
    }

    public void Dispose()
    {
        foreach (Held body in held)
        {
            body.Body.interpolation = body.Interpolation;
            body.Body.collisionDetectionMode = body.Collision;
        }
    }

    private readonly struct Held
    {
        public readonly Rigidbody Body;
        public readonly RigidbodyInterpolation Interpolation;
        public readonly CollisionDetectionMode Collision;

        public Held(Rigidbody body)
        {
            Body = body;
            Interpolation = body.interpolation;
            Collision = body.collisionDetectionMode;
        }
    }
}
