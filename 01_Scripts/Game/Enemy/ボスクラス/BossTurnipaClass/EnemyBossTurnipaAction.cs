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
using System.Collections;

public class EnemyBossTurnipaAction : MonoBehaviour
{
    public enum BossState
    {
        Idle,
        Move,
        JumpAttack,
        DashAttack,
        MagicAoeAttack,
        SummonSlime,
        Dizzy,
        Death,
        TreeRootAttack,
        CurveProjectileAttack,
        SectorAttack,
        StandingCenter,
        MagicBomobAttack,

    }

    

    public float stateCnt;                    // 各状態の継続時間などをカウントするタイマー
    public StateMachine<BossState> stateMachine;　　// 状態遷移を管理するステートマシン
    public string stateInfoText; // 状態番号を文字列に変換するためのテキスト
    
    private Animator animator;
    HighlightEffect highlightEffect;
    EnemyStatusBase enemyStatus;

    Transform playerTrans; // プレイヤーのTransform
    public float distToPlayer; // プレイヤーとの距離
    public Vector3 dirToPlayer; 

    public GameObject bossHpBar; // ボスのHPバーオブジェクト
    public bool isDead = false;
    public bool isPhrase2 = false;
    public bool isStandStill = false;
    public bool isShieldBroken = false; // シールドが壊れたかどうかのフラグ

    //Move
    public float moveSpeed = 2.8f; // 移動速度
    public float moveSpeedNormal = 2.8f;
    public float moveCntNext = 3.5f;

    //JumpAttack用変数
    [Header("JumpAttack用変数")]
    public ParticleSystem GroundHitEffectObj;
    public GameObject helicopterParticleLeft;
    public GameObject helicopterParticleRight;
    public GameObject aoeCircleObj;
    public AoeCircleDynamic aoeCircleIndicator;
    public float aoeCirclePositionTraceTimer = 3.5f;
    public float aoeCirclePositionTraceTimerMax = 3.5f; // AOEサークルの位置トレースタイマーの最大値
    public bool isAoeCircleStartFilling = false;
    public float aoeCircleFillDuration = 1.5f;
    public float aoeJumpAttackDamage = 25f;
    public float skyHeight = 4.2f;

    //MagicAttack用変数  
    [Header("MagicAttack用変数")]
    public bool isThisAttackFinished = false;
    public GameObject AoeAttackMagicCircleRoot;
    public GameObject AoeAttackMagicCircleTreeRoot; 
    public GameObject AoeAttackMagicExplosion;

    public GameObject AoeCurveProjectileSingal;

    public GameObject AoePosionPoolObj;
    public GameObject AoePoisonExplodeObj;
    public GameObject blackHoleObj;

    public ObjectPool AoeAttackMagicCircleTreeRootPool;

    //dashAttack用変数
    [Header("DashAttack用変数")]
    public float aoeRectPositionTraceTimer = 3.5f;
    public float aoeRectPositionTraceTimerMax = 3.5f; // AOEサークルの位置トレースタイマーの最大値
    public AoeRectIndicator aoeRectIndicator;
    public GameObject AoeDashAttackRectObj;
    public GameObject AoeCircleGroundAttack;
    public float aoeDashRectFillDuration = 1f;
    private float indicatorLingerTime = 0.2f; 

    public GameObject afterDashJumpCloudObj; // ダッシュ後のジャンプ時に表示する雲のオブジェクト


    //SummonSlime用変数
    [Header("SummonSlime用変数")]
    public GameObject slimePrefab; // スライムのプレハブ
    public bool hasSummonedSlime = false;

    //AttackTween
    public Vector3 punchRotationAmount = new Vector3(45f, 0f, 0f); //30 about X
    public float punchDuration        = 0.4f;
    public int   punchVibrato         = 4;   
    public float punchElasticity      = 0.2f;

    public int currentAtttackPattern = 0;
    public int phrase2AttackPattern = 0;

    //Sector Attack用変数
    [Header("Sector Attack用変数")]
    public ParticleSystem turnipaAcidEffect;
    public GameObject AoeMagicSector;

    //Dizzy変数
    [Header("Dizzy変数")]
    public ParticleSystem dizzyEffect;


    //Spiral Attack用変数
    [Header("Spiral Attack用変数")]
     public int   armCount     = 9;          // how many spiral arms (image ≈ 9–11)
     public int   stepCount    = 6;          // points per arm (increase for longer arms)
     public float firstRadius  = 2.4f;       // where the spiral starts (from boss centre)
     public float radialStep   = 2.1f;       // outward distance added each “ring”
     public float degPerStep   = 90f;        // how much the spiral twists every step
     public int   stepDelayMs  = 120;        // pause between rings (makes it blossom)
      
     public float circleScale  = 4.9f;       // copy-pasted from your other helpers
     public float fillTime     = 2.1f;
     public int   damage       = 15;


    //StandingCenter用関数
    public Vector3 centerPos = Vector3.zero;
    public ParticleSystem leaveShieldEffect;

    public GameObject channelEffectObject;


    private void OnEnable()
    {
        // イベントリスナーを登録
        EventManager.StartListening("BossProjectileSignal", ShotOnce);
    }

    private void OnDisable()
    {
        // イベントリスナーを解除
        EventManager.StopListening("BossProjectileSignal", ShotOnce);
    }

    public void ShotOnce()
    {
        ProjectileData shotData  = (ProjectileData)EventManager.GetData("BossProjectileSignal");

        ShotCurveProjectile(shotData.projectilePos);

    }

    void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player) playerTrans = player.transform;

        animator = GetComponent<Animator>();
        highlightEffect = GetComponent<HighlightEffect>();
        enemyStatus = GetComponent<EnemyStatusBase>();

        stateMachine = StateMachine<BossState>.Initialize(this);
        stateMachine.ChangeState(BossState.Move);

        originalScale = transform.localScale;

        //bossHpBar.SetActive(true); // ボスのHPバーを有効化
        BossHpBar hpbar = bossHpBar.GetComponentInChildren<BossHpBar>();
        hpbar.SetBossTargetObj(this.gameObject);
        bossHpBar.SetActive(true);
        


    }

    public float stretchTurnTime = 1.4f;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha5)) // キー入力でアニメーションのような動きを開始
        {
            //AnimationLikeMovement3(2.1f,stretchTurnTime);
            //AnimationLikeMovement4RotateLeftRight();
        }

        

        UpdateStateInfo();
        UpdatePlayerInfo();
        TryAnimSeq();

    }

    public async void TryAnimSeq()
    {

        if (Input.GetKeyDown(KeyCode.Alpha6)) // キー入力でアニメーションのような動きを開始
        {
            //AnimationLikeMovement2();
            //DoAttackAniTween();

            Sequence jumpSequence = CreateSlimeHopSequence();
            await jumpSequence.Play().AsyncWaitForCompletion();
        }

    }

    


    public void Move_Enter()
    {
        stateCnt = moveCntNext;
        moveSpeed = moveSpeedNormal;
        AnimationLikeMovementBounceUpDown(1);

    }

    public void Move_Update()
    {
        if (!isStandStill)
        {
            HomingPlayer();
            RotateTowardPlayer();
        }
        
        

        if(stateCnt <= 0)
        {

            if (enemyStatus.enemyHp < enemyStatus.enemyMaxHp / 2 && !isPhrase2)
            {
                isPhrase2 = true;
                ChangeToState(BossState.StandingCenter);

                return;
            }

            if (isPhrase2 && !isShieldBroken)
            {
                ChangeToState(BossState.MagicAoeAttack);
                currentAtttackPattern = 0;
                return;
            }

            if(isPhrase2 && isShieldBroken)
            {

            }


            //if (!hasSummonedSlime) { ChangeToState(BossState.SummonSlime); hasSummonedSlime = true; }
            //ChangeToState(BossState.JumpAttack);
            //ChangeToState(BossState.MagicAoeAttack);
            //ChangeToState(BossState.DashAttack);
            //ChangeToState(BossState.SectorAttack);
            //ChangeToState(BossState.TreeRootAttack);
            //ChangeToState(BossState.CurveProjectileAttack);

            switch (currentAtttackPattern)
            {
                case 0:
                    ChangeToState(BossState.DashAttack);
                    break;
                case 1:
                    ChangeToState(BossState.TreeRootAttack);
                    break;
                case 2:
                    ChangeToState(BossState.JumpAttack);
                    break;
                case 3:
                    ChangeToState(BossState.SectorAttack);
                    break;
                case 4:
                    ChangeToState(BossState.CurveProjectileAttack);
                    currentAtttackPattern = -1;
                    break;
                case 5:
                    //ChangeToState(BossState.MagicAoeAttack);
                    currentAtttackPattern = -1;
                    break;
                default:
                    break;
            }

            currentAtttackPattern++;

        }


    }

    public void Move_Exit()
    {
        currentTween?.Kill();
    }

    public void JumpAttack_Enter()
    {
        aoeCirclePositionTraceTimer = aoeCirclePositionTraceTimerMax; // AOEサークルの位置トレースタイマーをリセット
        isAoeCircleStartFilling = false; // AOEサークルの充填を開始しない

        transform.DOMoveY(skyHeight, aoeCirclePositionTraceTimerMax).SetEase(Ease.OutQuad);

        SetInnerGlow();

        GameObject aoeObj = Instantiate(aoeCircleObj);
        aoeCircleIndicator = aoeObj.GetComponent<AoeCircleDynamic>();

        helicopterParticleLeft.gameObject.SetActive(true);
        helicopterParticleRight.gameObject.SetActive(true);

        SoundEffect.Instance.Play(SoundList.TurnipaBossFlySe);

    }

    public void JumpAttack_Update()
    {
        if(aoeCirclePositionTraceTimer >= 0)
        {
             KeepRotatingY();
            //FacePlayerNow();
            aoeCirclePositionTraceTimer -= Time.deltaTime; // AOEサークルの位置トレースタイマーを減少
            
            Vector3 indicatorPos = playerTrans.position;
            indicatorPos.y = 0.1f; 
            float indicatorRadius = 7.7f;
            aoeCircleIndicator.UpdateTransform(indicatorPos, indicatorRadius);
        }
        

        if(aoeCirclePositionTraceTimer <= 0 && !isAoeCircleStartFilling)
        {
            SetInnerGlow(1.4f, 0.28f, 6);
            FacePlayerNow();
            isAoeCircleStartFilling = true; // AOEサークルの充填を開始
            aoeCircleIndicator.BeginFill(aoeCircleFillDuration, aoeJumpAttackDamage);
            StartCoroutine(FallAfterFill(aoeCircleFillDuration-0.35f)); // 充填後に落下する処理を開始
        }


    }

    public void JumpAttack_Exit()
    {

        moveCntNext = 0.4f;


    }


    private IEnumerator FallAfterFill(float waitTime)
    {
       
        yield return new WaitForSeconds(waitTime);

        //fix rotation x to 0 
        Vector3 fixedRotation = transform.rotation.eulerAngles;
        fixedRotation.x = 0f; // x軸の回転を0に固定
        fixedRotation.z = 0f; // z軸の回転を0に固定
        transform.rotation = Quaternion.Euler(fixedRotation);

 
        Vector3 jumpAttackTarget = aoeCircleIndicator.transform.position;
        jumpAttackTarget.y = 0.21f; 
        transform.DOMove(jumpAttackTarget, 0.35f).SetEase(Ease.InCubic).OnComplete(() => 
        {
            CameraShake.Instance.StartStrongShake();
            Invoke(nameof(ChangeToMoveState), 0.7f);

            helicopterParticleLeft.gameObject.SetActive(false);
            helicopterParticleRight.gameObject.SetActive(false);
            PlayGroundHitParticle();
            SoundEffect.Instance.Play(SoundList.SpiderBossFallGroundSe);

        });

    }

    public void PlayGroundHitParticle()
    {
        GroundHitEffectObj.gameObject.SetActive(true); // Ensure the particle system is active
        GroundHitEffectObj.Play();  
    }

    public void ResetPosY()
    {

    }

    

    public async void StraightLineTowardPlayerFormManyMagicAoeWithInterval()
    {
        // ─── Tunables ─────────────────────────────────────────────────────────────
    const float firstOffset = 2.8f;        // how far in front of the boss the 1st circle appears
    const float interval    = 4.2f;        // distance between consecutive circles
    const float circleScale = 4.9f;        // world–space diameter of every circle (you can expose)
    const float fillTime    = 2.1f;        // how long each circle charges before detonating
    const int   damage      = 15;          // damage passed to AoeCircle.InitCircle
    // ──────────────────────────────────────────────────────────────────────────

    // 1.  Early-out safety checks
    if (playerTrans == null) return;
    
    // 2.  Ground-plane vector from boss → player
    Vector3 start = transform.position;
    Vector3 end   = playerTrans.position;
    start.y = end.y = 0.5f;                // keep everything on the ground

    Vector3 dir   = (end - start).normalized;
    float totalDist = Vector3.Distance(start, end);

    // 3.  How many circles fit between boss and player?
    int count = Mathf.Max(1, Mathf.FloorToInt((totalDist - firstOffset) / interval)) + 3;

    // 4.  Spawn loop with 140 ms gaps
    for (int i = 0; i < count; i++)
    {
        Vector3 pos = start + dir * (firstOffset + i * interval);

        // clamp so we never go past the player’s feet
        //if (Vector3.Distance(pos, start) > totalDist - 0.5f) break;

        // ------ instantiate & prime the circle --------------------------------
        GameObject aoe = Instantiate(AoeAttackMagicCircleTreeRoot, pos, Quaternion.identity);
        AoeCircle circle = aoe.GetComponent<AoeCircle>();
            circle.hasSoundEffect = true;

        float finalScale = circleScale / 10f;   // your InitCircle expects both
        circle.InitCircle(circleScale, fillTime, damage, finalScale);

        ObjectLifeController life = aoe.GetComponent<ObjectLifeController>();
        life.lifeTimeMax = fillTime + 1.4f;
        life.lifeTime = fillTime + 1.4f;
            // ---------------------------------------------------------------------

            if (i % 2 != 0)SoundEffect.Instance.Play(SoundList.TurnipaBossPlaceAoeSe);

        await UniTask.Delay(140, cancellationToken: this.GetCancellationTokenOnDestroy());
    }



    }

    /// <summary>
/// Spawns four outward “rails” of magic circles (N-E-S-W) that charge
/// and detonate. Works very much like doing StraightLineTowardPlayerFormManyMagicAoeWithInterval
/// four times at once, but without copying any code from that method.
/// </summary>
public async void SpawnMagicAoeUpDownLeftRightMutiple()
{
        AnimationLikeMovement4RotateLeftRight();

    // ─── Tunables ─────────────────────────────────────────────────────────
    const float firstOffset = 2.8f;        // start this far from the boss
    const float interval    = 4.2f;        // spacing between rings
    const int   ringCount   = 4;           // how many circles per rail
    const float circleScale = 4.9f;        // world diameter of every circle
    const float fillTime    = 2.1f;        // charge-up time before detonation
    const int   damage      = 15;          // damage dealt (pass to InitCircle)
    const int   stepDelayMs = 140;         // delay between *rings* (NOT rails)
    // ──────────────────────────────────────────────────────────────────────

    Vector3 origin = transform.position;
    origin.y = 0.5f;                       // keep everything on the ground

    // Pre-compute constant data once – cheaper in a loop.
    float finalScale = circleScale / 10f;
    float lifeTime   = fillTime + 1.4f;

    // Four cardinal directions.  You can add diagonals later if you like.
    Vector3[] dirs =
    {
        Vector3.right,   // +X  (East)
        Vector3.left,    // –X  (West)
        Vector3.forward, // +Z  (North)
        Vector3.back     // –Z  (South)
    };

    // Outer loop = the “rings” marching outward.
    // Inside that we iterate the four rails *synchronously*,
    // so every ring appears at the same radius for all directions.
    for (int r = 0; r < ringCount; ++r)
    {
        float distFromBoss = firstOffset + r * interval;

        foreach (Vector3 dir in dirs)
        {
            Vector3 pos = origin + dir * distFromBoss;

            GameObject aoe = Instantiate(AoeAttackMagicCircleTreeRoot,
                                         pos, Quaternion.identity);

            // Prime the circle exactly as in your other helpers
            AoeCircle circle = aoe.GetComponent<AoeCircle>();
            circle.InitCircle(circleScale, fillTime, damage, finalScale);

            ObjectLifeController life = aoe.GetComponent<ObjectLifeController>();
            life.lifeTimeMax = lifeTime;
            life.lifeTime    = lifeTime;
        }

        // Small pause so the next “ring” feels like a pulse.
        await UniTask.Delay(stepDelayMs,
                            cancellationToken: this.GetCancellationTokenOnDestroy());
    }
}

    /// <summary>
/// Spawn several “spiral arms” of AOE circles that grow outward together,
/// creating a pin-wheel pattern.  All circles use the same scale / timing
/// you already defined for the other magic attacks, so the visuals stay
/// consistent with your game’s VFX.
/// </summary>
public async void SpawnMagicAoeSpiralShapeMutiple()
{
    AnimationLikeMovement4RotateLeftRight();        // optional flair

   
    Vector3 centre = transform.position;   // spiral origin = boss feet
    centre.y = 0.5f;                       // keep on ground plane

    float finalScale = circleScale / 10f;  // your InitCircle contract
    float lifeTime   = fillTime + 1.4f;

    // Pre-convert constants once
    float radPerStep = degPerStep * Mathf.Deg2Rad;
    float twoPiOverArm = 2f * Mathf.PI / armCount;

    // Outward march: each “s” is an expanding ring shared by every arm
    for (int s = 0; s < stepCount; ++s)
    {
        // Archimedean spiral  r = a + bθ    (here b = radialStep / radPerStep)
        float thetaBase = s * radPerStep;                    // common twist
        float radius    = firstRadius + s * radialStep;      // outward growth
        if (s % 2 != 0)SoundEffect.Instance.Play(SoundList.TurnipaBossPlaceAoeSe);


        for (int a = 0; a < armCount; ++a)
        {
            float theta = thetaBase + a * twoPiOverArm;      // arm offset

            // Convert polar → Cartesian
            Vector3 pos = centre + new Vector3(
                Mathf.Cos(theta) * radius,
                0f,
                Mathf.Sin(theta) * radius);

            // Build & prime the circle (identical to other attacks)
            GameObject aoe = Instantiate(AoeAttackMagicCircleTreeRoot,
                                         pos, Quaternion.identity);

            AoeCircle circle = aoe.GetComponent<AoeCircle>();
            circle.InitCircle(circleScale, fillTime, damage, finalScale);

            ObjectLifeController life = aoe.GetComponent<ObjectLifeController>();
            life.lifeTimeMax = lifeTime;
            life.lifeTime    = lifeTime;
        }

        // Delay before the next “ring” so the pattern blooms smoothly
        await UniTask.Delay(stepDelayMs,
                            cancellationToken: this.GetCancellationTokenOnDestroy());
    }
}


    /*************************************************************************
 * Drop N AOE circles along a Bézier curve that passes through the player.
 *   – ‘halfSpan’   = horizontal half-width of the arc (metres)
 *   – ‘bendFwd’    = how far to pull the arc forward/back along dir
 *   – ‘count’      = number of circles
 *   – other tunables match your existing straight-line helpers
 *************************************************************************/
public async UniTaskVoid SpawnMagicAoeCurveLine(
        float halfSpan = 6f,
        float bendFwd  = 4f,
        int   count    = 5,
        float circleScale = 4.9f,
        float fillTime    = 2.1f,
        int   damage      = 15,
        int   interDelay  = 140)
{
    if (playerTrans == null) return;

        AnimationLikeMovement4RotateLeftRight();

    // ─── 0.  Grab ground-plane positions ─────────────────────────────────
    Vector3 playerPos = playerTrans.position;
    Vector3 bossPos   = transform.position;

    playerPos.y = bossPos.y = 0.5f;        // keep everything on terrain

    // ─── 1.  Build local basis vectors ───────────────────────────────────
    Vector3 dir  = (playerPos - bossPos).normalized;          // boss → player
    Vector3 perp = Vector3.Cross(Vector3.up, dir).normalized; // left

    // ─── 2.  End-points (symmetric about the player) ─────────────────────
    Vector3 P0 = playerPos - perp * halfSpan;
    Vector3 P2 = playerPos + perp * halfSpan;

    // ─── 3.  Control point calculated so B(0.5)=playerPos, plus a bend ───
    Vector3 C  = 2f * playerPos - 0.5f * (P0 + P2)
               + dir * bendFwd;        // push the peak toward/away from boss

    // ─── 4.  Common numbers for InitCircle ───────────────────────────────
    float finalScale = circleScale / 10f;
    float lifeTime   = fillTime + 1.4f;

    // ─── 5.  Spawn loop along the Bézier 0…1 ─────────────────────────────
    for (int i = 0; i < count; ++i)
    {
        float t = i / (float)(count - 1);          // 0, 0.25, … 1

        // B(t) = (1-t)² P0 + 2(1-t)t C + t² P2
        Vector3 pos =
            (1 - t) * (1 - t) * P0 +
            2 * (1 - t) * t * C +
            t * t * P2;

        GameObject aoe = Instantiate(AoeCurveProjectileSingal, 
                                     pos, Quaternion.identity);//AoeAttackMagicCircleTreeRoot //AoePoisonExplodeObj

        AoeCircle circle = aoe.GetComponent<AoeCircle>();
        circle.InitCircle(circleScale, fillTime, damage, finalScale);

        var life = aoe.GetComponent<ObjectLifeController>();
        life.lifeTimeMax = lifeTime;
        life.lifeTime    = lifeTime;

        // Optional stagger so they don’t all appear in the same frame
        await UniTask.Delay(interDelay,
                            cancellationToken: this.GetCancellationTokenOnDestroy());
    }
}

    public async void SpawnACircleOfMagicAoe(float radius = 7.7f, float scale = 4.2f,int aoeNum = 12, float duration = 2.1f)
    {
        //Spawn a circle of multiple AOE circles around transform.position
        Vector3 circleCenter = transform.position;
        float circleRadius = radius; // AOEサークルの半径
        float aoeScale = scale;
        float aoeDuration = duration; // AOEサークルの持続時間
        int aoeCount = aoeNum; // AOEサークルの数

        for (int i = 0; i < aoeCount; i++)
        {
            float angle = i * (360f / aoeCount);
            float radian = angle * Mathf.Deg2Rad;
            Vector3 spawnPos = circleCenter + new Vector3(Mathf.Cos(radian) * circleRadius, 0, Mathf.Sin(radian) * circleRadius);

            GameObject aoeBall = Instantiate(AoeAttackMagicCircleRoot, spawnPos, Quaternion.identity);
            AoeCircle aoeCircle = aoeBall.GetComponent<AoeCircle>();
            aoeCircle.InitCircle(aoeScale, aoeDuration, 10, aoeScale / 10);

            ObjectLifeController aoeLife = aoeCircle.GetComponent<ObjectLifeController>();
            aoeLife.lifeTimeMax = aoeDuration + 0.14f;

            await UniTask.Delay(140
                , cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }

    public void SpawnCircleStep1()
    {
        SpawnACircleOfMagicAoeNow(7.7f,5.6f,11,2.8f);
        channelEffectObject.SetActive(true);

        blackHoleObj.SetActive(true);

        SoundEffect.Instance.Play(SoundList.TurnipaBossPlaceMultipleAoeSe);
    }

    public void SpawnCircleStep2()
    {
        SpawnACircleOfMagicAoeNow(13f,7.7f,14, 3.5f);  
        
        SoundEffect.Instance.Play(SoundList.TurnipaBossPlaceMultipleAoeSe);

    }

    public void SpawnCircleStep3()
    {
        SpawnACircleOfMagicAoeNow(0.21f,21.9f,1, 2.8f);  
        
        isShieldBroken = true;

        channelEffectObject.SetActive(false);

        SoundEffect.Instance.Play(SoundList.TurnipaBossPlaceMultipleAoeSe);

    }

    public void SpawnACircleOfMagicAoeNow(float radius = 4.9f, float scale = 4.9f,int aoeNum = 7, float duration = 2.1f)
    {
        //Spawn a circle of multiple AOE circles around transform.position
        Vector3 circleCenter = transform.position;
        float circleRadius = radius; // AOEサークルの半径
        float aoeScale = scale;
        float aoeDuration = duration; // AOEサークルの持続時間
        int aoeCount = aoeNum; // AOEサークルの数

        for (int i = 0; i < aoeCount; i++)
        {
            float angle = i * (360f / aoeCount);
            float radian = angle * Mathf.Deg2Rad;
            Vector3 spawnPos = circleCenter + new Vector3(Mathf.Cos(radian) * circleRadius, 0, Mathf.Sin(radian) * circleRadius);

            GameObject aoeBall = Instantiate(AoeAttackMagicCircleRoot, spawnPos, Quaternion.identity); //AoeAttackMagicCircleRoot
            AoeCircle aoeCircle = aoeBall.GetComponent<AoeCircle>();
            aoeCircle.InitCircle(aoeScale, aoeDuration, 25, aoeScale / 10);

            ObjectLifeController aoeLife = aoeCircle.GetComponent<ObjectLifeController>();
            aoeLife.lifeTimeMax = aoeDuration + 0.14f;

            
        }
    }


    public async void SpawnMagicAoeUpDownLeftRight()
    {

        float spawnRadius = 11f; // AOEサークルの半径
        int spawnNum = 4; // 4方向に配置

        // プレイヤーを中心に4方向にAOEサークルを配置
        List<Vector3> spawnPositions = new List<Vector3>
        {
            playerTrans.position + new Vector3(spawnRadius, 0, 0), // Right
            playerTrans.position + new Vector3(-spawnRadius, 0, 0), // Left
            playerTrans.position + new Vector3(0, 0, spawnRadius), // Up
            playerTrans.position + new Vector3(0, 0, -spawnRadius) // Down
        };

        foreach (Vector3 pos in spawnPositions)
        {
            float circleScale = Random.Range(2.8f, 5.6f); // ランダムなスケールを生成
            float finalScale = circleScale / 10; // スケールを10で割る
            float circleDuration = Random.Range(1.4f, 2.8f); // ランダムな持続時間を生成
            Vector3 aoeSpawnPos = new Vector3(pos.x, 0.5f, pos.z); // Y座標を0.5に固定
            GameObject aoeBall = Instantiate(AoeCurveProjectileSingal, aoeSpawnPos, Quaternion.identity);

            aoeBall.SetActive(true);
            AoeCircle aoeCircle = aoeBall.GetComponent<AoeCircle>();
            aoeCircle.InitCircle(circleScale, circleDuration, 10, finalScale);
            ObjectLifeController aoeLife = aoeCircle.GetComponent<ObjectLifeController>();
            aoeLife.lifeTimeMax = circleDuration + 0.14f;

            await UniTask.Delay(140
                , cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }

        public async void SpawnMagicAoe360Degree()
    {
        float spawnRadius = 11f;
        int spawnNum = 12; // 360度を12分割

        //spawn 12 aoe circles with player as center of circle evenly distributed around player in the margin of circle : 
        List<Vector3> spawnPositions = new List<Vector3>();
        for (int i = 0; i < spawnNum; i++)
        {
            float angle = i * (360f / spawnNum);
            float radian = angle * Mathf.Deg2Rad;
            Vector3 spawnPos = playerTrans.position + new Vector3(Mathf.Cos(radian) * spawnRadius, 0, Mathf.Sin(radian) * spawnRadius);
            spawnPositions.Add(spawnPos);
        }

        // Instantiate AOE circles at the calculated positions
        foreach (Vector3 pos in spawnPositions)
        {
            float circleScale = 7f; // ランダムなスケールを生成
            float finalScale = circleScale / 10; // スケールを10で割る
            float circleDuration = 2.8f; // ランダムな持続時間を生成
            Vector3 aoeSpawnPos = new Vector3(pos.x, 0.5f, pos.z); // Y座標を0.5に固定
            GameObject aoeBall = Instantiate(AoeCurveProjectileSingal, aoeSpawnPos, Quaternion.identity);

            aoeBall.SetActive(true);
            AoeCircle aoeCircle = aoeBall.GetComponent<AoeCircle>();
            aoeCircle.InitCircle(circleScale, circleDuration, 10, finalScale);
            ObjectLifeController aoeLife = aoeCircle.GetComponent<ObjectLifeController>();
            aoeLife.lifeTimeMax = circleDuration + 0.14f;

            await UniTask.Delay(140
                , cancellationToken: this.GetCancellationTokenOnDestroy());
        }

    }

    public async void SpawnMagicAoeAround()
    {
        AnimationLikeMovement4RotateLeftRight();

        float spawnRadius = 11f;
        int spawnNum = 7;

        List<Vector3> spawnPositions = new List<Vector3>();
        for (int i = 0; i < spawnNum; i++)
        {
            Vector3 spawnPos;
            bool validPosition;

            do
            {
                validPosition = true;
                float angle = Random.Range(0f, 360f);
                float radius = Random.Range(0f, spawnRadius);
                spawnPos = playerTrans.position + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

                foreach (Vector3 pos in spawnPositions)
                {
                    if (Vector3.Distance(spawnPos, pos) < 3.5f)
                    {
                        validPosition = false;
                        break;
                    }
                }

            } while (!validPosition);

            spawnPositions.Add(spawnPos);
        }

        foreach (Vector3 pos in spawnPositions)
        {
            float circleScale = Random.Range(2.8f, 5.6f); // ランダムなスケールを生成
            float finalScale = circleScale / 10; // スケールを10で割る
            float circleDuration = Random.Range(1.4f, 2.8f); // ランダムな持続時間を生成
            Vector3 aoeSpawnPos = new Vector3(pos.x, 0.5f, pos.z); // Y座標を0.5に固定
            GameObject aoeBall = Instantiate(AoeCurveProjectileSingal, aoeSpawnPos, Quaternion.identity);
            
            aoeBall.SetActive(true);
            AoeCircle aoeCircle = aoeBall.GetComponent<AoeCircle>();
            aoeCircle.InitCircle(circleScale, circleDuration, 10,finalScale);           
            ObjectLifeController aoeLife = aoeCircle.GetComponent<ObjectLifeController>();
            aoeLife.lifeTimeMax = circleDuration + 0.14f;

            await UniTask.Delay(140
                , cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }

    public async void MagicAoeAttack_Enter()
    {
        stateCnt = 3.5f; //7
        SetInnerGlow(1.4f, 0.28f, 6);
        AnimationLikeMovementBounceUpDown(1);

        switch(phrase2AttackPattern)
        {
            case 0:
                stateCnt = 3.5f;
                SpawnMagicAoeAround();
                SoundEffect.Instance.Play(SoundList.TurnipaBossPlaceMultipleAoeSe);
                //SpawnMagicAoeUpDownLeftRightMutiple();
                break;
            case 1:
                stateCnt = 3.5f;
                SpawnMagicAoeSpiralShapeMutiple();
                SoundEffect.Instance.Play(SoundList.TurnipaBossPlaceMultipleAoeSe);
                break;
            case 2:
                stateCnt = 10f;
                SpawnACircleOfMagicAoeNow();
                Invoke(nameof(SpawnCircleStep1), 1.9f);
                Invoke(nameof(SpawnCircleStep2), 3.5f);
                Invoke(nameof(SpawnCircleStep3), 7f);
                SoundEffect.Instance.Play(SoundList.TurnipaBossPlaceMultipleAoeSe);
                break;
            case 3:
                phrase2AttackPattern = -1;
                break;
        }

        phrase2AttackPattern++;

        //SpawnMagicAoeAround();
        //SpawnMagicAoe360Degree();
        //SpawnMagicAoeUpDownLeftRight();
        //SpawnMagicAoeUpDownLeftRightMutiple();
        //SpawnMagicAoeSpiralShapeMutiple();


        //SpawnACircleOfMagicAoe();

        //SpawnACircleOfMagicAoeNow();
        //Invoke(nameof(SpawnCircleStep1), 1.9f);
        //Invoke(nameof(SpawnCircleStep2), 3.5f);
        //Invoke(nameof(SpawnCircleStep3), 5.2f);

    }

    public void MagicAoeAttack_Update()
    {

        moveCntNext = 3.5f;
        if (stateCnt <= 0)
        {
            if (isShieldBroken) ChangeToState(BossState.Dizzy);
            else ChangeToState(BossState.Move);
        }

    }

    public void MagicAoeAttack_Exit()
    {

    }

    public void DashAttack_Enter()
    {
        stateCnt = 3.0f;
        StartCoroutine(DashAttackCoroutine());
    }

    public void DashAttack_Update()
    {


        //if (stateCnt <= 0) ChangeToMoveState();
    }

    public void DashAttack_Exit()
    {

    }

    private IEnumerator DashAttackCoroutine()
    {
        

        aoeRectPositionTraceTimer = aoeRectPositionTraceTimerMax; 
        GameObject aoeRect = Instantiate(AoeDashAttackRectObj);
        aoeRectIndicator = aoeRect.GetComponent<AoeRectIndicator>();

        Vector3 dashTargetPos;

        while (aoeRectPositionTraceTimer > 0f)
        {
            FacePlayerNow();
            aoeRectPositionTraceTimer -= Time.deltaTime;

            Vector3 indicatorPosition = transform.position + dirToPlayer * (distToPlayer * 0.5f);
            Quaternion indicatorRotation = Quaternion.LookRotation(dirToPlayer, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);
            Vector2 indicatorSize = new Vector2(distToPlayer, 3f); 

            aoeRectIndicator.UpdateTransform(indicatorPosition,indicatorRotation,indicatorSize);
            yield return null;
        }

        dashTargetPos = transform.position + dirToPlayer * distToPlayer;
        aoeRectIndicator.BeginFill(aoeDashRectFillDuration, indicatorLingerTime, 25, true);
        yield return new WaitForSeconds(aoeDashRectFillDuration + indicatorLingerTime);

        SoundEffect.Instance.Play(SoundList.SpiderBossDashSe);

        transform.DOMove(dashTargetPos, 0.5f).SetEase(Ease.Linear).OnComplete(() => {
            DoAttackAniTween();

            GameObject aoeCircleGroundAttack = Instantiate(AoeCircleGroundAttack, new Vector3(transform.position.x, 0.5f, transform.position.z), Quaternion.identity);
            SetInnerGlow(1.4f, 0.28f, 6);
            Invoke(nameof(ReleasePoisonCloud), 2.1f);
            Invoke(nameof(ChangeToMoveState), 4.9f);
        });
    }

    public void JumpOnce()
    {
        //dowtween jump to 1.4 and back to 0.5f in 0.4 seconds, 
        transform.DOMoveY(1.4f, 0.1f).SetEase(Ease.OutQuad).OnComplete(() => {
            transform.DOMoveY(0.5f, 0.1f).SetEase(Ease.InQuad);
        });

     

    }

    public void ReleasePoisonCloud()
    {

        //instantiate afterDashJumpCloudObj at the position of the boss
        GameObject cloud = Instantiate(afterDashJumpCloudObj, transform.position + new Vector3(0,1.1f,0), Quaternion.identity);
        ParticleSystem cloudPs = cloud.GetComponent<ParticleSystem>();
        cloudPs.Play();
        AnimationLikeMovement4RotateLeftRight();

        SoundEffect.Instance.Play(SoundList.TurnipaBossPosionCloudSe);

        //dowtween jump to 1.4 and back to 0.5f in 0.4 seconds, loop three times
        //Sequence jumpSequence = DOTween.Sequence();
        //for (int i = 0; i < 3; i++)
        //{
        //    jumpSequence.Append(transform.DOMoveY(1.4f, 0.28f).SetEase(Ease.OutQuad));
        //    jumpSequence.Append(transform.DOMoveY(0.5f, 0.1f).SetEase(Ease.InQuad));
        //    CameraShake.Instance.StartTurnipaJumpShake();

        //}

    }

    public void SummonSlime_Enter()
    {

        stateCnt = 3.5f;
        AnimationLikeMovement4RotateLeftRight();


        Invoke((nameof(SummonFourSlime)), 1.5f);

        SoundEffect.Instance.Play(SoundList.SpiderBossPhrase2Se);



    }

    public void SummonFourSlime()
    {
        for (int i = 0; i < 4; i++)
        {
            float angle = i * Mathf.PI / 2f; // 90度ごとに配置
            Vector3 spawnPos = transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 2.5f; // 半径2.5の円周上に配置
            GameObject slime =  Instantiate(slimePrefab, transform.position, Quaternion.identity);
            EnemyBossSlimeMobAction slimeAct = slime.GetComponent<EnemyBossSlimeMobAction>();
            slimeAct.spawnPosTarget = spawnPos;

        }

        SoundEffect.Instance.Play(SoundList.TurnipaBossSummonSlimeSe);

    }

    public void SummonSlime_Update()
    {



        if (stateCnt <= 0) ChangeToMoveState();
    }

    public void SummonSlime_Exit()
    {
        moveCntNext = 7f;



    }

    public void TreeRootAttack_Enter()
    {
        stateCnt = 3.5f;
        SetInnerGlow();

        AnimationLikeMovementBounceUpDown(1.4f);
        StraightLineTowardPlayerFormManyMagicAoeWithInterval();
    }

    public void TreeRootAttack_Update()
    {
     
        if(stateCnt <=0 ) ChangeToMoveState();
    }

    public void TreeRootAttack_Exit()
    {

    }

    public void CurveProjectileAttack_Enter()
    {
        stateCnt = 3.5f;
        SetInnerGlow();

        SpawnMagicAoeCurveLine();
    }

    public void CurveProjectileAttack_Update()
    {

        if (stateCnt <= 0)
        {
            ChangeToMoveState();
        }

    }

    public void CurveProjectileAttack_Exit()
    {

    }


    public void SectorAttack_Enter()
    {
        stateCnt = 3.5f;

        SetInnerGlow();

        FacePlayerNow(); 
        Vector3 spawnPos = transform.position + new Vector3(0,0.015f,0) ; // Y座標を0.5に固定
        GameObject aoeSector = Instantiate(AoeMagicSector, spawnPos, transform.rotation);


        //AnimationLikeMovementBounceUpDown(1.4f);
        Invoke(nameof(SplashAcid), 2f); // 少し遅れて酸を噴射



    }

    public void SectorAttack_Update()
    {
        if (stateCnt <= 0)
        {
            ChangeToMoveState();
            turnipaAcidEffect.gameObject.SetActive(false);
            turnipaAcidEffect.Stop();
        }
    }

    public void SectorAttack_Exit()
    {
        moveCntNext = 2.8f;
    }

    public void SplashAcid()
    {
        //AnimationLikeMovement4RotateLeftRight();

        turnipaAcidEffect.gameObject.SetActive(true);
        turnipaAcidEffect.Play();
        SoundEffect.Instance.Play(SoundList.TurnipaBossAcidSplashSe);
    }

    public void Dead_Enter()
    {
        animator.SetTrigger("isDead");

        SoundEffect.Instance.Play(SoundList.SpiderBossDeadSe);

        SoundEffect.Instance.Play(SoundList.SpiderBossPhrase2Se);
        DOTween.Kill(transform, complete: true);
        StopAllCoroutines();
        CancelInvoke();

        //Destroy all object with tag "Indicator" in hierchy
        GameObject[] indicators = GameObject.FindGameObjectsWithTag("Indicator");
        foreach (GameObject indicator in indicators)
        {
            Destroy(indicator);
        }

        //Destory all enemy tag object 
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            if(!enemy.GetComponent<EnemyStatusBase>().isBoss) Destroy(enemy);      
        }


    }

    public void Dizzy_Enter()
    {
        stateCnt = 11f; 

        currentTween?.Kill();

        gameObject.tag = "Enemy";

        leaveShieldEffect.gameObject.SetActive(false);
        dizzyEffect.gameObject.SetActive(true);

        //isShieldBroken = true;
        isStandStill = false;

        //transform.rotation = Quaternion.Euler(0, 0, 180f);
  
        DOTween.Kill(transform, complete: true);
        StopAllCoroutines();
        CancelInvoke();
    }

    public void Dizzy_Update()
    {

        if (stateCnt <= 0)
        {
            dizzyEffect.gameObject.SetActive(false);
            ChangeToMoveState();
        }

       
    }

    public void StandingCenter_Enter()
    {
        stateCnt = 3.5f;

        AnimationLikeMovementBounceUpDown(1);

        isStandStill = true;

        //dotween move to centerPos in 3 second 
        transform.DOMove(centerPos, 3f).SetEase(Ease.InOutQuad).OnComplete(() => {
            
            transform.rotation = Quaternion.Euler(0, 180f, 0);
            leaveShieldEffect.gameObject.SetActive(true); 
            leaveShieldEffect.Play();
            gameObject.tag = "UnSelectableEnemy";

            ChangeToState(BossState.SummonSlime); 

        });

    }

    public void StandingCenter_Update()
    {
        RotateTowardTarget(centerPos);

        ChangeToMoveState();
    }

     public void StandingCenter_Exit()
    {
        moveCntNext = 3.5f;
    }

    public void Death_Enter()
    {
        animator.SetTrigger("isDead");


        currentTween?.Kill();

        gameObject.tag = "UnSelectableEnemy";

        leaveShieldEffect.gameObject.SetActive(false);
        dizzyEffect.gameObject.SetActive(true);

        DOTween.Kill(transform, complete: true);
        StopAllCoroutines();
        CancelInvoke();

    }

    public void Death_Update()
    {
        // 何もしない
    }

    public void ChangeToDead()
    {
        ChangeToState(BossState.Death);
    }
    public void ChangeToState(BossState newState)
    {
        stateMachine.ChangeState(newState);
        stateInfoText = stateMachine.State.ToString(); // 状態名を文字列に変換
    }

    public void ChangeToMoveState() => stateMachine.ChangeState(BossState.Move);


    //=======Helper=======//
    public float rotYPow = 7.7f;
    public void KeepRotatingY() 
    {
        //rotate along y

        Vector3 currentRotation = transform.rotation.eulerAngles;
        currentRotation.y += rotYPow; // 1度ずつ回転
        transform.rotation = Quaternion.Euler(currentRotation);


        

    }

    void DoAttackAniTween()
    {
       transform.DOPunchRotation(punchRotationAmount,punchDuration,vibrato: punchVibrato,elasticity: punchElasticity).SetEase(Ease.OutQuad);
    }
    public void SetInnerGlow(float targetVal = 1.4f, float duration =0.28f, int loopTimes = 6)
    {
        highlightEffect.innerGlow = 0.1f; // 初期値を設定
        DOTween.To(() => highlightEffect.innerGlow, x => highlightEffect.innerGlow = x, targetVal, duration)
            .SetLoops(loopTimes, LoopType.Yoyo) // 指定回数往復
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                highlightEffect.innerGlow = 0.1f;
            });
 
    }

    public void UpdateStateInfo()
    {
        stateInfoText = stateMachine.State.ToString();
        stateCnt -= Time.deltaTime;
    }

    public void UpdatePlayerInfo() 
    {
        dirToPlayer = (playerTrans.position - transform.position).normalized; // プレイヤーの方向を計算
        distToPlayer = Vector3.Distance(transform.position, playerTrans.position); // プレイヤーとの距離を計算

    }

    public void HomingPlayer()
    {
        Vector3 direction = (playerTrans.position - transform.position).normalized;
        transform.position += direction * Time.deltaTime * moveSpeed; // 移動速度は調整可能
        
    }

    public void RotateTowardTarget(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        direction.y = 0; // 水平方向のみに回転
        if (direction == Vector3.zero) return; // 方向がゼロベクトルの場合は回転しない
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // 回転速度は調整可能

    }

    public void RotateTowardPlayer()
    {
        if (playerTrans == null) return;
        Vector3 direction = playerTrans.position - transform.position;
        direction.y = 0;
        if (direction == Vector3.zero) return;
        Quaternion targetLookRotation = Quaternion.LookRotation(-direction);
        float targetYAngle = targetLookRotation.eulerAngles.y;
        Quaternion finalTargetRotation = Quaternion.Euler(-180f, targetYAngle, 180f);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalTargetRotation, Time.deltaTime * 5f);

    }

    public void FacePlayerNow()
    {
        if (playerTrans == null) return;
        Vector3 direction = (playerTrans.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation; // プレイヤーの方向を向く

    }

    public void AnimationLikeMovement1()
    {
        // Cache original values so we can restore cleanly.
    Vector3 baseScale   = transform.localScale;
    Quaternion baseRot  = transform.rotation;
    float hopDistance   = 1.2f;   // forward units
    float hopHeight     = 0.7f;   // upward arc
    float totalTime     = 0.6f;   // full cycle

    // Build the sequence
    Sequence seq = DOTween.Sequence();

    // 1) Anticipation – quick squash
    seq.Append(transform.DOScale(
            new Vector3(baseScale.x * 1.25f,  // wider
                        baseScale.y * 0.75f,  // flatter
                        baseScale.z * 1.25f),
            totalTime * 0.15f)
        .SetEase(Ease.InQuad));

    // 2) Launch – stretch & jump
    seq.Append(transform.DOScale(
            new Vector3(baseScale.x * 0.8f,
                        baseScale.y * 1.3f,
                        baseScale.z * 0.8f),
            totalTime * 0.25f)
        .SetEase(Ease.OutQuad));

    //      Combine a forward hop with the stretch
    seq.Join(transform
        .DOLocalJump(
            transform.localPosition + transform.forward * hopDistance,
            hopHeight,
            1,
            totalTime * 0.25f)
        .SetEase(Ease.OutQuad));

    // 3) Landing – overshoot then settle
    seq.Append(transform.DOScale(
            new Vector3(baseScale.x * 1.15f,
                        baseScale.y * 0.9f,
                        baseScale.z * 1.15f),
            totalTime * 0.15f)
        .SetEase(Ease.InOutQuad));

    // 4) Return to idle shape with a little jelly wobble
    seq.Append(transform.DOScale(baseScale, totalTime * 0.1f)
        .SetEase(Ease.OutElastic));

    seq.Join(transform.DOShakeRotation(
            totalTime * 0.4f,
            new Vector3(10f, 0f, 10f),  // degrees on X & Z
            vibrato: 8));

    // 5) Micro-wobble loop (optional)
    seq.AppendCallback(() =>
        transform.DOShakeScale(0.3f, 0.05f, 6, 90, true)
                 .SetLoops(-1, LoopType.Yoyo)
                 .SetEase(Ease.InOutSine));

    // Make sure we reset cleanly if the object is disabled
    seq.OnKill(() =>
    {
        transform.localScale = baseScale;
        transform.rotation   = baseRot;
    });
    }

    public void AnimationLikeMovement2()
    {

    }


    //=======Animation=======//
    private Vector3 originalScale; // Store the original scale to return to
    private Tween currentAnimationTween;
   
    private Sequence CreateSlimeHopSequence()
{
    // Kill any previously running animation to avoid conflicts
    if (currentAnimationTween != null && currentAnimationTween.IsActive())
    {
        currentAnimationTween.Kill();
    }

    // --- Animation Parameters ---
    float anticipationDuration = 0.3f;
    float jumpDuration = 0.5f;
    float jumpPower = 1.5f;
    float settleDuration = 0.4f;
    Vector3 moveDirection = (playerTrans.position - transform.position).normalized;
    Vector3 targetPosition = transform.position + moveDirection * 2f;

    // --- Create the Animation Sequence ---
    Sequence s = DOTween.Sequence();
    
    // (The exact same sequence logic as in AnimationLikeMovement1)
    s.Append(transform.DOScale(new Vector3(originalScale.x * 1.2f, originalScale.y * 0.6f, originalScale.z * 1.2f), anticipationDuration).SetEase(Ease.InSine));
    s.AppendCallback(() => transform.rotation = Quaternion.LookRotation(moveDirection));
    s.Append(transform.DOJump(targetPosition, jumpPower, 1, jumpDuration).SetEase(Ease.OutSine));
    s.Insert(anticipationDuration, transform.DOScale(new Vector3(originalScale.x * 0.7f, originalScale.y * 1.3f, originalScale.z * 0.7f), jumpDuration * 0.8f).SetEase(Ease.InOutSine));
    s.Append(transform.DOShakeScale(settleDuration, new Vector3(0.5f, 0.8f, 0.5f), 10, 90).SetEase(Ease.OutElastic));
    s.Append(transform.DOScale(originalScale, 0.1f));

    // We set this so we can still kill it if needed, but we don't play it here.
    currentAnimationTween = s;
    s.SetAutoKill(false); // Important: prevent the sequence from being destroyed on completion if you want to reuse it.
    return s;
}

    Tween currentTween;   // add this field to the class if you haven’t yet
    public float stretchExtend = 1f;
public void AnimationLikeMovementBounceUpDown(float extend, float totalTimePerTurn = 2.1f)
{
    stretchExtend = extend;
    currentTween?.Kill();

    Vector3 baseScale = transform.localScale;
    //Quaternion baseRot = transform.rotation;
    float total = totalTimePerTurn;

    // Helper lambdas: returns the scaled vector once
    Vector3 StretchX(float delta) => new Vector3(baseScale.x + delta * stretchExtend, baseScale.y - delta * stretchExtend, baseScale.z);
    Vector3 SquashX(float delta) => new Vector3(baseScale.x - delta * stretchExtend, baseScale.y + delta * stretchExtend, baseScale.z);

    Sequence seq = DOTween.Sequence();

    float d = 0.4f;   // default extra stretch (1 ± 0.4)

    // Phase 1 – inward squash
    seq.Append(transform.DOScale(SquashX(d), total * 0.15f).SetEase(Ease.InQuad));

    // Phase 2 – stretch left
    seq.Append(transform.DOScale(StretchX(d), total * 0.20f).SetEase(Ease.OutQuad));
    //seq.Join(transform.DORotateQuaternion(baseRot * Quaternion.Euler(0,-10f*stretchExtend,0),total * 0.20f));

    // Phase 3 – rebound
    seq.Append(transform.DOScale(SquashX(d), total * 0.15f).SetEase(Ease.InOutQuad));
    //seq.Join(transform.DORotateQuaternion(baseRot, total * 0.15f));

    // Phase 4 – stretch right
    seq.Append(transform.DOScale(StretchX(d), total * 0.20f).SetEase(Ease.OutQuad));
    //seq.Join(transform.DORotateQuaternion(baseRot * Quaternion.Euler(0,10f*stretchExtend,0), total * 0.20f));

    // Phase 5 – settle
    //seq.Append(transform.DOScale(baseScale, total * 0.1f).SetEase(Ease.OutElastic, overshoot: 1f));
    seq.SetLoops(-1, LoopType.Yoyo);

    seq.OnKill(() => { 
        transform.localScale = baseScale; 
        //transform.rotation = baseRot; 
    });

    currentTween = seq;




}

    Tween lookAroundTween;
    public void AnimationLikeMovement4RotateLeftRight(int rotateTimes = 5,
                                   float edgeAngle = 50f,
                                   float edgeTime  = 0.14f)
{
    // ─── Safety: kill any previous run ───────────────────────────────────────
    lookAroundTween?.Kill();

    // Clamp to sane range
    rotateTimes = Mathf.Max(1, rotateTimes);

    // Cache orientations
    Quaternion baseRot  = transform.rotation;                              // centre
    Quaternion leftRot  = baseRot * Quaternion.Euler(0f, -edgeAngle, 0f);  // –70°
    Quaternion rightRot = baseRot * Quaternion.Euler(0f,  edgeAngle, 0f);  // +70°

    // Pre-compute constant angular speed so centre→edge takes edgeTime sec
    float degPerSec = edgeAngle / edgeTime;

    // Build the sequence ------------------------------------------------------
    Sequence seq = DOTween.Sequence();

    // Track the previous angle so we can time each segment for constant speed
    float prevAngle = 0f;                 // centre = 0°
    for (int i = 0; i < rotateTimes; i++)
    {
        bool goLeft = (i % 2 == 0);       // 0=L,1=R,2=L,…
        float nextAngle = goLeft ? -edgeAngle : edgeAngle; 

        // How far do we turn this hop?
        float delta = Mathf.Abs(nextAngle - prevAngle);      // degrees
        float duration = delta / degPerSec;                  // seconds

        // Choose target quaternion
        Quaternion targetRot = goLeft ? leftRot : rightRot;

        seq.Append(transform
            .DORotateQuaternion(targetRot, duration)
            .SetEase(Ease.InOutSine));

        prevAngle = nextAngle;            // update tracker
    }

    // Return to the exact original facing (centre) ---------------------------
    float backDelta   = Mathf.Abs(prevAngle);          // edge → 0
    float backTime    = backDelta / degPerSec;
    seq.Append(transform
        .DORotateQuaternion(baseRot, backTime)
        .SetEase(Ease.InOutSine));

    // Cleanup
    lookAroundTween = seq
        .OnKill(() => transform.rotation = baseRot);   // insurance reset
}

    //transform.DORotateQuaternion(baseRot, 0.14f).SetEase(Ease.OutQuad);


    [Header("Curve Projectile Attack Settings")]
    public GameObject curveProjectileObj;
    /*curved Projectile*/
    [SerializeField] AnimationCurve timeByDistance   = AnimationCurve.Linear(0, 0.4f, 20, 1.4f);
    [SerializeField] AnimationCurve heightByDistance = AnimationCurve.Linear(0, 3f, 20, 8f);
    [SerializeField] float minFlightTime  = 0.25f;     
    [SerializeField] float maxFlightTime  = 2.0f;
    [SerializeField] float maxArcHeight   = 10f;
    public void ShotCurveProjectile(Vector3 targetPoint, float size = 1f, bool isPoisonPoolObj = true)
    {
        float dist = Vector3.Distance(transform.position, targetPoint);
        float t    = Mathf.Clamp(timeByDistance.Evaluate(dist), minFlightTime, maxFlightTime);
        float h    = Mathf.Min(heightByDistance.Evaluate(dist), maxArcHeight);
    
        GameObject proj = Instantiate(curveProjectileObj, transform.position, Quaternion.identity); //curveProjectileObj

        SoundEffect.Instance.Play(SoundList.TurnipaBossCurveProjectileSe);

        proj.transform.DOJump(targetPoint, h, 1, t,snapping: false).SetEase(Ease.Linear) //where it lands, how high above its start it goes, number of “jumps” (use 1 for a single arc), total flight time
        .OnComplete(() => 
        {
            Destroy(proj);
            GameObject posionPool = null;
            if(isPoisonPoolObj)posionPool = Instantiate(AoePosionPoolObj, targetPoint, Quaternion.identity);
            else  posionPool = Instantiate(AoePoisonExplodeObj, targetPoint, Quaternion.identity);
        });

    }


}

