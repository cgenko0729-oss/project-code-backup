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

public class ApplyBuffHealthScaling : UpgradeEffectBase
{
    [Header("ダメージ増加量")]
    [Tooltip("現在のダメージ増加量")]public float damageAmount = 0;
    [Tooltip("ダメージ増加の最大量")] public float damageMaxAmount = 0;
    
    [Header("移動スピード増加量")]
    [Tooltip("現在の移動スピ―ド増加量")]public float speedAmount = 0;
    [Tooltip("移動スピ―ド増加の最大量")] public float speedMaxAmount = 0;

    private PlayerState playerState;

    public override void CanEnableBuff()
    {
        isEnable = BuffManager.Instance.isApplyBuffHealthScalilngEnabled;

        if(isEnable)
        {
            ActiveBuffManager.Instance.SetStacks(
                TraitType.LionSkill1_WarriorPride, 0);
        }

        damageAmount = 0;
        speedAmount = 0;
    }

    public override bool ActiveBuff()
    {
        if (playerState == null)
        {
            // PlayerStateを取得する
            playerState =
                GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerState>();
            
            // 取得を行ってもPlayerStateがない場合は処理を行わない
            if (playerState == null) { return false; }
        }

        // 前回のバフの効果量分減らす
        //BuffManager.Instance.gobalDamageAdd -= damageAmount;
        //BuffManager.Instance.gobalMoveSpeed -= speedAmount;
        //
        //// バフの獲得量を計算する
        //float progress = playerState.NowHp / playerState.MaxHp;
        //damageAmount = damageMaxAmount * progress;
        //speedAmount = speedMaxAmount * progress;
        //
        //// 今回のバフの効果量を追加する
        //BuffManager.Instance.gobalDamageAdd += damageAmount;
        //BuffManager.Instance.gobalMoveSpeed += speedAmount;

        return true;
    }
}

