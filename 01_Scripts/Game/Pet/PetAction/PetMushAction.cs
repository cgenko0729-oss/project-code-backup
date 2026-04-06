using TigerForge;
using UnityEngine;

public class PetMushAction : ActivePetActionBase, IPetActiveSkill
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("攻撃力の上昇倍率(%)")]
    public float attackPowerUpRate = 10f;

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

            float finalActiveSkillDuration = petData.activeSkillDuration*ActivePetManager.Instance.PetSkillDuration;

            //持続時間をリセット
            activeSkillDurationTime = finalActiveSkillDuration; 

            //バフを貰う
            ActiveSkillAction();

            //一斉攻撃を開始
            EventManager.EmitEvent(GameEvent.AllAttackStart);

            SoundEffect.Instance.Play(SoundList.DebugkeySe);

            SoundEffect.Instance.Play(skillEffectSound);

            //クールタイムリセット
            ResetCoolDown();

            Vector3 effectPos = new Vector3(0.0f, 0.5f, 0.0f);

            ItemManager.Instance.PlayTimeParticleInPlayer
                (ItemManager.Instance.itemKeepTimePs.itemPowUpPs,activeSkillDurationTime,effectPos);
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

        ChangeActiveSkill();
    }

    protected override void ActiveSkillAction()
    {
        //攻撃力を上昇させる
        BuffManager.Instance.gobalDamageAdd += attackPowerUpRate;
    }

    protected override void ResetActiveSkillAction()
    {
        //スキルの持続時間が終了したら、攻撃力を元に戻す
        BuffManager.Instance.gobalDamageAdd -= attackPowerUpRate;
    }

    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();

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

