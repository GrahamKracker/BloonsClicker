
namespace BloonsClicker.Upgrades.Path3;

public class EvenLargerClicks : CursorUpgrade
{
    public override int Cost => 700;
    public override string Description => "Clicks are even larger";
    public override int Tier => 4;
    public override Path Path => Path.Third;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.radius *= 3;
        if (projectile.HasBehavior<CreateProjectileOnContactModel>())
            projectile.GetBehavior<CreateProjectileOnContactModel>().projectile.radius *= 3;
    }
}