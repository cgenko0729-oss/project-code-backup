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

public class PetSnakeNormalAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("鈍足デバフの量")]
    public float debuffSpeedDownAmount = 0.8f;

    [Header("鈍足デバフの時間")]
    public float debuffSpeedDownTime = 3.0f;

    [Header("鈍足デバフになる確率")]
    public int debuffSpeedDownChance = 30;

    #endregion --------------------------------------------

    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();

    protected override void OnHitEnemy(EnemyStatusBase enemyStat)
    {
        //鈍足デバフを付与
        int randomValue = Random.Range(0, 100); // 1から100までのランダムな整数を生成
        if (randomValue <= debuffSpeedDownChance)
        {
            enemyStat.ApplySpeedDownDebuff(debuffSpeedDownAmount, debuffSpeedDownTime);
        }
    }
}

