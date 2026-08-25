using System;
using System.Collections.Generic;
using UnityEngine;

namespace WtcPractice;

public readonly struct TeleportGuard : IDisposable
{
    private readonly List<Rigidbody> bodies;
    private readonly List<RigidbodyInterpolation> interpolations;
    private readonly List<CollisionDetectionMode> collisions;

    public TeleportGuard(Rigidbody main, List<Rigidbody> parts)
    {
        bodies = new List<Rigidbody>(parts.Count + 1) { main };
        bodies.AddRange(parts);

        interpolations = new List<RigidbodyInterpolation>(bodies.Count);
        collisions = new List<CollisionDetectionMode>(bodies.Count);

        foreach (Rigidbody body in bodies)
        {
            interpolations.Add(body.interpolation);
            collisions.Add(body.collisionDetectionMode);

            body.interpolation = RigidbodyInterpolation.None;
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < bodies.Count; i++)
        {
            bodies[i].interpolation = interpolations[i];
            bodies[i].collisionDetectionMode = collisions[i];
        }
    }
}
