using DG.Tweening;
using TigerForge;
using UnityEngine;
using QFSW.MOP2;

public class PetBossTurnipaKingAction : ActivePetActionBase, IPetActiveSkill
{
    //アクティブスキルの総ダメージ量
    private float activeSkillDamageAmount = 0f;

    //ブラックホールのスケール
    private float MegaBlackHoleScale = 0f;
    private float MegaBlackHoleScaleMax = 1.5f;

    //現在のブラックホールのスケールを更に大きくするための変数
    private float mBHScaleBigChangeValue = 2.5f;

    //一定の回復量にいくとブラックホールのスケールが変化するための変数
    private float healAmountForScaleChange = 0f;

    [Header("ブラックホールのスケールを変化させるための回復量")]
    [Tooltip("例:10だとアクティブスキルのダメージが10超えれば変化する")]
    [SerializeField] private float healAmountForScaleChangeThreshold = 50f;

    [Header("ブラックホールのスケールを変化の値")]
    [SerializeField] private float MegaBlackHoleScaleChangeValue = 0.25f;

    [Header("メガブラックホールプレハブ")]
    [SerializeField] private GameObject MegaBlackHolePrefab;

    [Tooltip("発射する弾のプレハブ")]
    [SerializeField] private GameObject blackHoleProjectilePrefab;

    [Tooltip("弾を発射する位置")]
    [SerializeField] private Transform firePoint;

    [Header("変化時のサウンド")]
    [SerializeField] protected SoundList changeSound;

    [Header("スキルのオブジェクトプール")]
    public ObjectPool skillObjPool;

    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.StartListening(GameEvent.turnipaKingSkillPowUp, BlackPowerBallPowUp);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.StopListening(GameEvent.turnipaKingSkillPowUp, BlackPowerBallPowUp);
    }

    protected override void Start()
    {
        base.Start();

        //カブキングがいることをActivePetManagerに伝える
        ActivePetManager.Instance.isTurnipaKingInGame = true;

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

    public void BigBlackHole()
    {
        if (MegaBlackHolePrefab == null) return;
        MegaBlackHoleScale *=  mBHScaleBigChangeValue;
        MegaBlackHolePrefab.transform.DOScale(MegaBlackHoleScale, 0.5f).SetEase(Ease.OutBack);
        if(!SoundEffect.Instance.IsPlaying(changeSound))
        {
            SoundEffect.Instance.Play(changeSound);
        }
    }

    public void FireBlackHole()
    {
        if (blackHoleProjectilePrefab == null || firePoint == null) return;

        // 弾を生成
        GameObject projectileObj = Instantiate(blackHoleProjectilePrefab, firePoint.position, firePoint.rotation);

        projectileObj.transform.localScale = MegaBlackHolePrefab.transform.lossyScale;

        // 弾のスクリプトに溜めたパワーとスケールを渡す
        BlackHoleProjectile projectileScript = projectileObj.GetComponent<BlackHoleProjectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(activeSkillDamageAmount);
            projectileScript.SetDirection(firePoint.forward); // 発射方向を設定
        }

        if (!SoundEffect.Instance.IsPlaying(skillEffectSound)) // デバッグ用のサウンド以外が設定されている場合のみ再生
        {
            SoundEffect.Instance.Play(skillEffectSound);
        }

        // --- すべての状態をリセット ---
        activeSkillDamageAmount = 0f;
        healAmountForScaleChange = 0f;
        MegaBlackHoleScale = 0f;

        // チャージ中の見た目を滑らかに消す
        if (MegaBlackHolePrefab != null)
        {
            MegaBlackHolePrefab.transform.DOScale(0f, 0.3f).OnComplete(() =>
            {
                MegaBlackHolePrefab.SetActive(false); // アニメーション後に非アクティブ化
            });
        }
    }

    #region --ステート上書き----------------------------------

    protected override void ActiveSkillMotion_Update()
    {
        //アニメーション変更
        AnimationEndChange("ActiveSkillMotion");
    }

    #endregion --ステート上書き終了--

    protected override void Update()
    {
        base.Update();

        ChangeCoolDown();
    }

    protected override void ActiveSkillAction()
    {
        sm.ChangeState(PetActionStates.ActiveSkillMotion);
    }

    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction()
    {
        //nullチェック
        if (enemyTransform == null) { return; }

        lookingAtEnemies = false; // 敵を見ていない状態にする

        GameObject skill = skillObjPool.GetObject(); // オブジェクトプールからスキルのオブジェクトを取得

        skill.transform.position = transform.position; // スキルの位置を設定

        PetSkillData proj = skill.GetComponent<PetSkillData>();

        if (proj != null)
        {
            proj.SetPool(skillObjPool);
            // 元々の初期化処理
            proj.Initialize(takeDamages, this.gameObject);
        }
        PlayAttackSound();
    }

    //カブキングのスキルパワーアップイベントを受け取ったときの処理
    private void BlackPowerBallPowUp()
    {
        var healAmount = EventManager.GetFloat(GameEvent.turnipaKingSkillPowUp);
        if (healAmount <= 0) return;

        activeSkillDamageAmount += healAmount;
        healAmountForScaleChange += healAmount;

        while (healAmountForScaleChange >= healAmountForScaleChangeThreshold)
        {
            if (!MegaBlackHolePrefab.activeSelf)
            {
                MegaBlackHolePrefab.SetActive(true);
            }

            if (MegaBlackHoleScale < MegaBlackHoleScaleMax)
            {
                MegaBlackHoleScale += MegaBlackHoleScaleChangeValue;

                if (!SoundEffect.Instance.IsPlaying(changeSound)) // デバッグ用のサウンド以外が設定されている場合のみ再生
                {
                    SoundEffect.Instance.Play(changeSound);
                }
                if (MegaBlackHoleScale > MegaBlackHoleScaleMax)
                {
                    MegaBlackHoleScale = MegaBlackHoleScaleMax;
                }
            }
            healAmountForScaleChange -= healAmountForScaleChangeThreshold;
        }

        MegaBlackHolePrefab.transform.DOScale(MegaBlackHoleScale, 0.5f).SetEase(Ease.OutBack);

       

    }
}
