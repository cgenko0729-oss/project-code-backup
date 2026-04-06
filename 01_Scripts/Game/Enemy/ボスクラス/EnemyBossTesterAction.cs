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

public class EnemyBossTesterAction : BossActionBase
{
    public enum BossState
    {
        Idle,
        Move,
        Dizzy,
        Death,
        MagicAttack,
        LineAttack,
    }

    public float stateCnt;                    // 各状態の継続時間などをカウントするタイマー
    public StateMachine<BossState> stateMachine;　　// 状態遷移を管理するステートマシン
    public string stateInfoText; // 状態番号を文字列に変換するためのテキスト

    public Vector3 centerPos = Vector3.zero;

    public GameObject magicProjectilePrefab;

    public Transform headTransform;
    public float projectileSpawnInterval = 1.0f; 

    public int projectileCount = 5;
    public float formationRadius = 2.5f; // How far from the head the projectiles form up
    public float formationArcDegrees = 45f; // The total angle of the arc
    public float formationMoveTime = 1.0f; // Time for projectiles to get into position
    public float postFormationDelay = 2.0f; // The 2-second wait
    public float homingDuration = 3.0f; // The 3-second homing time

    public GameObject aoeRectObj;
    private AoeRectIndicatorLine aoeRectIndicator;

    
   

    private void Start()
    {
      
        stateMachine = StateMachine<BossState>.Initialize(this);
        stateMachine.ChangeState(BossState.Move);

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

        PlanStateChange(BossState.LineAttack);
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
        EnemyAnimUtil.SpiralIn(transform, playerTrans, 3, 7.7f, 4, 4);

        SpawnProjectiles();

    }

    void MagicAttack_Update()
    {
       
        PlanStateChange(BossState.Move);
    }

    void MagicAttack_Exit()
    {

    }

    public float rectSizeX = 7f;
    public float rectSizeZ = 14f;

    void LineAttack_Enter()
    {
        stateCnt = 5f;

        GameObject aoeRect = Instantiate(aoeRectObj, transform.position, Quaternion.identity);
        aoeRectIndicator = aoeRect.GetComponent<AoeRectIndicatorLine>();
       
        //Vector3 indicatorPosition = transform.position + dirToPlayer * (distToPlayer * 0.5f);
        Vector3 indicatorPosition = transform.position;
        Quaternion indicatorRotation =Quaternion.Euler(0f, 90f, 0f);
        Vector2 indicatorSize = new Vector2(rectSizeX, rectSizeZ); 
        
        aoeRectIndicator.UpdateTransform(indicatorPosition, indicatorRotation, indicatorSize);
        aoeRectIndicator.BeginFill(3f, 0.1f, 20, true);
    }

    void LineAttack_Update()
    {
        
        PlanStateChange(BossState.Move);
    }

    async void SpawnProjectiles()
    {
       
        float angleStep = formationArcDegrees / (projectileCount - 1);
        float startAngle = -formationArcDegrees / 2f;

        for (int i = 0; i < projectileCount; i++)
        {            
            float currentAngle = startAngle + (i * angleStep); // --- Calculate the target formation position for each projectile ---
            
            // Calculate the position in an arc in front of and above the boss's head
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 offset = rotation * (transform.forward * formationRadius) + (Vector3.up * 1.5f); // 1.5f is vertical offset
            Vector3 formationPosition = headTransform.position + offset;

            GameObject magicBall = Instantiate(magicProjectilePrefab, transform.position, Quaternion.identity);
            var projectileScript = magicBall.GetComponent<BossHomingBullet>();
            projectileScript.LaunchSequence(formationPosition, formationMoveTime, postFormationDelay, homingDuration);

            if (i < projectileCount - 1)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(projectileSpawnInterval), cancellationToken: this.GetCancellationTokenOnDestroy());
            }

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

