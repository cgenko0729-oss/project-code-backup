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

public class ApplyBuffPerCoinEffect : UpgradeEffectBase
{
    [SerializeField, Header("一度に追加される効果量")]
    [Tooltip("ダメージ増加量")]public float increaseDamageAmount = 0;
    [Tooltip("防御力増加量")]public float increaseDefenceAmount = 0;

    [SerializeField,Header("発動回数")]
    private int activeNumMax;

    private List<float> stuckItemDurations = new List<float>();

    public override void EffectUpdate()
    {
        // スタック1つ分の継続時間を更新する
        for (int i = 0; i < stuckItemDurations.Count; i++) 
        {
            stuckItemDurations[i] -= Time.deltaTime;

            // スタック１つ分の継続時間が切れたら効果量を１つ分戻す
            if(stuckItemDurations[i] <= 0 )
            {
                stuckItemDurations.RemoveAt(i);
                BuffManager.Instance.gobalPlayerDefenceAdd -= increaseDefenceAmount;
                BuffManager.Instance.gobalDamageAdd -= increaseDamageAmount;
                ActiveBuffManager.Instance.ReduceStack(TraitType.DogSkill1_DefenceMind);
            }
        }
    }

    public override void CanEnableBuff()
    {
        isEnable =  BuffManager.Instance.isApplyBuffPerCoinEnabled;

        if (isEnable == true)
        {
            ActiveBuffManager.Instance.SetStacks(TraitType.DogSkill1_DefenceMind, 0);
        }
    }
    
    public override bool ActiveBuff()
    {
        if (stuckItemDurations.Count < activeNumMax)
        {
            /*スタックの追加*/
            BuffManager.Instance.gobalPlayerDefenceAdd += increaseDefenceAmount;
            BuffManager.Instance.gobalDamageAdd += increaseDamageAmount;
            stuckItemDurations.Add(activeDuration);
            ActiveBuffManager.Instance.AddStack(TraitType.DogSkill1_DefenceMind);
            return true;
        }
        return false;
    }
}

