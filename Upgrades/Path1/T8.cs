using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Simulation.SMath;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;

namespace BloonsClicker.Upgrades;

public class StickyClicks : CursorUpgrade
{
    public override int Cost => 28000;
    protected override string Icon => Name;
    public override float Rate => .1f;
    public override string Description => "Attack now spawn a dart on screen that lasts for 5 seconds, or until it runs out of pierce.";
    public override int Tier => 8;
    protected override Main.LifeSpan LifeSpan => Main.LifeSpan.StickyClicks;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.AddBehavior(Game.instance.model.GetTower(TowerType.DartMonkey).GetWeapon().projectile.GetBehavior<DisplayModel>().Duplicate());
        projectile.display = Game.instance.model.GetTower(TowerType.DartMonkey).GetWeapon().projectile.display;
        
        projectile.AddBehavior(Game.instance.model.GetTower(TowerType.SpikeFactory).GetWeapon().projectile.GetBehavior<HeightOffsetProjectileModel>().Duplicate());
        projectile.AddBehavior(Game.instance.model.GetTower(TowerType.SpikeFactory).GetWeapon().projectile.GetBehavior<ArriveAtTargetModel>().Duplicate());
    }
}