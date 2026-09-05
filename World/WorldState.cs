using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WtcPractice;

public static class WorldState
{
    public static WorldSnapshot Capture()
    {
        Transform car = CarState.FindCarRoot();
        List<WorldBody> captured = new();
        int skipped = 0;

        foreach (Rigidbody body in Object.FindObjectsOfType<Rigidbody>(true))
        {
            if (body == null)
                continue;

            if (car != null && body.transform.IsChildOf(car))
                continue;

            try
            {
                captured.Add(new WorldBody(body));
            }
            catch (Exception e)
            {
                skipped++;
                if (skipped == 1)
                    PracticeCore.Log.Warning($"[world] could not read a physics object: {e.Message}");
            }
        }

        PracticeCore.Log.Msg($"[world] captured {captured.Count} physics objects, skipped {skipped}");
        return new WorldSnapshot(captured.ToArray());
    }

    public static int Restore(WorldSnapshot snapshot)
    {
        if (snapshot.Count == 0)
            return 0;

        List<Rigidbody> alive = Alive(snapshot);
        int gone = snapshot.Count - alive.Count;
        int restored = 0;
        int failed = 0;

        using (TeleportGuard teleport = new(alive))
        {
            foreach (WorldBody entry in snapshot.Bodies)
            {
                if (entry.Body == null)
                    continue;

                try
                {
                    Place(entry);
                    restored++;
                }
                catch (Exception e)
                {
                    failed++;
                    if (failed == 1)
                        PracticeCore.Log.Warning($"[world] could not move a physics object: {e.Message}");
                }
            }
        }

        Physics.SyncTransforms();

        if (gone > 0 || failed > 0)
            PracticeCore.Log.Warning($"[world] restored {restored} of {snapshot.Count} physics objects, {gone} destroyed since the checkpoint, {failed} refused");
        else
            PracticeCore.Log.Msg($"[world] restored {restored} physics objects");

        return restored;
    }

    private static List<Rigidbody> Alive(WorldSnapshot snapshot)
    {
        List<Rigidbody> alive = new(snapshot.Count);

        foreach (WorldBody entry in snapshot.Bodies)
        {
            if (entry.Body == null)
                continue;

            alive.Add(entry.Body);
        }

        return alive;
    }

    private static void Place(WorldBody entry)
    {
        Rigidbody body = entry.Body;
        GameObject owner = body.gameObject;

        if (entry.Active && !owner.activeSelf)
            owner.SetActive(true);

        body.isKinematic = entry.Kinematic;
        body.position = entry.Position;
        body.rotation = entry.Rotation;
        body.transform.SetPositionAndRotation(entry.Position, entry.Rotation);

        if (!entry.Kinematic)
        {
            body.velocity = entry.Velocity;
            body.angularVelocity = entry.AngularVelocity;
        }

        if (owner.activeInHierarchy && !entry.Kinematic)
        {
            if (entry.Sleeping)
                body.Sleep();
            else
                body.WakeUp();
        }

        if (!entry.Active && owner.activeSelf)
            owner.SetActive(false);
    }
}
