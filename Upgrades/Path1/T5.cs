namespace BloonsClicker.Upgrades;

public class HypersonicClicks : CursorUpgrade
{
    public override int Cost => 2300;
    protected override float ModifyRate(float rate) => rate * .77f;
    public override string Description => "Attack speed, pierce, and explosion damage increased. Can now pop black bloons.";
    public override int Tier => 5;
    public override Path Path => Path.First;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.pierce = 7;
        projectile.GetBehaviors<CreateProjectileOnContactModel>().First(x=> x.name.Contains("ExplosiveClick")).projectile.GetDamageModel().damage = 2;
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