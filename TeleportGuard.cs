using System;
using System.Collections.Generic;
using UnityEngine;

namespace WtcPractice;

public readonly struct TeleportGuard : IDisposable
{
    private readonly List<Held> held;

    public TeleportGuard(Rigidbody main, List<Rigidbody> parts)
    {
        held = new List<Held>(parts.Count + 1) { new(main) };

        foreach (Rigidbody part in parts)
            held.Add(new Held(part));

        foreach (Held body in held)
        {
            body.Body.interpolation = RigidbodyInterpolation.None;
            body.Body.collisionDetectionMode = CollisionDetectionMode.Discrete;
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
