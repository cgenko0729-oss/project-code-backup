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

public class ResultSkillDisplayDataHolder : Singleton<ResultSkillDisplayDataHolder>
{
    public resultSkillDisplay[] skillDisplays = new resultSkillDisplay[4];

    public Sprite totalDmgSprite;

    public Sprite skillDamageSprite;
    public Sprite skillCdSprite;
    public Sprite skillSpdSprite;
    public Sprite skillSizeSprite;
    public Sprite skillDurationSprite;
    public Sprite skillProjectilSprite;

    public TMP_ColorGradient plusStyle;
    public TMP_ColorGradient minusStyle;
    public TMP_ColorGradient zeroStyle;

    public int detailedPageNum = 1;

    

    public void SetAllSkillDisplays()
    {
        for (int i = 0; i < skillDisplays.Length; i++)
        {
            //if (SkillManager.Instance.activeSkillCastersHolder.Count >= i) skillDisplays[i].SetUpSkillDisplay(SkillManager.Instance.activeSkillCastersHolder[i]);
            //else skillDisplays[i].SetEmptyValue();

            if (i < SkillManager.Instance.activeSkillCastersHolder.Count)
                skillDisplays[i].SetUpSkillDisplay(SkillManager.Instance.activeSkillCastersHolder[i]);
            else
                skillDisplays[i].SetEmptyValue();
        }

        

    }

    public void ToNextDetailPage(bool isPrevious)
    {
        if (isPrevious)
        {
            ResultSkillDisplayDataHolder.Instance.detailedPageNum--;
            if (ResultSkillDisplayDataHolder.Instance.detailedPageNum <= 0) ResultSkillDisplayDataHolder.Instance.detailedPageNum = 3;
        }
        else
        {
            ResultSkillDisplayDataHolder.Instance.detailedPageNum++;
            if (ResultSkillDisplayDataHolder.Instance.detailedPageNum >= 4) ResultSkillDisplayDataHolder.Instance.detailedPageNum = 1;
        }

        EventManager.EmitEvent("DetailPageChanged");
    }

}

