using DG.Tweening;
using QFSW.MOP2;                //Object Pool
using TigerForge;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PetMuscleFrogAction : ActivePetActionBase, IPetActiveSkill
{
    [Header("アクティブスキルのジャンプ着地時の衝撃波オブジェクト")]
    [SerializeField] private GameObject activeSkillShockwave;

    [Header("通常攻撃のオブジェクトプール")]
    [SerializeField] private ObjectPool normalAttackPool;

    [Header("通常攻撃の撃つ位置")]
    [SerializeField] private GameObject normalAttackPosition;

    [Header("通常攻撃の速度")]
    [SerializeField] private float normalAttackSpeed = 0.75f;

    [Header("ジャンプの高さ")]
    [SerializeField] private float jumpPow = 3.0f;

    [Header("ジャンプしている時間")]
    [SerializeField]private float jumpDuration = 1.0f;

    [Header("敵の最大HPの割合ダメージ")]
    [SerializeField] private float enemyTakeDamageRate = 0.2f;

    //スキルの角度
    private Vector3 skillRot = new Vector3(0, 0, 0);

    private float originalAttackRange; 

    public override void PerformAttack() => PetAttackAction();
    protected override void PetAttackAction()
    {
        if (lookingAtEnemies)
        {
            // 敵の方向を計算
            Vector3 direction = (enemyTransform.position - normalAttackPosition.transform.position).normalized;

            direction.y = 0;
            direction = direction.normalized;

            skillRot = direction; // スキルの向きを設定
        }

        //通常攻撃のオブジェクトをプールから取得して生成
        GameObject skillObj = normalAttackPool.GetObject();
        skillObj.transform.position = normalAttackPosition.transform.position;

        PetSkillData proj = skillObj.GetComponent<PetSkillData>();
        if (proj != null)
        {
            proj.SetPool(normalAttackPool);
            proj.SetDirection(skillRot);
            proj.Initialize(takeDamages, this.gameObject);
            proj.speed = normalAttackSpeed;
        }

        PlayAttackSound();
    }

    protected override void Update()
    {
        base.Update();

        ChangeCoolDown();
    }


    #region --ステート上書き----------------------------------

    protected override void ActiveSkillMotion_Enter()
    {
        base.ActiveSkillMotion_Enter();

        lookingAtEnemies = true; // 敵を見ている状態にする
    }

    protected override void ActiveSkillMotion_Update()
    {
        // プレイヤーとの距離を測定
        if (enemyTransform)
        {
            if (lookingAtEnemies)
            {
                LookAt(enemyTransform);
            }
        }
        else
        {

            sm.ChangeState(PetActionStates.Idle);
            return;
        }

        //アニメーション変更
        AnimationEndChange("ActiveSkillMotion");
    }

    protected override void ActiveSkillMotion_Exit()
    {
        //敵を見るのをやめる
        lookingAtEnemies = false;

        attackRange = originalAttackRange; //攻撃範囲を元に戻す
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
        }
        else
        {
            SoundEffect.Instance.Play(SoundList.ButtonCancelSe);

            return;
        }
    }

    protected override void ActiveSkillAction()
    {
        attackRange *= 1.5f; //攻撃範囲を倍にする

        sm.ChangeState(PetActionStates.ActiveSkillMotion);
    }

    protected override void Start()
    {
        base.Start();

        originalAttackRange = attackRange;

        SetCoolDown();
    }

    private void JumpStart()
    {
        Vector3 targetPosition = enemyTransform.position;
        targetPosition.y = 0;

        transform.DOJump(
            targetPosition,     //最終的な着地地点
            jumpPow,            //ジャンプの最高到達点の高さ
            1,                  //ジャンプの回数
            jumpDuration        //ジャンプにかかる合計時間
        ).SetEase(Ease.OutQuad);
    }

    private void JumpAttack()
    {
        //画面揺らす
        CameraShake.Instance.StartShake(0.5f, 2.0f, 10f, 5f);

        //攻撃音再生
        if (!SoundEffect.Instance.IsPlaying(skillEffectSound))
        {
            SoundEffect.Instance.Play(skillEffectSound);
        }

        //エフェクト再生
        if (activeSkillShockwave != null)
        {
            //エフェクト生成
            GameObject hitObj = Instantiate(activeSkillShockwave, transform.position, Quaternion.identity);

            //スクリプトを取得
            PetSkillData hitEffectData = hitObj.GetComponent<PetSkillData>();

            //保持していたダメージ値を渡す
            if (hitEffectData != null)
            { 
                //当たった時用のダメージを初期化
                hitEffectData.Initialize(takeDamages, this.gameObject, LastAttackType.Other, enemyTakeDamageRate);
            }
        }


        //クールタイムリセット
        ResetCoolDown();
    }

    protected override GameObject FindNearestEnemy()
    {
        //索敵したいレイヤーをまとめたレイヤーマスクを作成
        int enemyMask = LayerMask.GetMask("EnemySpider", "EnemyMage", "EnemyDragon", "EnemyBossSpider", "EnemyMushroom");

        //ペットの位置からattackRangeの範囲内にある、指定したレイヤーのコライダーを全て取得
        Collider[] enemiesInAttackRange = Physics.OverlapSphere(transform.position, attackRange, enemyMask);

        //範囲内に敵が一人もいなければ、何もせずに処理を終了
        if (enemiesInAttackRange.Length == 0)
        {
            enemyTransform = null;
            return null;
        }

        //最もHPの高い敵を見つける
        GameObject toughestEnemy = null; 
        float maxHp = -1f;           

        foreach (Collider enemyCollider in enemiesInAttackRange)
        {
            EnemyStatusBase enemyStat = enemyCollider.GetComponent<EnemyStatusBase>();

            float sqrDist = (transform.position - enemyCollider.transform.position).sqrMagnitude;

            float minimumAttackDistance = 1.8f;

            if (sqrDist < minimumAttackDistance * minimumAttackDistance)
            {
                continue;
            }

            if (enemyStat != null && enemyStat.isAlive)
            {
                //最もHPの高い敵を更新
                if (enemyStat.enemyHp > maxHp)
                {
                    maxHp = enemyStat.enemyHp;
                    toughestEnemy = enemyCollider.gameObject;
                }
            }
        }

        enemyTransform = (toughestEnemy != null) ? toughestEnemy.transform : null;

        return toughestEnemy;
    }
}
