
namespace BloonsClicker.Upgrades.Path3;

public class ContinuousClicks : CursorUpgrade
{
    public override int Cost => 400;
    public override string Description => "Holding down the mouse button no longer reduces attack rate.";
    public override int Tier => 2;
    public override Path Path => Path.Third;
}