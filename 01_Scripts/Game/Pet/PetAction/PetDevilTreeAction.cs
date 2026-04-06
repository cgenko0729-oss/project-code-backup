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

public class PetDevilTreeAction : ActivePetActionBase, IPetActiveSkill
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("軽減率(%)")]
    public float damageReduction = 0.0f;

    #endregion --------------------------------------------
    protected override void Start()
    {
        base.Start();

        SetCoolDown();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.StartListening(GameEvent.AllAttackStart, CloneActiveSkill);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.StopListening(GameEvent.AllAttackStart, CloneActiveSkill);
    }

    public void PetActiveSkill()
    {
        //もしpetdataのアクティブスキルのクールタイムが変更されていたら、スキルを発動できないようにする
        if (petData.activeSkillRemainingCooldown == ResetCoolTime)
        {
            //アクティブスキルを使用中にする
            skillActive = true;

            float finalActiveSkillDuration = petData.activeSkillDuration * ActivePetManager.Instance.PetSkillDuration;

            //持続時間をリセット
            activeSkillDurationTime = finalActiveSkillDuration;

            //アクティブスキルの効果発動
            ActiveSkillAction();

            //一斉攻撃を開始
            EventManager.EmitEvent(GameEvent.AllAttackStart);

            SoundEffect.Instance.Play(SoundList.DebugkeySe);

            SoundEffect.Instance.Play(skillEffectSound);

            //クールタイムリセット
            ResetCoolDown();

            Vector3 effectPos = new Vector3(0.0f, 0.8f, 0.0f);

            ItemManager.Instance.PlayTimeParticleInPlayer
                (ItemManager.Instance.itemKeepTimePs.petSheeldPs, petData.activeSkillDuration,effectPos);

        }
        else
        {
            SoundEffect.Instance.Play(SoundList.ButtonCancelSe);

            return;
        }
    }

    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();

    protected override void Update()
    {
        base.Update();

        ChangeCoolDown();

        ChangeActiveSkill();
    }

    protected override void ActiveSkillAction()
    {
        BuffManager.Instance.gobalPlayerDefenceAdd += damageReduction;
    }

    protected override void ResetActiveSkillAction()
    {
        BuffManager.Instance.gobalPlayerDefenceAdd -= damageReduction;
    }

    protected override void CloneActiveSkill()
    {
        //自分がオリジナルか確認
        if (whoIam == PetCloneType.Original) return;

        //アクティブスキルを持っているか確認
        if (!petData.hasActiveSkill) return;

        //アクティブスキルを使用中にする
        skillActive = true;

        float finalActiveSkillDuration = petData.activeSkillDuration * ActivePetManager.Instance.PetSkillDuration;

        //持続時間をリセット
        activeSkillDurationTime = finalActiveSkillDuration;

        //違うならアクティブスキルの効果発動
        this.ActiveSkillAction();
    }
}


