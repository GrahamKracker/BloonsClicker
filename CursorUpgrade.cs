using System.Collections.Generic;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Utils;
using UnityEngine;
using Vector3 = Il2CppAssets.Scripts.Simulation.SMath.Vector3;

namespace BloonsClicker;

public abstract class CursorUpgrade : NamedModContent
{
    internal static readonly Dictionary<int, CursorUpgrade> Cache = new();
    public override void Register()
    {
        Cache.Add(Tier, this);
    }
    
    internal static readonly Dictionary<int, ProjectileModel> ProjectileModelCache = new();
    
    public static void GenerateProjectiles()
    {
        ProjectileModelCache.Clear();
        foreach ((int tier, _) in Cache)
        {
            ProjectileModelCache.Add(tier, GetProjectileModelForTier(tier));
        }
    }

    private static ProjectileModel GetProjectileModelForTier(int tier)
    {
        var model = Game.instance.model.GetTower(TowerType.DartMonkey).GetWeapon().projectile.Duplicate();
        for (var i = 0; i <= tier; i++)
        {
            model.name = Cache[i].Name;
            Cache[i].ModifyProjectile(model);
        }
        return model;
    }
    
    public abstract int Cost { get; }
    protected abstract string Icon { get; }
    public virtual SpriteReference IconReference => GetSpriteReferenceOrDefault(Icon);
    public abstract float Rate { get; }
    public abstract override string Description { get; }
    public abstract int Tier { get; }

    protected abstract Main.LifeSpan LifeSpan { get; }

    public virtual void Create(Projectile projectile)
    {
        Main.ProjectileAge[projectile] = LifeSpan;
        if (projectile.HasProjectileBehavior<ArriveAtTarget>())
        {
            projectile.GetProjectileBehavior<ArriveAtTarget>().targetPos =
                new Vector3(InGame.instance.inputManager.cursorPositionWorld);
        }
    }

    protected abstract void ModifyProjectile(ProjectileModel projectile);
    
    public ModHelperButton CreateButton(ModHelperScrollPanel panel)
    {
        var button = panel.AddButton(new Info(Name, 400), VanillaSprites.UpgradeContainerBlue, null);  
        
        var glow = button.AddImage(new Info("SelectedGlow", 450), VanillaSprites.SmallRoundGlowOutline);
        glow.SetActive(false);
        button.AddImage(new Info("Icon", 300), IconReference.guidRef);

        button.AddImage(new Info("PurchasedCheckmark", 150,150, new Vector2(.2f, .8f)), VanillaSprites.TickGreenIcon);
        
        var locked = button.AddImage(new Info("Locked", 400), VanillaSprites.UpgradeContainerGrey);
        locked.Image.color = new Color(0.5697f,0.5993f,0.6981f, 0.7412f);
        locked.AddImage(new Info("Lock", 200), VanillaSprites.LockIcon);
        
        button.AddText(new Info("Name", 400, 100,  new Vector2(.5f, .1f)), DisplayName);
        return button;
    }
}