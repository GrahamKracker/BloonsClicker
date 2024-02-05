using System;
using BTD_Mod_Helper.Api.Components;
using UnityEngine;
using UnityEngine.Serialization;

namespace BloonsClicker;

[RegisterTypeInIl2Cpp(false)]
public class CursorUpgradeButton : MonoBehaviour
{
    public CursorUpgradeButton(IntPtr ptr) : base(ptr)
    {
    }
    
    public CursorUpgrade Upgrade { get; set; } = null!;
    
    public ModHelperButton ModHelperButton { get; set; } = null!;
    
    public ModHelperImage PurchasedCheckmarkImage { get; set; }
    public ModHelperImage LockedImage { get; set; }
    
    public void UpdateLockState()
    {
        LockedImage.SetActive(false);
        PurchasedCheckmarkImage.SetActive(false);
        
        if(!UpgradeMenu.IsAvailable(Upgrade))
            LockedImage.SetActive(true);

        if (UpgradeMenu.PurchasedUpgrades[Upgrade.Path] >= Upgrade.Tier)
        {
            PurchasedCheckmarkImage.SetActive(true);
            LockedImage.SetActive(false);
        }
    }
    
    public static CursorUpgradeButton Create(CursorUpgrade upgrade, ModHelperPanel panel)
    {
        var button = panel.AddButton(new Info(upgrade.Name, 400), VanillaSprites.UpgradeContainerBlue, new Action(() =>
        {
            UpgradeMenu.UpdateSelectionPanel(upgrade);
        }));
        
        var glow = button.AddImage(new Info("SelectedGlow", 450), VanillaSprites.SmallRoundGlowOutline);
        glow.SetActive(false);
        button.AddImage(new Info("Icon", 300), upgrade.IconReference.guidRef);

        var purchasedCheckmark = button.AddImage(new Info("PurchasedCheckmark", 150,150, new Vector2(.2f, .8f)), VanillaSprites.TickGreenIcon);
        
        var locked = button.AddImage(new Info("Locked", 400), VanillaSprites.UpgradeContainerGrey);
        locked.Image.color = new Color(0.5697f,0.5993f,0.6981f, 0.7412f);
        locked.AddImage(new Info("Lock", 200), VanillaSprites.LockIcon);
        
        button.AddText(new Info("Name", 400, 100,  new Vector2(.5f, .1f)), upgrade.DisplayName);
        var modHelperButton = button;
        var cursorUpgradeButton = modHelperButton.gameObject.AddComponent<CursorUpgradeButton>();
        cursorUpgradeButton.PurchasedCheckmarkImage = purchasedCheckmark;
        cursorUpgradeButton.LockedImage = locked;
        
        cursorUpgradeButton.Upgrade = upgrade;
        cursorUpgradeButton.ModHelperButton = modHelperButton;
        UpgradeMenu.UpgradeButtons.Add(upgrade, cursorUpgradeButton);
        
        cursorUpgradeButton.UpdateLockState();
        
        return cursorUpgradeButton;
    }
}