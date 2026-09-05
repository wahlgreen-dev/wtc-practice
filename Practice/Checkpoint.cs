namespace WtcPractice;

public readonly struct Checkpoint
{
    public readonly CarSnapshot Car;
    public readonly WorldSnapshot World;

    public Checkpoint(CarSnapshot car, WorldSnapshot world)
    {
        Car = car;
        World = world;
    }
}
