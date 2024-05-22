using System;
using System.Collections.Generic;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Helpers;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppAssets.Scripts.Unity.UI_New.Settings;
using Il2CppTMPro;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace BloonsClicker;

//todo: setting toggle to make towers not clickable (for when you want to play )
public class UpgradeMenu : ModGameMenu<HotkeysScreen>
{
    private static ModHelperScrollPanel MainPanel { get; set; } = null!;
    private static InfoPanel InfoPanel { get; set; } = null!;

    private CanvasGroup CanvasGroup { get; set; } = null!;
    private Animator? Animator { get; set; } 
    private static CursorUpgrade SelectedUpgrade { get; set; } = null!;
    
    public const int UnPurchased = int.MinValue;
    public static Dictionary<Path, int> PurchasedUpgrades { get; set; } = Main.Paths.ToDictionary(path => path, _ => UnPurchased);

    public static Dictionary<CursorUpgrade, CursorUpgradeButton> UpgradeButtons { get; } = new();

    private const int MainWidth = 3600;
    private const int TierSpacing = 1000;

    public override void OnMenuUpdate()
    {
        if (Closing)
        {
            CanvasGroup.alpha -= .07f;
        }
        else if ((Input.GetKeyDown(KeyCode.Comma) || Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.Slash)) && InfoPanel.BuyButton != null)
        {
            InfoPanel.BuyButton.Button.onClick.Invoke();
        }
    }

    public override void OnMenuClosed()
    {
        if(Animator != null)
            Animator.Play("PopupSlideOut");
        UpgradeButtons.Clear();
    }

    public override bool OnMenuOpened(Object data)
    {
        var gameObject = GameMenu.gameObject;
        gameObject.DestroyAllChildren();
        GameMenu.saved = true;

        var basePanel = gameObject.AddModHelperPanel(new Info("BasePanel", InfoPreset.FillParent));
        MainPanel = basePanel.AddScrollPanel(new Info("UpgradesPanel", MainWidth, 1700, new Vector2(.5f, .6f)),
            RectTransform.Axis.Vertical, VanillaSprites.BlueInsertPanel, 75, 150);
        MainPanel.Mask.showMaskGraphic = false;


        Animator = basePanel.AddComponent<Animator>();
        Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        Animator.runtimeAnimatorController = Animations.PopupAnim;
        Animator.speed = .75f;
        Animator.Rebind();

        CanvasGroup = basePanel.AddComponent<CanvasGroup>();

        var infoPanel = basePanel.AddPanel(new Info("InfoPanel", 2600, 600, new Vector2(.5f, .175f)),
            VanillaSprites.MainBgPanel, RectTransform.Axis.Horizontal, 75, 150);
        InfoPanel = infoPanel.AddComponent<InfoPanel>();
        InfoPanel.panel = infoPanel;

        CreateInfoPanel();

        CreateUpgrades();

        CommonForegroundScreen.instance.Hide();
        CommonForegroundScreen.instance.Show(true, "Clicker Upgrades", false, false, false, false, false, false);

        foreach (var animator in basePanel.GetComponentsInChildren<Animator>())
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        return false;
    }

    private static void CreateInfoPanel()
    {
        var nameIconPanel = InfoPanel.panel.AddPanel(new Info("NameIconPanel", 500), VanillaSprites.MainBgPanel,
            RectTransform.Axis.Vertical, 50, 50);
        nameIconPanel.Background.color = new Color(0.3f, 0.05f, 0f, 0.5f);

        InfoPanel.NameText = nameIconPanel.AddText(new Info("NameText", 500, 50), "");

        InfoPanel.IconImage = nameIconPanel.AddImage(new Info("IconImage", 300), "");

        var descriptionPanel =
            InfoPanel.panel.AddPanel(new Info("DescriptionPanel", 1000, 350), VanillaSprites.MainBgPanel, null);
        descriptionPanel.Background.color = new Color(0.3f, 0.05f, 0f, 0.5f);

        InfoPanel.DescriptionText = descriptionPanel.AddText(new Info("DescriptionText", 900, 350), "");

        var buyPanel = InfoPanel.panel.AddPanel(new Info("BuyPanel")
        {
            Width = 500,
            Height = 550,
        }, VanillaSprites.MainBgPanel, RectTransform.Axis.Vertical, 35, 30);

        buyPanel.Background.color = new Color(0.3f, 0.05f, 0f, 0.5f);

        var popsPanel = buyPanel.AddPanel(new Info("PopsPanel")
        {
            Width = 500,
            Height = 25,
        }, null);

        popsPanel.AddImage(new Info("PopsIcon", -150, 0, 50, 50), VanillaSprites.PopIcon);

        InfoPanel.PopsText = popsPanel.AddText(new Info("PopsText", 125, 0, 450, 50), $"{Main.CursorPops:n0}");
        InfoPanel.PopsText.Text.alignment = TextAlignmentOptions.MidlineLeft;

        InfoPanel.BuyButton = buyPanel.AddButton(new Info("BuyButton")
        {
            FlexWidth = 1,
            Height = 200,
        }, VanillaSprites.GreenBtnLong, new Action(
            () =>
            {
                var cost = CostHelper.CostForDifficulty(SelectedUpgrade.Cost, InGame.instance.GetGameModel());
                
                if (!CanBuy(SelectedUpgrade))
                {
                    if (!IsAvailable(SelectedUpgrade))
                    {
                        /*PopupScreen.instance.ShowPopup(PopupScreen.Placement.menuCenter, "Not Available",
                            "This upgrade is not available for purchase. You either have already purchased this upgrade, or you need to purchase the previous upgrade(s) in this path.", null,
                            "Ok", null, null, Popup.TransitionAnim.Scale);*/
                        
                        
                        // i got annoyed with it lmao
                        return;
                    }
                    if (InGame.instance.GetCash() < cost)
                    {
                        PopupScreen.instance.ShowPopup(PopupScreen.Placement.menuCenter, "Not Enough Cash",
                            $"You need ${cost:n0} to purchase this upgrade.", null, "Ok", null, null,
                            Popup.TransitionAnim.Scale);
                        return;
                    }
                    
                    PopupScreen.instance.ShowPopup(PopupScreen.Placement.menuCenter, "Cannot Purchase",
                        "It appears something went wrong, try reloading your save or restarting your game. Contact GrahamKracker if this happens repeatedly.", null, "Ok", null, null,
                        Popup.TransitionAnim.Scale);
                    
                    return;
                }
                
                InGame.instance.AddCash(-cost);
                
                InfoPanel.CashText.SetText($"{InGame.instance.GetCash():n0}");
                
                PurchasedUpgrades[SelectedUpgrade.Path] = SelectedUpgrade.Tier;
                Main.CurrentUpgrades.Add(SelectedUpgrade);

                SelectedUpgrade.OnBuy();
                
                foreach (var button in UpgradeButtons.Values)
                {
                    button.UpdateLockState();
                }

                var sortedUpgrades = GetContent<CursorUpgrade>().Where(x => x.Path == SelectedUpgrade.Path)
                    .OrderBy(x => x.Tier).ToArray();
                var index = Array.IndexOf(sortedUpgrades, SelectedUpgrade) + 1;

                if (index >= sortedUpgrades.Length)
                {
                    UpdateSelectionPanel(sortedUpgrades[^1]);
                }
                else
                    UpdateSelectionPanel(sortedUpgrades[index]);
                
                CursorUpgrade.UpdateTower();
            }));

        InfoPanel.PurchaseText = InfoPanel.BuyButton.AddText(new Info("PurchaseText", 500, 250, new Vector2(.5f, .65f)), "Purchase", 70);
        InfoPanel.CostText = InfoPanel.BuyButton.AddText(new Info("CostText", 500, 50, new Vector2(.5f, .35f)), "");

        var cashPanel = buyPanel.AddPanel(new Info("CashPanel")
        {
            Width = 500,
            Height = 25,
        }, null);

        cashPanel.AddImage(new Info("CashIcon", -150, 0, 50, 50), VanillaSprites.CoinIcon);

        InfoPanel.CashText = cashPanel.AddText(new Info("CashText", 125, 0, 450, 50),
            $"{InGame.instance.GetCash():n0}");
        InfoPanel.CashText.Text.alignment = TextAlignmentOptions.MidlineLeft;

        InfoPanel.SellButton = buyPanel.AddButton(new Info("SellButton")
        {
            FlexWidth = 1,
            Height = 150,
        }, VanillaSprites.RedBtnLong, new Action(
            () =>
            {        
                PopupScreen.instance.SafelyQueue(screen =>
                {
                    float totalCost = PurchasedUpgrades.Aggregate<KeyValuePair<Path, int>, float>(0, (current, keyValuePair) =>
                    {
                        if (keyValuePair.Value < 0)
                            return current;
                        return current + CostHelper.CostForDifficulty(
                            CursorUpgrade.Cache[keyValuePair.Key][keyValuePair.Value].Cost,
                            InGame.instance.GetGameModel());
                    });
                    screen.ShowPopup(PopupScreen.Placement.menuCenter, "Sell Cursor Upgrades",
                        $"Would you like to sell ALL your cursor upgrades? You will get ${totalCost * .75f} out of your ${totalCost} back.",
                        new Action(() =>
                        {
                            InGame.instance.AddCash(totalCost * .75f);

                            InfoPanel.CashText.SetText($"{InGame.instance.GetCash():n0}");
                            
                            foreach (var upgrade in Main.CurrentUpgrades.OrderBy(x => x.Tier))
                            {
                                upgrade.OnSell();
                            }
                            
                            PurchasedUpgrades = Main.Paths.ToDictionary(path => path, _ => UnPurchased);
                            Main.CurrentUpgrades.Clear();
                            
                            foreach (var button in UpgradeButtons.Values)
                            {
                                button.UpdateLockState();
                            }

                            if (CursorUpgrade.CursorTower != null && !CursorUpgrade.CursorTower.IsDestroyed)
                                CursorUpgrade.CursorTower.SellTower();
                            
                            CursorUpgrade.CursorTower = null;


                            UpdateSelectionPanel(GetContent<CursorUpgrade>().First(x => x.Path == Path.Clicker));
                        }), "Yes", null, "No", Popup.TransitionAnim.Scale);
                });
                
            }));

        InfoPanel.SellText = InfoPanel.SellButton.AddText(new Info("SellText", 500, 150), "Sell", 70);
    }

    private static void CreateClickerButton()
    {
        var clickerPanel = MainPanel.AddPanel(new Info("Clicker Panel", 0, 0,
            MainWidth, 450, new Vector2(.5f, .5f)), VanillaSprites.BlueInsertPanel);
        clickerPanel.Background.enabled = false;
        MainPanel.AddScrollContent(clickerPanel);

        CursorUpgradeButton.Create(GetContent<CursorUpgrade>().First(x => x.Path == Path.Clicker), clickerPanel);
    }

    private static void CreateParagonButton()
    {
        var paragonPanel = MainPanel.AddPanel(new Info("Paragon Panel", 0, 0,
            MainWidth, 450, new Vector2(.5f, .5f)), VanillaSprites.BlueInsertPanel);
        paragonPanel.Background.enabled = false;
        MainPanel.AddScrollContent(paragonPanel);
        
        var paragonUpgrade = GetContent<CursorUpgrade>().Find(x => x.Path == Path.Paragon);

        if (paragonUpgrade is null)
        {
            return;
        }
        
        var upgradeButton = CursorUpgradeButton.Create(paragonUpgrade,
            paragonPanel);
        upgradeButton.ModHelperButton.Image.SetSprite(VanillaSprites.UpgradeContainerParagon);
    }

    private static void CreateUpgrades()
    {
        var maxTier = GetContent<CursorUpgrade>().Max(x => x.Tier);

        CreateClickerButton();

        for (int tier = 1; tier <= maxTier; tier++)
        {
            var tierPanel = MainPanel.AddPanel(new Info($"Tier{tier} Panel", 0, 0,
                MainWidth, 450, new Vector2(.5f, .5f)), VanillaSprites.BlueInsertPanel);

            tierPanel.Background.enabled = false;

            MainPanel.AddScrollContent(tierPanel);

            foreach (var upgrade in GetContent<CursorUpgrade>().Where(x => x.Tier == tier).OrderBy(x => x.Path))
            {
                var button = CursorUpgradeButton.Create(upgrade, tierPanel);

                var paths = Main.Paths.Where(x => (int)x > 0).ToArray();
                paths = paths.OrderBy(x => x).ToArray();

                var upgradePath = (int)(upgrade.Path - 1);

                float x;
                if (paths.Length % 2 == 0)
                {
                    x = (upgradePath - (int)(paths.Length / 2f) + .5f) * TierSpacing;
                }
                else
                {
                    x = (upgradePath - (int)(paths.Length / 2f)) * TierSpacing;
                }

                var transform = button.ModHelperButton.transform;
                var position = transform.localPosition;
                transform.localPosition = position with { x = x };
            }
        }

        CreateParagonButton();

        SelectedUpgrade = GetContent<CursorUpgrade>().First(x => x.Path == Path.Clicker);
        UpdateSelectionPanel(SelectedUpgrade);
    }

    public static void UpdateSelectionPanel(CursorUpgrade upgrade)
    {
        InfoPanel.NameText.SetText(upgrade.DisplayName);
        InfoPanel.IconImage.Image.SetSprite(upgrade.IconReference);
        InfoPanel.DescriptionText.SetText(upgrade.Description);
        InfoPanel.CostText.SetText(
            $"${CostHelper.CostForDifficulty(upgrade.Cost, InGame.instance.GetGameModel()):n0}");
        
        SelectedUpgrade = upgrade;

        var selectedButton = UpgradeButtons[upgrade];
        foreach (var button in UpgradeButtons.Values)
        {
            button.transform.FindChild("SelectedGlow").gameObject.SetActive(false);
        }

        selectedButton.transform.FindChild("SelectedGlow").gameObject.SetActive(true);
        
        var canBuy = CanBuy(upgrade);

        InfoPanel.BuyButton.Button.interactable = canBuy;
        InfoPanel.BuyButton.Image.color = canBuy ? InfoPanel.BuyButton.Button.colors.normalColor : InfoPanel.BuyButton.Button.colors.disabledColor;
        InfoPanel.PurchaseText.Text.color = canBuy
            ? InfoPanel.BuyButton.Button.colors.normalColor
            : InfoPanel.BuyButton.Button.colors.disabledColor;
        InfoPanel.CostText.Text.color = canBuy
            ? InfoPanel.BuyButton.Button.colors.normalColor
            : InfoPanel.BuyButton.Button.colors.disabledColor;
        
        var canSell = PurchasedUpgrades.Any(x => x.Value >= 0);

        InfoPanel.SellButton.Button.interactable = canSell;
        InfoPanel.SellButton.Image.color = canSell ? InfoPanel.SellButton.Button.colors.normalColor : InfoPanel.SellButton.Button.colors.disabledColor;
        InfoPanel.SellText.Text.color = canSell
            ? InfoPanel.SellButton.Button.colors.normalColor
            : InfoPanel.SellButton.Button.colors.disabledColor;
    }

    private static bool CanBuy(CursorUpgrade upgrade) =>
        IsAvailable(upgrade) && InGame.instance.GetCash() >=
        CostHelper.CostForDifficulty(upgrade.Cost, InGame.instance.GetGameModel());

    public static bool IsAvailable(CursorUpgrade upgrade)
    {
        if (upgrade.Path == Path.Clicker && PurchasedUpgrades[Path.Clicker] == UnPurchased)
            return true;
        if(PurchasedUpgrades[Path.Clicker] == UnPurchased)
            return false;

        if (upgrade.Path == Path.Paragon && PurchasedUpgrades[Path.Paragon] != UnPurchased)
            return false;

        if (upgrade.Path == Path.Paragon && PurchasedUpgrades[Path.Paragon] == UnPurchased)
        {
            return Main.Paths.Where(path => path != Path.Paragon).All(path => PurchasedUpgrades[path] >= CursorUpgrade.Cache[path].Values.Max(x => x.Tier));
        }
        
        return (PurchasedUpgrades[upgrade.Path] + 1 == upgrade.Tier && PurchasedUpgrades[Path.Clicker] != UnPurchased) || (PurchasedUpgrades[upgrade.Path] == UnPurchased && upgrade.Tier == 1);
    }
}