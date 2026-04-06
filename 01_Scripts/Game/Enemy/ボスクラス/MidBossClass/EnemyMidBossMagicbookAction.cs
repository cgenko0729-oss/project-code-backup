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
using Unity.Collections;
using UnityEditor;

public class EnemyMidBossMagicbookAction : BossActionBase
{
    public enum BossState
    {
        Idle,
        Move,
        TrapAttack,
        BiteAttack,
        MaxState,
    }

    [Header("継承先の情報")]
    public StateMachine<BossState> stateMachine;
    public float[] stateCntValue = new float[(int)BossState.MaxState];
    public float stateCnt = 0;

    [SerializeField] private float attackDist;
    [SerializeField] private float bitAttackDist;
    [SerializeField] private float bitAttackMoveSpeed;
    [SerializeField] private int createTrapNumMax;
    [SerializeField] private int createTrapNumMin;
    [SerializeField] private float createTrapDistance;
    [SerializeField] private bool alreadyCreateTrap;
    [SerializeField] private GameObject magicCircleEffect;
    [SerializeField] private GameObject trapPrefab;
    [SerializeField] private Collider biteCollider;

    void Start()
    {
        InitBoss();

        stateMachine = StateMachine<BossState>.Initialize(this);
        stateMachine.ChangeState(BossState.Move);

        moveSpeed = moveSpeedNormal;

        if (biteCollider != null)
        {
            biteCollider.enabled = false;
        }
    }

    void Update()
    {
        stateCnt -= Time.deltaTime;
    }

    bool IsAnimationPlaying()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime < 1.0f;
    }

    /* ---------- Idle_State ---------- */
    void Idle_Enter()
    {
        stateCnt = stateCntValue[(int)BossState.Idle];

    }

    void Idle_Update()
    {
        if (!IsAnimationPlaying())
        {
            animator.Play("Anim_Magicbook_Idle");
        }

        // Idleステートを抜ける処理
        if (stateCnt <= 0)
        {
            stateMachine.ChangeState(BossState.Move);
        }
    }

    void Idle_Exit()
    {

    }
    /* ------------------------------- */

    /* ---------- Move_State ---------- */
    void Move_Enter()
    {
        stateCnt = stateCntValue[(int)BossState.Move];
        animator.Play("Anim_Magicbook_Move");

        moveSpeed = moveSpeedNormal;
    }

    void Move_Update()
    {
        // プレイヤーへの追尾移動処理
        RotateTowardPlayer();
        HomingPlayer();

        UpdatePlayerInfo();

        // 攻撃ステートへの移行判定
        if (stateCnt <= 0 && distToPlayer <= attackDist)
        {
            // BiteAttackステートへ移行
            if (distToPlayer <= bitAttackDist)
            {
                stateMachine.ChangeState(BossState.BiteAttack);
            }
            // TrapAttackステートへ移行
            else
            {
                stateMachine.ChangeState(BossState.TrapAttack);
            }
        }

        //// Moveステートを抜ける処理
        //if (stateCnt <= 0)
        //{
        //    stateMachine.ChangeState(BossState.Idle);
        //}
    }

    void Move_Exit()
    {

    }
    /* ------------------------------- */

    /* ---------- TrapAttack_State ---------- */
    void TrapAttack_Enter()
    {
        stateCnt = stateCntValue[(int)BossState.TrapAttack];
        animator.Play("Anim_Magicbook_Die");

        if (magicCircleEffect != null)
        {
            magicCircleEffect.SetActive(true);
        }

        alreadyCreateTrap = false;

        Debug.Log("TrapAttackステートに移行");
    }

    void TrapAttack_Update()
    {
        if (trapPrefab != null && alreadyCreateTrap == false)
        {
            int createNum = Random.Range(createTrapNumMin, createTrapNumMax);
            for (int t = 0; t < createNum; t++)
            {
                // 生成する座標を計算する
                Vector3 createPos = playerTrans.position;
                createPos += new Vector3(
                    Random.Range(-createTrapDistance, createTrapDistance),
                    0,
                    Random.Range(-createTrapDistance, createTrapDistance));


                // 計算した座標に罠を生成
                var trap = Instantiate<GameObject>(trapPrefab);
                trap.transform.position = createPos;
            }

            alreadyCreateTrap = true;
        }

        if (stateCnt <= 0)
        {
            stateMachine.ChangeState(BossState.Idle);
        }
    }

    void TrapAttack_Exit()
    {
        if (magicCircleEffect != null)
        {
            magicCircleEffect.SetActive(false);
        }

        animator.Play("Anim_Magicbook_Close");
    }
    /* ------------------------------- */

    /* ---------- BiteAttack_State ---------- */
    void BiteAttack_Enter()
    {
        stateCnt = stateCntValue[(int)BossState.BiteAttack];
        animator.Play("Anim_Magicbook_Attack");

        moveSpeed = bitAttackMoveSpeed;

        // 当たり判定をONにする
        if (biteCollider != null)
        {
            biteCollider.enabled = true;
        }

        Debug.Log("BiteAttackステートに移行");
    }

    void BiteAttack_Update()
    {
        // プレイヤーへの追尾移動処理
        RotateTowardPlayer();
        HomingPlayer();

        if (stateCnt <= 0)
        {
            stateMachine.ChangeState(BossState.Idle);
        }
    }

    void BiteAttack_Exit()
    {
        // 当たり判定をOFFにする
        if (biteCollider != null)
        {
            biteCollider.enabled = false;
        }
    }
    /* ------------------------------- */
}

