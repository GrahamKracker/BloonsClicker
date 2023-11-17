using System.Collections.Generic;
using BloonsClicker;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Helpers;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Simulation.Track;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.RightMenu;
using UnityEngine;
using Action = System.Action;

[assembly: MelonInfo(typeof(BloonsClicker.Main), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace BloonsClicker;

[HarmonyPatch]
public class Main : BloonsTD6Mod
{
    public static CursorUpgrade? CurrentUpgrade;

    private static float timeSinceLastSpawn;
    
    private static float timeMouseHeld;

    private static void ClearUpgrades()
    {
        UpgradeMenu.purchasedDict.Clear();
        CurrentUpgrade = null;
    }
    
    public override void OnMainMenu()
    {
        CursorUpgrade.GenerateProjectiles();
        ClearUpgrades();
    }

    public override void OnRestart()
    {
        ClearUpgrades();
    }

    internal static readonly Dictionary<Projectile, LifeSpan> ProjectileAge = new();

    public record LifeSpan(float DestroyAfter)
    {
        public float Time;
        public readonly float DestroyAfter = DestroyAfter;
        
        public static LifeSpan NormalClick => new(.25f);
        public static LifeSpan StickyClicks => new(5);
        public static LifeSpan PermaClicks => new(30);
    }

    public static float CursorPops;
    
    
    public override void OnUpdate()
    {
        timeSinceLastSpawn += Time.deltaTime;

        // ReSharper disable once Unity.NoNullPropagation
        if (InGame.instance?.GetSimulation() is null)
            return;

        foreach (var (projectile, lifeSpan) in ProjectileAge)
        {
            lifeSpan.Time += Time.deltaTime;
            if (lifeSpan.Time > lifeSpan.DestroyAfter)
            {
                projectile.Destroy();
                ProjectileAge.Remove(projectile);
            }
        }

        if (CurrentUpgrade is null)
        {
            if (InGame.instance.GetCash() >= CostHelper.CostForDifficulty(CursorUpgrade.Cache.First().Value.Cost, InGame.instance.GetGameModel()))
            {
                // ReSharper disable once Unity.NoNullPropagation
                _cursorUpgradeImage?.gameObject.SetActive(true);
            }
            return;
        }


        if (_cursorUpgradeImage is not null && CursorUpgrade.Cache.ContainsKey(CurrentUpgrade.Tier + 1) && InGame.instance.GetCash() >= CostHelper.CostForDifficulty(CursorUpgrade.Cache[CurrentUpgrade.Tier + 1].Cost, InGame.instance.GetGameModel()))
        {
            _cursorUpgradeImage.gameObject.SetActive(true);
        }
        else
        {
            // ReSharper disable once Unity.NoNullPropagation
            _cursorUpgradeImage?.gameObject.SetActive(false);
        }

        if(InGame.instance.inputManager.cursorDown)
            timeMouseHeld += Time.deltaTime;
        else
            timeMouseHeld = 0;
        
        if (!InGame.instance.inputManager.cursorInWorld ||
            !InGame.instance.inputManager.cursorDown)
            return;
        
        if (timeSinceLastSpawn < (timeMouseHeld > .35f ? CurrentUpgrade.Rate * 1.3f : CurrentUpgrade.Rate))
            return;

        timeSinceLastSpawn = 0;

        var model = CursorUpgrade.ProjectileModelCache[CurrentUpgrade.Tier];
        var proj = InGame.instance.GetMainFactory().CreateEntityWithBehavior<Projectile, ProjectileModel>(model);

        proj.Position.X = InGame.instance.inputManager.cursorPositionWorld.x;
        proj.Position.Y = InGame.instance.inputManager.cursorPositionWorld.y;
        proj.Position.Z = 20;

        proj.pierce = model.pierce;

        proj.direction.X = 0;
        proj.direction.Y = 0;
        proj.direction.Z = 0;

        var projEmittedFrom = proj.emittedFrom;
        projEmittedFrom.x = InGame.instance.inputManager.cursorPositionWorld.x;
        projEmittedFrom.y = InGame.instance.inputManager.cursorPositionWorld.y;
        projEmittedFrom.z = 20;
        proj.emittedFrom = projEmittedFrom;

        CurrentUpgrade.Create(proj);
    }

    [HarmonyPatch(typeof(Bloon), nameof(Bloon.Damage))]
    [HarmonyPrefix]
    static void Bloon_Damage(float totalAmount, Projectile projectile)
    {
        if (projectile != null && (CurrentUpgrade?.Name == projectile.model.name || projectile.model.name == "ExplosiveClick" || projectile.model.name == "SmallDart"))
            CursorPops += totalAmount;
    }

    private static ModHelperImage? _cursorUpgradeImage;
    
    [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.Initialise))]
    [HarmonyPostfix]
    static void ShopMenu_Initialise(ShopMenu __instance)
    {
        if (__instance.powersButton.transform.parent.FindChild("CursorUpgradePanel") != null)
            return;
        var panel = __instance.powersButton.transform.parent.gameObject.AddModHelperPanel(new Info("CursorUpgradePanel"));
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
    static void OnMapSaved(Map __instance, MapSaveDataModel mapData)
    {
        var json = "";
        json = UpgradeMenu.purchasedDict.Aggregate(json,
            (current, upgrade) => current + (upgrade.Key.Tier + ": " + upgrade.Value + "\n"));
        mapData.metaData["CursorUpgrade"] = json;
    }

    [HarmonyPatch(typeof(Map), nameof(Map.SetSaveData))]
    [HarmonyPostfix]
    static void OnMapLoaded(Map __instance, MapSaveDataModel mapData)
    {
        if (mapData.metaData.TryGetValue("CursorUpgrade", out var data))
        {
            UpgradeMenu.purchasedDict = data.Split("\n").Where(x => !string.IsNullOrEmpty(x))
                .ToDictionary(line => CursorUpgrade.Cache[int.Parse(line.Split(": ")[0])],
                    line => bool.Parse(line.Split(": ")[1]));
        }
    }

    #endregion
}