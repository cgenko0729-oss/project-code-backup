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

public class IncreaseDamagePerAttackEffect : UpgradeEffectBase
{
    [SerializeField, Header("一度に追加される効果量")]
    [Tooltip("ダメージ増加量")] public float increaseDamageAmount = 0;
    
    [SerializeField, Header("発動回数")]
    private int activeNumMax;

    private List<float> stuckItemDurations = new List<float>();

    public override void EffectUpdate()
    {
        // スタック1つ分の継続時間を更新する
        for (int i = 0; i < stuckItemDurations.Count; i++)
        {
            stuckItemDurations[i] -= Time.deltaTime;

            // スタック１つ分の継続時間が切れたら効果量を１つ分戻す
            if (stuckItemDurations[i] <= 0)
            {
                BuffManager.Instance.gobalDamageAdd -= increaseDamageAmount;
                stuckItemDurations.RemoveAt(i);
                ActiveBuffManager.Instance.ReduceStack(TraitType.DogSkill3_DogNail);
            }
        }
    }

    public override void CanEnableBuff()
    {
        isEnable = BuffManager.Instance.isIncreaseDamagePerAttackEnabled;

        if (isEnable == true)
        {
            ActiveBuffManager.Instance.SetStacks(TraitType.DogSkill3_DogNail, 0);
        }
    }

    public override bool ActiveBuff()
    {
        // 最大発動回数に達していなければスタックを追加
        if (stuckItemDurations.Count < activeNumMax)
        {
            BuffManager.Instance.gobalDamageAdd += increaseDamageAmount;
            stuckItemDurations.Add(activeDuration);
            ActiveBuffManager.Instance.AddStack(TraitType.DogSkill3_DogNail);
            return true;
        }
        return false;
    }
}

