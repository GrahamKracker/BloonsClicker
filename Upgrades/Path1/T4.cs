namespace BloonsClicker.Upgrades;

public class ExplosiveClicks : CursorUpgrade
{
    public override int Cost => 1600;
    /// <inheritdoc />
    protected override float ModifyRate(float rate) => rate * .85f;
    public override string Description => "Attack now deals more damage, and explodes on impact. Attack rate slightly increased.";
    public override int Tier => 4;
    public override Path Path => Path.First;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.GetDamageModel().damage++;
        
        var bombProjectile = Game.instance.model.GetTowerFromId(TowerType.BombShooter).GetWeapon().projectile;
        var bombcreate = bombProjectile.GetBehavior<CreateProjectileOnContactModel>().Duplicate();
        bombcreate.name = "CreateProjectileOnContactModel_ExplosiveClick_";
        bombcreate.projectile.pierce = 3;
        bombcreate.projectile.GetDamageModel().damage = 1;
        bombcreate.projectile.name = "ExplosiveClick";
        Main.ProjectileNameCache.Add("ExplosiveClick");
        bombcreate.passOnCollidedWith = true;
        
        projectile.AddBehavior(bombProjectile.GetBehavior<CreateSoundOnProjectileCollisionModel>().Duplicate());
        projectile.AddBehavior(bombProjectile.GetBehavior<CreateEffectOnContactModel>().Duplicate());
        projectile.AddBehavior(bombcreate);
    }

    /// <inheritdoc />
    protected override void PostModifyProjectile(ProjectileModel projectile)
    {
        foreach (var behavior in projectile.GetDescendants<DamageModel>().ToList())
        {
            behavior.immuneBloonProperties &= BloonProperties.Black;
            behavior.immuneBloonProperties &= ~BloonProperties.Lead;
        }
    }
}