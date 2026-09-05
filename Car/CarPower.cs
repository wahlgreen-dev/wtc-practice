using Il2Cpp;

namespace WtcPractice;

public readonly struct CarPower
{
    public readonly float Power;

    public readonly float SinceStartSpend;
    public readonly float SinceStopSpend;

    public readonly float SpentSinceStartSpend;
    public readonly bool HasStoppedTrying;

    public readonly bool CanSpend;
    public readonly bool CanCharge;

    public CarPower(ActionPowerState actionPower, float now)
    {
        Power = actionPower.power;
        SinceStartSpend = now - actionPower.lastStartSpendTime;
        SinceStopSpend = now - actionPower.lastStopSpendTime;
        SpentSinceStartSpend = actionPower.powerSpentSinceLastStartSpend;
        HasStoppedTrying = actionPower.hasStoppedTryingToSpendSinceLastSpendStopped;
        CanSpend = actionPower.canSpend;
        CanCharge = actionPower.canCharge;
    }

    public void RestoreTo(ActionPowerState actionPower, float now)
    {
        actionPower.lastStartSpendTime = now - SinceStartSpend;
        actionPower.lastStopSpendTime = now - SinceStopSpend;
        actionPower.powerSpentSinceLastStartSpend = SpentSinceStartSpend;
        actionPower.hasStoppedTryingToSpendSinceLastSpendStopped = HasStoppedTrying;
        actionPower.canSpend = CanSpend;
        actionPower.canCharge = CanCharge;

        actionPower.SetPower(Power);
    }
}
