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



public class ApplyBuffSpeedScalingEffect : UpgradeEffectBase
{
    [System.Serializable]
    public struct BuffActiveData
    {
        [Tooltip("１回分の発動に必要な移動スピード量")] public float intervalMoveSpeed;
        [Tooltip("1回分の効果量")] public float oneAmount;
        [Tooltip("最終的に適用される効果量")] public float finalAmount;
        [Tooltip("効果の最大量")] public float amountMax;
        public int stuckNum;

        public void CalcFinalAmount(float finalMoveSpeed)
        {
            stuckNum = (int)(finalMoveSpeed / intervalMoveSpeed);
            finalAmount = oneAmount * stuckNum;
            if(finalAmount >= amountMax)
            {
                finalAmount = amountMax;
            }
        }
    }

    [Header("ダメージ増加量の計算")]
    public BuffActiveData increaseDamage;
    [Header("スキルクールダウン減少量の計算")]
    public BuffActiveData decreaseSkillCooldown;
    [Header("発動間隔")] 
    public float activeInterval = 0;

    private PlayerState playerState;
    private float intervalTimer = 0;

    private void Start()
    {
        intervalTimer = activeInterval;
    }

    public override void EffectUpdate()
    {
        intervalTimer -= Time.deltaTime;
        if(intervalTimer <= 0)
        {
            intervalTimer = activeInterval;

            if (playerState != null)
            {
                // PlayerStateを取得する
                playerState =
                    GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerState>();
            }
            if (playerState == null) { return; }

            // 今までのバフ効果量を減らす
            BuffManager.Instance.gobalDamageAdd -= increaseDamage.finalAmount;
            BuffManager.Instance.gobalCooldownAdd -= decreaseSkillCooldown.finalAmount;

            // バフの効果量を計算する
            float finalMoveSpeed = playerState.FinalMoveSpeed;

            // ダメージ増加量の計算
            increaseDamage.CalcFinalAmount(finalMoveSpeed);
            // スキルクールダウン減少量の計算
            decreaseSkillCooldown.CalcFinalAmount(finalMoveSpeed);

            // バフ効果量を適用する
            BuffManager.Instance.gobalDamageAdd += increaseDamage.finalAmount;
            BuffManager.Instance.gobalCooldownAdd += decreaseSkillCooldown.finalAmount;

            ActiveBuffManager.Instance.SetStacks
                (TraitType.RabbitSkill3_ArcherSpeed, increaseDamage.stuckNum);
        }
    }

    public override void CanEnableBuff()
    {
        isEnable = BuffManager.Instance.isApplyBuffSpeedScalingEnabled;

        if (isEnable == true)
        {
            ActiveBuffManager.Instance.SetStacks(TraitType.RabbitSkill3_ArcherSpeed, 0);
        }
    }
}

