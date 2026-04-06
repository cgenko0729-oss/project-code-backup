using MonsterLove.StateMachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PetMidBossBeeAction : PetNagaAction
{
    [Header("近接攻撃に切り替える距離")]
    [SerializeField] private float meleeSwitchDistance = 3.0f;

    protected override void Update() 
    {
        base.Update();

        if (sm.State == PetActionStates.ActiveSkillMotion || sm.State == PetActionStates.Attack)
        {
            return;
        }

        if (enemyTransform != null)
        {
            float dist = Vector3.Distance(transform.position, enemyTransform.position);

            if (dist <= meleeSwitchDistance)
            {
                sm.ChangeState(PetActionStates.ActiveSkillMotion);
            }
        }
    }

    #region ステート上書き---------------------------------------


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

    protected override void ActiveSkillMotion_Enter()
    {
        base.ActiveSkillMotion_Enter();

        petAnimator.speed = this.currentAttackSpeedMultiplier;

        lookingAtEnemies = true;   //敵を見ている状態にする
    }
    protected override void ActiveSkillMotion_Update()
    {
        //敵のTransformがnullでないことを確認
        if (enemyTransform && lookingAtEnemies)
        {
            LookAt(enemyTransform);
        }

        AnimationEndChange("ActiveSkillMotion");
    }

    protected override void ActiveSkillMotion_Exit()
    {
        //念のため、自身のコライダーを消す
        DisableAttackCollider();

        //敵を見ている状態を解除
        lookingAtEnemies = false;
    }

    //IdleMotion------------------------------------------
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
                if (petData.petRoles == null || !petData.petRoles.Contains(PetRole.NoAttack))
                {
                    float dist = Vector3.Distance(transform.position, nearestEnemy.transform.position);
                    if (dist <= attackRange)
                    {
                        // 近くの敵が攻撃範囲内なら敵追従状態に遷移
                        sm.ChangeState(PetActionStates.HomingEnemy);
                        return;
                    }
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
    //-----------------------------------------------

    #endregion --------------------------------------------------

    //近距離攻撃
    public void PetMeleeAttack()
    {
        //nullチェック
        if (enemyTransform == null) { return; }

        //敵を見ていない状態にする
        lookingAtEnemies = false;

        //自身のコライダーを消す
        if (petAttackCol != null)
        {
            petAttackCol.enabled = true; // コライダーを付ける
        }
    }

    protected override void DisableAttackCollider()
    {
        if (petAttackCol != null)
        {
            petAttackCol.enabled = false;
        }
    }

    protected override void SetupProjectile(PetSkillData proj)
    {
        proj.SetPool(skillObjPool);
        // 元々の初期化処理
        proj.Initialize(takeDamages, this.gameObject);
        proj.SetDirection(skillRot);
        proj.speed = petSkillSpeed;

        attackCounter++;

        int ResetCount = 3;

        //3回毎に一度、攻撃音を鳴らす
        if (attackCounter >= ResetCount)
        {
            //効果音がもう再生されていたら、スルー
            if (!SoundEffect.Instance.IsPlaying(skillEffectSound))
            {
                SoundEffect.Instance.Play(skillEffectSound);

                //カウンターをリセット
                attackCounter = 0;
            }
        }
    }
}
