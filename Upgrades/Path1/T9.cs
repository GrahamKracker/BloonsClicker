using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;

namespace BloonsClicker.Upgrades;

public class PermaClicks : CursorUpgrade
{
    public override int Cost => 60000;
    protected override float ModifyRate(float rate) => rate * .75f;
    public override string Description => "Darts on the ground are now much stronger.";
    public override int Tier => 9;
    protected override Main.LifeSpan LifeSpan => Main.LifeSpan.PermaClicks;

    /// <inheritdoc />
    public override Path Path => Path.First;

    /// <inheritdoc />
    protected override bool PlacesOnTrack => true;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.pierce = 16;
        projectile.GetDamageModel().damage = 12;

        var createProjectileOnContact = projectile.GetBehaviors<CreateProjectileOnContactModel>().First(x=>x.name.Contains("SmallDart"));
        var smalldart = createProjectileOnContact.projectile;
        smalldart.pierce = 8;
        smalldart.GetDamageModel().damage = 6;

        createProjectileOnContact.emission.Cast<ArcEmissionModel>().count = 8;
    }
}