using System;
using System.Collections.Generic;
using System.Globalization;
using BloonsClicker;
using BloonsClicker.Upgrades.Path3;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Helpers;
using Il2CppAssets.Scripts;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Input;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Simulation.Track;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.BloonMenu;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.RightMenu;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using Il2CppAssets.Scripts.Utils;
using Il2CppInterop.Runtime;
using Newtonsoft.Json;
using UnityEngine;
using Action = System.Action;
using Main = BloonsClicker.Main;
using Vector2 = Il2CppAssets.Scripts.Simulation.SMath.Vector2;

// ReSharper disable InconsistentNaming

[assembly: MelonInfo(typeof(Main), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace BloonsClicker;

[HarmonyPatch]
public class Main : BloonsTD6Mod
{
    public static IEnumerable<Path> Paths { get; } = Enum.GetValues(typeof(Path)).Cast<Path>();
    
    public static SortedSet<CursorUpgrade> CurrentUpgrades { get; } = new(Comparer<CursorUpgrade>.Create((a, b) => a.Tier == b.Tier ? a.Path.CompareTo(b.Path) : a.Tier.CompareTo(b.Tier)));
    
    public static readonly HashSet<int> ProjectileHitBloon = [];

    public static float TimeSinceLastAttack { get; set; } = float.MaxValue;

    public static float TimeMouseHeld { get; private set; }

    private static void ResetCursor()
    {
        UpgradeMenu.PurchasedUpgrades = Paths.ToDictionary(path => path, _ => UpgradeMenu.UnPurchased);
        CurrentUpgrades.Clear();
        CursorPops = 0;
        TimeSinceLastAttack = float.MaxValue;
        TimeMouseHeld = 0;
    }

    public override void OnMainMenu()
    {
        ResetCursor();
    }

    public override void OnRestart()
    {
        ResetCursor();
    }
    /// <inheritdoc />
    public override void OnMatchStart()
    {
        ModGameMenu.Open<UpgradeMenu>(); //todo: remove this
    }
    
    internal static readonly Dictionary<Projectile, LifeSpan> ProjectileAge = new();
    
    internal static readonly HashSet<string> ProjectileNameCache = [];

    public class LifeSpan(float destroyAfter) : IComparable<LifeSpan>
    {
        public float Time { get; set; }

        public readonly float DestroyAfter = destroyAfter;

        public static LifeSpan NormalClick => new(.25f);
        public static LifeSpan StickyClicks => new(5);
        public static LifeSpan PermaClicks => new(30);
        
        public static bool operator >(LifeSpan a, LifeSpan b)
        {
            return a.DestroyAfter > b.DestroyAfter;
        }
        public static bool operator <(LifeSpan a, LifeSpan b)
        {
            return a.DestroyAfter < b.DestroyAfter;
        }

        /// <inheritdoc />
        public int CompareTo(LifeSpan? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            int destroyAfterComparison = DestroyAfter.CompareTo(other.DestroyAfter);
            if (destroyAfterComparison != 0) return destroyAfterComparison;
            return Time.CompareTo(other.Time);
        }
        
        public static object operator >=(LifeSpan a, LifeSpan b)
        {
            return a.DestroyAfter >= b.DestroyAfter;
        }
        
        public static object operator <=(LifeSpan a, LifeSpan b)
        {
            return a.DestroyAfter <= b.DestroyAfter;
        }
        
        public static object operator ==(LifeSpan a, LifeSpan b)
        {
            return a.DestroyAfter == b.DestroyAfter;
        }
        
        public static object operator !=(LifeSpan a, LifeSpan b)
        {
            return a.DestroyAfter != b.DestroyAfter;
        }
    }

    [HarmonyPatch(typeof(TowerInventory), nameof(TowerInventory.GetTowerInventoryCount))]
    [HarmonyPrefix]
    static void TowerInventory_GetTowerInventoryCount(TowerInventory __instance, TowerModel def)
    {
        if (def.baseId == ModContent.GetInstance<ClickerTower>().Id && !__instance.towerCounts.TryGetValue(def.baseId, out _))
        {
            __instance.towerCounts[def.baseId] = 0;
        }
    }

    /// <inheritdoc />
    public override void OnTowerLoaded(Tower tower, TowerSaveDataModel saveData)
    {
        if(tower.towerModel.baseId == ModContent.GetInstance<ClickerTower>().Id)
        {
            tower.towerModel.GetAttackModel().weapons[0].projectile = CursorUpgrade.GetProjectileModel();
            tower.towerModel.GetAttackModel().weapons[0].rate = CursorUpgrade.GetRawRate();
            CursorUpgrade.CursorTower = tower;
        }
    }

    public static float CursorPops { get; private set; }

    public override void OnUpdate()
    {
        // ReSharper disable once Unity.NoNullPropagation
        if (InGame.instance?.GetSimulation() is null)
            return;
        
        var delta = Time.deltaTime;

        if (InGame.instance.inputManager.cursorDown)
            TimeMouseHeld += delta;
        else
            TimeMouseHeld = 0;
        TimeSinceLastAttack += delta;

        foreach (var (projectile, lifeSpan) in ProjectileAge)
        {
            lifeSpan.Time += delta;
            if (lifeSpan.Time > lifeSpan.DestroyAfter)
            {
                projectile.Expire();
                ProjectileAge.Remove(projectile);
            }
        }

        HandleCursorUpgradeRoller();
        
        if(CurrentUpgrades.Count == 0)
            return;
        
        if (CursorUpgrade.CursorTower is { IsDestroyed: false })
        {
            var position = new Vector2(InGame.instance.inputManager.cursorPositionWorld);
            CursorUpgrade.CursorTower.PositionTower(position);
        }
        
        foreach (var upgrade in CurrentUpgrades.OrderBy(x=>x.Tier))
        {
            upgrade.OnUpdate();
        }

        if (!InGame.instance.inputManager.cursorInWorld ||
            !InGame.instance.inputManager.cursorDown)
            return;
        
        CursorUpgrade.TryCreateProjectile();
    }

    private static void HandleCursorUpgradeRoller()
    {
        if (_cursorUpgradeImage == null)
        {
            if (ShopMenu.instance == null)
                return;
            if (ShopMenu.instance.powersButton.transform.parent.FindChild("CursorUpgradePanel") != null)
                return;
            var panel =
                ShopMenu.instance.powersButton.transform.parent.gameObject.AddModHelperPanel(new Info("CursorUpgradePanel"));
            panel.transform.localPosition = Vector3.zero;
            var cursorUpgradeButton = panel.AddButton(new Info("CursorUpgradeButton", 275),
                ModContent.GetSpriteReference<Main>("CursorUpgrade").guidRef,
                new Action(() => ModGameMenu.Open<UpgradeMenu>()));

            var position = ShopMenu.instance.powersButton.transform.localPosition;
            cursorUpgradeButton.transform.localPosition = position with { x = position.x - 300 };

            _cursorUpgradeImage = cursorUpgradeButton.AddImage(new Info("CursorUpgradeImage", 350),
                VanillaSprites.SmallSquareGlowOutline);

            _cursorUpgradeImage.gameObject.AddComponent<Roller>();

            _cursorUpgradeImage.gameObject.SetActive(false);
        }
        
        var nextUpgrades = new HashSet<CursorUpgrade>();
        foreach (var path in Paths)
        {
            var tier = UpgradeMenu.PurchasedUpgrades[path];
            if (tier == UpgradeMenu.UnPurchased)
            {
                tier = path == Path.Clicker ? 0 : 1;
            }
            else
            {
                tier++;
            }

            if (CursorUpgrade.Cache[path].TryGetValue(tier, out var upgrade))
            {
                nextUpgrades.Add(upgrade);
            }
        }

        _cursorUpgradeImage.gameObject.SetActive(nextUpgrades.Any(upgrade => InGame.instance.GetCash() >= CostHelper.CostForDifficulty(upgrade.Cost, InGame.instance.GetGameModel())));
    }

    [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnDestroy))]
    [HarmonyPrefix]
    static void Projectile_Destroy(Projectile __instance)
    {
        if (ProjectileNameCache.Contains(__instance.model.name))
        {
            foreach (var upgrade in CurrentUpgrades.OrderBy(x => x.Tier))
            {
                upgrade.OnDestroy(__instance);
            }
        }
        ProjectileHitBloon.Remove(__instance.Id.Id);
    }

    [HarmonyPatch(typeof(Projectile), nameof(Projectile.CollideBloon))]
    [HarmonyPrefix]
    static void Projectile_CollideBloon(Projectile __instance)
    {
        ProjectileHitBloon.Add(__instance.Id.Id);
    }
    
    [HarmonyPatch(typeof(Bloon), nameof(Bloon.RecieveDamage))]
    [HarmonyPostfix]
    static void Bloon_ApplyDamageToBloon(Bloon __instance, int amount, Projectile projectile)
    {
        if (projectile?.model?.name != null && ProjectileNameCache.Contains(projectile.model.name))
        {
            CursorPops += __instance.damageResult.damageUsed;
        }
    }
    
    [HarmonyPatch(typeof(BloonMenu), nameof(BloonMenu.OnClickedResetDamage))]
    [HarmonyPostfix]
    static void BloonMenu_OnClickedResetDamage() => CursorPops = 0;

    private static ModHelperImage? _cursorUpgradeImage;
    
    [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.Initialise))]
    [HarmonyPostfix]
    static void ShopMenu_Initialise(ShopMenu __instance)
    {
        if (__instance.powersButton.transform.parent.FindChild("CursorUpgradePanel") != null)
            return;
        var panel =
            __instance.powersButton.transform.parent.gameObject.AddModHelperPanel(new Info("CursorUpgradePanel"));
        panel.transform.localPosition = Vector3.zero;
        var cursorUpgradeButton = panel.AddButton(new Info("CursorUpgradeButton", 275),
            ModContent.GetSpriteReference<Main>("CursorUpgrade").guidRef,
            new Action(() => ModGameMenu.Open<UpgradeMenu>()));

        var position = __instance.powersButton.transform.localPosition;
        cursorUpgradeButton.transform.localPosition = position with { x = position.x - 300 };

        _cursorUpgradeImage = cursorUpgradeButton.AddImage(new Info("CursorUpgradeImage", 350),
            VanillaSprites.SmallSquareGlowOutline);

        _cursorUpgradeImage.gameObject.AddComponent<Roller>();

        _cursorUpgradeImage.gameObject.SetActive(false);
    }

    #region Saving

    [HarmonyPatch(typeof(Map), nameof(Map.GetSaveData))]
    [HarmonyPostfix]
    static void OnMapSaved(MapSaveDataModel mapData)
    {
        var json = JsonConvert.SerializeObject(UpgradeMenu.PurchasedUpgrades);
        mapData.metaData["CursorUpgrade"] = json;

        mapData.metaData["CursorPops"] = CursorPops.ToString(CultureInfo.InvariantCulture);
        
        foreach (var upgrade in CurrentUpgrades.OrderBy(x => x.Tier))
        {
            upgrade.OnMapSaved(mapData);
        }
    }

    [HarmonyPatch(typeof(Map), nameof(Map.SetSaveData))]
    [HarmonyPostfix]
    static void OnMapLoaded(MapSaveDataModel mapData)
    {            
        CurrentUpgrades.Clear();
        if (mapData.metaData.TryGetValue("CursorUpgrade", out var data))
        {
            UpgradeMenu.PurchasedUpgrades = JsonConvert.DeserializeObject<Dictionary<Path, int>>(data) ?? new Dictionary<Path, int>();
            
            foreach (var (path, tier) in UpgradeMenu.PurchasedUpgrades)
            {
                for (var i = 0; i <= tier; i++)
                {
                    if (!CursorUpgrade.Cache[path].TryGetValue(i, out var upgrade))
                        continue;
                    CurrentUpgrades.Add(upgrade);
                }
            }
        }
        else
        {
            UpgradeMenu.PurchasedUpgrades = Paths.ToDictionary(path => path, _ => UpgradeMenu.UnPurchased);
        }

        if (mapData.metaData.TryGetValue("CursorPops", out var cursorPops))
        {
            CursorPops = float.Parse(cursorPops, CultureInfo.InvariantCulture);
        }
        else
        {
            CursorPops = 0;
        }
        
        foreach (var upgrade in CurrentUpgrades.OrderBy(x => x.Tier))
        {
            upgrade.OnMapLoaded(mapData);
        }

        CursorUpgrade.UpdateTower();
    }

    #endregion
}