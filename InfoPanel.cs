using System;
using BTD_Mod_Helper.Api.Components;
using UnityEngine;

namespace BloonsClicker;

[RegisterTypeInIl2Cpp(false)]
public class InfoPanel : MonoBehaviour
{
    public InfoPanel(IntPtr ptr) : base(ptr)
    {
    }
    
    public ModHelperPanel panel { get; set; } = null!;
    public ModHelperText NameText { get; set; } = null!;
    public ModHelperText CostText { get; set; } = null!;
    public ModHelperImage IconImage { get; set; } = null!;
    public ModHelperText DescriptionText { get; set; } = null!;
    public ModHelperText CashText { get; set; } = null!;
    public ModHelperButton BuyButton { get; set; } = null!;
    public ModHelperText PurchaseText { get; set; } = null!;
    public ModHelperText PopsText { get; set; } = null!;
    public ModHelperButton SellButton { get; set; } = null!;
    public ModHelperText SellText { get; set; } = null!;
}