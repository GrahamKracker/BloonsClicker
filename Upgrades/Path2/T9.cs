
using System.Reflection;
using System.Runtime.InteropServices;
using BTD_Mod_Helper.Api.Hooks;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppInterop.Runtime;

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
            0, 0, 0, false, false, false, false, false);
        BehaviorName = behavior.name;
        projectile.AddBehavior(behavior);
    }
    
    public class FrostbiteBloonDamageHook : ModHook<FrostbiteBloonDamageHook.BloonDamageDelegate,
        FrostbiteBloonDamageHook.BloonDamageManagedDelegate>
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void BloonDamageDelegate(nint @this, ref float totalAmount, nint projectile,
            byte distributeToChildren, byte overrideDistributeBlocker, byte createEffect, nint tower,
            int immuneBloonProperties, int originalImmuneBloonProperties, byte canDestroyProjectile,
            byte ignoreNonTargetable, byte blockSpawnChildren, byte ignoreInvunerable,
            HookNullable<int> powerActivatedByPlayerId, nint methodInfo);


        public delegate bool BloonDamageManagedDelegate(ref Bloon @this, ref float totalAmount,
            ref Projectile projectile,
            ref bool distributeToChildren, ref bool overrideDistributeBlocker, ref bool createEffect, ref Tower tower,
            ref BloonProperties immuneBloonProperties, ref BloonProperties originalImmuneBloonProperties,
            ref bool canDestroyProjectile, ref bool ignoreNonTargetable, ref bool blockSpawnChildren,
            ref bool ignoreInvunerable,
            ref HookNullable<int> powerActivatedByPlayerId);

        protected override BloonDamageDelegate HookMethod =>
            BloonDamage;

        protected override MethodInfo TargetMethod => AccessTools.Method(typeof(Bloon), nameof(Bloon.Damage));

        private void BloonDamage(nint @this, ref float totalAmount, nint projectile, byte distributeToChildren,
            byte overrideDistributeBlocker, byte createEffect, nint tower, int immuneBloonProperties,
            int originalImmuneBloonProperties, byte canDestroyProjectile, byte ignoreNonTargetable,
            byte blockSpawnChildren, byte ignoreInvunerable, HookNullable<int> powerActivatedByPlayerId,
            nint methodInfo)
        {
            MethodInfo = methodInfo;

            var bloonValue = IL2CPP.PointerToValueGeneric<Bloon>(@this, false, false);
            var projectileValue = IL2CPP.PointerToValueGeneric<Projectile>(projectile, false, false);

            if (projectileValue == null)
                return;
            if (projectileValue.projectileModel.HasBehaviorWithName(BehaviorName) &&
                bloonValue.bloonModel.bloonProperties.HasFlag(BloonProperties.Frozen))
            {
                totalAmount += bloonValue.bloonModel.GetMaxHealth() / 4;
            }
        }
    }
}