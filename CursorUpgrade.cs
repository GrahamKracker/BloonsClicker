﻿using System.Collections.Generic;
using BTD_Mod_Helper.Api;
using Il2CppAssets.Scripts;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Models.Towers.Mods;
using Il2CppAssets.Scripts.Models.Towers.Weapons;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Simulation.Towers.Weapons;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Utils;
using static BloonsClicker.Main;
using Vector3 = Il2CppAssets.Scripts.Simulation.SMath.Vector3;

namespace BloonsClicker;

[HarmonyPatch]
public abstract class CursorUpgrade : NamedModContent
{
    internal static readonly Dictionary<Path, Dictionary<int, CursorUpgrade>> Cache =
        Paths.ToDictionary(path => path, _ => new Dictionary<int, CursorUpgrade>());

    public static Tower? CursorTower { get; set; }

    public override void Register()
    {
        Cache[Path].Add(Tier, this);
    }

    /// <inheritdoc />
    public sealed override IEnumerable<ModContent> Load()
    {
        return base.Load();
    }

    public static void UpdateTower()
    {
        var tower = GetTowerModel<ClickerTower>().Duplicate();
        tower.GetWeapon().projectile = GetProjectileModel();
        tower.GetWeapon().rate = GetRawRate();
        tower.GetWeapon().Rate = GetRawRate();
        tower.GetWeapon().rateFrames = (int)(GetRawRate() * 60);
        
        foreach (var upgrade in CurrentUpgrades.OrderBy(x => x.Tier))
        {
            upgrade.ModifyCursorTower(tower);
        }

        if (CursorTower is { IsDestroyed: false }) //todo: fix bug when leaving and coming back
        {
            CursorTower.UpdateRootModel(tower);
        }
        else
        {
            CursorTower = InGame.instance.GetTowerManager().CreateTower(tower,
                new Vector3(InGame.instance.inputManager.cursorPositionWorld),
                InGame.Bridge.MyPlayerNumber,
                ObjectId.FromData(1), ObjectId.FromData(4294967295));
        }
    }

    public static ProjectileModel GetProjectileModel()
    {
        var purchasedUpgrades = UpgradeMenu.PurchasedUpgrades;
        string key = Paths.Aggregate("", (current, path) => current + $"{path}:{purchasedUpgrades[path]}");

        if (ProjectileModelCache.TryGetValue(key, out var model))
        {
            return model;
        }

        var newProjectile = Game.instance.model.GetTower(TowerType.DartMonkey).GetWeapon().projectile.Duplicate();
        
        var maxTier = Cache.Values.Where(x=>x.Values.Count > 0 && x.Keys.Count > 0).Max(x => x.Values.Max(y => y.Tier));
        
        try
        {
            Cache[Path.Clicker][0].ModifyProjectile(newProjectile);
        }
        catch (System.Exception e)
        {
            MelonLogger.Msg($"Failed to modify original projectile");
            MelonLogger.Error(e);
        }
        
        for (var tier = 1; tier <= maxTier; tier++)
        {
            foreach (var path in Paths.Where(path => path != Path.Clicker && Cache[path].ContainsKey(tier)))
            {
                if (UpgradeMenu.PurchasedUpgrades[path] < tier)
                    continue;
                try
                {
                    Cache[path][tier].ModifyProjectile(newProjectile);
                }
                catch (System.Exception e)
                {
                    MelonLogger.Msg($"Failed to modify projectile for path:{path}, tier:{tier}");
                    MelonLogger.Error(e);
                }
            }
        }

        if (UpgradeMenu.PurchasedUpgrades[Path.Paragon] != UpgradeMenu.UnPurchased)
        {
            try
            {
                Cache[Path.Paragon].First().Value.ModifyProjectile(newProjectile);
            }
            catch (System.Exception e)
            {
                MelonLogger.Msg($"Failed to modify projectile for paragon");
                MelonLogger.Error(e);
            }
        }

        try
        {
            Cache[Path.Clicker][0].PostModifyProjectile(newProjectile);
        }
        catch (System.Exception e)
        {
            MelonLogger.Msg($"Failed to post modify original projectile");
            MelonLogger.Error(e);
        }
        
        for (var tier = 1; tier <= maxTier; tier++)
        {
            foreach (var path in Paths.Where(path => path != Path.Clicker && Cache[path].ContainsKey(tier)))
            {
                if (UpgradeMenu.PurchasedUpgrades[path] < tier)
                    continue;
                try
                {
                    Cache[path][tier].PostModifyProjectile(newProjectile);
                }
                catch (System.Exception e)
                {
                    MelonLogger.Msg($"Failed to post modify projectile for path:{path}, tier:{tier}");
                    MelonLogger.Error(e);
                }
            }
        }

        if (UpgradeMenu.PurchasedUpgrades[Path.Paragon] != UpgradeMenu.UnPurchased)
        {
            try
            {
                Cache[Path.Paragon].First().Value.PostModifyProjectile(newProjectile);
            }
            catch (System.Exception e)
            {
                MelonLogger.Msg($"Failed to post modify projectile for paragon");
                MelonLogger.Error(e);
            }
        }

        newProjectile.name = key;
        newProjectile.id = key;
        ProjectileModelCache[key] = newProjectile;
        ProjectileNameCache.Add(newProjectile.name);
        return newProjectile;
    }

    protected static float RateModifier { get; set; } = 1;

    private static float GetRate()
    {
        var rate = (CursorTower is { IsDestroyed: false } ? CursorTower.towerModel.GetWeapon().Rate : GetRawRate());
        
        if(!CurrentUpgrades.Any(upgrade => upgrade.PlacesOnTrack))
            return rate * RateModifier;
        
        return rate;
    }

    public static float GetRawRate()
    {
        string key = Paths.Aggregate("",
            (current, path) => current + $"{path}:{UpgradeMenu.PurchasedUpgrades[path]}");
        if (RateCache.TryGetValue(key, out var dictrate))
        {
            return dictrate;
        }

        var rate = 0f;
        rate = Cache[Path.Clicker][0].ModifyRate(rate);

        rate = Paths.Where(path => path != Path.Clicker).OrderBy(x => x).Aggregate(rate,
            (current1, path) => Cache[path].Keys.Where(tier => UpgradeMenu.PurchasedUpgrades[path] >= tier)
                .OrderBy(x => x).Aggregate(current1, (current, tier) => Cache[path][tier].ModifyRate(current)));

        RateCache[key] = rate;

        return rate;
    }

    private static void Setup(Projectile projectile)
    {
        projectile.Position.X = InGame.instance.inputManager.cursorPositionWorld.x;
        projectile.Position.Y = InGame.instance.inputManager.cursorPositionWorld.y;
        projectile.Position.Z = 20;

        projectile.direction.X = 0;
        projectile.direction.Y = 0;
        projectile.direction.Z = 0;

        projectile.emittedFrom = new Vector3(InGame.instance.InputManager.cursorPositionWorld.x,
            InGame.instance.InputManager.cursorPositionWorld.y, 20);
        
        var maxLife = CurrentUpgrades.Max(x => x.LifeSpan) ?? LifeSpan.NormalClick;

        ProjectileAge[projectile] = maxLife;
    }
    
    public static void TryCreateProjectile(bool checkRate = true)
    {
        var rate = GetRate();
        if(TimeMouseHeld > .35f && UpgradeMenu.PurchasedUpgrades[Path.Third] < 2)
            rate *= 1.15f;
        
        if (checkRate && TimeSinceLastAttack < rate)
            return;

        var model = CursorTower is { IsDestroyed: false } ? CursorTower.towerModel.GetWeapon().projectile : GetProjectileModel();
        
        var proj = InGame.instance.GetMainFactory().CreateEntityWithBehavior<Projectile, ProjectileModel>(model);

        proj.pierce = model.pierce;

        Setup(proj);
        
        foreach (var upgrade in CurrentUpgrades.OrderBy(x => x.Tier))
        {
            upgrade.OnCreate(proj);
        }

        TimeSinceLastAttack = 0;
        RateModifier = 1;
    }

    private static readonly Dictionary<string, float> RateCache = new();

    private static readonly Dictionary<string, ProjectileModel> ProjectileModelCache = new();

    public abstract int Cost { get; }
    protected virtual string Icon => Name;
    public SpriteReference IconReference => GetSpriteReferenceOrDefault(Icon);
    public abstract override string Description { get; }
    public abstract int Tier { get; }
    public abstract Path Path { get; }
    protected virtual float ModifyRate(float rate) => rate;
    protected virtual LifeSpan LifeSpan => LifeSpan.NormalClick;

    public virtual void OnCreate(Projectile projectile)
    {
    }

    public virtual void OnDestroy(Projectile projectile)
    {
    }

    protected virtual void ModifyProjectile(ProjectileModel projectile)
    {
    }

    protected virtual void PostModifyProjectile(ProjectileModel projectile)
    {
    }

    public virtual void OnUpdate()
    {
    }

    public virtual void OnSell()
    {
    }
    
    public virtual void ModifyCursorTower(TowerModel towerModel)
    {
    }

    public virtual void OnMapSaved(MapSaveDataModel mapData)
    {
    }
    
    public virtual void OnMapLoaded(MapSaveDataModel mapData)
    {
    }

    protected virtual bool PlacesOnTrack => false;

    public virtual void OnBuy()
    {
    }
}