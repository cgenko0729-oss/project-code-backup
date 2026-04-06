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
using HighlightPlus;
using MaykerStudio.Demo;
using System.Collections;

public class EnemyBossDragonAction : BossActionBase
{
    public enum BossState
    {
        Idle,
        Move,
        Death,
        TakeOff,
        Landing,
        FlyTornadoAttack,
        FireBreatheAttack,
        Glide,
        FlyIdle,
        FlyFireBreatheAttack,
        ClawAttack,
        TornadoAttack,
        FireBeamAttack,
        FlyFireBallAttack,
        JumpAttack,
        FireBallAttack,
        RoarAttack,
        DashFallingAttack,
        DashFallDizzy,
        FlyFalling,




    }

    public float stateCnt;                    
    public StateMachine<BossState> stateMachine;　　
    public string stateInfoText;

    public BossState nextState = BossState.Move;

    public float attackCnt = 0f;
    public float attackCntMax = 1f;

   

    public float turningCnt = 0f;

    HighlightEffect playerHighlight;

     public float roarCnt = 0f;
    public bool hasRoared = false;


    //====FlyAttack====//
    [Header("=======FlyAttack=======")]
    public Transform mouthTrans;
    public ParticleSystem fireBreathePs;

    public bool isEffectPlayed = false;
    public float effectLastCnt = 3f;

    public Vector3 centerPos = Vector3.zero;
    public float skyHeight = 4.2f; //3.5
    public float landHeight = 0f;
    public Vector3 landPos = new Vector3(0, 4.9f, 4.2f);

    public ParticleSystem landingParticle;
    public ParticleSystem takeOffParticle;


    //====GlideAttack====//
    [Header("=======GlideAttack=======")]
    public bool isGliding = false; 
    float glideX = 0f;
    float glideZ = 0f;
    Vector3 glideTargetPos = new Vector3(0,3.5f,0);
    Vector3 glideStart = new Vector3(-21f, 3.5f, 0);
    Vector3 glideEnd  = new Vector3(21f, 3.5f, 0);

    int glideCount = 0;

    public GameObject aoeRectObj;
    private AoeRectIndicatorLine aoeRectIndicator;
    public GameObject testArrow;


    //====ClawAttack====//
    [Header("=======ClawAttack=======")]
    public ParticleSystem clawAttackDust;

    //====TornadoAttack====//
    [Header("=======TornadoAttack=======")]
    public GameObject tornadoAttackObj;
    public GameObject tornadoForwardAttackObj;


    [Header("=======FireBeamAttack=======")]
    public GameObject fireBeamObj;
    public ParticleSystem fireBeamPs;

    public GameObject fireBallObj;
    public GameObject meteorObj;

    public float aoeRangeMaxX = 19f;
    public float aoeRangeMaxZ = 17f;
    public float aoeRangeMinX = -11f;
    public float aoeRangeMinZ = -17f;

    public ParticleSystem tailAttackPs;
    public GameObject tailAttackAoeCircleObj;

    public GameObject groundDustPs;
    public GameObject aoeCircleObj;
    public AoeCircleDynamic aoeCircleIndicator;
    public float aoeCirclePositionTraceTimer = 3.5f;
    public float aoeCirclePositionTraceTimerMax = 3.5f;
    public bool isAoeCircleStartFilling = false;
    public float aoeCircleFillDuration = 1.5f;
    public float aoeJumpAttackDamage = 25f;

    public ParticleSystem dizzyEffect;

    [Header("=======Sound=======")]
    public AudioSource source;
    public AudioClip roarSe;
    public AudioClip WingAttackSe;
    public AudioClip fireBallSe;
    public AudioClip fireBretheSe;
    public AudioClip groundShakeSe;
    public AudioClip WindGlideSe;
    public AudioClip fireBurnSe;
    public AudioClip closeAttackExploSe;
    public AudioClip moveWingSe;
    public AudioClip dragonShoutNormal;
    public AudioClip bigWingShake;
    public AudioClip flyEarthShakeSe;


    void Start()
    {
        stateMachine = StateMachine<BossState>.Initialize(this);
        stateMachine.ChangeState(BossState.Move);

        InitBoss();

        fireBreathePs.Stop();
        fireBeamPs.Stop();

        playerHighlight = playerTrans.GetComponent<HighlightEffect>();

        currentAtttackPattern = 0;

        dizzyEffect.Stop();

    }

    public void TargetPlayerFx()
    {
       
        //playerHighlight.targetFX = true;
        playerHighlight.TargetFX();
    }

    void Update()
    {
        attackCnt -= Time.deltaTime;
        roarCnt -= Time.deltaTime;
        turningCnt -= Time.deltaTime;

        UpdateStateInfo();
        UpdatePlayerInfo();


    }

    public void ChangeToDead()
    {
        ChangeToState(BossState.Death); 

    }

    public void Death_Enter()
    {

        

        SoundEffect.Instance.PlayOneSound(roarSe, GameManager.Instance.gameVol);
        DOTween.Kill(transform, complete: true);
        StopAllCoroutines();
        CancelInvoke();

        animator.SetTrigger("isDead");
        stateCnt = 21f;

    }

    public void Death_Update()
    {



    }

    void Move_Enter()
    {
        stateCnt = 3f;
        animator.SetBool("isMoving",true);
        animator.SetBool("isFlying", false);
    }

    void Move_Update()
    {

        //PlanStateChange(BossState.TakeOff);
        //PlanStateChange(BossState.ClawAttack);
        //PlanStateChange(BossState.TornadoAttack);
        //PlanStateChange(BossState.FireBeamAttack);
        //PlanStateChange(BossState.JumpAttack);
        //PlanStateChange(BossState.FireBallAttack);
        //PlanStateChange(BossState.RoarAttack);

        RotateTowardPlayer();
        HomingPlayer();

        if(stateCnt <= 0)
        {

            switch (currentAtttackPattern)
            {
                case 0:
                    ChangeToState(BossState.ClawAttack);
                    break;
                case 1:
                    ChangeToState(BossState.FireBallAttack);
                    break;
                case 2:
                    ChangeToState(BossState.TornadoAttack);
                    break;
                case 3:
                    ChangeToState(BossState.RoarAttack);
                    break;
                case 4:
                    ChangeToState(BossState.TakeOff);
                    nextState = BossState.Glide;
                    currentAtttackPattern = -1;
                    break;
                //case 5:
                //    ChangeToState(BossState.TakeOff);
                //    nextState = BossState.DashFallingAttack;
                //    currentAtttackPattern = -1;
                //    break;


            }

            currentAtttackPattern++;

        }

        





    }

    void PlanNextState(BossState _nextState)
    {
        nextState = _nextState;
    }


    //source.PlayOneShot(roarSe, GameManager.Instance.gameVol);

    //a coroutine to play roarSe 4 times with 0.7f second interval
    public IEnumerator PlayRoarSoundRepeatedly(float interval, int repeatCount)
    {
        for (int i = 0; i < repeatCount; i++)
        {
            source.PlayOneShot(moveWingSe, GameManager.Instance.gameVol);
            yield return new WaitForSeconds(interval);
        }
    }


    void FlyFalling_Enter()
    {
        stateCnt = 10f;
        attackCnt = 7f;

        //dotween move posY to 0 in 1.4f second
        animator.SetBool("isFlyFalling", true);

        transform.DOMoveY(0.28f, 0.35f).SetEase(Ease.Linear).OnComplete(() => {
            CameraShake.Instance.StartShake();
            DashDustPos = transform.position;
            SpawnDustParticle();
            animator.SetBool("isFlyFalling", false);
            animator.SetTrigger("isFlyFallDizzy");

            transform.DOMoveY(0f, 0.1f).SetEase(Ease.Linear).OnComplete(() => {
                attackCnt = 5.6f;
            });

        });

    }

    void FlyFalling_Update()
    {

        if(attackCnt <= 0)
        {
            attackCnt = 21f;
            animator.SetTrigger("isFlyFallRecover");
        }
        

        if (stateCnt <= 0)
        {
            
            ChangeToState(BossState.Move);
        }
    }

    void FlyFalling_Exit()
    {
        
    }



    Vector3 DashDustPos = Vector3.zero;
    void DashFallingAttack_Enter()
    {
        //DashFallTargetPoint = playerTrans.position + new Vector3(0, -0.77f, 0);
        //DashDustPos = playerTrans.position;
        //attackCnt = 2.1f;

        stateCnt = 7.7f; //2.1f
       
     
        animator.SetBool("isDashFalling", false);
        animator.SetBool("isFlying", true);
        transform.DOMoveY(7.7f, 1.4f).SetEase(Ease.InOutSine);

        aoeCirclePositionTraceTimer = aoeCirclePositionTraceTimerMax; // AOEサークルの位置トレースタイマーをリセット
        isAoeCircleStartFilling = false;
        GameObject aoeObj = Instantiate(aoeCircleObj);
        aoeCircleIndicator = aoeObj.GetComponent<AoeCircleDynamic>();

        source.PlayOneShot(dragonShoutNormal, GameManager.Instance.gameVol * 0.7f);

    }

    void DashFallingAttack_Update()
    {
        if (aoeCirclePositionTraceTimer >= 0)
        {
            RotateTowardPlayer();
            aoeCirclePositionTraceTimer -= Time.deltaTime;
            Vector3 indicatorPos = playerTrans.position;
            indicatorPos.y = 0.1f; 
            float indicatorRadius = 7.7f;
            aoeCircleIndicator.UpdateTransform(indicatorPos, indicatorRadius);
        }

        if (aoeCirclePositionTraceTimer <= 0 && !isAoeCircleStartFilling)
        {
            
            isAoeCircleStartFilling = true; // AOEサークルの充填を開始
            aoeCircleIndicator.BeginFill(aoeCircleFillDuration, aoeJumpAttackDamage);
            StartCoroutine(FallAfterFill(aoeCircleFillDuration - 0.49f));

        }




        //if (attackCnt <= 0)
        //{
        //    attackCnt = 21f;
        //    animator.SetBool("isDashFalling",true);
            

        //    transform.DOMove(DashFallTargetPoint, 0.49f).SetEase(Ease.Linear).OnComplete(() => {
        //        SpawnDustParticle();
        //        stateCnt = 0;
        //    });
        //}


        //PlanStateChange(BossState.Move);
        if (stateCnt <= 0)
        {
            ChangeToState(BossState.DashFallDizzy);
        }

    }

    private IEnumerator FallAfterFill(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        animator.SetBool("isDashFalling",true);
        Vector3 jumpAttackTarget = aoeCircleIndicator.transform.position + new Vector3(0,-0.7f,0);
        DashDustPos = jumpAttackTarget;
        FaceTargetNow(jumpAttackTarget);

        transform.DOMove(jumpAttackTarget, 0.5f).SetEase(Ease.InCubic).OnComplete(() => {
            CameraShake.Instance.StartTurnipaJumpShake();
            SpawnDustParticle();
            stateCnt = 0;
            source.PlayOneShot(groundShakeSe, GameManager.Instance.gameVol * 0.7f);
        });


    }

    void SpawnDustParticle()
    {
        Instantiate(groundDustPs, DashDustPos + new Vector3(0, 0.1f, 0), Quaternion.identity);

    }

    void DashFallingAttack_Exit()
    {
        animator.SetBool("isFlying", false);
        animator.SetBool("isDashFalling", false);
    }

    void DashFallDizzy_Enter()
    {
        animator.SetBool("isDashFallDizzy", true);

        stateCnt = 14f;

        attackCnt = 2.8f;

        //dizzyEffect.gameObject.SetActive(true);
        //dizzyEffect.Play();

        transform.DOMoveY(0f, 0.0f).SetEase(Ease.InOutSine).OnComplete(() => {
            
        });
    }

    void DashFallDizzy_Update()
    {
        if(attackCnt > 0)
        {
            RotateTowardPlayer();
        }
        
        
        PlanStateChange(BossState.Move);
    }

    void DashFallDizzy_Exit()
    {
        animator.SetBool("isDashFallDizzy", false);

        dizzyEffect.Stop();
        dizzyEffect.gameObject.SetActive(false);
    }

    void RoarAttack_Enter()
    {
        roarCnt = 1.4f;
        hasRoared = false;

        attackCnt = 4.9f;

        stateCnt = 11f; //4.2f

        animator.SetBool("isIdle", true);
        animator.SetBool("isMoving", false);

    }

    void RoarAttack_Update()
    {

        if(roarCnt >0 && !hasRoared)
        {
            RotateTowardPlayer();

        }

        if(roarCnt <= 0)
        {
            roarCnt = 21f;
            animator.SetTrigger("isRoarAttack");
            hasRoared = true;
            Invoke(nameof(DelayRoarImpact), 0.35f);
            source.PlayOneShot(roarSe, GameManager.Instance.gameVol);


            
        }

        if(attackCnt <= 0)
        {
            attackCnt = 21f;
            animator.SetTrigger("isTailAttack");
            Invoke(nameof(PlayTailAttackParticle), 1f);
            Invoke(nameof(PlayTailAttack2), 2.1f);
            Invoke(nameof(PlayTailAttack3), 3.2f);
        }


        PlanStateChange(BossState.Move);

    }


    void RoarAttack_Exit()
    {
        animator.SetBool("isIdle", false);
    }

    void DelayRoarImpact()
    {
        GameVolumeManager.Instance.PlayRoarAttackImapact();

        Invoke(nameof(CheckApplyPlayerDizzy), 0.42f);

    }

    void CheckApplyPlayerDizzy()
    {
        if(distToPlayer<14)
        {
            ItemManager.Instance.pickUpSpdUpWing = true;
            ItemManager.Instance.spdUpAmount = 0.14f;
            ItemManager.Instance.spdUpTime = 4.9f;
            EffectManager.Instance.CreatePlayerDizzyEffect();
        }
    }

    void SpawnTornadoProjectile(int spawnNum)
    {
        //with transform.postion as center , in 360 degree , spawn
        Vector3 spawnPos = transform.position;

        for (int i = 0; i < spawnNum; i++)
        {
           GameObject tornado = Instantiate(tornadoForwardAttackObj, spawnPos, Quaternion.identity);
            //SkillSMove tornadoSMove = tornado.GetComponent<SkillSMove>();
            //tornadoSMove.isActivated = false;
            SkillForwardMove tornadoMove = tornado.GetComponent<SkillForwardMove>();
            float angle = i * (360f / spawnNum) * Mathf.Deg2Rad; // ラジアンに変換
            Vector3 targetPos = new Vector3(
                this.transform.position.x + Mathf.Cos(angle) * 10f,
                this.transform.position.y,
                this.transform.position.z + Mathf.Sin(angle) * 10f
            );
            tornadoMove.moveVec = (targetPos - this.transform.position).normalized * 5f;

        }

    }

    void PlayTailAttackParticle()
    {
        tailAttackPs.Stop();
        tailAttackPs.Play();

        SpawnTornadoProjectile(7);

        source.PlayOneShot(bigWingShake, GameManager.Instance.gameVol);
    }

    void PlayTailAttack2()
    {
        tailAttackPs.Stop();
        tailAttackPs.Play();

        SpawnTornadoProjectile(14);
        source.PlayOneShot(bigWingShake, GameManager.Instance.gameVol);
    }

    void PlayTailAttack3()
    {
        tailAttackPs.Stop();
        tailAttackPs.Play();

        SpawnTornadoProjectile(21);
        source.PlayOneShot(bigWingShake, GameManager.Instance.gameVol);
    }


    void FireBallAttack_Enter()
    {

        animator.SetBool("isIdle", true);
        animator.SetBool("isMoving", false);

        stateCnt = 7f;
        attackCnt = 1.4f;
        SetInnerGlow();


    }

    void FireBallAttack_Update()
    {
        RotateTowardPlayer();

        if (attackCnt <= 0)
        {
            attackCnt = 1.4f;
            animator.SetTrigger("isFireBallAttack");
            ShotCurveProjectile(playerTrans.position);
            source.PlayOneShot(fireBallSe, GameManager.Instance.gameVol);
            //SpawnMeteorObj();

        }

        if (stateCnt <= 0)
        {
            ChangeToMoveState();
            source.PlayOneShot(fireBurnSe, GameManager.Instance.gameVol * 0.7f);
        }


        //PlanStateChange(BossState.Move);
    }

    void FireBallAttack_Exit()
    {
        animator.SetBool("isIdle", false);
    }

    public Vector3 lastMeteorPos = Vector3.zero; //last meteor position

    void SpawnMeteorObj()
    {
        //spawn a meteor at a random position within the aoe range max and min, the next meteor should keep a distance of 7f from the last meteor:
        float x = Random.Range(aoeRangeMinX, aoeRangeMaxX);
        float z = Random.Range(aoeRangeMinZ, aoeRangeMaxZ);
        float y = 0f;

        Vector3 spawnPos = new Vector3(x, y, z);
        if (Vector3.Distance(spawnPos, lastMeteorPos) < 4.9f)
        {
            //if the distance is less than 7f, spawn a new position
            SpawnMeteorObj();
            return;
        }

        //the meteor should be within dist of 7f from the player, if not, spawn a new position
        if (Vector3.Distance(spawnPos, playerTrans.position) > 4.9f)
        {
            SpawnMeteorObj();
            return;
        }


        lastMeteorPos = spawnPos; //update the last meteor position
        GameObject meteor = Instantiate(meteorObj, spawnPos + new Vector3(0, 10f, 0), Quaternion.identity); //spawn at a height of 10f



    }

    void JumpAttack_Enter()
    {
        stateCnt = 5.6f;
        attackCnt = 2.1f;
        SetInnerGlow();

        curveTarget = playerTrans.position + new Vector3(0,4.5f,0);
        FacePlayerNow();

        animator.SetBool("isIdle", true);
        animator.SetBool("isMoving", false);
    }

    void JumpAttack_Update()
    {
        if(attackCnt <= 0)
        {
            attackCnt = 14f;
            animator.SetTrigger("isJumpAttack");
            CurveJump(curveTarget); 

            animator.SetBool("isIdle", false);

        }

        PlanStateChange(BossState.Move); 


    }

    void JumpAttack_Exit()
    {
        animator.SetBool("isIdle", false);
    }

    [SerializeField] AnimationCurve timeByDistance   = AnimationCurve.Linear(0, 0.4f, 20, 1.4f);
    [SerializeField] AnimationCurve heightByDistance = AnimationCurve.Linear(0, 3f, 20, 8f);
    [SerializeField] float minFlightTime  = 0.25f;     
    [SerializeField] float maxFlightTime  = 2.0f;
    [SerializeField] float maxArcHeight   = 10f;
    public Vector3 curveTarget;

    void ShotCurveProjectile(Vector3 targetPoint)
    {
        float dist = Vector3.Distance(transform.position, targetPoint);
        float t    = Mathf.Clamp(timeByDistance.Evaluate(dist), minFlightTime, maxFlightTime);
        float h    = Mathf.Min(heightByDistance.Evaluate(dist), maxArcHeight);
    
        GameObject proj = Instantiate(fireBallObj, mouthTrans.position + new Vector3(0,0.35f,0), Quaternion.identity); //curveProjectileObj

        proj.transform.DOJump(targetPoint, h, 1, t,snapping: false).SetEase(Ease.Linear) //where it lands, how high above its start it goes, number of “jumps” (use 1 for a single arc), total flight time
        .OnComplete(() => 
        {
            //Destroy(proj);
            //GameObject posionPool = null;
            //if(isPoisonPoolObj)posionPool = Instantiate(AoePosionPoolObj, targetPoint, Quaternion.identity);
            //else  posionPool = Instantiate(AoePoisonExplodeObj, targetPoint, Quaternion.identity);
        });

    }
    void CurveJump(Vector3 targetPoint)
    {
        //float dist = Vector3.Distance(transform.position, targetPoint);
        //float t    = Mathf.Clamp(timeByDistance.Evaluate(dist), minFlightTime, maxFlightTime);
        //float h    = Mathf.Min(heightByDistance.Evaluate(dist), maxArcHeight);

        //transform.DOJump(targetPoint, h, 1, t, snapping: false).SetEase(Ease.Linear) //where it lands, how high above its start it goes, number of “jumps” (use 1 for a single arc), total flight time
        //.OnComplete(() => {
        //    animator.SetBool("isIdle", true);
        //});

        //Dotween move to targetPoint in 1 second
        transform.DOMove(targetPoint, 1.4f).SetEase(Ease.InOutSine).OnComplete(() => {
            animator.SetBool("isIdle", true);
        });

    }

    void FlyFireBallAttack_Enter()
    {

        stateCnt = 7f;
        attackCnt = 1.4f;

    }

    void FlyFireBallAttack_Update()
    {
        RotateTowardPlayer();

        if (attackCnt <= 0)
        {
            attackCnt = 1.4f;
            animator.SetTrigger("isFireBallAttack");
           
        }


        PlanStateChange(BossState.FlyIdle);
    }

    void FireBeamAttack_Enter()
    {

        stateCnt = 4.2f;

        attackCnt = 1.4f;

        animator.SetBool("isMoving", false);
        animator.SetBool("isIdle", true); 

    }

    void FireBeamAttack_Update()
    {
        RotateTowardPlayer(1f);

        if(attackCnt <= 0)
        {
            attackCnt = 10f;
            animator.SetBool("isIdle", false); 
            animator.SetBool("isFireBreatheAttack",true);
            //fireBeamObj.SetActive(true); 
            fireBeamPs.Play();
        }

        PlanStateChange(BossState.Move);
    }

    void FireBeamAttack_Exit()
    {
        //fireBeamObj.SetActive(false);
        fireBeamPs.Stop();
        animator.SetBool("isFireBreatheAttack", false);
        animator.SetBool("isIdle", false);

    }

    void TakeOff_Enter()
    {
        stateCnt = 3.5f;
        animator.SetBool("isFlying", true);
        animator.SetBool("isMoving", false);
        animator.SetTrigger("isTakeOff");
        
        CameraShake.Instance.StartPhrase2GroundShake();
        transform.DOMoveY(skyHeight, 1.4f).SetEase(Ease.InOutSine);

        //nextState = BossState.FlyTornadoAttack;
        //nextState = BossState.Glide;
        //nextState = BossState.FlyFireBreatheAttack;
        //nextState = BossState.FlyFireBallAttack;
        //nextState = BossState.DashFallingAttack;
        //nextState = BossState.FlyFalling;

        Invoke(nameof(PlayTakeOffParticle), 0.28f); 

        StartCoroutine(PlayRoarSoundRepeatedly(1.4f, 3)); 

        source.PlayOneShot(flyEarthShakeSe, GameManager.Instance.gameVol * 0.7f);
    }

    void TakeOff_Update()
    {

        PlanStateChange(nextState);
    }

    void PlayTakeOffParticle()
    {
        takeOffParticle.Stop();
        takeOffParticle.Play();
    }

    void FlyTornadoAttack_Enter()
    {
        stateCnt = 4.2f;

        fireBreathePs.Play();
        isEffectPlayed = true;
        effectLastCnt = 2.8f;

        FacePlayerNow();
    }

    void FlyTornadoAttack_Update()
    {
        //RotateTowardPlayer();
        
        PlanStateChange(BossState.Glide);


        if (isEffectPlayed)
        {
            effectLastCnt -= Time.deltaTime;

            if (effectLastCnt <= 0)
            {
                isEffectPlayed = false;
                effectLastCnt = 70f;
                fireBreathePs.Stop();
            }
        }


    }

    

    void GlideToTarget()
    {
        if (glideCount >= 3) return;

        glideCount++;

        glideX = Random.Range(-21f, 21f);
        glideZ = Random.Range(21f, 21f);

        if (Random.Range(0f, 1f) > 0.5f)
        {
            glideZ *= -1f;
        }

        glideStart = new Vector3(glideX, skyHeight, glideZ); //start position
        glideEnd = new Vector3(-glideX, skyHeight, -glideZ); //end position

        transform.position = glideStart;

        glideTargetPos = glideEnd;
        transform.DOMove(glideTargetPos, 2.8f).SetEase(Ease.Linear).OnComplete(() => { //InOutSine

            if (glideCount >= 3) ChangeToState(BossState.FlyIdle);
            else  GlideToTarget();
        });

    }

   

    private float glideRadius = 21.9f; // How far from the player the glide starts/ends.
    private int maxGlideCount = 3;   // Total number of glides in one sequence.
    private float glideDuration = 2.1f; // How long each glide takes.
    private Vector3 lastGlideDirection; // To store the direction of the last glide.
    async void Glide_Enter()
    {
        isGliding = true;
        animator.SetBool("isGliding", true);

        // Start the sequence of glides
        await PerformGlideSequence();
    }

    private async UniTask PerformGlideSequence()
    {
        // Use a for loop for the specified number of glides
        for (int i = 0; i < maxGlideCount; i++)
        {
            // --- 1. WAIT before the glide ---
            // This fulfills the "wait for 2 seconds" requirement.
            TargetPlayerFx();
            
           
            // --- 2. CALCULATE the glide path ---
            // Get the player's position, but keep the dragon at its flying height
            Vector3 playerPosOnGlidePlane = new Vector3(playerTrans.position.x, skyHeight, playerTrans.position.z);

            Vector3 currentGlideDirection;

            if (i == 0) // For the first glide, pick a random direction
            {
                Vector2 randomCircle = Random.insideUnitCircle.normalized; // A random direction on the XZ plane
                currentGlideDirection = new Vector3(randomCircle.x, 0, randomCircle.y);
            }
            else // For subsequent glides, use the opposite direction
            {
                currentGlideDirection = -lastGlideDirection;
            }

            

            // Calculate the start and end points based on the player's current position
            Vector3 startPos = playerPosOnGlidePlane - currentGlideDirection * glideRadius;
            Vector3 endPos = playerPosOnGlidePlane + currentGlideDirection * glideRadius;

            // Store this direction for the next iteration
            lastGlideDirection = currentGlideDirection;

            // --- 3. EXECUTE the glide ---
            // Instantly move the dragon to the starting position
            transform.position = startPos;
            
            // Make the dragon look towards its destination
            //RotateTowardTarget(endPos);
            FacePlayerNow();


            // --- 3. CALCULATE ROTATIONS CORRECTLY ---
        
        // FIX: The direction of the dragon's movement.
        Vector3 glidePathDirection = endPos - startPos;

        // FIX: The rotation needed to align an object WITH the glide path.
        // This is for your AOE rectangle.
        Quaternion glidePathRotation = Quaternion.LookRotation(glidePathDirection);

        // FIX: The direction the dragon is COMING FROM. This is the opposite of the glide path direction.
        Vector3 incomingDirection = -glidePathDirection;
        
        // FIX: The rotation for the arrow, pointing FROM where the dragon will appear.
        Quaternion arrowRotation = Quaternion.LookRotation(incomingDirection);


            Quaternion fixGlideRot = Quaternion.LookRotation(glidePathDirection);
            Quaternion aoeOffset = Quaternion.Euler(90f, -90f, 0f); 
            Quaternion arrowOffset = Quaternion.Euler(0f, 0f, 0f); 
            Quaternion aoeFinalRot = fixGlideRot * aoeOffset; 
            Quaternion arrowFinalRot = arrowRotation * arrowOffset;


            // --- 4. EXECUTE the glide ---
            transform.position = startPos;
        FacePlayerNow(); // This is a design choice, it looks at the player, not the glide end point. That's fine.

        // Use the correct rotation for the AOE rectangle
        Vector3 indicatorPosition = playerPosOnGlidePlane + new Vector3(0,-0.7f,0); // The indicator should be centered on the player
        Vector2 indicatorSize = new Vector2(4.9f, glideRadius * 2.8f); // Width of 2, length is the full glide path
        GameObject aoeRect = Instantiate(aoeRectObj, indicatorPosition, aoeFinalRot);
        aoeRectIndicator = aoeRect.GetComponent<AoeRectIndicatorLine>();
        aoeRectIndicator.UpdateTransform(indicatorPosition, aoeFinalRot, indicatorSize);
        aoeRectIndicator.BeginFill(1.7f, 0.1f, 20, true);

        // FIX: Spawn the testArrow using the CORRECTLY calculated 'arrowRotation'
        GameObject arrow = Instantiate(testArrow, playerTrans.position + new Vector3(0, 3.5f, 0), arrowFinalRot); // Raised y for visibility
                                                                                                                  // You might want to destroy this arrow after a few seconds
            arrow.transform.position = playerTrans.position + new Vector3(0, 3.5f, 0);
            

            Destroy(arrow, 2.1f);
            source.PlayOneShot(dragonShoutNormal, GameManager.Instance.gameVol * 0.7f);




            await UniTask.Delay(System.TimeSpan.FromSeconds(1.49), ignoreTimeScale: false);

            fireBreathePs.Stop();
            fireBreathePs.Play();
            source.PlayOneShot(fireBretheSe, GameManager.Instance.gameVol);

            // Use DOTween to move to the end position and await its completion
            await transform.DOMove(endPos, glideDuration).SetEase(Ease.Linear).OnComplete(() => {
                fireBreathePs.Stop();
            }).ToUniTask();
        }

        // --- 4. FINISH the state ---
        // After all glides are complete, change to the next state
        //ChangeToState(BossState.FlyIdle);
        ChangeToState(BossState.DashFallingAttack);

    }

    // The update loop for Glide is now empty because all logic is in the async sequence.
    void Glide_Update()
    {
        // No longer needed. The async method handles the flow.
    }

    void Glide_Exit()
    {
        animator.SetBool("isGliding", false);
        isGliding = false; 
    }


    void FlyIdle_Enter()
    {
        stateCnt = 2.1f;
    }

    void FlyIdle_Update()
    {
        PlanStateChange(BossState.Landing);
    }

    void Landing_Enter()
    {
        stateCnt = 2.89f;
        
        animator.SetTrigger("isLanding");

        CameraShake.Instance.StartPhrase2GroundShake();
        transform.DOMoveY(landHeight, 2.19f).SetEase(Ease.InOutSine).OnComplete(() => {
            PlayLandingParticle();
        });

        //set rotation X to 0 now:
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);


        source.PlayOneShot(flyEarthShakeSe, GameManager.Instance.gameVol * 0.7f);

    }

    void Landing_Update()
    {

        PlanStateChange(BossState.Move);
    }

    void PlayLandingParticle()
    {
        landingParticle.Stop();
        landingParticle.Play();
    }

    void FlyFireBreatheAttack_Enter()
    {
        transform.position = new Vector3(0, 7f, 4.2f);
        transform.DOMoveY(4.9f, 0.77f).SetEase(Ease.InOutSine);

        stateCnt = 7f;
        attackCnt = 1.1f;
    }

    void FlyFireBreatheAttack_Update()
    {
        FaceTargetNow(centerPos);


        if (attackCnt <= 0)
        {
            centerPos = playerTrans.position;
            FaceTargetNow(centerPos);

            fireBreathePs.Play();
            attackCnt = 70f;
            isEffectPlayed = true;
            effectLastCnt = 2.8f;
        }

        if (isEffectPlayed)
        {
            effectLastCnt -= Time.deltaTime;

            if (effectLastCnt <= 0)
            {
                isEffectPlayed = false;
                effectLastCnt = 70f;
                fireBreathePs.Stop();
            }
        }

        if(stateCnt <= 0)
        {

            ChangeToState(BossState.FlyIdle);

            
        }

    }

    void FireBreatheAttack_Enter()
    {
        animator.SetBool("isMoving",false);
        animator.SetTrigger("isFireBreathe");

        //transform.DOMoveX(3.59f, 1f).SetEase(Ease.InOutSine);

        //teleoport to the landPos
        transform.position = landPos;
        transform.DOMoveY(skyHeight, 1f).SetEase(Ease.InOutSine);

        stateCnt = 7f;
        attackCnt = 3f;
    }

    void FireBreatheAttack_Update()
    {
        RotateTowardTarget(centerPos);


        if (attackCnt <= 0)
        {
            fireBreathePs.Play();
            attackCnt = 70f;
            isEffectPlayed = true;
            effectLastCnt = 2.8f;
        }

        if (isEffectPlayed)
        {
            effectLastCnt -= Time.deltaTime;

            if (effectLastCnt <= 0)
            {
                isEffectPlayed = false;
                effectLastCnt = 70f;
                fireBreathePs.Stop();
            }
        }

        if(stateCnt <= 0)
        {
            stateCnt = 70f;
            //dotween move y to 0
            transform.DOMoveY(0f, 1f).SetEase(Ease.InOutSine).OnComplete(() => {
                ChangeToMoveState();
            });

            
        }
    }

    void FireBreatheAttack_Exit()
    {

    }

    void ClawAttack_Enter()
    {
        
        animator.SetBool("isMoving", false);
        animator.SetBool("isIdle", true);

        stateCnt = 4.2f;
        attackCnt = 1.4f;
        turningCnt = 1.4f;

        SetInnerGlow();

    }

    void ClawAttack_Update()
    {
        if(turningCnt >=0)RotateTowardPlayer();

        if(attackCnt <= 0)
        {
            attackCnt = 7f;
            animator.SetTrigger("isClawAttack");
            Invoke(nameof(PlayDust), 1.1f); 
        }


        PlanStateChange(BossState.Move);
    }

    void ClawAttack_Exit()
    {
        animator.SetBool("isIdle", false);
    }

    void PlayDust()
    {
        clawAttackDust.Stop();
        clawAttackDust.Play(); 
        source.PlayOneShot(closeAttackExploSe, GameManager.Instance.gameVol);

        BossHitBoxCollisionHandler hitBox = clawAttackDust.GetComponent<BossHitBoxCollisionHandler>();
        hitBox.EnableHitBox(1f);
    }

    void TornadoAttack_Enter()
    {
        
        animator.SetBool("isIdle", true);
        animator.SetBool("isMoving", false);

        SetInnerGlow();

        stateCnt = 7.7f;
        attackCnt = 1.4f;
        turningCnt = 1.4f;

    }

    void TornadoAttack_Update()
    {
        if(attackCnt <=0)
        {
            animator.SetBool("isTornadoAttack", true);
            attackCnt = 1.4f;
            turningCnt = 0.7f;

            int projectileCount = 3;
            float sectorAngle = 70f;

            float angleStep = (projectileCount > 1) ? sectorAngle / (projectileCount - 1) : 0f;
            float halfSector = sectorAngle * 0.5f;

            for(int i = 0; i < 3; i++)
            {
                float currentAngle = -halfSector + angleStep * i;
                Vector3 dir = Quaternion.Euler(0f, currentAngle, 0f) * transform.forward;

                Vector3 spawnPos = transform.position + transform.forward * 1.5f;
            GameObject tornado = Instantiate(tornadoAttackObj, spawnPos, Quaternion.identity);
            SkillForwardMove tornadoMove= tornado.GetComponent<SkillForwardMove>();
            tornadoMove.moveVec = dir * 7.9f;

            SkillSMove tornadoSMove = tornado.GetComponent<SkillSMove>();
            float amp = Random.Range(0.79f, 1.49f);
            float wave = Random.Range(79f, 149f);
            float spd = Random.Range(4.9f, 7f);
            tornadoSMove.Configure(dir, spd, amp, wave, true); 

            source.PlayOneShot(WingAttackSe, GameManager.Instance.gameVol);

            }

            //Vector3 spawnPos = transform.position + transform.forward * 1.5f;
            //GameObject tornado = Instantiate(tornadoAttackObj, spawnPos, Quaternion.identity);
            //SkillForwardMove tornadoMove= tornado.GetComponent<SkillForwardMove>();
            //tornadoMove.moveVec = transform.forward * 7.9f;

            //SkillSMove tornadoSMove = tornado.GetComponent<SkillSMove>();
            //float amp = Random.Range(0.79f, 1.49f);
            //float wave = Random.Range(79f, 149f);
            //float spd = Random.Range(4.9f, 7f);
            //tornadoSMove.Configure(transform.forward, spd, amp, wave, true); 

            //source.PlayOneShot(WingAttackSe, GameManager.Instance.gameVol);



        }

        if(turningCnt > 0)
        {
            RotateTowardPlayer();
        }

        PlanStateChange(BossState.Move);


    }

    void TornadoAttack_Exit()
    {
        animator.SetBool("isTornadoAttack", false);
        animator.SetBool("isIdle", false);
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

    public void PlaySound()
    {

    }

    void PlanStateChange(BossState targetState)
    {
       if(stateCnt <= 0)
        {
            ChangeToState(targetState);
        }
    }

    //private void OnTriggerEnter(Collision collision)
    //{
    //    if(!isGliding) return;

    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        EventManager.EmitEventData("ChangePlayerHp",-20);
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (!isGliding) return;

        if (other.CompareTag("Player"))
        {
            EventManager.EmitEventData(GameEvent.ChangePlayerHp, -20f);
            
        }
    }

}

//python : https://forms.gle/zKke4VQgcj1z8FtbA