
using BTD_Mod_Helper.Api.Display;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Models.Towers.Weapons;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using UnityEngine;

namespace BloonsClicker.Upgrades.Path3;

public class BuffingClicks : CursorUpgrade
{
    public override int Cost => 120_000;
    public override string Description => $"Towers that are clicked on get temporarily buffed.";
    public override int Tier => 10;
    public override Path Path => Path.Third;

    private const int BuffTime = 6;

    private const string BuffIcon = "BuffingClicksBuffIcon";
    /// <inheritdoc />
    public override void OnCreate(Projectile projectile)
    {
        InGame.instance.GetTowerManager().GetTowersInRange(projectile.Position, projectile.radius).ForEach(tower =>
        {
            if(tower.IsDestroyed || tower.towerModel.baseId == GetTowerModel<ClickerTower>().baseId)
                return;
            var buffIndicator = Game.instance.model.buffIndicatorModels.First(x => x.name.Contains(GetInstance<
                BuffingClicksBuffIndicator>().Id));
            tower.AddMutator(new DamageSupport.MutatorTower(0, true, BuffIcon, buffIndicator), BuffTime * 60);
        });
    }

    [HarmonyPatch(typeof(DamageSupport.MutatorTower), nameof(DamageSupport.MutatorTower.Mutate))]
    [HarmonyPrefix]
    private static bool DamageSupport_MutatorTower_Mutate(DamageSupport.MutatorTower __instance, Model model,
        ref bool __result)
    {
        if (__instance.id != "BuffingClicksTODO")
            return true;

        foreach (var damageModel in model.GetDescendants<DamageModel>().ToList())
        {
            damageModel.damage *= 1.5f;
        }

        foreach (var weaponModel in model.GetDescendants<WeaponModel>().ToList())
        {
            weaponModel.Rate *= 1.5f;
        }

        foreach (var weaponModel in model.GetDescendants<AttackModel>().ToList())
        {
            weaponModel.range *= 1.5f;
        }

        if (model.TryCast<TowerModel>() != null)
        {
            model.Cast<TowerModel>().range *= 1.5f;
        }

        foreach (var projectileModel in model.GetDescendants<ProjectileModel>().ToList())
        {
            projectileModel.pierce *= 1.5f;
            projectileModel.maxPierce *= 1.5f;
        }

        __result = true;
        return false;
    }
}

public class BuffingClicksBuffIndicator : ModBuffIcon
{
    public override string Icon => GetType().Name;
    
}
