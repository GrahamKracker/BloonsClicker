namespace BloonsClicker.Upgrades;

public class RapidClicks : CursorUpgrade
{
    public override int Cost => 425;
    /// <inheritdoc />
    protected override float ModifyRate(float rate)
    {
        return .7f * rate;
    }
    public override string Description => "Attack rate increased by 30%";
    public override int Tier => 2;
    public override Path Path => Path.First;
}