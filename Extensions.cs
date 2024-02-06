using System;
using System.Collections.Generic;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Reflection;

namespace BloonsClicker;

public static class Extensions
{
    public static void UpdateCollisionPass(this ProjectileModel projectile, int collisionPass)
    {
        projectile.collisionPasses ??= Array.Empty<int>();

        if (!projectile.collisionPasses.Contains(collisionPass))
        {
            projectile.collisionPasses = projectile.collisionPasses.Concat(new []
            {
                collisionPass
            }).ToArray();
        }
    }
    
    private static readonly Dictionary<string, FieldInfo> BehaviorsFieldCache = new();

    public static bool HasBehaviorWithName(this Model model, string name, bool throwIfNotFound = true)
    {
        var type = model.GetIl2CppType();
        if (!BehaviorsFieldCache.TryGetValue(type.FullName, out var behaviorsField))
        {
            behaviorsField = type.GetField("behaviors");
            if (behaviorsField == null)
            {
                if (throwIfNotFound)
                    throw new InvalidOperationException($"Model {model.name} does not have a behaviors field");
                return false;
            }
            BehaviorsFieldCache.Add(type.FullName, behaviorsField);
        }
        
        var behaviors = behaviorsField.GetValue(model).Cast<Il2CppReferenceArray<Model>>();
        return Enumerable.Any(behaviors, behavior => behavior.name == name);
    }
    
    private static readonly Dictionary<string, float> MaxHealthCache = new();

    public static float GetMaxHealth(this BloonModel bloonModel)
    {
        if (MaxHealthCache.TryGetValue(bloonModel.id, out var maxHealth))
        {
            return maxHealth;
        }
        var totalHealth = 0;
        bloonModel.UpdateChildBloonModels();
        foreach (var child in bloonModel.GetChildBloonModels(InGame.instance.bridge.Simulation))
        {
            totalHealth += child.maxHealth;
        }
        totalHealth += bloonModel.maxHealth;
        MaxHealthCache[bloonModel.id] = totalHealth;
        return totalHealth;
    }

}