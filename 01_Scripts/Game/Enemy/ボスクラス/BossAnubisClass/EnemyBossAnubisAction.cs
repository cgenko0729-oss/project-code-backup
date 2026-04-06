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
using System;
using System.Collections;

public class EnemyBossAnubisAction : BossActionBase
{
     public enum BossState
    {
        Idle,
        Move,
        Dizzy,
        Death,     
        ThrowStoneAttack,
        TrialAttackAllway,
        TrialAttackUpDown,
        TrialAttackLeftRight,
        SummonAttack,
        ThunderPillarAttack,
        ThunderLineAttack,
        JumpAttack,
        BackSlashAttack,
        BoostAttack,
        HomingMissileAttack,
        RotateAttack,

    }

    public float stateCnt;                    // 各状態の継続時間などをカウントするタイマー
    public StateMachine<BossState> stateMachine;　　// 状態遷移を管理するステートマシン
    public string stateInfoText; // 状態番号を文字列に変換するためのテキスト

    public float attackCnt = 0f;
    public float turningCnt = 0f;

    public Vector3 centerPos = Vector3.zero;

    public ParticleSystem magicCastRingAPs;
    public ParticleSystem magicCastRingBPs;

    //Thunder Line& Pillar Attack
    public GameObject thunderPillarObj;
    public GameObject thunderLineObj;


    public ParticleSystem junpLandingDustPs;
    public GameObject magicProjectileObj;
    [SerializeField] AnimationCurve timeByDistance   = AnimationCurve.Linear(0, 0.4f, 20, 1.4f);
    [SerializeField] AnimationCurve heightByDistance = AnimationCurve.Linear(0, 3f, 20, 8f);
    [SerializeField] float minFlightTime  = 0.25f;     
    [SerializeField] float maxFlightTime  = 2.0f;
    [SerializeField] float maxArcHeight   = 10f;
    public Vector3 jumpTargetPoint = Vector3.zero;

    public PlayerState playerState;
    public Vector3 playerFaceVec;

    public List<Vector3> spawnedPillarPositions = new List<Vector3>();
    public int spawnCount = 7;
    public float spawnInterval = 0.21f;
    public float pillarRadius = 10f;
    public float pillarMinDistance = 1.4f;

    public GameObject boostAttackCircleObj;
    public AoeCircle aoeCircle;
    public bool isFinishedBoost = false;

    public AudioClip teleportWarpSe;
    public AudioClip teleportSe;
    public AudioClip thunderSlashAttackSe;

    private void Start()
    {
        InitBoss();

        stateMachine = StateMachine<BossState>.Initialize(this);
        stateMachine.ChangeState(BossState.Move);

        currentAtttackPattern = 0;

        playerState = playerTrans.GetComponent<PlayerState>();
        playerFaceVec = playerTrans.forward;
    }

    private void Update()
    {
        UpdateStateInfo();
        UpdateGobalCounter();

    }


    void Move_Enter()
    {
        stateCnt = 4.2f;
        animator.SetBool("isIdleing", false);
        animator.SetBool("isMoving", true);

    }

    void Move_Update()
    {
        HomingPlayer();
        RotateTowardPlayer();

        if (stateCnt <= 0)
        {
            switch (currentAtttackPattern)
            {
                case 0:
                    PlanStateChange(BossState.ThunderPillarAttack);
                    break;
                case 1:
                    PlanStateChange(BossState.ThunderLineAttack);
                    break;
                case 2:
                    PlanStateChange(BossState.JumpAttack);
                    break;
                case 3:
                    PlanStateChange(BossState.BackSlashAttack);
                    break;
                case 4:
                    PlanStateChange(BossState.ThunderLineAttack);
                    currentAtttackPattern = -1;
                    break;

                default:
                    break;
            }

            currentAtttackPattern++;
        }





        //PlanStateChange(BossState.ThunderPillarAttack);
        //PlanStateChange(BossState.ThunderLineAttack);
        //PlanStateChange(BossState.JumpAttack);
        //PlanStateChange(BossState.BackSlashAttack);
        //PlanStateChange(BossState.BoostAttack);
        //PlanStateChange(BossState.RotateAttack);

    }

    void Dizzy_Enter()
    {

    }

    void Dizzy_Update()
    {

    }

    void Death_Enter()
    {
        animator.SetBool("isIdleing", false);
        animator.SetBool("isMoving", false);
        animator.SetTrigger("isDead");

    }

    void Death_Update()
    {
       
    }

    void RotateAttack_Enter()
    {
        stateCnt = 10f;
        attackCnt = 1.4f;
        SetInnerGlow();

        EnemyAnimUtil.TeleportBlink(transform, Vector3.zero, 1f, 1f,
           () => {
               //PlayAttackAni();
           });
    }

    public float rotateSpeed = 21f;

    void RotateAttack_Update()
    {
        

        //keep rotateing in y axis
        this.transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        PlanStateChange(BossState.Move);

    }




    public GameObject homingMissleObject;

    void HomingMissileAttack_Enter()
    {
        stateCnt = 7f;
        attackCnt = 2.1f;
        SetInnerGlow();
    }

    void HHomingMissileAttack_Update()
    {
        if(attackCnt <= 0)
        {
            attackCnt = 0.21f;
            SpawnHomingMissile();
        }

    }

    void SpawnHomingMissile()
    {
        Instantiate(homingMissleObject, this.transform.position + Vector3.up * 2.1f, Quaternion.identity);
    }

    void BoostAttack_Enter()
    {
        magicCastRingAPs.Play();
        stateCnt = 21f;

        attackCnt = 1.4f;

        isFinishedBoost = false;

        aoeCircle = Instantiate(boostAttackCircleObj, this.transform.position , Quaternion.Euler(0f, 0f, 0f)).GetComponent<AoeCircle>();

        PlayIdleAni();
        PlayAttackAni();

    }

    void BoostAttack_Update()
    {

        if(attackCnt <= 0f)
        {
            attackCnt = 1.4f;
            aoeCircle.AddOuterCircleSize(0.14f);
        }

        if(stateCnt <= 0 && !isFinishedBoost)
        {
            isFinishedBoost = true;
            aoeCircle.StartExpandCircle();
            Invoke(nameof(ChangeToMoveState), 2.8f);
        }
        

    }

    void BoostAttack_Exit()
    {

    }



    void ThunderPillarAttack_Enter()
    {
        stateCnt = 7f;
        attackCnt = 3.5f;

        magicCastRingAPs.Play();      
        animator.SetBool("isIdleing", true);
        animator.SetBool("isMoving", false);

    }

    void ThunderPillarAttack_Update()
    {

        RotateTowardPlayer();

        if(attackCnt <= 0f)
        {
            attackCnt = 7f;
            EnemyEffectManager.Instance.SpawnAoeCircle(playerTrans.position,4.9f, 2.8f);
            //Instantiate(thunderPillarObj, new Vector3(playerTrans.position.x, 8.4f, playerTrans.position.z), Quaternion.identity);
            
            StartCoroutine(SpawnThunderPillars(spawnCount, spawnInterval, pillarRadius, pillarMinDistance));
            
            PlayAttackAni();

            //SoundEffect.Instance.PlayOneSound(thunderSlashAttackSe, 0.7f);    
        }


        PlanStateChange(BossState.Move);
    }

    void ThunderPillarAttack_Exit()
    {
        animator.SetBool("isIdleing", false);

    }


    IEnumerator SpawnThunderPillars(int count, float spawnInterval, float radius, float minDistance)
    {
        spawnedPillarPositions.Clear();

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = Vector3.zero;
            bool valid = false;
            int attempt = 0;

            while (!valid && attempt < 30) // avoid infinite loop
            {
                attempt++;

                // random position in circle (XZ plane)
                Vector2 randCircle = UnityEngine.Random.insideUnitCircle * radius;
                spawnPos = playerTrans.position + new Vector3(randCircle.x, 0f, randCircle.y);

                // pillar spawn height
                spawnPos.y = 8.4f;

                // 1) check distance to player
                if (Vector3.Distance(spawnPos, playerTrans.position) < minDistance) continue;

                // 2) check distance to other pillars
                bool tooClose = false;
                foreach (var pos in spawnedPillarPositions)
                {
                    if (Vector3.Distance(spawnPos, pos) < minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose) valid = true;
            }

            if (valid)
            {
                SoundEffect.Instance.Play(SoundList.TurnipaBossPlaceAoeSe);
                spawnedPillarPositions.Add(spawnPos);
                Instantiate(thunderPillarObj, spawnPos, Quaternion.identity);
                EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos,4.9f, 2.8f);
                DOVirtual.DelayedCall(2.8f, DelayPlayThunderCircleSe);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void DelayPlayThunderCircleSe()
    {
        //SoundEffect.Instance.Play(SoundList.SpiderBossThunderBallSe);
        SoundEffect.Instance.PlayOneSound(teleportSe, 0.7f);
    }



    void ThunderLineAttack_Enter()
    {
        stateCnt = 7f;
        attackCnt = 1.5f;
        attackTime = 0;

        animator.SetBool("isIdleing", true);
        animator.SetBool("isMoving", false);
        SetInnerGlow();

    }


     public float spd = 3.59f;
     public float space = 8.4f;
    public float thunderlineWaitTime = 2.1f;
     public float spd2 = 3.5f;
     public float space2 = 7.7f;

    void ThunderLineAttack_Update()
    {
        if (attackCnt <= 0f)
        {
            animator.SetTrigger("isAttacking");
            FacePlayerNow();

            attackTime++;
            
            attackCnt = thunderlineWaitTime;

             if(attackTime >= 2)
            {
                //stateCnt = 0;
                //ChangeToMoveState();
                attackCnt = 999f;

            }
            
            PlayAttackAni();
            DelayPlayThunderSlashSe();

            //spawn two thunder line obj in left and right of player with 7f distance

           

            if(attackTime == 1)
            {
                Vector3 spawnPosLeft = playerTrans.position + new Vector3(-space, 0, 0);
            Quaternion spawnRotLeft = Quaternion.LookRotation(playerTrans.position - spawnPosLeft);
            Vector3 moveVecLeft = (playerTrans.position - spawnPosLeft).normalized;
            SkillForwardMove lineMove = Instantiate(thunderLineObj, new Vector3(spawnPosLeft.x, 0.21f, spawnPosLeft.z), spawnRotLeft).GetComponent<SkillForwardMove>();
            lineMove.moveVec = moveVecLeft*spd;

            Vector3 spawnPosRight = playerTrans.position + new Vector3(space, 0, 0);
            Quaternion spawnRotRight = Quaternion.LookRotation(playerTrans.position - spawnPosRight);
            Vector3 moveVecRight = (playerTrans.position - spawnPosRight).normalized;
            SkillForwardMove lineMoveR = Instantiate(thunderLineObj, new Vector3(spawnPosRight.x, 0.21f, spawnPosRight.z), spawnRotRight).GetComponent<SkillForwardMove>();
            lineMoveR.moveVec = moveVecRight * spd;

            }
            else if (attackTime == 2)
            {
               Vector3 spawnPosUp = playerTrans.position + new Vector3(0, 0, space2);
            Quaternion spawnRotUp = Quaternion.LookRotation(playerTrans.position - spawnPosUp);
            Vector3 moveVecUp = (playerTrans.position - spawnPosUp).normalized;
            SkillForwardMove lineMoveU = Instantiate(thunderLineObj, new Vector3(spawnPosUp.x, 0.21f, spawnPosUp.z), spawnRotUp).GetComponent<SkillForwardMove>();
            lineMoveU.moveVec = moveVecUp * spd2;

            Vector3 spawnPosDown = playerTrans.position + new Vector3(0, 0, -space2);
            Quaternion spawnRotDown = Quaternion.LookRotation(playerTrans.position - spawnPosDown);
            Vector3 moveVecDown = (playerTrans.position - spawnPosDown).normalized;
            SkillForwardMove lineMoveD = Instantiate(thunderLineObj, new Vector3(spawnPosDown.x, 0.21f, spawnPosDown.z), spawnRotDown).GetComponent<SkillForwardMove>();
            lineMoveD.moveVec = moveVecDown * spd2;
            }

            

            





        }

        PlanStateChange(BossState.Move);

    }

    void ThunderLineAttack_Exit()
    {
        animator.SetBool("isIdleing", false);

    }

    void JumpAttack_Enter()
    {
        stateCnt = 13f;

        animator.SetBool("isIdleing", true);
        animator.SetBool("isMoving", false);

        SetInnerGlow();

        jumpTargetPoint = playerTrans.position;
        FaceTargetNow(jumpTargetPoint);

        attackCnt = 1.4f;
        EnemyEffectManager.Instance.SpawnAoeCircle(jumpTargetPoint, 4.9f, 1.4f);

        attackTime = 0;

    }

    void DelayPlayThunderSlashSe()
    {
        SoundEffect.Instance.PlayOneSound(thunderSlashAttackSe, 0.7f);  
    }

    void JumpAttack_Update()
    {
       
        //if(attackCnt >= 0f)
        //{
        //    RotateTowardPlayer();
        //}

        if(attackCnt <= 0f)
        {
             attackTime++;
            attackCnt = 10f;
            
            
            float dist = Vector3.Distance(transform.position, jumpTargetPoint);
            float t = Mathf.Clamp(timeByDistance.Evaluate(dist), minFlightTime, maxFlightTime);
            float h = Mathf.Min(heightByDistance.Evaluate(dist), maxArcHeight);

            SoundEffect.Instance.PlayOneSound(teleportWarpSe, 0.7f);

            Invoke(nameof(PlayAttackAni), t/1.5f);
            Invoke(nameof(DelayPlayThunderSlashSe), t / 1.5f);

            //transform.DOJump(jumpTargetPoint, h, 1, t, snapping: false).SetEase(Ease.Linear) //where it lands, how high above its start it goes, number of “jumps” (use 1 for a single arc), total flight time
            //.OnComplete(() => {
            //    junpLandingDustPs.Play();

            //    ReleaseACirclesOfMagicBullet(7);

            //    Invoke(nameof(PrepareNewJump), 1.4f);

            //    //Release Circle Magic Bullet Attack
            //});

            FacePlayerNow();

            EnemyAnimUtil.TeleportBlink(transform, jumpTargetPoint, t/2, t/2,
           () => {
               junpLandingDustPs.Play();
               FacePlayerNow();
                
                ReleaseACirclesOfMagicBullet(7);

                if(attackTime <3)Invoke(nameof(PrepareNewJump), 1.4f);
           });

        }

        

        PlanStateChange(BossState.Move);
    }

    void PrepareNewJump()
    {
        attackCnt = 2.1f;
        jumpTargetPoint = playerTrans.position;
        FaceTargetNow(jumpTargetPoint);
        EnemyEffectManager.Instance.SpawnAoeCircle(jumpTargetPoint, 4.9f, 2.8f);
    }

    void ReleaseACirclesOfMagicBullet(int spawnNum)
    {

        //float angleStep = 360f / bulletNum;
        //float angle = 0f;

        for (int i = 0; i < spawnNum; i++)
        {
            
            float angle = i * (360f / spawnNum) * Mathf.Deg2Rad; // ラジアンに変換
            Vector3 targetPos = new Vector3(
                this.transform.position.x + Mathf.Cos(angle) * 10f,
                this.transform.position.y,
                this.transform.position.z + Mathf.Sin(angle) * 10f
            );

            Quaternion spawnRot = Quaternion.LookRotation(targetPos - this.transform.position);

            GameObject bulletObj = Instantiate(magicProjectileObj, this.transform.position + Vector3.up * 2.1f, spawnRot);
            SkillForwardMove bulletMove = bulletObj.GetComponent<SkillForwardMove>();
            bulletMove.moveVec = (targetPos - this.transform.position).normalized * 11.5f;

        }
    }


    public int attackTime = 0;

    void BackSlashAttack_Enter()
    {

        Vector3 playerBack = -playerTrans.forward; // directly behind the player
        Vector3 teleportPos = playerTrans.position + playerBack * 3f + Vector3.up * 0.2f;


        stateCnt = 4.2f;

        PlayIdleAni();

        attackTime = 0;

        EnemyAnimUtil.TeleportBlink(transform, teleportPos, 0.77f, 0.77f,
            () => {
                SetInnerGlow();
                //ReleaseACirclesOfMagicBullet(5);
                Invoke(nameof(BackSlash), 1f);
                Invoke(nameof(PlayAttackAni), 1f);

            }
            );

        //EnemyAnimUtil.SlamWindupAndImpact(this.transform);



    }

    void BackSlashAttack_Update()
    {
        RotateTowardPlayer();

        PlanStateChange(BossState.Move);

    }

    void BackSlash()
    {
        //PlayAttackAni();
        SoundEffect.Instance.PlayOneSound(thunderSlashAttackSe, 0.7f);  

        FacePlayerNow();

        int projectileCount = 3;
        float sectorAngle = 70f;

        float angleStep = (projectileCount > 1) ? sectorAngle / (projectileCount - 1) : 0f;
        float halfSector = sectorAngle * 0.5f;

        for (int i = 0; i < 3; i++)
        {
            float currentAngle = -halfSector + angleStep * i;
            Vector3 dir = Quaternion.Euler(0f, currentAngle, 0f) * transform.forward;

            Vector3 spawnPos = transform.position + transform.forward * 1.5f;
            GameObject slash = Instantiate(magicProjectileObj, spawnPos + Vector3.up * 2.1f, Quaternion.LookRotation(dir));
            SkillForwardMove slashMove = slash.GetComponent<SkillForwardMove>();
            slashMove.moveVec = dir * 11.5f;

        }

        attackTime++;

        if(attackTime < 3)
        {
            Invoke(nameof(BackSlash), 0.7f);
        }
        else
        {
            stateCnt = 0.1f;
            
        }


    }

    //void SlashOnce()
    //{

    //}



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

    public void PlayAttackAni()
    {
        animator.SetTrigger("isAttacking");
        
    }

    void PlayIdleAni()
    {
        animator.SetBool("isIdleing", true);
        animator.SetBool("isMoving", false);
    }


}

