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

public class EnemyBossWitchAction : BossActionBase
{
    public enum BossState
    {
        Idle,
        Move,
        Dizzy,
        Death,
        MagicAttack,
        LineAttack,
        RotateAttack,
        CloseAttack,
        FlyMagicAttack,
        SlashWaveAttack,
    }

    public float stateCnt;                    // 各状態の継続時間などをカウントするタイマー
    public StateMachine<BossState> stateMachine;　　// 状態遷移を管理するステートマシン
    public string stateInfoText; // 状態番号を文字列に変換するためのテキスト

    public Vector3 centerPos = Vector3.zero;

    public GameObject magicBallPrefab;

   

    public GameObject aoeRectObj;
    private AoeRectIndicatorLine aoeRectIndicator;


    public Vector3 attackTagetPoint = Vector3.zero; // 攻撃対象の位置
    public float attackCnt = 0f;
    public float attackCntMax = 1f;
    public GameObject rotateMagicBall;

    public GameObject closeAttackIndicator;

    public GameObject[] lineAttackObj;

    public GameObject flyAttackAoeCircle;
   
    public Transform rootTrans;

    public GameObject slashWaveAttackObj;

    private void Start()
    {
      
        stateMachine = StateMachine<BossState>.Initialize(this);
        stateMachine.ChangeState(BossState.SlashWaveAttack);

        InitBoss();
    }

    private void Update()
    {
        UpdateStateInfo();
        UpdatePlayerInfo();
    }


    void Move_Enter()
    {
        stateCnt = 3f;
    }

    void Move_Update()
    {

        //PlanStateChange(BossState.LineAttack);
        //PlanStateChange(BossState.FlyMagicAttack);
        PlanStateChange(BossState.SlashWaveAttack);


        RotateTowardPlayer();
        HomingPlayer();

    }


    void SlashWaveAttack_Enter()
    {
        stateCnt = 4.2f;
        FacePlayerNow();

        ShotSlashWaveProjectile(3, 21);
    }

    void SlashWaveAttack_Update()
    {

        PlanStateChange(BossState.Move);
    }

    public void ShotSlashWaveProjectile(int projectileCount, float sectorAngle)
    {         
        Vector3 spawnOffset = Vector3.up * 0.5f; 
        //float speed = 8f;                

        float angleStep = (projectileCount > 1)? sectorAngle / (projectileCount - 1) : 0f;
        float halfSector = sectorAngle * 0.5f; //-halfSector = left → right
        Vector3 origin = transform.position + spawnOffset;

        for (int i = 0; i < projectileCount; i++)
        {
           float currentAngle = -halfSector + angleStep * i;
           Vector3 dir = Quaternion.Euler(0f, currentAngle, 0f) * transform.forward;
           Quaternion look = Quaternion.LookRotation(dir);
           Quaternion offset = Quaternion.Euler(-90f, 0f, 0f);

           GameObject web = Instantiate(slashWaveAttackObj,origin,look * offset);
           var mov = web.GetComponent<EffectMoveController>();
           if (mov != null) mov.moveVec = dir;
                                   
        }

    } 
   

    void FlyMagicAttack_Enter()
    {
        stateCnt = 5.6f;

        FacePlayerNow();
        attackTagetPoint = playerTrans.position + (transform.forward * 3.5f); // 攻撃対象の位置をプレイヤーの後ろに設定
        attackCnt = 1f;

         transform.DOMove(attackTagetPoint, 4f).SetEase(Ease.InOutQuad).OnComplete(() => {
            
        });

    
    }

    void FlyMagicAttack_Update()
    {

        attackCnt -= Time.deltaTime;

        if(attackCnt <= 0)
        {
            attackCnt = 1f;
            Instantiate(flyAttackAoeCircle, transform.position + new Vector3(0, 0.1f, 0), Quaternion.identity);
        }

        PlanStateChange(BossState.Move);

      

    }

    void FlyMagicAttack_Exit()
    {
       
    }

    void CloseAttack_Enter()
    {
        stateCnt = 5f;

        Vector3 playerForward = playerTrans.forward;
        float distAroundPlayer = 2.8f;
        Vector3 posAroundPlayer = playerTrans.position + playerForward * distAroundPlayer;


        EnemyAnimUtil.TeleportBlink(transform, posAroundPlayer,0.77f,0.77f,CloseAttackAnimDelay);

    }

    void CloseAttackAnimDelay()
    {
        FacePlayerNow();
        Quaternion rotTowardPlayer = Quaternion.LookRotation(playerTrans.position - transform.position);
        GameObject aoeSectorObj = Instantiate(closeAttackIndicator.gameObject, transform.position + new Vector3(0, 0.1f, 0), rotTowardPlayer);
        AoeSector sector = aoeSectorObj.GetComponentInChildren<AoeSector>();
        sector.fillDuraiton = 0.35f;


        Invoke("CloseAttackAnim", 0.7f);
    }

    void CloseAttackAnim()
    {
        animator.SetTrigger("isAttack");
        animator.SetBool("isMoving", true);
    }

    void CloseAttack_Update()
    {
        
        PlanStateChange(BossState.Move);
    }


    void RotateAttack_Enter()
    {
        stateCnt = 7f;

    }

    void KeepRotating()
    {
        //keep rotatiing in y 
        Vector3 rotation = new Vector3(0, 1, 0) * Time.deltaTime * 365f; // 50 degrees per second
        transform.Rotate(rotation, Space.World);


    }

    void RotateAttack_Update()
    {
        attackCnt -= Time.deltaTime;

        if(attackCnt <= 0f)
        {
            attackCnt = 0.21f;
            //Spawn rotate magic ball
            SkillForwardMove magicBall = Instantiate(rotateMagicBall, transform.position + new Vector3(0,0.7f,0), Quaternion.identity).GetComponent<SkillForwardMove>();
            magicBall.moveVec = transform.forward * 7f;
            Debug.Log(magicBall.moveVec);

        }

        KeepRotating();

        PlanStateChange(BossState.Move);
    }

    async void MagicAttack_Enter()
    {
        FacePlayerNow();
        stateCnt = 4f;

        //EnemyAnimUtil.TurnAround360Degree(transform);
        //EnemyAnimUtil.Backstep(transform);
        //EnemyAnimUtil.StrafeArcAround(transform, playerTrans, 7.7f, 360, 3);
        //EnemyAnimUtil.JumpTo(transform, new Vector3(0, 0.2f, 0));
        //EnemyAnimUtil.HoverBob(transform);
        //EnemyAnimUtil.TeleportBlink(transform, playerTrans.position);
        //EnemyAnimUtil.SlamWindupAndImpact(transform);
        //EnemyAnimUtil.ZigZagAdvance(transform, transform.forward);
        //EnemyAnimUtil.SpiralIn(transform, playerTrans, 3, 7.7f, 4, 4);

        SpawnProjectiles();

    }

    void MagicAttack_Update()
    {
       
        PlanStateChange(BossState.Move);
    }

    void MagicAttack_Exit()
    {

    }

    void LineAttack_Enter()
    {
        stateCnt = 5f;

        foreach (GameObject lineAttack in lineAttackObj)
        {
            lineAttack.SetActive(true);
        }

    }

    void LineAttack_Update()
    {

        KeepRotating();
        
        PlanStateChange(BossState.Move);
    }

    void LineAttack_Exit()
    {

        foreach (GameObject lineAttack in lineAttackObj)
        {
            lineAttack.SetActive(false);
        }
    }

    async void SpawnProjectiles()
    {
        int projectileNum = 3; 
        float slice = 360f / projectileNum;

        for(int i = 0; i < projectileNum; i++)
        {
            float phase = slice * i * Mathf.Deg2Rad;
            BossMagicBallCircleMove magicBall = Instantiate(magicBallPrefab, transform.position, Quaternion.identity).GetComponent<BossMagicBallCircleMove>();
            magicBall.Init(transform, phase, 0.21f, 2.8f, 7f+(i*1f));
        }
        
       
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
   
    public void UpdateStateInfo()
    {
        stateInfoText = stateMachine.State.ToString();
        stateCnt -= Time.deltaTime;
    }

    void PlanStateChange(BossState targetState)
    {
       if(stateCnt <= 0)
        {
            ChangeToState(targetState);
        }
    }
}

