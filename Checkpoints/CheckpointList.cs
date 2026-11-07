using System.Collections.Generic;

namespace WtcPractice;

public class CheckpointList
{
    private readonly List<Checkpoint> checkpoints = new();
    private int currentIndex = -1;

    public Checkpoint Current => currentIndex < 0 ? null : checkpoints[currentIndex];

    public int Count => checkpoints.Count;

    public int Position => currentIndex + 1;

    public void Add(Checkpoint checkpoint)
    {
        checkpoints.RemoveRange(currentIndex + 1, checkpoints.Count - currentIndex - 1);
        checkpoints.Add(checkpoint);
        currentIndex = checkpoints.Count - 1;
    }

    public bool Step(int direction)
    {
        int next = currentIndex + direction;
        if (next < 0 || next >= checkpoints.Count)
            return false;

        currentIndex = next;
        return true;
    }

    public void Clear()
    {
        checkpoints.Clear();
        currentIndex = -1;
    }
}
