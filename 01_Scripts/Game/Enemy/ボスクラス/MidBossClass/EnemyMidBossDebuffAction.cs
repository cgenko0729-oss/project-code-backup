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

public class EnemyMidBossDebuffAction : BossActionBase
{
    public enum BossState
    {
        Idle,
        Move,
        Dizzy,
        Death,     
        DebuffAttack,
    }

    public float stateCnt;                    // 各状態の継続時間などをカウントするタイマー
    public StateMachine<BossState> stateMachine;　　// 状態遷移を管理するステートマシン
    public string stateInfoText; // 状態番号を文字列に変換するためのテキスト

    public float attackCnt = 0f;
    public float turningCnt = 0f;

    public Vector3 centerPos = Vector3.zero;

    public GameObject aoeCircleDynamicObj;
    public AoeCircleDynamic aoeCircleIndicator;
    public float aoeTraceCnt = 0;
    public bool isStartFilling = false;
    public bool isEndFilling = false;

    public GameObject debuggBoxDashItem;
    public GameObject debuggBoxCastItem;
    public GameObject debuggBoxMoveItem;

    public bool hasFirstAttack = false;

    public float distNeedToAttack = 2.8f;

    public AudioClip appearSe;

    public float aoeCircleRadius = 7f;

    private void Start()
    {
        InitBoss();

        stateMachine = StateMachine<BossState>.Initialize(this);
        stateMachine.ChangeState(BossState.DebuffAttack);

        currentAtttackPattern = 0;
    }

    private void Update()
    {
        UpdateStateInfo();
        UpdateGobalCounter();
        UpdatePlayerInfo();
    }


    void Move_Enter()
    {
        stateCnt = 4.2f;

        

        //if (!hasFirstAttack)
        //{
        //    hasFirstAttack = true;
        //    stateCnt = 49f;
        //}
        //else
        //{
        //    stateCnt = 140f;
        //}

    }

    void Move_Update()
    {
        //HomingPlayer();
        //RotateTowardPlayer();


        PlanStateChange(BossState.DebuffAttack);

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

    void DebuffAttack_Enter()
    {
        stateCnt = 7.7f;

        attackCnt = 2.1f;

        aoeTraceCnt = 0;

        isStartFilling = false;
        isEndFilling = false;

        SetInnerGlow(0.77f,0.28f,4);
        //SoundEffect.Instance.PlayOneSound(appearSe, 0.7f);


        switch (StageManager.Instance.mapData.stageDifficulty)
                    {
                        case DifficultyType.Normal:
                            //aoeCircleRadius = 1.49f;
                            aoeCircleRadius = 7f;
                            break;
                        case DifficultyType.Hard:
                           //aoeCircleRadius = 2.19f;
                           aoeCircleRadius = 7.9f;
                            break;
                        case DifficultyType.Nightmare:
                            //aoeCircleRadius = 2.19f;
                            aoeCircleRadius = 11.1f;
                            break;
                        case DifficultyType.Hell:
                           // aoeCircleRadius = 2.19f;
                            aoeCircleRadius = 11.1f;
                            Debug.Log("Hell Mode AOE Radius:"+ aoeCircleRadius);
                break;
                        default:
                            //aoeCircleRadius = 2.19f;
                            break;
                    }
    }

    void DebuffAttack_Update()
    {

        

        if (!isStartFilling)
        {
            HomingPlayer();
            RotateTowardPlayer();

            //fix pos Y in 1.76: 
            transform.position = new Vector3(transform.position.x, 2.1f, transform.position.z);
        }
        

        if(distToPlayer < distNeedToAttack && !isStartFilling)
        {
            

            //transform.position = new Vector3(transform.position.x, 1.76f, transform.position.z);

            isStartFilling = true;
            aoeTraceCnt = 3.5f;
            aoeCircleIndicator = Instantiate(aoeCircleDynamicObj,transform.position,Quaternion.identity).GetComponent<AoeCircleDynamic>();
            
            int randomIndex = Random.Range(0, 3);
            if(randomIndex == 1) aoeCircleIndicator.isBlockCast = true;
            else if (randomIndex == 2) aoeCircleIndicator.isBlockDash = true;
            else if (randomIndex == 0) aoeCircleIndicator.isBlockMove = true;

            Quaternion rot = Quaternion.Euler(-90f, 0f, 0f);
            Instantiate(debuggBoxDashItem,transform.position, rot);

            SoundEffect.Instance.PlayOneSound(appearSe, 0.7f);

        }

        if (isStartFilling)
        {

            transform.position += (transform.position - playerTrans.position).normalized * 14f * Time.deltaTime;   //HomingAwayFromPlayer
            RotateOppositeAgainstPlayer(14);

            if (aoeTraceCnt > 0)
            {
                aoeTraceCnt -= Time.deltaTime;
                Vector3 indicatorPos = playerTrans.position;
                indicatorPos.y = 0.1f; 
                float indicatorRadius = 7f;
                aoeCircleIndicator.UpdateTransform(indicatorPos, aoeCircleRadius);

            }
            else
            {
                if (!isEndFilling)
                {
                    isEndFilling = true;

                    

                    

                    aoeCircleIndicator.BeginFill(1.49f, 1f, 1);
                    Vector3 indicatorPos = playerTrans.position;
                indicatorPos.y = 0.1f; 
                    aoeCircleIndicator.UpdateTransform(indicatorPos, aoeCircleRadius);
                    //ChangeToState(BossState.Move);

                    Invoke("DelayDestry", 4.9f);

                }
            }

        }

        

        


        //PlanStateChange(BossState.Move);

    }


    //==========================Utility&Helper===================================//

   public void DelayDestry()
    {
        Destroy(gameObject);

    }

    public void RotateOppositeAgainstPlayer(float rotateSpeed = 5f)
    {
        if (playerTrans == null) return;
        Vector3 direction = transform.position - playerTrans.position;
        direction.y = 0;
        if (direction == Vector3.zero) return;
        Quaternion targetLookRotation = Quaternion.LookRotation(-direction);
        float targetYAngle = targetLookRotation.eulerAngles.y;
        Quaternion finalTargetRotation = Quaternion.Euler(-180f, targetYAngle, 180f);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalTargetRotation, Time.deltaTime * rotateSpeed);
    }

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

