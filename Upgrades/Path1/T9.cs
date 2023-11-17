using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Simulation.SMath;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;

namespace BloonsClicker.Upgrades;

public class PermaClicks : CursorUpgrade
{
    public override int Cost => 60000;
    protected override string Icon => Name;
    public override float Rate => .05f;
    public override string Description => "Darts on the ground now last for 30 seconds, they now have 16 pierce, and 12 damage, and release 8 projectiles on contact, each with 8 pierce, and 6 damage. Attack rate increased to every "+Rate+" seconds";
    public override int Tier => 9;
    protected override Main.LifeSpan LifeSpan => Main.LifeSpan.PermaClicks;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.pierce = 16;
        projectile.GetDamageModel().damage = 12;

        var createProjectileOnContact = projectile.GetBehavior<CreateProjectileOnContactModel>();
        var smalldart = createProjectileOnContact.projectile;
        smalldart.pierce = 8;
        smalldart.GetDamageModel().damage = 6;
        
        createProjectileOnContact.emission = new ArcEmissionModel("ArcEmissionModel_", 8, 0, 360, null, false, false);
    }
}