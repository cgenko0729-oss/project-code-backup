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

public class EnemyMidBossWarriorAction : BossActionBase
{
    public enum BossState
    {
        Idle,
        Move,
        Dizzy,
        Death,  
        CloaseAttack,
    }

    public float stateCnt;                    // 各状態の継続時間などをカウントするタイマー
    public StateMachine<BossState> stateMachine;　　// 状態遷移を管理するステートマシン
    public string stateInfoText; // 状態番号を文字列に変換するためのテキスト

    public float attackCnt = 0f;
    public float turningCnt = 0f;

    public Vector3 centerPos = Vector3.zero;

    public GameObject aoeRectObj;
    private AoeRectIndicatorLine aoeRectIndicator;

    private void Start()
    {
        InitBoss();

        stateMachine = StateMachine<BossState>.Initialize(this);
        stateMachine.ChangeState(BossState.Move);

        currentAtttackPattern = 0;

        animator.applyRootMotion = false;
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

    }

    void Move_Update()
    {
        HomingPlayer();
        RotateTowardPlayer();

        PlanStateChange(BossState.CloaseAttack);
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

    void CloaseAttack_Enter()
    {
        animator.SetBool("isIdling", true);

        stateCnt = 5f;

        SetInnerGlow();

        GameObject aoeRect = Instantiate(aoeRectObj, transform.position, Quaternion.identity);
        aoeRectIndicator = aoeRect.GetComponent<AoeRectIndicatorLine>();

        
        //Quaternion indicatorRotation =Quaternion.Euler(0f, 90f, 0f);
        
        //rotation = from here to player
        Quaternion indicatorRotation = Quaternion.LookRotation(playerTrans.position - transform.position);
        Quaternion aoeOffset = Quaternion.Euler(0f, -90f, 0f); 
        Quaternion aoeFinalRot = indicatorRotation * aoeOffset; 

        Vector3 indicatorPosition = transform.position;
        //indicatorPosition will be place in front of enemy toward player
        Vector3 dirToPlayer = (playerTrans.position - transform.position).normalized;
        float distToPlayerindi = Vector3.Distance(transform.position, playerTrans.position);
        indicatorPosition += dirToPlayer * (distToPlayerindi * 0.5f);


        float distToPlayerLength = Vector3.Distance(transform.position, playerTrans.position);

        Vector2 indicatorSize = new Vector2(1.4f, distToPlayerLength); 

         aoeRectIndicator.UpdateTransform(indicatorPosition, aoeFinalRot, indicatorSize);
         aoeRectIndicator.BeginFill(1.42f, 0.1f, 15, true);

        float landingOffset = 2.19f;
        Vector3 enemyPos  = transform.position;
        Vector3 playerPos = playerTrans.position;
        Vector3 dir = playerPos - enemyPos; 
        dir.y = 0f;
        float dist = dir.magnitude;
        if (dist > 0.001f) dir /= dist; else dir = transform.forward;
        float stop = Mathf.Max(0f, Mathf.Min(landingOffset, dist - 0.1f));
        Vector3 targetPoint = playerPos - dir * stop;
        // keep current height to avoid sudden Y jumps
        targetPoint.y = enemyPos.y;


        DOVirtual.DelayedCall(1f, () =>
        {
            animator.SetTrigger("isAttacking");
            transform.DOMove(targetPoint, 0.42f).SetEase(Ease.InQuad);
        });

    }

    void CloaseAttack_Update()
    {

        PlanStateChange(BossState.Move);

    }

    void CloaseAttack_Exit()
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
        stateInfoText = stateMachine.State.ToString();
        stateCnt -= Time.deltaTime;
    }

    public void UpdateGobalCounter()
    {
        attackCnt -= Time.deltaTime;
        turningCnt -= Time.deltaTime;
    }
}

