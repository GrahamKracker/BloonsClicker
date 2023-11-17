using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;

namespace BloonsClicker.Upgrades;

public class DartSprayer : CursorUpgrade
{
    public override int Cost => 7500;
    protected override string Icon => VanillaSprites.AirburstDartsUpgradeIcon;
    public override float Rate => .4f;

    public override string Description => "Instead of exploding on impact, attack now shoot out 4 small darts in a cross-like pattern, each with 5 pierce, and 4 damage. The darts can damage all bloon types.";

    public override int Tier => 6;
    protected override Main.LifeSpan LifeSpan => Main.LifeSpan.NormalClick;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        var smalldart = Game.instance.model.GetTower(TowerType.DartMonkey).GetWeapon().projectile.Duplicate();
        smalldart.pierce = 5;
        smalldart.GetDamageModel().damage = 4;
        smalldart.name = "SmallDart";
        
        foreach (var behavior in smalldart.GetDescendants<FilterInvisibleModel>().ToList())
        {
            behavior.isActive = false;
        }
        
        projectile.RemoveBehavior<CreateProjectileOnContactModel>();
        projectile.RemoveBehavior<CreateSoundOnProjectileCollisionModel>();
        
        projectile.AddBehavior(new CreateProjectileOnContactModel("CreateProjectileOnContactModel_", 
            smalldart, new ArcEmissionModel("ArcEmissionModel_", 4, 0, 360, null, false, false),
            true, false, false));
        
        foreach(var behavior in projectile.GetDescendants<DamageModel>().ToList())
        {
            behavior.immuneBloonProperties = BloonProperties.None;
        }
        
        foreach (var behavior in projectile.GetDescendants<FilterInvisibleModel>().ToList())
        {
            behavior.isActive = false;
        }
    }
}