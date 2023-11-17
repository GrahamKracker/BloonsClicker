using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Simulation.SMath;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Utils;

namespace BloonsClicker.Upgrades;

public class NuclearClicks : CursorUpgrade
{
    public override int Cost => 250_000;
    protected override string Icon => Name;
    public override float Rate => .01f;
    public override string Description => "Dart damage and pierce doubled. Now releases 16 smaller darts on contact that also have double stats. Attack rate increased to every "+Rate+" seconds";
    public override int Tier => 10;
    protected override Main.LifeSpan LifeSpan => Main.LifeSpan.PermaClicks;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.pierce = 32;
        projectile.GetDamageModel().damage = 24;

        var createEffectOnContactModel = Game.instance.model.GetTowerFromId(TowerType.BombShooter).GetWeapon().projectile.GetBehavior<CreateEffectOnContactModel>().Duplicate();

        createEffectOnContactModel.effectModel.assetId = new PrefabReference()
            { guidRef = "b1324f2f4c3809643b7ef1d8c112442a" };
        
        projectile.AddBehavior(createEffectOnContactModel);
        
        
        projectile.GetBehavior<CreateProjectileOnContactModel>().emission.Cast<ArcEmissionModel>().count = 16;
        
        projectile.GetBehavior<CreateProjectileOnContactModel>().projectile.pierce *= 2;
        projectile.GetBehavior<CreateProjectileOnContactModel>().projectile.GetDamageModel().damage *= 2;
        
    }
}