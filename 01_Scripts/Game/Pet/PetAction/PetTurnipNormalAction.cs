using Cysharp.Threading.Tasks;
using DG.Tweening;
using Hellmade.Sound; //SoundManager
using MonsterLove.StateMachine; //StateMachine
using NUnit.Framework;
using QFSW.MOP2;                //Object Pool
using System.Collections.Generic;
using TigerForge;               //EventManager
using TMPro;    
using UnityEngine;
using UnityEngine.UI;

public class PetTurnipNormalAction : ActivePetActionBase, IPetActiveSkill
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("回復量(%)")]
    public float healAmount = 0f;

    #endregion --------------------------------------------

    protected override void Start()
    {
        base.Start();

        SetCoolDown();
    }

    public void PetActiveSkill()
    {
        //もしpetdataのアクティブスキルのクールタイムが変更されていたら、スキルを発動できないようにする
        if (petData.activeSkillRemainingCooldown == ResetCoolTime)
        {
            //アクティブスキルの効果発動
            ActiveSkillAction();

            SoundEffect.Instance.Play(SoundList.DebugkeySe);

            SoundEffect.Instance.Play(skillEffectSound);

            //一斉攻撃を開始
            EventManager.EmitEvent(GameEvent.AllAttackStart);

            //クールタイムリセット
            ResetCoolDown();

            ItemManager.Instance.PlayTimeParticleInPlayer
                (ItemManager.Instance.itemKeepTimePs.petHealPs, petData.activeSkillDuration, ItemManager.Instance.defaultEffectPos);

        }
        else
        {
            SoundEffect.Instance.Play(SoundList.ButtonCancelSe);

            return;
        }


    }

    protected override void Update()
    {
        base.Update();

        ChangeCoolDown();
    }

    protected override void ActiveSkillAction()
    {
        //回復量を計算する(割合)
        float heal = ItemManager.Instance.getPlayerNowHP * (healAmount / 100);

        //プレイヤーのHPを回復させる
        EventManager.EmitEventData("ChangePlayerHp", heal);
    }

    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();
}

