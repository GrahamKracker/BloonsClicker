namespace BloonsClicker.Upgrades;

public class RapidClicks : CursorUpgrade
{
    public override int Cost => 525;
    protected override string Icon => Name;
    public override float Rate => .7f;
    public override string Description => "Attack rate increased to 1 every "+Rate+" seconds";
    public override int Tier => 2;
    protected override Main.LifeSpan LifeSpan => Main.LifeSpan.NormalClick;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
    }
}