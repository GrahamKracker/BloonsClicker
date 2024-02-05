using System.Collections;
using System.Collections.Generic;
using Il2CppAssets.Scripts.Simulation.Bloons;
using UnityEngine;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using System;
using BloonsClicker.Upgrades.Path2.SlowAreaClasses;
using Il2CppAssets.Scripts;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Simulation.Behaviors;
using UnityEngine.AddressableAssets;

namespace BloonsClicker.Upgrades.Path2;

public class SlowArea : CursorUpgrade
{
    public override int Cost => 3500;
    public override string Description => $"Clicks leave a temporary slowing area";
    public override int Tier => 6;
    public override Path Path => Path.Second;
    private float SlowLifeSpan => UpgradeMenu.PurchasedUpgrades[Path] < 10 ? 4f : Blizzard.SlowLifeSpan;

    private const float SlowAreaCooldown = .4f;
    private float SlowRadius => UpgradeMenu.PurchasedUpgrades[Path] < 10 ? 22.5f : Blizzard.SlowRadius;
    
    private const float PhysicsRate = .05f;

    private float _lastAttackTime = Time.time;

    private const string SlowAreaPrefabKey = "12e86af1959e20a46a959673bbf077e6";

    private float _lastPhysicsTime = Time.time;
    
    private float SlowAmount => UpgradeMenu.PurchasedUpgrades[Path] < 10 ? .5f : Blizzard.SlowAmount;
    
    
    public Vector3 Scale => UpgradeMenu.PurchasedUpgrades[Path] < 10 ? Vector3.one : Blizzard.Scale;
    
    
    private static readonly Vector3 OffscreenPosition = Factory.kOffscreenPosition;

    private static readonly ObjectPool<Area> SlowAreaPool = new(() =>
    {
        var slowAreaGameObject = Addressables.InstantiateAsync(SlowAreaPrefabKey, OffscreenPosition, Quaternion.identity).WaitForCompletion();
        slowAreaGameObject.SetActive(false);
        var slowArea = new Area(default, slowAreaGameObject);
        return slowArea;
    }, null!, area =>
    {
        area.GameObject.transform.position = OffscreenPosition;
        area.GameObject.SetActive(false);
        area.Position = default;
    });

    /// <inheritdoc />
    public override void OnCreate(Projectile projectile)
    {
        //only place tower if its been .5 seconds since last attack
        if (Time.time - _lastAttackTime < SlowAreaCooldown)
        {
            return;
        }

        _lastAttackTime = Time.time;

        MelonCoroutines.Start(WaitForSlowArea(MakeSlowArea()));
    }
    
    /// <inheritdoc />
    public override void OnUpdate()
    {
        if (Time.time - _lastPhysicsTime < PhysicsRate)
        {
            return;
        }

        _lastPhysicsTime = Time.time;
        
        
        var bloons = InGame.instance.GetFactory<Bloon>().up.list;
        
        HashSet<ObjectId> bloonsInASlowArea = [];

        foreach (var area in SlowAreas)
        {
            var position = area.Position;
            var collisionState =
                InGame.instance.UnityToSimulation.Simulation.collisionChecker.GetInRange<Bloon>(position.x, -position.z,
                    area.Radius);
            if (collisionState == null)
                continue;
            while (collisionState.MoveNext())
            {
                var bloon = collisionState.Current;
                if (bloon == null)
                    continue;
                bloonsInASlowArea.Add(bloon.Id);

                if (bloon.GetMutatorById("SlowArea_" + UpgradeMenu.PurchasedUpgrades[Path]) != null ||
                    (bloon.bloonModel.IsMoabBloon() && UpgradeMenu.PurchasedUpgrades[Path] < 7))
                    continue;

                if (bloon.bloonModel.IsMoabBloon())
                {
                    bloon.AddMutator(new SlowModel.SlowMutator(Blizzard.SlowAmountZOMGBelow,
                        "SlowArea_" + UpgradeMenu.PurchasedUpgrades[Path], "", false, true, 0));
                    return;
                }

                bloon.AddMutator(new SlowModel.SlowMutator(SlowAmount,
                    "SlowArea_" + UpgradeMenu.PurchasedUpgrades[Path], "", false, true, 0));
            }
        }
        
        foreach (var bloon in bloons.Where(bloon => !bloonsInASlowArea.Contains(bloon.Id)))
        {
            if(bloon.mutators?._items == null)
                continue;
            foreach (var timedMutator in bloon.mutators._items)
            {
                if (timedMutator == null)
                    continue;
                if (timedMutator.mutator.id.StartsWith("SlowArea_"))
                    bloon.RemoveMutatorsById(timedMutator.mutator.id);
            }
        }
        
    }

    private IEnumerator WaitForSlowArea(Area slowArea)
    {
        yield return new WaitForSeconds(SlowLifeSpan);
        SlowAreaPool.Release(slowArea);
        SlowAreas.Remove(slowArea);
    }

    private Area MakeSlowArea()
    {
        var position = InGame.instance.GetUnityWorldFromCursor();
        var slowArea = SlowAreaPool.Get();
        slowArea.GameObject.transform.position = position;
        slowArea.GameObject.SetActive(true);
        slowArea.Position = position;
        slowArea.GameObject.transform.localScale = Scale;
        slowArea.Radius = SlowRadius;
        
        SlowAreas.Add(slowArea);
        
        return slowArea;
    }

    private sealed class Area(Vector3 position, GameObject gameObject)
    {
        public Vector3 Position { get; set; } = position;
        public GameObject GameObject { get; } = gameObject;
        public float Radius { get; set; }
    }

    private static readonly HashSet<Area> SlowAreas = [];
}