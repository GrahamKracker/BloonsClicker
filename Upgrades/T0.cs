using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Utils;

namespace BloonsClicker.Upgrades;

public class Clicker : CursorUpgrade
{
    public override int Cost => 500;

    /// <inheritdoc />
    protected override float ModifyRate(float rate) => 1f;

    public override string Description => "Pops bloons on click. Holding down the mouse button will continuously attack, but at a 15% slower rate.";
    public override int Tier => 0;

    /// <inheritdoc />
    public override Path Path => Path.Clicker;

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