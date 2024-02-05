using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;

namespace BloonsClicker.Upgrades;

public class DartSprayer : CursorUpgrade
{
    public override int Cost => 8500;
    protected override string Icon => VanillaSprites.AirburstDartsUpgradeIcon;
    public override string Description => "Clicks shoot out piercing darts on contact.";

    public override int Tier => 6;
    public override Path Path => Path.First;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        var smalldart = Game.instance.model.GetTower(TowerType.DartMonkey).GetWeapon().projectile.Duplicate();
        smalldart.pierce = 5;
        smalldart.GetDamageModel().damage = 4;
        smalldart.name = "SmallDart";
        Main.ProjectileNameCache.Add("SmallDart");
        
        projectile.AddBehavior(new CreateProjectileOnContactModel("CreateProjectileOnContactModel_SmallDart_", 
            smalldart, new ArcEmissionModel("ArcEmissionModel_", 4, 0, 360, null, false, false),
            true, false, false));
    }

    /// <inheritdoc />
    protected override void PostModifyProjectile(ProjectileModel projectile)
    {
        foreach (var behavior in projectile.GetDescendants<DamageModel>().ToList())
        {
            behavior.immuneBloonProperties = BloonProperties.None;
        }
    }
}