using Cysharp.Threading.Tasks;
using DG.Tweening;
using Hellmade.Sound; //SoundManager
using MonsterLove.StateMachine; //StateMachine
using QFSW.MOP2;                //Object Pool
using System.Collections.Generic;
using TigerForge;               //EventManager
using TMPro;    
using UnityEngine;
using UnityEngine.UI;     
using UnityEngine.VFX;



public class ApplyBuffProjNumScalingEffect : UpgradeEffectBase
{
    [System.Serializable]
    public struct BuffActiveData
    {
        [Tooltip("1回分の効果量")] public float oneAmount;
        [Tooltip("最終的に適用される効果量")] public float finalAmount;
        [Tooltip("効果の最大量")] public float amountMax;

        public void CalcFinalAmount(int finalProjNum)
        {
            finalAmount = oneAmount * finalProjNum;
            if(finalAmount >= amountMax)
            {
                finalAmount = amountMax;
            }
        }
    }

    [Header("サイズ増加量の計算")]
    public BuffActiveData increaseSize;
    [Header("クリティカル率増加量の計算")]
    public BuffActiveData increaseCrit;
    [Header("発動間隔")] 
    public float activeInterval = 0;

    private SkillCasterBase skillCasterBase;
    private float intervalTimer = 0;
    private float durationTimer = 0;

    private void Start()
    {
        intervalTimer = activeInterval;
        isActived = false;
    }

    public override void EffectUpdate()
    {
        // 発動間隔の更新
        intervalTimer -= Time.deltaTime;
        if(intervalTimer <= 0)
        {
            intervalTimer = activeInterval;
            durationTimer = activeDuration;
            isActived = true;

            // まだスキルキャスターの取得ができていなければ取得を行う
            if(skillCasterBase == null)
            {
                skillCasterBase =
                    SkillManager.Instance.activeSkillCastersHolder.
                    Find(caster => caster.casterIdType == SkillIdType.arrow);
            
                // アクティブ状態のSkillCaster一覧から「Arrow」を
                // 探しても見つからなければ処理を行わない
                if(skillCasterBase == null) { return; }
            }

            int projNum = skillCasterBase.projectileNumFinal;
            // 効果量の適用
            ApplyBuffAmount(projNum);
        }

        // 継続時間の更新
        if(isActived == true)
        {
            durationTimer -= Time.deltaTime;
            if(durationTimer <= 0)
            {
                durationTimer = 0;
                isActived = false;

                // 効果量をリセットする
                ApplyBuffAmount(0);
            }
        }
    }

    // 効果量をBuffManagerに適用する
    private void ApplyBuffAmount(int projNum)
    {
        // 今までのバフ効果量を減らす
        BuffManager.Instance.gobalSizeAdd -= increaseSize.finalAmount;
        BuffManager.Instance.gobalCritChanceAdd -= increaseCrit.finalAmount;

        // サイズ増加量の計算
        increaseSize.CalcFinalAmount(projNum);
        // クリティカル率増加量の計算
        increaseCrit.CalcFinalAmount(projNum);

        // バフ効果量を適用する
        BuffManager.Instance.gobalSizeAdd += increaseSize.finalAmount;
        BuffManager.Instance.gobalCritChanceAdd += increaseCrit.finalAmount;

        ActiveBuffManager.Instance.SetStacks(
            TraitType.RabbitSkill1_ArcherSpread, projNum);
    }

    public override void CanEnableBuff()
    {
        isEnable = BuffManager.Instance.isApplyBuffProjNumScalingEnabled;

        if (isEnable == true)
        {
            ActiveBuffManager.Instance.SetStacks(TraitType.RabbitSkill1_ArcherSpread, 0);
        }
    }

    public override bool ActiveBuff()
    {
        return false;
    }
}

