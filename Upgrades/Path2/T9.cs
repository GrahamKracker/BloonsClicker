
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;

namespace BloonsClicker.Upgrades.Path2;

public class Frostbite : CursorUpgrade
{
    public override int Cost => 125_000;
    public override string Description => "Frozen Bloons hit by the cursor lose part of their total health.";
    public override int Tier => 9;

    /// <inheritdoc />
    public override Path Path => Path.Second;

    private static string BehaviorName { get; set; } = string.Empty;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        var behavior = new CashModel("CashModel_Frostbite_", 0,
            0, 0, 0, false, false, false, false);
        BehaviorName = behavior.name;
        projectile.AddBehavior(behavior);
    }
    
    [HarmonyPatch(typeof(Bloon), nameof(Bloon.Damage))]
    [HarmonyPrefix]
    private static void Bloon_Damage(Bloon __instance, Projectile projectile, ref float totalAmount)
    {
        if (projectile == null)
            return;
        if (projectile.projectileModel.HasBehaviorWithName(BehaviorName) &&
            __instance.bloonModel.bloonProperties.HasFlag(BloonProperties.Frozen))
        {
            totalAmount += __instance.bloonModel.GetMaxHealth() / 4;
        }
        
    }
}