using Il2CppAssets.Scripts.Models.Effects;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Utils;
using Il2CppNinjaKiwi.Common.ResourceUtils;

namespace BloonsClicker.Upgrades;

public class NuclearClicks : CursorUpgrade
{
    public override int Cost => 325_000;
    protected override float ModifyRate(float rate) => rate * .5f;
    public override string Description => "Pinnacle of Clicker technology. Dart damage and pierce significantly increased. Releases many smaller darts on contact.";
    public override int Tier => 10;
    protected override Main.LifeSpan LifeSpan => Main.LifeSpan.PermaClicks;
    public override Path Path => Path.First;

    /// <inheritdoc />
    protected override bool PlacesOnTrack => true;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.pierce *= 2;
        projectile.GetDamageModel().damage *= 2;

        projectile.GetBehavior<CreateEffectOnContactModel>().effectModel = new EffectModel("EffectModel_",
            new PrefabReference { guidRef = "b1324f2f4c3809643b7ef1d8c112442a" }, 1, 6);
        
        var createProjectileOnContact = projectile.GetBehaviors<CreateProjectileOnContactModel>()
            .First(x => x.name.Contains("SmallDart"));

        createProjectileOnContact.emission.Cast<ArcEmissionModel>().count = 16;

        createProjectileOnContact.projectile.pierce *= 2;
        createProjectileOnContact.projectile.GetDamageModel().damage *= 2;
        createProjectileOnContact.passOnCollidedWith = false;
    }
}