namespace BloonsClicker.Upgrades.Path2;

public class MoabFreeze : CursorUpgrade
{
    public override int Cost => 5500;
    public override string Description => "Clicks are now able to freeze MOABs.";
    public override int Tier => 7;
    public override Path Path => Path.Second;

    /// <inheritdoc />
    protected override void PostModifyProjectile(ProjectileModel projectile)
    {
        foreach (var freezeModel in projectile.GetDescendants<FreezeModel>().ToList())
        {
            freezeModel.canFreezeMoabs = true;
        }
    }
}