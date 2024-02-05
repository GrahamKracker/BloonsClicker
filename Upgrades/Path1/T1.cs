namespace BloonsClicker.Upgrades;

public class StrongerClicks : CursorUpgrade
{
    public override int Cost => 250;
    public override string Description => "Pierce and damage increased";
    public override int Tier => 1;
    public override Path Path => Path.First;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        projectile.pierce++;
        projectile.GetDamageModel().damage++;
    }
}