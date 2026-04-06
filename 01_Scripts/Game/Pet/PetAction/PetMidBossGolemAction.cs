using TigerForge;
using UnityEngine;

public class PetMidBossGolemAction : ActivePetActionBase, IPetActiveSkill
{
    [Header("ペットスキルのオブジェクト")]
    [SerializeField] private GameObject petSkillObj;
 
    [Header("攻撃力の上昇倍率(%)")]
    public float attackPowerUpRate = 10f;

    protected override void Start()
    {
        base.Start();

        SetCoolDown();
    }

    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();

    protected override void Update()
    {
        base.Update();

        ChangeCoolDown();
    }

    #region --ステート上書き----------------------------------

    protected override void ActiveSkillMotion_Update()
    {
        //アニメーション変更
        AnimationEndChange("ActiveSkillMotion");
    }

    #endregion --ステート上書き終了--

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

    protected override void ActiveSkillAction()
    {
        sm.ChangeState(PetActionStates.ActiveSkillMotion);
    }

    //地ならし
    private void EarthCrush()
    {
        //画面揺らす
        CameraShake.Instance.StartShake(0.8f, 1.4f,5f,5f);

        //攻撃音再生
        if (!SoundEffect.Instance.IsPlaying(skillEffectSound))
        {
            SoundEffect.Instance.Play(skillEffectSound);
        }

        //エフェクト再生
        if (petSkillObj != null)
        {
            //エフェクト生成
            GameObject hitObj = Instantiate(petSkillObj, transform.position, Quaternion.identity);

            //スクリプトを取得
            PetSkillData hitEffectData = hitObj.GetComponent<PetSkillData>();

            //保持していたダメージ値を渡す
            if (hitEffectData != null)
            {
                float finalDamage = this.takeDamages * attackPowerUpRate / 100;

                //当たった時用のダメージを初期化
                hitEffectData.Initialize(finalDamage, this.gameObject);
            }
        }
    }
}
