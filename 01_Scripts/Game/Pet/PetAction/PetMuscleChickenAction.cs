using UnityEngine;
using QFSW.MOP2;
using System.Collections;

public class PetMuscleChickenAction : ActivePetActionBase
{
    [Header("レーザー")]
    [SerializeField] private GameObject laserObject;

    [Header("レーザーのパーティクル")]
    [SerializeField] private ParticleSystem laserParticle;

    [Header("波動を撃つ確率")]
    [SerializeField] private int waveAttackChance = 30;

    [Header("波動のエフェクト（プレハブプール）")]
    [SerializeField] private ObjectPool laserWavePrefabPool;

    [Header("波動の連撃設定")]
    [SerializeField] private int   laserWaveCount    = 5;     //波動が出る回数
    [SerializeField] private float laserWaveInterval = 0.15f; //次の波動が出るまでの時間
    [SerializeField] private float distanceStep      = 1.5f;  //波動ごとの距離間隔

    //保存用の敵の方向
    private Vector3 keepDirection = Vector3.zero;

    #region ステート上書き---------------------------------------

    protected override void Walk_Update() //ステート中ずっと実行
    {
        // プレイヤーとの距離を測定
        if (playerToDist > runDist)
        {
            // プレイヤーとの距離がrunDist以上なら走る
            sm.ChangeState(PetActionStates.Run);
            return;
        }
        if (playerToDist < runDist && stateCnt <= 0)
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
                if (dist <= attackRange && stateCnt <= 0)
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
                Vector3 direction = (enemyTransform.position - transform.position).normalized;
                direction.y = 0;

                //保存!
                keepDirection = direction;

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

            //lookingAtEnemies = true; // 敵を見ている状態にする
            //stateCnt = attackCooldown; // 再度クールダウンタイマーをリセット
        }
    }

    protected override void Attack_Exit()
    {
        base.Attack_Exit();
        laserObject.SetActive(false);
        laserParticle.Stop();

        //敵を見ていない状態にする
        lookingAtEnemies = false;

        if (petAttackCol != null)
        {
            petAttackCol.enabled = false;
        }

        stateCnt = 1.49f;
    }

    protected override void IdleMotion_Enter()
    {
        petAnimator.SetBool("isIdle", false);
        petAnimator.SetTrigger("IdleMotion");
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

    #endregion --------------------------------------------------

    protected override void DisableAttackCollider()
    {
        return;
    }

    protected override void OnHitEnemy(EnemyStatusBase enemyStat)
    {
        enemyStat.ApplyFireDebuff();
    }

    private void AttackColliderDelete()
    {
        laserObject.SetActive(false);
        laserParticle.Stop();

        if (petAttackCol != null)
        {
            petAttackCol.enabled = false; // コライダーを消す
        }
    }

    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction()
    {
        //nullチェック
        if (enemyTransform == null) { return; }

        laserObject.SetActive(true);
        laserParticle.Play();

        if(SoundEffect.Instance.IsPlaying(skillEffectSound))
        {
            SoundEffect.Instance.Play(skillEffectSound);
        }

        lookingAtEnemies = false; //敵を見ていない状態にする

        if (petAttackCol != null)
        {
            petAttackCol.enabled = true;
        }
    }

    private void LaserWave()
    {
        int randomValue = Random.Range(0, 100);
        if (randomValue <= waveAttackChance)
        {
            StartCoroutine(SpawnLaserWave(keepDirection));
        }
        else
        {
            return;
        }
    }

    private IEnumerator SpawnLaserWave(Vector3 direction)
    {
        Vector3 startPos = transform.position + (direction * 1.5f);

        if (SoundEffect.Instance.IsPlaying(attackSound))
        {
            SoundEffect.Instance.Play(attackSound);
        }

        //CameraShake.Instance.StartSmallShake();

        for (int i = 0; i < laserWaveCount; i++)
        {
            

            Vector3 spawnPos = startPos + (direction * (distanceStep * i));

            spawnPos.y = transform.position.y;

            GameObject thunder = laserWavePrefabPool.GetObject();

            thunder.transform.position = spawnPos;
            thunder.transform.rotation = Quaternion.LookRotation(direction);

            PetSkillData skillData = thunder.GetComponent<PetSkillData>();
            if (skillData != null)
            {
                skillData.Initialize(takeDamages * 1.5f, this.gameObject);
                skillData.SetPool(laserWavePrefabPool);
                skillData.speed = 0f;
            }

            yield return new WaitForSeconds(laserWaveInterval);
        }
    }
}
