using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;

namespace BloonsClicker.Upgrades.Path3;

public class CorrosiveClicks : CursorUpgrade
{
    public override int Cost => 90000;
    public override string Description => "Bloons hit by the cursor corrode and take more damage indefinitely.";
    public override int Tier => 9;
    private static string _mutatorName { get; set; } = string.Empty;
    private const float DamageMultiplier = 4;
    /// <inheritdoc />
    public override Path Path => Path.Third;

    private static string BehaviorName { get; set; } = string.Empty;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        _mutatorName = Name+":Normal";
        ProjectileBehaviorModel projectileBehaviorModel = new CashModel("CashModel_CorrosiveClicks_", 0,
            0, 0, 0, false, false, false, false);
        BehaviorName = projectileBehaviorModel.name;
        projectile.AddBehavior(projectileBehaviorModel);
    }
    
    [HarmonyPatch(typeof(Bloon), nameof(Bloon.Damage))]
    [HarmonyPrefix]
    private static void Bloon_Damage(Bloon __instance, Projectile projectile, ref float totalAmount)
    {
        if(__instance.GetMutatorById(_mutatorName) != null)
            totalAmount *= DamageMultiplier;
        
        if(projectile == null)
            return;
        
        if (projectile.projectileModel.HasBehaviorWithName(BehaviorName))
        {
            __instance.RemoveMutatorsById(_mutatorName);
            __instance.AddMutator(new BehaviorMutator(_mutatorName));
        }
    }
}