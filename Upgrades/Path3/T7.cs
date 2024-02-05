using System;
using Il2CppAssets.Scripts;
using Il2CppAssets.Scripts.Simulation.SMath;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;

namespace BloonsClicker.Upgrades.Path3;

public class BuffableClicker : CursorUpgrade
{
    public override int Cost => 3750;

    public override string Description => "Allows the clicker to be buffed. The clicker is counted as a primary tower.";
    public override int Tier => 7;
    public override Path Path => Path.Third;

    /// <inheritdoc />
    public override void ModifyCursorTower(TowerModel towerModel)
    {
        towerModel.isSubTower = false;
    }

    /// <inheritdoc />
    public override void OnUpdate()
    {
        if(TowerSelectionMenu.instance.selectedTower == null)
        {        
            BuffIndicatorUi.instance.UpdatePlacedTowerBuffs(CursorTower.GetTowerToSim());
        }
    }
}