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

public class ApplyPinchBuffEffect : UpgradeEffectBase
{
    [System.Serializable]
    public struct BuffAmount
    {
        [Tooltip("1層分の効果量")]public float oneAmount;
        [Tooltip("適用される効果量")]public float finalAmount;
        [Tooltip("最大効果量")]public float amountMax;

        public float CalcFinalAmount(int stuckNum)
        {
            finalAmount = oneAmount * stuckNum;
            if(finalAmount >= amountMax)
            {
                finalAmount = amountMax;
            }

            return finalAmount;
        }
    }

    [SerializeField, Header("発動時の防御力増加量")]
    private float increaseDeffenceAmount;

    [Header("受けたダメージ量によるバフ効果量の計算")]
    [SerializeField, Tooltip("必要なダメージ量の間隔")]
    private float intervalDamage = 0;
    [SerializeField,Header("サイズ増加量の計算")]
    private BuffAmount increaseSize;
    [SerializeField,Header("クリティカル率増加の計算")]
    private BuffAmount increaseCritChance;
    [SerializeField,Header("発動回数")]
    private int activedStuckNum = 0;         // 何層分発動しているか

    public float durationCounter = 0;
    public float activateHpAmount = 0;      // バフが発動するHP量
    private PlayerState playerState;         // HP確認用のPlayerState

    public override void EffectUpdate()
    {
        if(durationCounter <= 0) { return; }

        durationCounter -= Time.deltaTime;
        if (durationCounter <= 0)
        {
            // 継続時間が切れたので効果量をリセットする
            buffManagerInst.gobalPlayerDefenceAdd -= increaseDeffenceAmount;
            
            isActived = false;
        }
    }

    public override void CanEnableBuff()
    {
        buffManagerInst = BuffManager.Instance;
        isEnable = buffManagerInst.isApplyPinchBuffEnabled;

        if (isEnable == true) 
        {
            ActiveBuffManager.Instance.SetStacks(
                TraitType.OwlSkill1_EmergencyBarrier, 0);
        }

        playerState = 
            GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerState>();
        increaseSize.finalAmount = 0;
        increaseCritChance.finalAmount = 0;
        activedStuckNum = 0;
        durationCounter = 0;
        isActived = false;

        if(playerState != null)
        {
            activateHpAmount = playerState.MaxHp / 3;
        }
#if UNITY_EDITOR
        else
        {
            Debug.Log("PlayerStateがNULLです");
        }
#endif
    }

    public override bool ActiveBuff()
    {
        if (playerState == null || activateHpAmount == 0){ return false; }
        
        // 現在のHPが発動するHP以下になったらバフを獲得する
        if(isActived == false &&
            playerState.NowHp <= activateHpAmount)
        {
            isActived = true;

            buffManagerInst.gobalPlayerDefenceAdd += increaseDeffenceAmount;
            durationCounter = activeDuration;
        }

        // 発動しているならバフの効果量を再計算する
        if(isActived == true)
        {
            // 前回のバフへの追加量をリセットする
            buffManagerInst.gobalSizeAdd -= increaseSize.finalAmount;
            buffManagerInst.gobalCritChanceAdd -=increaseCritChance.finalAmount;
            
            // 今回の効果量を計算して追加する
            activedStuckNum = (int)(PlayerDataManager.Instance.totalDamage / intervalDamage);
            buffManagerInst.gobalSizeAdd += 
                increaseSize.CalcFinalAmount(activedStuckNum);
            buffManagerInst.gobalCritChanceAdd += 
                increaseCritChance.CalcFinalAmount(activedStuckNum);

            ActiveBuffManager.Instance.SetStacks(
                TraitType.OwlSkill1_EmergencyBarrier, activedStuckNum);
        }
        
        return isActived;
    }
}

