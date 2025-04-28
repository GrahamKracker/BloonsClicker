using System.Reflection;
using System.Runtime.InteropServices;
using BloonsClicker.Upgrades.Path2;
using BTD_Mod_Helper.Api.Hooks;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppInterop.Runtime;

namespace BloonsClicker.Upgrades.Path3;

public class CorrosiveClicks : CursorUpgrade
{
    public override int Cost => 90000;
    public override string Description => "Bloons hit by the cursor are corroded and take more damage indefinitely.";
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
            0, 0, 0, false, false, false, false, false);
        BehaviorName = projectileBehaviorModel.name;
        projectile.AddBehavior(projectileBehaviorModel);
    }

    public class CorrosiveClicksBloonDamageHook : ModHook<CorrosiveClicksBloonDamageHook.BloonDamageDelegate,
        CorrosiveClicksBloonDamageHook.BloonDamageManagedDelegate>
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

            if (bloonValue?.GetMutatorById(_mutatorName) != null)
                totalAmount *= DamageMultiplier;

            if (projectileValue == null || bloonValue == null)
                return;

            if (projectileValue.projectileModel.HasBehaviorWithName(BehaviorName))
            {
                bloonValue.RemoveMutatorsById(_mutatorName);
                bloonValue.AddMutator(new BehaviorMutator(_mutatorName));
            }

        }
    }
}