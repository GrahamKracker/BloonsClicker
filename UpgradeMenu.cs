using System.Collections.Generic;
using System.Globalization;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Helpers;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Settings;
using Il2CppTMPro;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace BloonsClicker;

public class UpgradeMenu : ModGameMenu<HotkeysScreen>
{
    private static ModHelperScrollPanel MainPanel;
    private static ModHelperPanel InfoPanel;
    
    private static ModHelperButton? BuyButton;
    private static ModHelperText CostText;
    private static ModHelperText DescriptionText;
    private static ModHelperText NameText;
    private static ModHelperImage IconImage;
    private static ModHelperText CashText;
    private static ModHelperText PopsText;
    
    private CanvasGroup canvasGroup;
    private Animator _animator;
    private static CursorUpgrade _currentUpgrade;
    
    public override void OnMenuUpdate()
    {
        if (Closing)
        {
            canvasGroup.alpha -= .07f;
        }
        else if ((Input.GetKeyDown(KeyCode.Comma) || Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.Slash)) && BuyButton is not null)
        {
            BuyButton.Button.onClick.Invoke();
        }
    }
    public override void OnMenuClosed()
    {
        _animator.Play("PopupSlideOut");
        upgradeButtons.Clear();
    }
    
    public override bool OnMenuOpened(Object data)
    {
        var gameObject = GameMenu.gameObject;
        gameObject.DestroyAllChildren();
        GameMenu.saved = true;

        var basePanel = gameObject.AddModHelperPanel(new Info("BasePanel", InfoPreset.FillParent));
        
        MainPanel = basePanel.AddScrollPanel(new Info("UpgradesPanel",3600, 1700, new Vector2(.5f, .6f)), RectTransform.Axis.Vertical, VanillaSprites.BlueInsertPanel, 150, 150);
        MainPanel.Mask.showMaskGraphic = false;
        
        InfoPanel = basePanel.AddPanel(new Info("InfoPanel", 2600, 600, new Vector2(.5f, .175f)), VanillaSprites.MainBgPanel, RectTransform.Axis.Horizontal, 150, 150);
        
        _animator = basePanel.AddComponent<Animator>();
        _animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        _animator.runtimeAnimatorController = Animations.PopupAnim;
        _animator.speed = .75f;
        _animator.Rebind();
        
        canvasGroup = basePanel.AddComponent<CanvasGroup>();
        
        CreateInfoPanel();
        
        CreateUpgrades();
        
        CommonForegroundScreen.instance.Hide();
        CommonForegroundScreen.instance.Show(true, "Clicker Upgrades", false, false, false, false, false, false);
        
        foreach(var animator in basePanel.GetComponentsInChildren<Animator>())
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        
        var firstBuyable = upgradeButtons.OrderBy(x=>x.Key.Tier).FirstOrDefault(currentUpgrade=>!purchasedDict[currentUpgrade.Key]).Value;
        
        if(firstBuyable == null)
            return true;
        
        firstBuyable.transform.FindChild("PurchasedCheckmark").gameObject.SetActive(false);
        firstBuyable.transform.FindChild("Locked").gameObject.SetActive(false);

        return true;
    }

    private static void CreateInfoPanel()
    {
        var nameIconPanel = InfoPanel.AddPanel(new Info("NameIconPanel",500), VanillaSprites.MainBgPanel, RectTransform.Axis.Vertical, 50, 50);
        nameIconPanel.Background.color = new Color(0.3f, 0.05f, 0f, 0.5f);
        
        NameText = nameIconPanel.AddText(new Info("NameText", 500, 50), "");     
        
        IconImage = nameIconPanel.AddImage(new Info("IconImage", 300), "");
        
        
        var descriptionPanel = InfoPanel.AddPanel(new Info("DescriptionPanel", 1000, 350), VanillaSprites.MainBgPanel, null);
        descriptionPanel.Background.color = new Color(0.3f, 0.05f, 0f, 0.5f);

        DescriptionText = descriptionPanel.AddText(new Info("DescriptionText", 900, 350), "");
        
        var buyPanel = InfoPanel.AddPanel(new Info("BuyPanel", 500), VanillaSprites.MainBgPanel, RectTransform.Axis.Vertical, 25, 50);
        
        buyPanel.Background.color = new Color(0.3f, 0.05f, 0f, 0.5f);
        
        var popsPanel = buyPanel.AddPanel(new Info("PopsPanel"){
            Width = 500,
            Height = 50,
        }, null);
        
        popsPanel.AddImage(new Info("PopsIcon", -150, 0, 50, 50), VanillaSprites.PopIcon);
        
        PopsText = popsPanel.AddText(new Info("PopsText",125, 0, 450, 50), $"{Main.CursorPops:n0}");
        PopsText.Text.alignment = TextAlignmentOptions.MidlineLeft;
        
        BuyButton = buyPanel.AddButton(new Info("BuyButton")
        {
            FlexWidth = 1,
            Height = 250,
        }, VanillaSprites.GreenBtnLong, new System.Action(
            () =>
            {
                var cost = CostHelper.CostForDifficulty(_currentUpgrade.Cost, InGame.instance.GetGameModel());
                if(InGame.instance.GetCash() < cost || purchasedDict[_currentUpgrade]) 
                    return;
                InGame.instance.GetSimulation().cashManagers._entries[0].value.cash.Value -= cost;
                InGame.instance.bridge.OnCashChangedSim();
                UpdateCash();
                Main.CurrentUpgrade = _currentUpgrade;
                purchasedDict[_currentUpgrade] = true;
                var sortedUpgrades = GetContent<CursorUpgrade>().OrderBy(x => x.Tier).ToArray();
                var index = System.Array.IndexOf(sortedUpgrades, _currentUpgrade)+1;
                
                foreach (var cursorUpgrade in sortedUpgrades.Where(x=>x.Tier <= _currentUpgrade.Tier))
                {
                    upgradeButtons[cursorUpgrade].transform.FindChild("PurchasedCheckmark").gameObject.SetActive(true);
                    upgradeButtons[cursorUpgrade].transform.FindChild("Locked").gameObject.SetActive(false);
                }
                foreach (var cursorUpgrade in sortedUpgrades.Where(x=>x.Tier > _currentUpgrade.Tier))
                {
                    upgradeButtons[cursorUpgrade].transform.FindChild("PurchasedCheckmark").gameObject.SetActive(false);
                    upgradeButtons[cursorUpgrade].transform.FindChild("Locked").gameObject.SetActive(true);
                }

                if (index >= sortedUpgrades.Length)
                {
                    UpdateSeletionPanel(sortedUpgrades[^1]);
                    return;
                }

                upgradeButtons[sortedUpgrades[index]].transform.FindChild("PurchasedCheckmark").gameObject.SetActive(false);
                upgradeButtons[sortedUpgrades[index]].transform.FindChild("Locked").gameObject.SetActive(false);
                
                UpdateSeletionPanel(sortedUpgrades[index]);
            }));
        
        BuyButton.AddText(new Info("PurchaseText", 500, 250, new Vector2(.5f, .6f)), "Purchase", 70);
                CostText = BuyButton.AddText(new Info("CostText", 500, 50, new Vector2(.5f, .35f)), "");

        var cashPanel = buyPanel.AddPanel(new Info("CashPanel")
        {
            Width = 500,
            Height = 50,
        }, null);
        
        cashPanel.AddImage(new Info("CashIcon", -150, 0, 50, 50), VanillaSprites.CoinIcon);
        
        CashText = cashPanel.AddText(new Info("CashText",125, 0, 450, 50), $"{InGame.instance.GetSimulation()?.cashManagers?._entries[0]?.value?.cash?.Value:n0}");
        CashText.Text.alignment = TextAlignmentOptions.MidlineLeft;
    }

    private static void UpdateCash()
    {
        CashText.SetText($"{InGame.instance.GetSimulation()?.cashManagers?._entries[0]?.value?.cash?.Value:n0}");
    }

    private static Dictionary<CursorUpgrade, ModHelperButton> upgradeButtons = new();
    internal static Dictionary<CursorUpgrade, bool> purchasedDict = new();
    private static void CreateUpgrades()
    {
        var sortedUpgrades = GetContent<CursorUpgrade>().OrderBy(x => x.Tier).ToList();
        foreach (var cursorUpgrade in sortedUpgrades)
        {
            var button = cursorUpgrade.CreateButton(MainPanel);
            button.Button.onClick.AddListener(() =>
            {
                UpdateSeletionPanel(cursorUpgrade);
            });
            MainPanel.AddScrollContent(button);
            upgradeButtons[cursorUpgrade] = button;
            purchasedDict.TryAdd(cursorUpgrade, false);
            
            button.transform.FindChild("PurchasedCheckmark").gameObject.SetActive(false);
            button.transform.FindChild("Locked").gameObject.SetActive(false);
        }

        var nextUpgrade = sortedUpgrades.FirstOrDefault(upgrade => !purchasedDict[upgrade]) ?? sortedUpgrades.Last();
        
        foreach (var cursorUpgrade in sortedUpgrades.Where(x=>x.Tier > nextUpgrade.Tier))
        {
            upgradeButtons[cursorUpgrade].transform.FindChild("PurchasedCheckmark").gameObject.SetActive(false);
            upgradeButtons[cursorUpgrade].transform.FindChild("Locked").gameObject.SetActive(true);
        }
        foreach (var cursorUpgrade in sortedUpgrades.Where(x=>x.Tier < nextUpgrade.Tier))
        {
            upgradeButtons[cursorUpgrade].transform.FindChild("PurchasedCheckmark").gameObject.SetActive(true);
            upgradeButtons[cursorUpgrade].transform.FindChild("Locked").gameObject.SetActive(false);
        }

        if(sortedUpgrades.FirstOrDefault(upgrade => !purchasedDict[upgrade]) == null)
            upgradeButtons[nextUpgrade].transform.FindChild("PurchasedCheckmark").gameObject.SetActive(true);

        UpdateSeletionPanel(nextUpgrade);
    }
    
    
    
    private static void UpdateSeletionPanel(CursorUpgrade upgrade)
    {
        NameText.SetText(upgrade.DisplayName);
        IconImage.Image.SetSprite(upgrade.IconReference);
        DescriptionText.SetText(upgrade.Description);
        CostText.SetText($"${CostHelper.CostForDifficulty(upgrade.Cost, InGame.instance.GetGameModel()):n0}");
        _currentUpgrade = upgrade;
        var selectedButton = upgradeButtons[upgrade];
        foreach (var button in upgradeButtons.Values)
        {
            button.transform.FindChild("SelectedGlow").gameObject.SetActive(false);
        }
        selectedButton.transform.FindChild("SelectedGlow").gameObject.SetActive(true);

        var canBuy = purchasedDict.FirstOrDefault(x => x.Value == false).Key == upgrade && InGame.instance.GetSimulation()?.cashManagers?._entries[0]?.value?.cash?.Value >= CostHelper.CostForDifficulty(upgrade.Cost, InGame.instance.GetGameModel()); 
        
        BuyButton.Button.interactable = canBuy;
        BuyButton.Image.color = canBuy ? BuyButton.Button.colors.normalColor : BuyButton.Button.colors.disabledColor;
        BuyButton.transform.FindChild("PurchaseText").GetComponent<ModHelperText>().Text.color = canBuy ? BuyButton.Button.colors.normalColor : BuyButton.Button.colors.disabledColor;
        CostText.Text.color = canBuy ? BuyButton.Button.colors.normalColor : BuyButton.Button.colors.disabledColor;
        
        
    }
}