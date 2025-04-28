namespace BloonsClicker.Upgrades.Path2;

public class SlowingClicks : CursorUpgrade
{
    public override int Cost => 950;
    public override string Description => "Bloons remain slowed after freezing wears off";
    public override int Tier => 3;
    public override Path Path => Path.Second;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        var slowModel = new SlowModel("SlowModel_SlowingClicks_", .5f, 9999999f, "SlowingClicks:Normal", 9999999, "", true,
            false, null,
            true, false, false, 0, 0, false);
        
        projectile.AddBehavior(slowModel);
    }
}