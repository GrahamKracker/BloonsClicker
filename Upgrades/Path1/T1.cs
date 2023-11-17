namespace BloonsClicker.Upgrades;

public class StrongerClicks : CursorUpgrade
{
    public override int Cost => 250;
    protected override string Icon => Name;
    public override float Rate => 1f;
    public override string Description => "Pierce increased to 3 and damage increased to 2";
    public override int Tier => 1;
    protected override Main.LifeSpan LifeSpan => Main.LifeSpan.NormalClick;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.pierce = 3;
        projectile.GetDamageModel().damage = 2;
    }
}