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

public class PetSpiderEliteAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("ボーナスに必要な数")]
    [SerializeField] private int requiredCount = 3;

    [Header("絆ボーナス倍率")]
    [SerializeField] private float bondBonusMultiplier = 1.5f;

    #endregion --------------------------------------------

    protected override void Start()
    {
        base.Start();

        int count = ActivePetManager.Instance.CountPetsByRace(PetRace.Insect);

        //カウントが3以上ならエフェクト付ける
        if (count >= requiredCount)
        {
            //爆発前エフェクト位置調整
            Vector3 EffectPos = ItemManager.Instance.defaultEffectPos + new Vector3(0, 0.5f, 0);

            //爆発前エフェクト再生
            ItemManager.Instance.PlayTimeParticleInObject(this.gameObject,
                                                          ItemManager.Instance.itemKeepTimePs.ExplodeBeforeLightPs,
                                                          EffectPos);
        }
    }

    public override void PerformAttack() => PetAttackAction();
    protected override void PetAttackAction() => base.PetAttackAction();

    protected override void OnTakeDamegesEnemy(EnemyStatusBase enemyStat)
    {
        //仲間の蜘蛛の数を調べる
        int count = ActivePetManager.Instance.CountPetsByRace(PetRace.Insect);

        //仲間の蜘蛛の数分の倍率アップ
        float currentRate = count;

        //絆ボーナス初期化
        float currentBondBonus = 1f;

        //カウントが3以上ならさらにボーナス
        if (count >= requiredCount)
        {
            //1.5倍ボーナス
            currentBondBonus = bondBonusMultiplier;
        }

        //最終ダメージ計算
        float finalDamage = takeDamages * currentRate * currentBondBonus;

        //敵にダメージを与える
        FinalDanages(enemyStat, finalDamage);
    }
}

