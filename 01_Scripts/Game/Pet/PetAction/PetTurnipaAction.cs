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

public class PetTurnipaAction : MonoBehaviour
{
    public enum PetState
    {
        Idle,
        Move,
        Attack,
    }


    public StateMachine<PetState> sm;
    public float stateCnt = 3f; // 状態遷移のカウントダウン
    public string statetext;

    Transform playerTrans;

    public Animator animator;


    void Start()
    {

        animator = GetComponent<Animator>();
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;

        sm = StateMachine<PetState>.Initialize(this);
        sm.ChangeState(PetState.Move);

        

    }
    void Update()
    {

        stateCnt -= Time.deltaTime; // 状態遷移のカウントダウン

        statetext = sm.State.ToString(); // 状態名を文字列に変換


        //if press T key 
        if (Input.GetKeyDown(KeyCode.T))
        {
           sm.ChangeState(PetState.Idle);
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            sm.ChangeState(PetState.Move);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            sm.ChangeState(PetState.Attack);
        }



    }

    //Enter , Update , Exit 

    private void Move_Enter() //Walk State入る時一回実行する
    {
        animator.SetBool("isWalking", true);
        animator.SetBool("isIdle", false);
        Debug.Log("Walk State Entered");

        stateCnt = 3f;
    }

    private void Move_Update()　//Walk Stateにいる時ずっと実行する
    {


        if(stateCnt <= 0)
        {
            sm.ChangeState(PetState.Idle);
        }

    }

    private void Move_Exit()　//Walk Stateにいない時一回実行する
    {
        
    }

    private void Idle_Enter() //Walk State入る時一回実行する
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isIdle", true);
        Debug.Log("Idle State Entered");


        stateCnt = 4f;

    }

    private void Idle_Update()　//Walk Stateにいる時ずっと実行する
    {


        if(stateCnt <= 0)
        {
            sm.ChangeState(PetState.Move);
        }

    }

    private void Idle_Exit()　//Walk Stateにいない時一回実行する
    {
        
    }

    private void Attack_Enter() //Walk State入る時一回実行する
    {
        animator.SetTrigger("isAttack");
        animator.SetBool("isWalking", false);
        animator.SetBool("isIdle", false);



    }

    private void Attack_Update()　//Walk Stateにいる時ずっと実行する
    {

    }

    private void Attack_Exit()　//Walk Stateにいない時一回実行する
    {
        
    }



}

