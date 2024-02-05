namespace BloonsClicker.Upgrades;

public class Fingerdash : CursorUpgrade
{
    public override int Cost => 15550;
    /// <inheritdoc />
    protected override float ModifyRate(float rate) => rate * .25f;
    public override string Description => "Attacks significantly faster.";
    public override int Tier => 7;
    public override Path Path => Path.First;

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
    }
}