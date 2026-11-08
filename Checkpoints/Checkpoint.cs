using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WtcPractice;

public class Checkpoint
{
    public Checkpoint(string levelId, float time, CarSnapshot car)
    {
        LevelId = levelId;
        Time = time;
        Car = car;
    }

    public string LevelId { get; }

    public float Time { get; }

    public CarSnapshot Car { get; }

    public GameObject Flag { get; set; }

    public bool BelongsTo(string levelId) =>
        !string.IsNullOrEmpty(levelId) && string.Equals(LevelId, levelId, StringComparison.Ordinal);

    public void RemoveFlag()
    {
        if (Flag != null)
            Object.Destroy(Flag);

        Flag = null;
    }
}
