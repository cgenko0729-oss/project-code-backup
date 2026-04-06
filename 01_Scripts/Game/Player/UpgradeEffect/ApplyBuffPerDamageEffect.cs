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

public class ApplyBuffPerDamageEffect : UpgradeEffectBase
{
    [Header("一度に追加される効果量")]
    [SerializeField,Tooltip("幸運値増加量")]
    private float increaseLuckAmount = 0;
    [SerializeField, Tooltip("クリティカル率増加量")]
    private float increaseCritAmount = 0;

    [Header("発動回数")]
    [SerializeField, Tooltip("最大発動回数")] 
    private int activeNumMax = 0;
    [SerializeField, Tooltip("現在の発動回数")]
    private int isActivedNum = 0;

    public override void CanEnableBuff()
    {
        isEnable = BuffManager.Instance.isApplyBuffPerDamageEnabled;

        if (isEnable)
        {
            ActiveBuffManager.Instance.SetStacks(
                TraitType.LionSkill2_Berserker, 0);
        }

        isActivedNum = 0;
    }

    public override bool ActiveBuff()
    {
        // 最大発動回数に達していなければスタックを追加
        if (isActivedNum < activeNumMax)
        {
            BuffManager.Instance.gobalLuckAdd += increaseLuckAmount;
            BuffManager.Instance.gobalCritChanceAdd += increaseCritAmount;
            isActivedNum++;

            ActiveBuffManager.Instance.SetStacks(
                TraitType.LionSkill2_Berserker, isActivedNum);

            return true;
        }
        return false;
    }
}

