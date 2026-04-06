using TigerForge;
using UnityEngine;

public class PetTurnipSweetAction : ActivePetActionBase, IPetActiveSkill
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("爆発準備時間")]
    [SerializeField] private float explosionDelayTime = 5f;

    [Header("スキル発動時のハイライトエフェクト")]
    [Tooltip("有効化するハイライトエフェクトのコンポーネント")]
    [SerializeField] private Behaviour highlightEffect;

    [Header("追加の走行速度")]
    [SerializeField] private float moveSpeed_RunPlus = 9f;

    [Header("追加の攻撃範囲")]
    [SerializeField] private float attackRangePlus = 13f;

    [Header("ダメージを与えた時のエフェクトオブジェクト")]
    [SerializeField] private GameObject petSkillObj;

    #endregion --------------------------------------------

    protected override void Start()
    {
        base.Start();

        //固定で
        this.takeDamages = 999;

        SetCoolDown();
    }

    public void PetActiveSkill()
    {
        //もしpetdataのアクティブスキルのクールタイムが変更されていたら、スキルを発動できないようにする
        if (petData.activeSkillRemainingCooldown == ResetCoolTime)
        {
            if (sm.State != PetActionStates.HomingEnemy)
            {
                //アクティブスキルの効果発動
                ActiveSkillAction();

                //一斉攻撃を開始
                EventManager.EmitEvent(GameEvent.AllAttackStart);

                //入力音再生
                SoundEffect.Instance.Play(skillEffectSound);

                SoundEffect.Instance.Play(SoundList.DebugkeySe);
            }
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
        moveSpeed_Run += moveSpeed_RunPlus;
        attackRange += attackRangePlus;

        //爆発前エフェクト位置調整
        Vector3 EffectPos = ItemManager.Instance.defaultEffectPos + new Vector3(0, 0.5f, 0);

        //爆発前エフェクト再生
        ItemManager.Instance.PlayTimeParticleInObject(this.gameObject,
                                              ItemManager.Instance.itemKeepTimePs.ExplodeBeforeLightPs,
                                              explosionDelayTime,
                                              EffectPos);

        sm.ChangeState(PetActionStates.HomingEnemy);
    }

    protected override void ResetActiveSkillAction()
    {
        moveSpeed_Run -= moveSpeed_RunPlus;
        attackRange -= attackRangePlus;

        //もしプレイヤーとの距離がワープ距離を超えていたらワープする
        if (playerToDist > warpDist)
        {
            sm.ChangeState(PetActionStates.Warp);
        }
        else sm.ChangeState(PetActionStates.Idle);

        //ハイライトエフェクトを無効化
        if (highlightEffect != null)
        {
            highlightEffect.enabled = false;
        }

        skillActive = false;
    }

    #region --ステート上書き--

    //HomingEnemy------------------------------------------
    protected override void HomingEnemy_Enter()
    {
        base.HomingEnemy_Enter();

        //一定時間後に爆発する
        stateCnt = explosionDelayTime;

        skillActive = false;
    }

    protected override void HomingEnemy_Update()
    {
        //敵のTransformがnullでないことを確認
        if (enemyTransform)
        {
            //一生粘着する
            Homing(enemyTransform, moveSpeed_Run);
        }

        if (stateCnt <= stateZeroCnt && !skillActive)
        {
            skillActive = true;

            //爆発エフェクト再生
            sm.ChangeState(PetActionStates.ActiveSkillMotion);
            return;
        }
    }
    //-----------------------------------------------

    //ActiveSkillMotion------------------------------------------
    protected override void ActiveSkillMotion_Enter()
    {
        base.ActiveSkillMotion_Enter();

        //ハイライトエフェクトを有効化
        if (highlightEffect != null)
        {
            highlightEffect.enabled = true;
        }

        //攻撃音再生
        if (!SoundEffect.Instance.IsPlaying(SoundList.PetExplosionSe))
        {
            SoundEffect.Instance.Play(SoundList.PetExplosionSe);
        }
        
        //爆発エフェクト再生
        if (petSkillObj != null)
        {
            //エフェクト生成
            GameObject hitObj = Instantiate(petSkillObj, transform.position, Quaternion.identity);

            //スクリプトを取得
            PetSkillData hitEffectData = hitObj.GetComponent<PetSkillData>();

            //保持していたダメージ値を渡す
            if (hitEffectData != null)
            {
                //当たった時用のダメージを初期化
                hitEffectData.Initialize(this.takeDamages, this.gameObject);
            }
        }

        //煙エフェクト再生
        ItemManager.Instance.PlayTimeParticleInObject(this.gameObject, 
            　　　　　　　　　　　　　　　　　ItemManager.Instance.itemKeepTimePs.RisingSmokePs,
                                              ItemManager.Instance.defaultEffectPos);

        //クールタイムリセット
        ResetCoolDown();
    }

    protected override void ActiveSkillMotion_Update()
    {
        //もしアクティブスキルのクールタイムが0以下なら
        if (petData.activeSkillRemainingCooldown < ResetCoolTime)
        {
            //生き返る
            ResetActiveSkillAction();
            return;
        }
    }

    //Warp------------------------------------------
    protected override void Warp_Update()
    {
        //アニメーション変更
        AnimationEndChange("WarpAction");
    }
    //-----------------------------------------------

    #endregion --ステート上書き終了--

    //ここの土ならいいカブが生まれそうだぜ
    public void Shedding()
    {
        SoundEffect.Instance.Play(SoundList.SheddingSe);

        ItemManager.Instance.PlayTimeParticleInObject(this.gameObject,
                                             ItemManager.Instance.itemKeepTimePs.SheddingPs,
                                             ItemManager.Instance.defaultEffectPos);
    }

    public override void PerformAttack() => PetAttackAction();
    protected override void PetAttackAction() => base.PetAttackAction();
}
