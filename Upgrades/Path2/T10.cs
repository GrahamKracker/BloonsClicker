using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Utils;
using UnityEngine;

namespace BloonsClicker.Upgrades.Path2;

public class Blizzard : CursorUpgrade
{
    public override int Cost => 90_000;
    public override string Description => "Slow area massively increaseds. ZOMGs and below are slowed significantly slowed.";
    public override int Tier => 10;
    public override Path Path => Path.Second;

    public const float SlowRadius = 45f;
    public const float SlowAmount = .1f;
    public const float SlowAmountZOMGBelow = .25f;

    public const float SlowLifeSpan = 10f;
    
    public static readonly Vector3 Scale = new(2f, 2f, 2f);
}