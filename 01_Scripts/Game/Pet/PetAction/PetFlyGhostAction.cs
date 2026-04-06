using HighlightPlus;
using TigerForge;
using UnityEngine;

public class PetFlyGhostAction : ActivePetActionBase, IPetActiveSkill
{
    #region 固有能力---------------------------------------

    [Header("召喚するペットのプレハブ")]
    [SerializeField] private GameObject summonPetPrefab;

    [Header("召喚数")]
    [SerializeField] private int summonCount = 2;

    //離れる距離
    private float offsetDistance = 2f;

    #endregion --------------------------------------------


    #region --ステート上書き----------------------------------

    // 重複していたメソッドを削除
    protected override void ActiveSkillMotion_Update()
    {
        //アニメーション変更
        AnimationEndChange("ActiveSkillMotion");
    }

    #endregion --ステート上書き終了--

    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();

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
            //アクティブスキルを使用中にする
            skillActive = true;

            //アクティブスキルの効果発動
            ActiveSkillAction();

            //一斉攻撃を開始
            EventManager.EmitEvent(GameEvent.AllAttackStart);

            //入力音再生
            SoundEffect.Instance.Play(skillEffectSound);

            SoundEffect.Instance.Play(SoundList.DebugkeySe);

            //クールタイムリセット
            ResetCoolDown();
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
        sm.ChangeState(PetActionStates.ActiveSkillMotion);

        if (summonPetPrefab == null) return;

        int FinalCloneCount = summonCount + ActivePetManager.Instance.PetCloneCount;

        //召喚数だけペットを召喚
        for (int i = 0; i < FinalCloneCount; i++)
        {       
            float angle = i * (360f / FinalCloneCount);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * (this.transform.right * offsetDistance);

            Vector3 finalOffset = this.transform.position + offset;

            //スキル持続時間を設定
            float finalSkillDuration = petData.activeSkillDuration * ActivePetManager.Instance.PetSkillDuration;

            //持続時間をリセット
            activeSkillDurationTime = finalSkillDuration;

            //召喚ペット生成
            GameObject Phantom = Instantiate(summonPetPrefab, finalOffset, this.transform.localRotation);

            ActivePetActionBase PhantomStatus = Phantom.GetComponent<ActivePetActionBase>();
            if (PhantomStatus != null)
            {
                //編成位置を設定
                PhantomStatus.SetFormationOffset(offset);

                //アクティブスキルの持続時間を設定
                PhantomStatus.SetSelfTimer(finalSkillDuration);

                //アクティブスキルモーションへ強制移行
                PhantomStatus.ForceChangeState(PetActionStates.ActiveSkillMotion);
            }
            ActivePetManager.Instance.RegisterActivePetList(Phantom);
        }
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
