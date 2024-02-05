using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;

namespace BloonsClicker.Upgrades.Path3;

public class EnhancedClicks : CursorUpgrade
{
    public override int Cost => 650;
    public override string Description => "If a click doesn't hit a bloon, the cooldown between clicks is halved. This effect is disabled if the clicker leaves a projectile.";
    public override int Tier => 3;
    public override Path Path => Path.Third;

    /// <inheritdoc />
    public override void OnDestroy(Projectile projectile)
    {
        if (!Main.ProjectileHitBloon.Contains(projectile.Id.Id))
        {
            RateModifier = .5f;
        }
    }
}