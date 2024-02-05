using Il2CppAssets.Scripts.Models.GenericBehaviors;

namespace BloonsClicker.Upgrades;

public class StickyClicks : CursorUpgrade
{
    public override int Cost => 28000;
    public override string Description => "Attacks spawn a temporary dart on screen.";
    public override int Tier => 8;
    protected override Main.LifeSpan LifeSpan => Main.LifeSpan.StickyClicks;
    public override Path Path => Path.First;
    /// <inheritdoc />
    protected override bool PlacesOnTrack => true;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.AddBehavior(Game.instance.model.GetTower(TowerType.DartMonkey).GetWeapon().projectile.GetBehavior<DisplayModel>().Duplicate());
        projectile.display = Game.instance.model.GetTower(TowerType.DartMonkey).GetWeapon().projectile.display;
    }
}