namespace BloonsClicker.Upgrades;

public class Fingerdash : CursorUpgrade
{
    public override int Cost => 15550;
    protected override string Icon => Name;
    public override float Rate => .1f;
    public override string Description => "Now attacks every " + Rate + " seconds";
    public override int Tier => 7;
    protected override Main.LifeSpan LifeSpan => Main.LifeSpan.NormalClick;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
    }
}