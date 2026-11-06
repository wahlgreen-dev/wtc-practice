using System;

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

    public bool BelongsTo(string levelId) =>
        !string.IsNullOrEmpty(levelId) && string.Equals(LevelId, levelId, StringComparison.Ordinal);
}
