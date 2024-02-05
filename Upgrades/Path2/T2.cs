
namespace BloonsClicker.Upgrades.Path2;

public class BetterFreeze : CursorUpgrade
{
    public override int Cost => 400;
    public override string Description => "Bloons are now frozen for longer and freeze goes through more layers";
    public override int Tier => 2;
    public override Path Path => Path.Second;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.GetBehavior<FreezeModel>().lifespan = 2;
        projectile.GetBehavior<FreezeModel>().lifespanFrames = 120;
        projectile.GetBehavior<FreezeModel>().Lifespan = 2;
        projectile.GetBehavior<FreezeModel>().layers = 3; // 2 layers + 1 layer from the upgrade
    }
}