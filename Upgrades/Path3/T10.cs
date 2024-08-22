
using BTD_Mod_Helper.Api.Display;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Models.Towers.Weapons;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using UnityEngine;

namespace BloonsClicker.Upgrades.Path3;

public class BuffingAura : CursorUpgrade
{
    public override int Cost => 120_000;
    public override string Description => $"Towers nearby the cursor get heavily buffed.";
    public override int Tier => 10;
    public override Path Path => Path.Third;

    private const int BuffTime = 3;
    
    private const float BuffMultiplier = 5f;

    private const float PhysicsRate = 0.5f;

    private float _lastCheckTime = Time.time;
    /// <inheritdoc />
    public override void OnUpdate()
    {
        if (Time.time - _lastCheckTime < PhysicsRate)
        {
            return;
        }

        _lastCheckTime = Time.time;
        
        var position = InGame.instance.GetUnityWorldFromCursor();
        
        if(CursorTower == null)
            return;
        
        InGame.instance.GetTowerManager().GetTowersInRange(position.ToSMathVector(), CursorTower.towerModel.GetDescendant<ProjectileModel>().radius).ForEach(tower =>
        {
            if (tower == null || tower.IsDestroyed || tower.towerModel.baseId == GetTowerModel<ClickerTower>().baseId)
                return;
            var buffIndicator = Game.instance.model.buffIndicatorModels.First(x => x.name.Contains(GetInstance<
                BuffingAuraBuffIcon>().Id));
            tower.AddMutator(new DamageSupport.MutatorTower(0, true, "BuffingAura", buffIndicator), BuffTime * 60);
        });
    }

    [HarmonyPatch(typeof(DamageSupport.MutatorTower), nameof(DamageSupport.MutatorTower.Mutate))]
    [HarmonyPrefix]
    private static bool DamageSupport_MutatorTower_Mutate(DamageSupport.MutatorTower __instance, Model model,
        ref bool __result)
    {
        if (__instance.id != "BuffingAura")
            return true;

        foreach (var damageModel in model.GetDescendants<DamageModel>().ToList())
        {
            damageModel.damage *= BuffMultiplier;
        }

        foreach (var weaponModel in model.GetDescendants<WeaponModel>().ToList())
        {
            weaponModel.Rate *= BuffMultiplier;
        }

        foreach (var weaponModel in model.GetDescendants<AttackModel>().ToList())
        {
            weaponModel.range *= BuffMultiplier;
        }

        if (model.TryCast<TowerModel>() != null)
        {
            model.Cast<TowerModel>().range *= BuffMultiplier;
        }

        foreach (var projectileModel in model.GetDescendants<ProjectileModel>().ToList())
        {
            projectileModel.pierce *= BuffMultiplier;
            projectileModel.maxPierce *= BuffMultiplier;
        }

        __result = true;
        return false;
    }
}

public class BuffingAuraBuffIcon : ModBuffIcon
{
    public override string Icon => GetType().Name;

}