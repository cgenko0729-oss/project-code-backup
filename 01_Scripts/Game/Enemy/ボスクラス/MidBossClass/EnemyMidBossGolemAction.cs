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

public class EnemyMidBossGolemAction : BossActionBase
{
    public GameObject stoneObj;

    
    public enum BossState
    {
        Idle,
        Move,
        Dizzy,
        Death,      
        StoneAttack,
    }

    public float stateCnt;                    // 各状態の継続時間などをカウントするタイマー
    public StateMachine<BossState> stateMachine;　　// 状態遷移を管理するステートマシン
    public string stateInfoText; // 状態番号を文字列に変換するためのテキスト

    public float attackCnt = 0f;
    public float turningCnt = 0f;

    public Vector3 centerPos = Vector3.zero;

    private void Start()
    {
        InitBoss();

        stateMachine = StateMachine<BossState>.Initialize(this);
        stateMachine.ChangeState(BossState.Move);

        currentAtttackPattern = 0;
    }

    private void Update()
    {
        UpdateStateInfo();
        UpdateGobalCounter();

    }


    void Move_Enter()
    {
        stateCnt = 4.2f;
        animator.SetBool("isMoving", true);
        animator.SetBool("isIdling",false);

    }

    void Move_Update()
    {
        HomingPlayer();
        RotateTowardPlayer();

        float distToPlayer = Vector3.Distance(transform.position, playerTrans.position);

        //PlanStateChange(BossState.StoneAttack);

        if (stateCnt <=0 && distToPlayer < 11f)
        {
            ChangeToState(BossState.StoneAttack);
        }
    }

    void Move_Exit()
    {
        animator.SetBool("isMoving", false);
    }

    void Dizzy_Enter()
    {

    }

    void Dizzy_Update()
    {

    }

    void Death_Enter()
    {

    }

    void Death_Update()
    {
       
    }

    void StoneAttack_Enter()
    {
        stateCnt = 7f;
        attackCnt = 2.1f;
        animator.SetBool("isIdling", true);
        SetInnerGlow();
    }

    void StoneAttack_Update()
    {
        RotateTowardPlayer();

        if (attackCnt <= 0f)
        {
            animator.SetTrigger("isAttacking");
            ThrowStoneAttack();
            attackCnt = 2.1f;
        }

        PlanStateChange(BossState.Move);
    }

    void StoneAttack_Exit()
    {
        animator.SetBool("isIdling", false);
    }


    //==========================Utility&Helper===================================//
    public void ChangeToState(BossState newState)
    {
        stateMachine.ChangeState(newState);
        stateInfoText = stateMachine.State.ToString(); // 状態名を文字列に変換
    }
    public void ChangeToMoveState() => stateMachine.ChangeState(BossState.Move);

    public void ChangeToDeath()
    {
        ChangeToState(BossState.Death);
    }

    void PlanStateChange(BossState targetState)
    {
       if(stateCnt <= 0)
        {
            ChangeToState(targetState);
        }
    }
   
    public void UpdateStateInfo()
    {
        //stateInfoText = stateMachine.State.ToString();
        stateCnt -= Time.deltaTime;
    }

    public void UpdateGobalCounter()
    {
        attackCnt -= Time.deltaTime;
        turningCnt -= Time.deltaTime;
    }

   

    

    

    void ThrowStoneAttack()
    {
        //SoundEffect.Instance.Play(SoundList.GolemAttackSe);

        Vector3 spawnPos = transform.position + Vector3.up * 5f + transform.forward * 1.5f;
        Quaternion rot = Quaternion.LookRotation(transform.forward, Vector3.up);

       GameObject stone = Instantiate(stoneObj, spawnPos, rot);

    }

}

