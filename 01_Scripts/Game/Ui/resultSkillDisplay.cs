using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager
using Cysharp.Threading.Tasks;

public class resultSkillDisplay : MonoBehaviour
{
    public Image[] traitIcon = new Image[3];

    public Image totalDmgIcon;
    

    public Image skillIcon;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillLevelText;
    public TextMeshProUGUI TotalDmgNumberText;

   

    public Image skillDamageIcon;
    public Image skillCdIcon;
    public Image skillSpdIcon;
    public Image skillSizeIcon;
    public Image skillDurationIcon;
    public Image skillProjectilIcon;

    public TextMeshProUGUI skillDmgText;
    public TextMeshProUGUI skilCdText;
    public TextMeshProUGUI skillSpdText;
    public TextMeshProUGUI skillSizeText;
    public TextMeshProUGUI skillProjectilNumText;
    public TextMeshProUGUI skillDurationText;

     ResultSkillDisplayDataHolder holder;
    
    







    void Start()
    {
        holder = ResultSkillDisplayDataHolder.Instance;

        totalDmgIcon.sprite = holder.totalDmgSprite;

        skillDamageIcon.sprite = holder.skillDamageSprite;
        skillCdIcon.sprite = holder.skillCdSprite;
        skillSpdIcon.sprite = holder.skillSpdSprite;
        skillSizeIcon.sprite = holder.skillSizeSprite;
        skillProjectilIcon.sprite = holder.skillProjectilSprite;


    }

    void Update()
    {
        
    }

    public void SetEmptyValue()
    {
        //skillIcon.sprite = null;
        skillNameText.text = "";
        skillDurationText.text = "";
        skillDmgText.text = "";
        skilCdText.text = "";
        skillSpdText.text = "";
        skillSizeText.text = "";
        skillProjectilNumText.text = "";

        //for (int i = 0; i < traitIcon.Length; i++)
        //{
        //    traitIcon[i].sprite = null;
        //}
    }

    public void SetUpSkillDisplay(SkillCasterBase caster)
    {

        skillIcon.sprite = caster.casterSpriteImage;
        skillNameText.text = L.SkillName(caster.casterIdType);

        //i want text format will be  + scaler% , - scaler% , 0% , depending on the value, and if plus apply holder.plusStyle (TMP_ColorGradient) to color
        
        SetScalerText(skillDurationText, caster.durationScaler);
        SetScalerText(skillDmgText, caster.damageScaler);
        SetScalerText(skilCdText, caster.coolDownFactor,false,true); //invert cd scaler for display
        SetScalerText(skillSpdText, caster.speedScaler);
        SetScalerText(skillSizeText, caster.sizeScaler);
        SetScalerText(skillProjectilNumText, caster.projectileNumScaler,true,false);




        //skillDmgText.text = caster.damageScaler.ToString("F0");
        //skilCdText.text = caster.coolDownFactor.ToString("F0");
        //skillSpdText.text = caster.speedScaler.ToString("F0");
        //skillSizeText.text = caster.sizeScaler.ToString("F0");
        //skillProjectilNumText.text = caster.projectileNumScaler.ToString();

        for (int i = 0; i < traitIcon.Length; i++)
        {
            if (caster.traitDataHoldingList.Count > i)
                if(caster.traitDataHoldingList[i].icon)traitIcon[i].sprite = caster.traitDataHoldingList[i].icon;
            //else
                //traitIcon[i].gameObject.SetActive(false);
        }


    }

    //private void SetScalerText(TextMeshProUGUI text, float scaler, bool isBullet = false, bool isCooldown = false)
    //{
    //    string formatted;
    //    if (scaler > 0)
    //    {
    //        if (isCooldown) formatted = $"-{scaler:F0}%";
    //        else if (isBullet) formatted = formatted = $"+{scaler:F0}";
    //        else formatted = $"+{scaler:F0}%";
            
    //        text.colorGradientPreset = holder.plusStyle;
    //            //text.colorGradientEnabled = true;
    //    }
    //    else if (scaler < 0)
    //    {
    //        if(isCooldown) formatted = formatted = $"+{Mathf.Abs(scaler):F0}%";
    //        else if (isBullet) formatted = $"-{Mathf.Abs(scaler):F0}";
    //        else formatted = $"-{Mathf.Abs(scaler):F0}%";
            
    //        text.colorGradientPreset = holder.minusStyle;
    
    //     }
    //    else
    //    {
    //        if(isBullet) formatted = "0";
    //        formatted = "0%";
            
    //        text.colorGradientPreset = holder.zeroStyle;
    
    //    }
    //    text.text = formatted;
    //}

    private void SetScalerText(TextMeshProUGUI text, float value, bool isBullet = false, bool isCooldown = false)
{
    float finalValue = value;

    // 1. CONVERSION LOGIC
    // If it is a Cooldown, we must convert the Multiplier (0.87) to a Percentage (-13)
    if (isCooldown)
    {
        // Example: (0.87 - 1.0) * 100 = -13
        finalValue = (value - 1.0f) * 100f;
    }

    // 2. ZERO CHECK
    // Use a small epsilon for float comparison to catch 0, 0.0001, etc.
    if (Mathf.Abs(finalValue) < 0.01f) 
    {
        text.text = isBullet ? "0" : "0%";
        text.colorGradientPreset = holder.zeroStyle;
        // text.colorGradientEnabled = true; // Ensure this is enabled in editor or here
        return;
    }

    // 3. COLOR LOGIC
    // Determine if this value represents a "Buff" (Good) or "Debuff" (Bad)
    bool isGoodStat;

    if (isCooldown)
    {
        // For Cooldowns: Negative numbers (Time Reduction) are GOOD
        // Example: -13% is Good. +20% is Bad.
        isGoodStat = finalValue < 0;
    }
    else
    {
        // For Damage/Speed: Positive numbers are GOOD
        // Example: +30% is Good. -10% is Bad.
        isGoodStat = finalValue > 0;
    }

    // Apply the gradient based on whether the stat is Good or Bad
    text.colorGradientPreset = isGoodStat ? holder.plusStyle : holder.minusStyle;

    // 4. TEXT FORMATTING
    string formattedText;

    if (isBullet)
    {
        // Projectiles are integers (e.g., +1, -1)
        // "+0;-0" format forces a plus sign for positive, minus for negative
        formattedText = finalValue.ToString("+0;-0");
    }
    else
    {
        // Percentages (e.g., +30%, -13%)
        formattedText = finalValue.ToString("+0;-0") + "%";
    }

    text.text = formattedText;
}

}

