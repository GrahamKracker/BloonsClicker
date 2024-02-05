using System;
using System.Collections.Generic;
using Il2CppAssets.Scripts.Models.Bloons.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Weapons;

namespace BloonsClicker.Upgrades.Path3;

public class LargerClicks : CursorUpgrade
{
    public override int Cost => 125;
    public override string Description => "Clicks are larger and pierce through more Bloons.";
    public override int Tier => 1;
    public override Path Path => Path.Third;
    
    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.radius *= 1.5f;
        if (projectile.HasBehavior<CreateProjectileOnContactModel>())
            projectile.GetBehaviors<CreateProjectileOnContactModel>().ForEach(x => x.projectile.radius *= 1.5f);
        
    }
}