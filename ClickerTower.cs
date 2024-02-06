using System.Collections.Generic;
using BTD_Mod_Helper.Api.Towers;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Il2CppAssets.Scripts.Models.TowerSets;
using Il2CppAssets.Scripts.Utils;

namespace BloonsClicker;

public class ClickerTower : ModTower
{
    /// <inheritdoc />
    public override bool DontAddToShop => true;


    /// <inheritdoc />
    public override void ModifyBaseTowerModel(TowerModel towerModel)
    {
        IEnumerable<string> defaultMods =
        [
            "GlobalAbilityCooldowns",
            "MonkeyEducation",
            "BetterSellDeals",
            "VeteranMonkeyTraining"
        ];
        towerModel.mods = towerModel.mods.Where(mod => defaultMods.Contains(mod.name)).ToIl2CppReferenceArray();

        towerModel.ignoreTowerForSelection = true;
        towerModel.footprint.ignoresTowerOverlap = true;
        towerModel.footprint.doesntBlockTowerPlacement = true;
        towerModel.footprint.ignoresPlacementCheck = true;
        
        towerModel.GetBehavior<DisplayModel>().display = new PrefabReference { guidRef = "" };
        towerModel.display = new PrefabReference { guidRef = "" };
        
        towerModel.RemoveBehavior<CreateEffectOnPlaceModel>();
        towerModel.RemoveBehavior<CreateSoundOnTowerPlaceModel>();
        towerModel.RemoveBehavior<CreateEffectOnSellModel>();
        towerModel.RemoveBehavior<CreateSoundOnSellModel>();
        towerModel.RemoveBehavior<CreateSoundOnUpgradeModel>();
        towerModel.RemoveBehavior<PlayAnimationIndexModel>();
        towerModel.RemoveBehavior<CreateEffectOnUpgradeModel>();
        
        var attackModel = towerModel.GetAttackModel();
        attackModel.RemoveBehavior<RotateToTargetModel>();
        attackModel.RemoveBehavior<AttackFilterModel>();
        attackModel.RemoveBehavior<TargetFirstPrioCamoModel>();
        attackModel.RemoveBehavior<TargetLastPrioCamoModel>();
        attackModel.RemoveBehavior<TargetClosePrioCamoModel>();
        attackModel.RemoveBehavior<TargetStrongPrioCamoModel>();


        towerModel.isSubTower = true;
    }

    /// <inheritdoc />
    public override TowerSet TowerSet => TowerSet.Primary;

    /// <inheritdoc />
    public override string BaseTower => TowerType.DartMonkey;

    /// <inheritdoc />
    public override int Cost => 0;
}