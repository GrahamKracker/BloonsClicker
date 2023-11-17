namespace BloonsClicker.Upgrades;

public class PowerfulClicks : CursorUpgrade
{
    public override int Cost => 800;
    protected override string Icon => Name;
    public override float Rate => .7f;
    public override string Description => "Click radius increased. Damage increased to 3, pierce increased to 4, and now hits camo bloons";
    public override int Tier => 3;
    protected override Main.LifeSpan LifeSpan => Main.LifeSpan.NormalClick;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.GetDamageModel().damage = 3;
        projectile.pierce = 4;
        projectile.radius = 7.5f;

        foreach (var behavior in projectile.GetDescendants<FilterInvisibleModel>().ToList())
        {
            behavior.isActive = false;
        }
    }
}