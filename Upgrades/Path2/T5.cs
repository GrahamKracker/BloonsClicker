using Il2CppAssets.Scripts.Models.Bloons.Behaviors;

namespace BloonsClicker.Upgrades.Path2;

public class FireAndIce : CursorUpgrade
{
    public override int Cost => 2700;
    public override string Description => "Firey clicks burn bloons as well as freeze them";
    public override int Tier => 5;
    public override Path Path => Path.Second;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        var burn = Game.instance.model.GetTower(TowerType.WizardMonkey, 0, 3).GetDescendant<AddBehaviorToBloonModel>()
            .Duplicate();
        const float numTicks = 4;
        burn.lifespan = numTicks;
        burn.lifespanFrames = (int)(numTicks * 60);
        burn.name = "FireandIce:Burn";
        var damageOverTimeModel = burn.GetBehavior<DamageOverTimeModel>();
        damageOverTimeModel.interval = (burn.lifespan / numTicks) * .9f;
        damageOverTimeModel.intervalFrames = (int)(burn.lifespanFrames / numTicks * .9f);

        damageOverTimeModel.damage = 1;
        damageOverTimeModel.displayLifetime = burn.lifespan * 1.1f;
        damageOverTimeModel.immuneBloonProperties = projectile.GetDamageModel().immuneBloonProperties;

        projectile.AddBehavior(burn);
    }
    
    protected override void PostModifyProjectile(ProjectileModel projectile)
    {
        var burn = projectile.behaviors.First(behavior => behavior.name == "FireandIce:Burn").Cast<AddBehaviorToBloonModel>();
        projectile.UpdateCollisionPass(burn.collisionPass);
    }
}