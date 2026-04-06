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

public class EnemyBossDevilTreeAction : BossActionBase
{
    public enum BossState
    {
        Idle,
        Move,
        Dizzy,
        Death,      
    }

    public float stateCnt;                    // 各状態の継続時間などをカウントするタイマー
    public StateMachine<BossState> stateMachine;　　// 状態遷移を管理するステートマシン
    public string stateInfoText; // 状態番号を文字列に変換するためのテキスト
    
    public Vector3 centerPos = Vector3.zero;


    void Start()
    {
        InitBoss();
        stateMachine = StateMachine<BossState>.Initialize(this);
        stateMachine.ChangeState(BossState.Move);
    }

    void Update()
    {
        UpdatePlayerInfo();
        UpdateStateInfo();
        
    }

    void Move_Enter()
    {

    }

    void Move_Update()
    {

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
    public void UpdateStateInfo()
    {
        stateInfoText = stateMachine.State.ToString();
        stateCnt -= Time.deltaTime;
    }

    public void ChangeToState(BossState newState)
    {
        stateMachine.ChangeState(newState);
        stateInfoText = stateMachine.State.ToString(); // 状態名を文字列に変換
    }
    public void ChangeToMoveState() => stateMachine.ChangeState(BossState.Move);
   

}

