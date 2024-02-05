using Il2CppAssets.Scripts.Models.Bloons.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;

namespace BloonsClicker.Upgrades.Path3;

public class AmplifiedClicks : CursorUpgrade
{
    public override int Cost => 1400;
    public override string Description => "Clicks that dont hit any Bloon now have almost 0 cooldown.";
    public override int Tier => 5;
    public override Path Path => Path.Third;

    /// <inheritdoc />
    public override void OnDestroy(Projectile projectile)
    {
        if (!Main.ProjectileHitBloon.Contains(projectile.Id.Id))
        {
            RateModifier = 0.01f; //not quite 0 but close enough, 0 causes performance issues with projectiles being spawned every single frame
        }
    }
}