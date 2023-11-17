namespace BloonsClicker.Upgrades;

public class HypersonicClicks : CursorUpgrade
{
    public override int Cost => 2300;
    protected override string Icon => Name;
    public override float Rate => .4f;
    public override string Description => "Attack speed increased to 1 every "+Rate+" seconds, pierce increased to 7, explosion damage increased to 2, and can now pop black bloons";
    public override int Tier => 5;
    protected override Main.LifeSpan LifeSpan => Main.LifeSpan.NormalClick;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.pierce = 7;
        projectile.GetBehavior<CreateProjectileOnContactModel>().projectile.GetDamageModel().damage = 2;
        foreach (var behavior in projectile.GetDescendants<FilterInvisibleModel>().ToList())
        {
            behavior.isActive = false;
        }
        foreach (var behavior in projectile.GetDescendants<DamageModel>().ToList())
        {
            behavior.immuneBloonProperties = BloonProperties.None;
        }
    }
}