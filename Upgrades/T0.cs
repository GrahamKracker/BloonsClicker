using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Utils;

namespace BloonsClicker.Upgrades;

public class Clicker : CursorUpgrade
{
    public override int Cost => 500;
    protected override string Icon => Name;
    public override float Rate => 1.05f;
    public override string Description => "Pops bloons on click, 1 damage, 2 pierce, attacks every "+Rate+" seconds. Holding down the mouse button will continually attack, but at a 30% slower rate.";
    public override int Tier => 0;
    protected override Main.LifeSpan LifeSpan => Main.LifeSpan.NormalClick;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.pierce = 2;
        projectile.GetDamageModel().damage = 1;
        
        projectile.RemoveBehavior<TravelStraitModel>();
        projectile.display = new PrefabReference
        {
            guidRef = null,
        };
        
        projectile.GetBehavior<DisplayModel>().display = new PrefabReference
        {
            guidRef = null,
        };
        projectile.radius = 5;
    }
}