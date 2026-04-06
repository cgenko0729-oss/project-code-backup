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

public class PetBeeAction : ActivePetActionBase
{

    #region 固有能力---------------------------------------

    [Header("ダメージ倍率")]
    [Tooltip("ダメージ倍率を設定します\n(1.0で通常ダメージ、2.0で2倍ダメージ)")]
    public float damageMultiplier = 5.0f;

    #endregion --------------------------------------------

    //攻撃メゾット
    public override void PerformAttack()
    {
        PetAttackAction();
    }

    protected override void PetAttackAction()
    {
        base.PetAttackAction();
    }

    protected override void OnHitEnemy(EnemyStatusBase enemyStat)
    {
        //鈍足デバフを付与
        if (enemyStat != null)
        {
            //最終ダメージ
            float finalDamage = takeDamages;
        }
    }

    protected override void OnTakeDamegesEnemy(EnemyStatusBase enemyStat)
    {
        //最終ダメージ
        float finalDamage = takeDamages;

        //敵が毒デバフ状態ならダメージアップ
        if (enemyStat.isPoisonDebuff)
        {
            finalDamage *= damageMultiplier;
        }

        //敵にダメージを与える
        FinalDanages(enemyStat, finalDamage);
    }
}

