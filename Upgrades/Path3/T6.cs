using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using UnityEngine;

namespace BloonsClicker.Upgrades.Path3;

public class AutomagicClicks : CursorUpgrade
{
    public override int Cost => 2600;
    public override string Description => $"The cursor will now automatically click on bloons in range";
    public override int Tier => 6;
    public override Path Path => Path.Third;

    private const float PhysicsRate = 0.1f;
    
    private float _lastCheckTime = Time.time;
    
    /// <inheritdoc />
    public override void OnUpdate()
    {
        if (Time.time - _lastCheckTime < PhysicsRate)
        {
            return;
        }
        
        _lastCheckTime = Time.time;
        
        var projectile = GetProjectileModel();
        var position = InGame.instance.GetUnityWorldFromCursor();

        var collisionState = InGame.instance.bridge.Simulation.collisionChecker.GetInRange<Bloon>(position.x, -position.z, projectile.radius);

        while (collisionState.MoveNext())
        {
            var bloon = collisionState.Current;
            if (bloon == null)
                continue;
            TryCreateProjectile();
            break;
        }
    }
}