
using Il2CppAssets.Scripts.Models.Bloons.Behaviors;

namespace BloonsClicker.Upgrades.Path2;

public class IcicleClicks : CursorUpgrade
{
    public override int Cost => 7500;

    public override string Description => "Bloons hit by the cursor will grow damaging icicles that do massive damage to MOAB and ceramic Bloons.";
    public override int Tier => 8;
    public override Path Path => Path.Second;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        var addBehaviorToBloonModel = Game.instance.model.GetTower(TowerType.IceMonkey, 0,0,4).GetDescendant<AddBehaviorToBloonModel>().Duplicate();
        
        var carryProjectileModel = addBehaviorToBloonModel.behaviors[0].Cast<CarryProjectileModel>();
        
        carryProjectileModel.projectile.AddBehavior(new DamageModifierForTagModel("DamageModifierForTagModel_", "Moabs,Ceramic", 1, 15, false, false));
        carryProjectileModel.projectile.hasDamageModifiers = true;
        carryProjectileModel.projectile.RemoveFilter<FilterBloonIfDamageTypeModel>();
        carryProjectileModel.projectile.pierce = 10;
        carryProjectileModel.projectile.maxPierce = 10;
        
        projectile.AddBehavior(addBehaviorToBloonModel);
    }
}