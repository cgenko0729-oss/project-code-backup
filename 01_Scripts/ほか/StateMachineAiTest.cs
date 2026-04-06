using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class StateMachineAiTest : MonoBehaviour
{


    public enum EnemyState {
        Idle,
        Chase,
        Attack,
        Dead

    }

    StateMachine<EnemyState> fsm;

    public bool isIdle = false;
    public bool isChase = false;
    public bool isAttack = false;
    public bool isDead = false;

    public float stateCnt= 70f;


    void Awake()
    {
        fsm = StateMachine<EnemyState>.Initialize(this, EnemyState.Idle);
        //fsm = new StateMachine<DragonStates>(this);
        //fsm.ChangeState(DragonStates.Idle);
        
    }

    void Idle_Enter()
    {
        isIdle = true;
        isChase = false;
        isAttack = false;
        isDead = false;

        Debug.Log("Entering Idle State");

    }

    void Idle_Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("Input Space Key in Idle State!");
            Debug.Log("Transitioning from Idle State to Chase State");
            fsm.ChangeState(EnemyState.Chase);
        }

        stateCnt -= Time.deltaTime;
    }

    void Idle_Exit()
    {
        Debug.Log("Exiting Idle State");
    }

    void Chase_Enter()
    {
        isIdle = false;
        isChase = true;
        isAttack = false;
        isDead = false;

        Debug.Log("Entering Chase State");
    }

    void Chase_Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("Input Space Key in Chase State!");
            Debug.Log("Transitioning from Chase State to Attack State");
            fsm.ChangeState(EnemyState.Attack);
        }
    }

    void Chase_Exit()
    {
        Debug.Log("Exiting Chase State");
    }

    void Attack_Enter()
    {
        isIdle = false;
        isChase = false;
        isAttack = true;
        isDead = false;

        Debug.Log("Entering Attack State");
    }

    void Attack_Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("Input Space Key in Attack State!");
            Debug.Log("Transitioning from Attack State to Dead State");
            fsm.ChangeState(EnemyState.Dead);
        }
    }
    void Attack_Exit()
    {
        Debug.Log("Exiting Attack State");
    }



    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

