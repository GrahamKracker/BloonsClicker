using System.Collections.Generic;
using BTD_Mod_Helper.Api;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Abilities;
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

//todo: fix errors in log
public class Puppeteer : CursorUpgrade
{
    public override int Cost => 5500;

    public override string Description => "Adds the ability to pick up a tower and attach it to the cursor.";
    public override int Tier => 8;
    public override Path Path => Path.Third;

    private const string PickupSprite = "PuppetterPickup";
    private const string PutDownSprite = "PuppetterPutDown";

    private static Tower? AttachedTower { get; set; }

    private static bool _selectingDoorGunner;
    
    private const float UpdateRate = 0.02f;
    
    private static float LastUpdate { get; set; }
    
    /// <inheritdoc />
    public override void OnUpdate()
    {
        if (Time.time - LastUpdate < UpdateRate)
            return;
        LastUpdate = Time.time;
        if (AttachedTower is { IsDestroyed: false })
        {
            var position = new Vector2(InGame.instance.inputManager.cursorPositionWorld);
            AttachedTower.PositionTower(position);
        }
    }

    [HarmonyPatch(typeof(Ability), nameof(Ability.CanUseAbility))]
    [HarmonyPrefix]
    private static bool Ability_CanUseAbility(Ability __instance, ref bool __result)
    {
        if (__instance.abilityModel.displayName == "Puppeteer")
        {
            if(AttachedTower is { IsDestroyed: false })
            {
                __result = true;
                return false;
            }
            if (InGame.instance.bridge.GetAllTowers().Any(x => !CursorTower!.Equals(x.tower) && !x.tower.isSelectionBlocked && !x.Def.isSubTower && !x.Def.isPowerTower && !x.Def.ignoreTowerForSelection))
            {
                __result = true;
                return false;
            }
            __result = false;
            return false;
        }
        return true;
    }

    /// <inheritdoc />
    public override void OnSell()
    {
        if (AttachedTower is { IsDestroyed: false })
        {
            AttachedTower.isSelectionBlocked = false;
            AttachedTower = null;
        }
    }

    [HarmonyPatch(typeof(Ability), nameof(Ability.Activate))]
    [HarmonyPrefix]
    private static void Ability_Activate(Ability __instance)
    {
        if (__instance.abilityModel.displayName == "Puppeteer")
        {
            if (AttachedTower is { IsDestroyed: false })
            {
                AttachedTower.isSelectionBlocked = false;
                AttachedTower = null;
                var towerModel = CursorTower.towerModel.Duplicate();
                towerModel.GetBehavior<AbilityModel>().icon = GetSpriteReferenceOrDefault<Main>(PickupSprite);
                CursorTower.UpdateRootModel(towerModel);

                AbilityMenu.instance.TowerChanged(CursorTower.GetTowerToSim());
            }
            else
            {
                var inputManager = InGame.instance.inputManager;
                inputManager.inCustomMode = true;
                inputManager.CancelAllPlacementActions();
                inputManager.HidePlacementBlockingUI();
                inputManager.HideCoopPlacementArea();
                CancelPurchaseButton cancelPlacementBtn = RightMenu.instance.cancelPlacementBtn;
                cancelPlacementBtn.animator.SetInteger(cancelPlacementBtn.visibleStateLabel, 1);
                inputManager.OnHelperMessageChanged.Invoke("Select a Tower", -1);
                cancelPlacementBtn.gameObject.GetComponent<Button>().onClick.AddListener(() => { TryCancel(); });

                int num = 0;
                foreach (var tower in InGame.instance.bridge.GetAllTowers().ToList().Where(x =>
                             !CursorTower!.Equals(x.tower) && !x.tower.isSelectionBlocked && !x.Def.isSubTower &&
                             !x.Def.isPowerTower && !x.Def.ignoreTowerForSelection))
                {
                    CreateSelectionImage(tower);
                    num++;
                }

                _selectingDoorGunner = true;

                if (num == 0)
                {
                    TryCancel();
                }
            }
        }
    }

    private static readonly List<(Tower, GameObject)> SelectedTowerMarkers = [];

    private static void CreateSelectionImage(TowerToSimulation towerToSimulation)
    {
        var rot = UnityEngine.Quaternion.Euler(45, 0, 0);
        var holder = new GameObject("SelectedTowerMarkerHolder")
        {
            transform =
            {
                parent = Game.instance.GetDisplayFactory().DisplayRoot,
            }
        };

        var selectedTowerMarkerGo = new GameObject("SelectedTowerMarker")
        {
            transform =
            {
                parent = holder.transform,
                position = new UnityEngine.Vector3(towerToSimulation.tower.Position.X, 100,
                    -towerToSimulation.tower.Position.Y - 55f),
                rotation = rot,
            }
        };

        var offsetTowardsCamera = holder.AddComponent<OffsetTowardsCamera>();
        offsetTowardsCamera.offset = 0.2f;
        offsetTowardsCamera.offsetRotation = new UnityEngine.Vector3(0, 0.2f, 0);

        var sr = selectedTowerMarkerGo.AddComponent<SpriteRenderer>();
        sr.color = new UnityEngine.Color(0, 0.9843f, 0.2627f, 1);
        sr.sprite = ModContent.GetSprite<Main>("SelectedTowerMarker");
        sr.sortingLayerName = "Bloons";
        sr.sortingOrder = 32767;
        SelectedTowerMarkers.Add((towerToSimulation.tower, selectedTowerMarkerGo));
    }


    [HarmonyPatch(typeof(TowerSelectionMenu), nameof(TowerSelectionMenu.SelectTower))]
    [HarmonyPostfix]
    static void Tower_Selected(TowerSelectionMenu __instance)
    {
        var tower = __instance.selectedTower.tower;

        if (_selectingDoorGunner &&
            SelectedTowerMarkers.Exists(x => x.Item1.Equals(tower) && !CursorTower!.Equals(tower)))
        {
            AttachedTower = tower;
            AttachedTower.isSelectionBlocked = true;


            var towerModel = CursorTower.towerModel.Duplicate();
            towerModel.GetBehavior<AbilityModel>().icon = GetSpriteReferenceOrDefault<Main>(PutDownSprite);
            CursorTower.UpdateRootModel(towerModel);

            AbilityMenu.instance.TowerChanged(CursorTower.GetTowerToSim());

            TryCancel();
        }
    }

    [HarmonyPatch(typeof(InputManager), nameof(InputManager.EnterPlacementMode))]
    [HarmonyPostfix]
    static void InputManager_EnterPlacementMode()
    {
        TryCancel();
    }

    private static bool TryCancel()
    {
        if (UpgradeMenu.PurchasedUpgrades[Path.Third] >= 8 && _selectingDoorGunner)
        {
            InGame.instance.inputManager.ExitCustomMode();
            InGame.instance.inputManager.CancelAllPlacementActions();
            _selectingDoorGunner = false;

            foreach (var gameObject in SelectedTowerMarkers.Select(selectedTowerMarker => selectedTowerMarker.Item2))
            {
                if (gameObject != null)
                    gameObject.Destroy();
            }

            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.ProcessEscape))]
    [HarmonyPrefix]
    static bool MenuManager_ProcessEscape()
    {
        return TryCancel();
    }

    [HarmonyPatch(typeof(InputManager), nameof(InputManager.IsCursorInWorld))]
    [HarmonyPostfix]
    static void InputManager_IsCursorInWorld(bool __result)
    {
        if (!__result)
            TryCancel();
    }

    /// <inheritdoc />
    public override void ModifyCursorTower(TowerModel towerModel)
    {
        var abilityModel = Game.instance.model.GetTower(TowerType.BoomerangMonkey, 0,4).GetDescendant<AbilityModel>().Duplicate();

        abilityModel.RemoveBehaviors();
        abilityModel.icon = GetSpriteReferenceOrDefault(PickupSprite);
        abilityModel.cooldown = 3;
        abilityModel.Cooldown = 3;
        abilityModel.cooldownFrames = 180;
        abilityModel.addedViaUpgrade = null;
        abilityModel.name = "AbilityModel_Puppeteer_";
        abilityModel.displayName = "Puppeteer";
        abilityModel.description = "Allows you to pick up a tower and attach it to the cursor";
        abilityModel.restrictAbilityAfterMaxRoundTimer = false;
        abilityModel.canActivateBetweenRounds = true;
        
        towerModel.AddBehavior(abilityModel);
        
        AbilityMenu.instance.TowerChanged(CursorTower.GetTowerToSim());
    }
    
}