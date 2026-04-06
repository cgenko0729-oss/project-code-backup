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

public class EnemyMidBossMageAction : BossActionBase
{
    public enum BossState
    {
        Idle,
        Move,
        Dizzy,
        Death, 
        MagicAttack,
    }

    public float stateCnt;                    // 各状態の継続時間などをカウントするタイマー
    public StateMachine<BossState> stateMachine;　　// 状態遷移を管理するステートマシン
    public string stateInfoText; // 状態番号を文字列に変換するためのテキスト

    public float attackCnt = 0f;
    public float turningCnt = 0f;

    public Vector3 centerPos = Vector3.zero;

    public GameObject aoeMagicCircleObj;

    private void Start()
    {
        InitBoss();
      
        stateMachine = StateMachine<BossState>.Initialize(this);
        stateMachine.ChangeState(BossState.Move);
    }

    private void Update()
    {
        UpdateStateInfo();
        UpdateGobalCounter();
    }


    void Move_Enter()
    {
        stateCnt = 4.2f;

    }

    void Move_Update()
    {
        HomingPlayer();
        RotateTowardPlayer();


        PlanStateChange(BossState.MagicAttack);
    }

    void MagicAttack_Enter()
    {
        stateCnt = 7.7f;
        attackCnt = 3.5f;
        

    }

    void MagicAttack_Update()
    {

        if(attackCnt <= 0f)
        {
            attackCnt = 2.8f;

            float circleScale = Random.Range(2.8f, 5.6f); // ランダムなスケールを生成
            float finalScale = circleScale / 10; // スケールを10で割る
            float circleDuration = Random.Range(1.4f, 2.8f); // ランダムな持続時間を生成
            GameObject aoeBall = Instantiate(aoeMagicCircleObj, playerTrans.position + new Vector3(0,0.4f,0), Quaternion.identity);
            
            aoeBall.SetActive(true);
            AoeCircle aoeCircle = aoeBall.GetComponent<AoeCircle>();
            aoeCircle.InitCircle(circleScale, circleDuration, 10,finalScale); 

        }

        PlanStateChange(BossState.Move);
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
        stateInfoText = stateMachine.State.ToString();
        stateCnt -= Time.deltaTime;
    }

    public void UpdateGobalCounter()
    {
        attackCnt -= Time.deltaTime;
        turningCnt -= Time.deltaTime;
    }
    


}

