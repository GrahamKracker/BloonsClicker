namespace BloonsClicker.Upgrades;

public class ExplosiveClicks : CursorUpgrade
{
    public override int Cost => 1200;
    protected override string Icon => Name;
    public override float Rate => .6f;
    public override string Description => "Attack now deals 4 damage, and explodes on impact, explosion deals 1 damage with 3 pierce. Attack rate increased to 1 every "+Rate+" seconds";
    public override int Tier => 4;
    protected override Main.LifeSpan LifeSpan => Main.LifeSpan.NormalClick;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.GetDamageModel().damage = 4;
        projectile.radius = 10f;
        
        var bombProjectile = Game.instance.model.GetTowerFromId(TowerType.BombShooter).GetWeapon().projectile;
        var bombcreate = bombProjectile.GetBehavior<CreateProjectileOnContactModel>().Duplicate();
        bombcreate.projectile.pierce = 3;
        bombcreate.projectile.GetDamageModel().damage = 1;
        bombcreate.projectile.name = "ExplosiveClick";
        bombcreate.passOnCollidedWith = true;
        
        projectile.AddBehavior(bombProjectile.GetBehavior<CreateSoundOnProjectileCollisionModel>().Duplicate());
        projectile.AddBehavior(bombProjectile.GetBehavior<CreateEffectOnContactModel>().Duplicate());
        projectile.AddBehavior(bombcreate);
        
        foreach(var behavior in projectile.GetDescendants<DamageModel>().ToList())
        {
            behavior.immuneBloonProperties = BloonProperties.Black;
        }
        
        foreach (var behavior in projectile.GetDescendants<FilterInvisibleModel>().ToList())
        {
            behavior.isActive = false;
        }
    }
}