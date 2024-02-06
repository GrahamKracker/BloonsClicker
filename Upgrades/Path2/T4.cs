using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;

namespace BloonsClicker.Upgrades.Path2;

public class IntenseIce : CursorUpgrade
{
    public override int Cost => 1200;
    public override string Description => "Attacks deal extra damage to frozen bloons";
    public override int Tier => 4;
    public override Path Path => Path.Second;
    private const float DamageMultiplier = 2f;

    private static string BehaviorName { get; set; } = string.Empty;
    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        ProjectileBehaviorModel projectileBehaviorModel = new CashModel("CashModel_IntenseIce_", 0,
            0, 0, 0, false, false, false, false);
        BehaviorName = projectileBehaviorModel.name;
        projectile.AddBehavior(projectileBehaviorModel);
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
            totalAmount *= DamageMultiplier;
        }
    }
}