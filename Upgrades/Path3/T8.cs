using System.Collections.Generic;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Display;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Models.Towers.Weapons;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity.Menu;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.AbilitiesMenu;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.RightMenu;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.StoreMenu;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = Il2CppAssets.Scripts.Simulation.SMath.Vector2;

namespace BloonsClicker.Upgrades.Path3;

public class BuffingClicks : CursorUpgrade
{
    public override int Cost => 15_000;
    public override string Description => $"Towers that are clicked on gain a strong, temporary buff. This buff can be applied to an unlimited amount of towers at once.";
    public override int Tier => 8;
    public override Path Path => Path.Third;

    private const int BuffTime = 5;
    
    private const float BuffMultiplier = 2f;

    /// <inheritdoc />
    public override void OnCreate(Projectile projectile)
    {
        InGame.instance.GetTowerManager().GetTowersInRange(projectile.Position, projectile.Radius).ForEach(tower =>
        {
            if(tower == null || tower.IsDestroyed || tower.towerModel.baseId == GetTowerModel<ClickerTower>().baseId)
                return;
            var buffIndicator = Game.instance.model.buffIndicatorModels.First(x => x.name.Contains(GetInstance<
                BuffingClicksBuffIcon>().Id));
            tower.AddMutator(new DamageSupport.MutatorTower(0, true, "BuffingClicks", buffIndicator), BuffTime * 60);
        });
    }

    [HarmonyPatch(typeof(DamageSupport.MutatorTower), nameof(DamageSupport.MutatorTower.Mutate))]
    [HarmonyPrefix]
    private static bool DamageSupport_MutatorTower_Mutate(DamageSupport.MutatorTower __instance, Model model,
        ref bool __result)
    {
        if (__instance.id != "BuffingClicks")
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

public class BuffingClicksBuffIcon : ModBuffIcon
{
    public override string Icon => GetType().Name;
    
}
