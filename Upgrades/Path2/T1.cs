using System;
using System.Collections.Generic;
using Il2CppAssets.Scripts.Models.Bloons.Behaviors;

namespace BloonsClicker.Upgrades.Path2;

public class FreezingClicks : CursorUpgrade
{
    public override int Cost => 375;
    public override string Description => "Clicks now freeze bloons";
    public override int Tier => 1;
    public override Path Path => Path.Second;

    /// <inheritdoc />
    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        var freezeModel =
            new FreezeModel("FreezeModel_BloonsClickerPath2_", 0, 1f, 
                "FreezeModel_BloonsClickerPath2_:Freeze", 2, "Ice", true, 
                new GrowBlockModel("GrowBlockModel_FreezeGrowBlock"), null, 0, false, false);
        projectile.AddBehavior(freezeModel);
    }

    protected override void PostModifyProjectile(ProjectileModel projectile)
    {
        projectile.UpdateCollisionPass(projectile.GetBehavior<FreezeModel>().collisionPass);
        foreach (var behavior in projectile.GetDescendants<DamageModel>().ToList())
        {
            behavior.immuneBloonProperties &= ~BloonProperties.Frozen;
            behavior.immuneBloonPropertiesOriginal &= ~BloonProperties.Frozen;

            behavior.immuneBloonProperties &= BloonProperties.White;
            behavior.immuneBloonPropertiesOriginal &= BloonProperties.White;
        }
    }
}