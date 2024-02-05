using System;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using UnityEngine;
using Cursor = Il2CppAssets.Cursor;

namespace BloonsClicker.Upgrades;


public class BlessedCursor : CursorUpgrade
{
    public override int Cost => 650_000;
    public override string Description => "The power of the gods is contained within";
    public override int Tier => -1;
    protected override Main.LifeSpan LifeSpan => Main.LifeSpan.PermaClicks;
    public override Path Path => Path.Paragon;
    
    private Texture2D? _cursorDown;
    private Texture2D? _cursorUp;

    private void CreateTextures()
    {
        Texture2D? cursorDownTemp = new Texture2D(2, 2, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point,
            name = "BlessedCursorDown",
        };
        var cursorDownbytes = this.mod.MelonAssembly.Assembly.GetEmbeddedResource("BlessedCursorDown.png");
        cursorDownTemp.LoadImage(cursorDownbytes.GetByteArray());
        
        _cursorDown = cursorDownTemp;

        Texture2D? cursorUpTemp = new Texture2D(2, 2, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point,
            name = "BlessedCursorUp"
        };
        var cursorUpbytes = this.mod.MelonAssembly.Assembly.GetEmbeddedResource("BlessedCursorUp.png");
        cursorUpTemp.LoadImage(cursorUpbytes.GetByteArray());
        
        _cursorUp = cursorUpTemp;

        if (_cursorDown == null || _cursorUp == null)
            throw new InvalidOperationException("Failed to load cursor textures");
    }

    private static Cursor.CursorSprites _defaultCursor { get; set; } = null!;

    /// <inheritdoc />
    public override void OnUpdate()
    {
        if (_cursorDown == null || _cursorUp == null)
        {
            CreateTextures();
        }
        
        if (Cursor.instance.activeConfig.textureDown.name != _cursorDown.name)
        {
            _defaultCursor = Cursor.instance.activeConfig;
            Cursor.instance.activeConfig = new Cursor.CursorSprites
            {
                hotspot = _defaultCursor.hotspot,
                textureDown = _cursorDown,
                textureUp = _cursorUp,
            };
        }
    }

    /// <inheritdoc />
    public override void OnSell()
    {
        Cursor.instance.activeConfig = _defaultCursor;
    }


    //todo: make the thing lightning, water, ice, the whole shebang

    protected override void ModifyProjectile(ProjectileModel projectile)
    {
        var proj = Game.instance.model.GetTower(TowerType.Druid, 4)
                    .GetDescendants<ProjectileModel>().ToList().Find(x=>x.id == "SpawningProjectile")!.Duplicate();
        
        var createProjectileOnExpireModel = new CreateProjectileOnExhaustFractionModel("CreateProjectileOnExhaustFractionModel_", proj,
            new ArcEmissionModel("ArcEmissionModel_", 6, 0, 360, null, false, 
                false), .2f, -1, true, false, false);
        projectile.AddBehavior(createProjectileOnExpireModel);
    }
    
}