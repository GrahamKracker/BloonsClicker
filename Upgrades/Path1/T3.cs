namespace BloonsClicker.Upgrades;

public class PowerfulClicks : CursorUpgrade
{
    public override int Cost => 775;
    public override string Description => "Damage and pierce increased, and now hits camo bloons";
    public override int Tier => 3;

    /// <inheritdoc />
    public override Path Path => Path.First;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.GetDamageModel().damage++;
        projectile.pierce++;
    }

    /// <inheritdoc />
    protected override void PostModifyProjectile(ProjectileModel projectile)
    {
        foreach (var behavior in projectile.GetDescendants<FilterInvisibleModel>().ToList())
        {
            behavior.isActive = false;
        }
    }
}