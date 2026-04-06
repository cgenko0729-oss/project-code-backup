using QFSW.MOP2;                //Object Pool
using UnityEngine;

public class PetNagaAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("スキルのオブジェクトプール")]
    public ObjectPool skillObjPool;

    [Header("ペットのスキルの速度")]
    public float petSkillSpeed = 10.0f;

    [Header("ペットの攻撃オブジェクト")]
    public GameObject petSkillObject;

    [Header("スキルの撃つ位置")] 
    public GameObject petSkillPoint;

    [Header("待機モーションの時間")]
    public float idleMotionCooldown;

    [Header("敵を攻撃しない距離")]
    public float minimumAttackDistance = 3.0f;

    //スキルの角度
    protected Vector3 skillRot = new Vector3(0, 0, 0);

    #endregion --------------------------------------------


    #region 各ステート---------------------------------------

    protected override void Walk_Update() //ステート中ずっと実行
    {
        // プレイヤーとの距離を測定
        if (playerToDist > runDist)
        {
            // プレイヤーとの距離がrunDist以上なら走る
            sm.ChangeState(PetActionStates.Run);
            return;
        }
        if (playerToDist < runDist)
        {
            if (nearestEnemy != null)
            {
                float dist = Vector3.Distance(transform.position, nearestEnemy.transform.position);
                if (dist <= attackRange)
                {
                    // 近くの敵が攻撃範囲内なら攻撃状態に遷移
                    sm.ChangeState(PetActionStates.Attack);
                    return;
                }
            }
        }
        if (playerToDist > walkDist)
        {
            if (playerTransform)
            {
                Homing(playerTransform, moveSpeed_Walk);
                return;
            }
        }
        if (playerToDist <= walkDist && playerToDist > idleDist)
        {
            sm.ChangeState(PetActionStates.Idle);
            return;
        }
    }

    protected override void Idle_Update()
    {
        if (playerToDist < runDist)
        {
            if (nearestEnemy != null)
            {
                float dist = Vector3.Distance(transform.position, nearestEnemy.transform.position);
                if (dist <= attackRange)
                {
                    // 近くの敵が攻撃範囲内なら攻撃状態に遷移
                    sm.ChangeState(PetActionStates.Attack);
                    return;
                }
            }
        }
        if (playerToDist > runDist)
        {
            // プレイヤーとの距離がrunDist以上なら走る
            sm.ChangeState(PetActionStates.Run);
            return;
        }
        if (playerToDist > walkDist)
        {
            // プレイヤーとの距離がwalkDist以上なら歩く
            sm.ChangeState(PetActionStates.Walk);
            return;
        }

        if (stateCnt <= stateZeroCnt)
        {
            // 待機状態のカウンターが0以下になったら待機モーションに遷移
            sm.ChangeState(PetActionStates.IdleMotion);
            return;
        }
    }

    protected override void Attack_Update()
    {
        // プレイヤーとの距離を測定
        if (enemyTransform)
        {
            if (lookingAtEnemies)
            {
                LookAt(enemyTransform);
            }
        }

        if (stateCnt <= stateZeroCnt)
        {
            if (playerToDist > walkDist)
            {
                // プレイヤーとの距離がwalkDist以上なら歩く
                sm.ChangeState(PetActionStates.Walk);
                return;
            }
            if (playerToDist <= idleDist)
            {
                // プレイヤーとの距離がidleDist以下なら待機状態に遷移
                sm.ChangeState(PetActionStates.Idle);
                return;
            }
            if (playerToDist > runDist)
            {
                // プレイヤーとの距離がrunDist以上なら走る
                sm.ChangeState(PetActionStates.Run);
                return;
            }

            lookingAtEnemies = true; // 敵を見ている状態にする
            stateCnt = attackCooldown; // 再度クールダウンタイマーをリセット
        }
    }
    
    protected override void IdleMotion_Enter()
    {
        base.IdleMotion_Enter();

        stateCnt = idleMotionCooldown; // 待機モーションのカウンターをリセット
    }

    protected override void IdleMotion_Update()
    {
        if (playerToDist < runDist)
        {
            if (nearestEnemy != null)
            {
                float dist = Vector3.Distance(transform.position, nearestEnemy.transform.position);
                if (dist <= attackRange)
                {
                    // 近くの敵が攻撃範囲内なら攻撃状態に遷移
                    sm.ChangeState(PetActionStates.Attack);
                    return;
                }
            }
        }
        if (playerToDist > runDist)
        {
            // プレイヤーとの距離がrunDist以上なら走る
            sm.ChangeState(PetActionStates.Run);
            return;
        }
        if (playerToDist > walkDist)
        {
            // プレイヤーとの距離がwalkDist以上なら歩く
            sm.ChangeState(PetActionStates.Walk);
            return;
        }
    }

    #endregion --------------------------------------------

    public override void PerformAttack()
    {
        PetAttackAction();
    }

    protected override void PetAttackAction()
    {
        //nullチェック
        if (enemyTransform == null) { return; }

        if (lookingAtEnemies)
        {
            // 敵の方向を計算
            Vector3 direction = (enemyTransform.position - petSkillPoint.transform.position).normalized;

            direction.y = 0;
            direction = direction.normalized;

            skillRot = direction; // スキルの向きを設定
        }

        lookingAtEnemies = false; // 敵を見ていない状態にする

        GameObject skill = skillObjPool.GetObject(); // オブジェクトプールからスキルのオブジェクトを取得

        skill.transform.position = petSkillPoint.transform.position; // スキルの位置を設定

        //回転を一度取得して修正後に再設定
        Vector3 fixedEulerAngles = transform.rotation.eulerAngles;
        fixedEulerAngles.x = 0; // X軸の回転を固定
        fixedEulerAngles.z = 0; // Z軸の回転を固定
        transform.rotation = Quaternion.Euler(fixedEulerAngles);

        skill.transform.rotation = Quaternion.Euler(fixedEulerAngles); // スキルの回転を初期化

        PetSkillData proj = skill.GetComponent<PetSkillData>();

        // スキルオブジェクトの初期化処理を呼び出す
        SetupProjectile(proj);

        if (petData.petType == PetType.Naga)
        {
            //3回毎に1度、攻撃音を鳴らす
            PlayAttackSound();
        }
        if(petData.petType == PetType.CasterRat)
        {
            PlayAttackSounds();
        }
    }

    protected override GameObject FindNearestEnemy()
    {
        // 索敵したいレイヤーをまとめたレイヤーマスクを作成
        int enemyMask = LayerMask.GetMask("EnemySpider", "EnemyMage", "EnemyDragon", "EnemyBossSpider", "EnemyMushroom");

        // ペットの位置からattackRangeの範囲内にある、指定したレイヤーのコライダーを全て取得
        Collider[] enemiesInAttackRange = Physics.OverlapSphere(transform.position, attackRange, enemyMask);

        // 範囲内に敵が一人もいなければ、何もせずに処理を終了
        if (enemiesInAttackRange.Length == 0)
        {
            enemyTransform = null;
            return null;
        }

        GameObject closestEnemy = null;
        float minSqrDistance = Mathf.Infinity;

        // 見つかったコライダーのリストをループして、どれが一番近いか調べる
        foreach (Collider enemyCollider in enemiesInAttackRange)
        {
            // コライダーから、その親であるゲームオブジェクトの位置までの距離を計算（高速な2乗距離で）
            float sqrDist = (transform.position - enemyCollider.transform.position).sqrMagnitude;

            //平方フィートで比較するため、minimumAttackDistanceも2乗する
            if (sqrDist < minimumAttackDistance * minimumAttackDistance)
            {
                // 近すぎるので、この敵は無視して次の敵のチェックに移る
                continue;
            }

            // もし、今まで見つけたどの敵よりも距離が近ければ
            if (sqrDist < minSqrDistance)
            {
                // こちらを新しい「最も近い敵」として記憶する
                minSqrDistance = sqrDist;
                closestEnemy = enemyCollider.gameObject; // コライダーからゲームオブジェクト本体を取得
            }
        }

        // 最も近い敵をenemyTransformに設定
        enemyTransform = closestEnemy?.transform;

        // 最も近い敵を返す
        return closestEnemy;
    }

    protected virtual void SetupProjectile(PetSkillData proj)
    {
        proj.SetPool(skillObjPool);
        // 元々の初期化処理
        proj.Initialize(takeDamages, this.gameObject);
        proj.SetDirection(skillRot);
        proj.speed = petSkillSpeed;
    }

    private void NagaShout()
    {
        // Nagaの叫び声を再生する処理を追加
        SoundEffect.Instance.Play(SoundList.NagaShout);
    }

    public void PlayAttackSounds()
    {
        if (attackSoundCoolTime <= 0f)
        {
            //攻撃音のクールタイムをリセット
            attackSoundCoolTime = attackCooldownOriginal * attackSoundCoolMax;
            SoundEffect.Instance.Play(attackSound); //攻撃音を再生
        }
    }
}


