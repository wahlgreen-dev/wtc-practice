namespace WtcPractice;

public readonly struct WorldSnapshot
{
    public readonly WorldBody[] Bodies;

    public WorldSnapshot(WorldBody[] bodies)
    {
        Bodies = bodies;
    }

    public int Count => Bodies == null ? 0 : Bodies.Length;
}
