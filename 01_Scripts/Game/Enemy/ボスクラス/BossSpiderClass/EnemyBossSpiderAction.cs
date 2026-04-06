using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine

using HighlightPlus;
using System.Collections;
using UnityEngine.Rendering;
//using System;

public class EnemyBossSpiderAction : MonoBehaviour
{

    public enum BossState
    {
        Spawn,
        Idle,
        Move,
        WebBallAttack,   // 糸弾攻撃
        DashAttack,      // ダッシュ攻撃
        JumpAttack,      // ジャンプ攻撃
        SpinAttack,  
        MagicBallAttack, // 魔法弾攻撃
        ShotFireAttack, // 火炎放射攻撃
        ShotStickyWebAttack, // 粘り強い糸攻撃
        HanglingAttack, // ぶら下がり攻撃
        Dizzy,
        Attack,
        Dead
    }

    /*AoeCircle*/
    public GameObject AoeAtkMagicCircle;　　　//魔法aoe 
    public GameObject AoeJumpAttackCircle;   //ジャンプ攻撃aoe
    public GameObject AoeAtkWebCircle;       //糸ネットaoe
    public GameObject AoeAtkDashRectangle;   //ダッシュaoe 
    public GameObject AoeMagicSector;      　//魔法放射aoe

    public GameObject SpiderDenObject;    // 巣穴オブジェクト
    public GameObject MagicProjectileObj; // 魔法弾
    public GameObject WebProjectileObj;   // 糸弾
    public GameObject WebGroundObject;   // 地面の糸

    public Vector3 spiderShotThreadOriginPos; // 糸の発射元位置
    public GameObject SpiderShotThread; //糸
    public GameObject HanglingThread; //ぶら下がり糸

    public GameObject BossBattleField; // ボスバトルフィールド
    public GameObject BossHpBar; // ボスのHPバー

    /*Particle & Effect*/
    public ParticleSystem spiderFireEffect; //ボスの火炎放射エフェクト
    public ParticleSystem GroundHitEffectObj; // 地面に当たったときのエフェクト
    public ParticleSystem dashParticle; // ダッシュのパーティクルエフェクト
    public GameObject dizzyEffectObj; // めまいエフェクトオブジェクト
    HighlightEffect highlightEffect;  //ボスのシェーダーとアウトラインエフェクト 

    public float stateCnt;                    // 各状態の継続時間などをカウントするタイマー
    public StateMachine<BossState> stateMachine;　　// 状態遷移を管理するステートマシン
    public string stateInfoText; // 状態番号を文字列に変換するためのテキスト
    public int stateInfo;
    public int preStateInfo; // 前の状態番号（enum を int に変換したもの）
    public int nextStateInfo; // 次の状態番号（enum を int に変換したもの）

    private Animator animator;

    Transform playerTrans; // プレイヤーのTransform
    public float distToPlayer; // プレイヤーとの距離
    public Vector3 playerDir;

    public int randAttackPattern; //ランダムで選択される攻撃パターン
    public int preAttackPattern; // 前の攻撃パターン

    public float distNeedToTakeAction = 21f; // 攻撃可能な距離
    public bool hasAttacked = false; // 既に攻撃したかどうかのフラグ

    public float distNeedToDashAtk = 14f;
    public float distNeedToWebShotAtk = 10f;
    //public float distNeedTo

    public float moveSpeed = 3.5f; // 移動速度
    public float moveSpeedNormal = 3.5f;

    private float bossFieldPosXMax = 14.5f; // ボスフィールドのX軸の最大値
    private float bossFieldPosXMin = -14f; // ボスフィールドのX軸の最小値
    private float bossFieldPosZMax = 14f; // ボスフィールドのZ軸の最大値
    private float bossFieldPosZMin = -14.5f; // ボスフィールドのZ軸の最小値

    public float facePlayerCnt; // プレイヤーの方向を向くためのカウントダウン

    public Vector3 punchRotationAmount = new Vector3(45f, 0f, 0f); //30 about X
    public float punchDuration        = 0.4f;
    public int   punchVibrato         = 4;   
    public float punchElasticity      = 0.2f;

    /*ダッシュ*/
    Vector3 dashTargetPoint;

    /*climing*/
    public Transform threadMuzzle;

    /*curved Projectile*/
    [SerializeField] AnimationCurve timeByDistance   = AnimationCurve.Linear(0, 0.4f, 20, 1.4f);
    [SerializeField] AnimationCurve heightByDistance = AnimationCurve.Linear(0, 3f, 20, 8f);
    [SerializeField] float minFlightTime  = 0.25f;     
    [SerializeField] float maxFlightTime  = 2.0f;
    [SerializeField] float maxArcHeight   = 10f;

    public float projectileDirectionTracingTimer;
    public float projectileDirectionTracingTimerMax = 2f; // 糸の発射方向をプレイヤーを追跡する時間

    [Header("WebBall Attack　設定")]
    [SerializeField] private int webProjectileCount = 3;
    [SerializeField] private float webAttackSectorAngle = 45f;
    [SerializeField] private float indicatorFillTime = 1f;
    [SerializeField] private float indicatorLingerTime = 0.2f;
    [SerializeField] private int webAttackDamage = 0;

    private List<AoeRectIndicator> _activeIndicators = new List<AoeRectIndicator>();

    public float dashDirectionTracingTimer = 0f; // ダッシュの方向を追跡するタイマー
    public float dashDirectionTracingTimerMax = 2f; // ダッシュの方向を追跡する時間

    public float CirclePositionTracingTimer = 0f; // 円の位置を追跡するタイマー
    public float CirclePositionTracingTimerMax = 2.5f; // 円の位置を追跡する時間
    public bool circleFinishedFilling = false; // 円の位置追跡が完了したかどうかのフラグ
    public AoeCircleDynamic activeJumpIndicator;
    private bool isJumpAttackFilling = false; // ジャンプ攻撃の円が満たされているかどうかのフラグ

    public ShakeProfile bigHitShake; 
    public ShakeProfile footstepShake;

    public Transform hanglingTrans;
    public float hanglingAttackCnt = 5f;
    public float hanglingAttackCntMax = 5f;
    public int hanglingAttackTime = 3;
    public int hanglingAttackTimeMax = 3;

    private ObjectShaker objectShaker;

    public bool isNextStatePlanned = false; // 次の状態が計画されているかどうかのフラグ
    public float nextStateCnt; // 次の状態のカウントダウン時間

    public  Renderer enemyRenderer;

    public bool isPhrase2 = false; // フェーズ2に入ったかどうかのフラグ

    EnemyStatusBase eStatus; // ボスのステータスを管理するクラス

    public bool allDenDestroyed = false; // すべての巣穴が破壊されたかどうかのフラグ

    public bool isDeadStated = false;

    public bool hasMagicCrystalAtk = false;

    public void ChangeToMoveState() => stateMachine.ChangeState(BossState.Move);   
    public void ChangeToJumpAttack() => stateMachine.ChangeState(BossState.JumpAttack);
    public void ChangeToWebBallAttack() => stateMachine.ChangeState(BossState.WebBallAttack);
    public void ChangeToDashAttack() => stateMachine.ChangeState(BossState.DashAttack);
    public void ChangeToSpinAttack() => stateMachine.ChangeState(BossState.SpinAttack);
    public void ChangeToDizzy() => stateMachine.ChangeState(BossState.Dizzy);
    public void ChangeToIdle() => stateMachine.ChangeState(BossState.Idle);
    public void ChangeToState(BossState nextState)
    {
        preStateInfo = (int)stateMachine.State; // 現在の状態を保存
        stateMachine.ChangeState(nextState); // 次の状態に変更
        nextStateInfo = (int)stateMachine.State; // 次の状態を保存
        //Debug.Log("ChangeToState: " + nextState + ", PreState: " + preStateInfo + ", NextState: " + nextStateInfo);
    }

    public void ChangeToDead() => stateMachine.ChangeState(BossState.Dead); // ボスの死亡状態に遷移

    public void PlanStateChange(BossState nextStateInfo, float _stateCnt)
    {
        preStateInfo = (int)stateMachine.State; // 現在の状態を保存
        this.nextStateInfo = (int)nextStateInfo; // 次の状態を保存
        stateCnt = _stateCnt; // 次の状態に遷移するまでのカウントダウン時間を設定
        ChangeToState(BossState.Idle);  // 一時的にIdle状態に遷移して、カウントダウンが終了したら次の状態に遷移する
    }

    public void ChangeToNextStateInfo()
    {
        preStateInfo = (int)stateMachine.State; // 現在の状態を保存
        stateMachine.ChangeState((BossState)nextStateInfo); // 次の状態に変更
        nextStateInfo = (int)stateMachine.State; // 次の状態を保存
    }

    public void SetNextPlannedState(BossState state, float _nextStateCnt, float distToChangeState = 21f)
    {
        isNextStatePlanned = true;
        nextStateInfo = (int)state; // 次の状態を保存
        nextStateCnt = _nextStateCnt; // 次の状態に遷移するまでのカウントダウン時間を設定
        distNeedToTakeAction = distToChangeState; // 次の状態に遷移するための距離を設定
    }

    public void TransitionToNextState()
    {
        if (!isNextStatePlanned) return; // 次の状態が計画されていない場合は何もしない
        isNextStatePlanned = false; // 次の状態の計画をリセット
        ChangeToState((BossState)nextStateInfo); // 次の状態に変更
        return;
    }

    public void InitNextStateInfo()
    {
        if (!isNextStatePlanned) return; 
        stateCnt = nextStateCnt;
    }

    public void OnEnable()
    {
        EventManager.StartListening("AllSpiderDenDestroyed", OnAllSpiderDenDestroy);
    }

    public void OnDisable()
    {
        EventManager.StopListening("AllSpiderDenDestroyed", OnAllSpiderDenDestroy);
    }

    public void OnAllSpiderDenDestroy()
    {
        transform.DOMoveY(0.35f, 1f).SetEase(Ease.OutQuad).OnComplete(() => {       
            ChangeToDizzy();
        });
        allDenDestroyed = true;
        
    }

    public void Awake()
    {
        
        var player = GameObject.FindWithTag("Player");
        if (player) playerTrans = player.transform;

        animator = GetComponent<Animator>();
        highlightEffect = GetComponent<HighlightEffect>();

        stateMachine = StateMachine<BossState>.Initialize(this);
        stateMachine.ChangeState(BossState.Move);

        eStatus = GetComponent<EnemyStatusBase>();

        StopFireMagic();

        randAttackPattern = 0;

    }

    public void Start()
    {
        objectShaker = GetComponent<ObjectShaker>();

        Invoke(nameof(ActivateBossHpBar), 1.4f);

    }

    public void ActivateBossHpBar()
    {
        BossHpBar.SetActive(true); // ボスのHPバーをアクティブにする
    }

    public void SetInnerGlow(float targetVal, float duration, int loopTimes)
    {
        highlightEffect.innerGlow = 0.1f; // 初期値を設定
        DOTween.To(() => highlightEffect.innerGlow, x => highlightEffect.innerGlow = x, targetVal, duration)
            .SetLoops(loopTimes, LoopType.Yoyo) // 指定回数往復
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                highlightEffect.innerGlow = 0.1f;
            });

        //DoTween to fluctuate float vlue highlightProfile.innerGlowPower from 0 to 1.5 in 2 seconds, then back to 0 in 2 seconds, repeat forever     
        //highlightEffect.innerGlow = 0f; // 初期値を設定
        //DOTween.To(() => highlightEffect.innerGlow, x => highlightEffect.innerGlow = x, 0.56f, 1f)
        //    .SetLoops(-1, LoopType.Yoyo) // 無限ループで往復
        //    .SetEase(Ease.InOutSine); // イージングを設定

    }


    public void Update()
    {
        stateInfo = (int)stateMachine.State;
        stateInfoText = stateMachine.State.ToString() + " (" + stateInfo + ")"; // 状態番号を文字列に変換
        stateCnt -= Time.deltaTime;
        GetPlayerInfo();

        //if(eStatus.enemyHp < eStatus.enemyMaxHp/2 && !isPhrase2)
        //{
        //    isPhrase2 = true; // フェーズ2に入ったフラグを立てる
        //    EventManager.EmitEvent("BossPhase2Start"); // フェーズ2開始イベントを発行
        //    ChangeToState(BossState.HanglingAttack);
        //    //SetNextPlannedState(BossState.HanglingAttack, 15f, 21f); // フェーズ2開始時にぶら下がり攻撃を計画
        //}

        //if(Input.GetKeyDown(KeyCode.F)) // Fキーを押したら
        //{
        //    //SetInnerGlow(0.77f, 0.28f, 6); 
        //    ShakeCameraLight();
        //}


        Debug();
    }


    //===================State Machine=================================//
    public void Move_Enter()
    {
        if (isDeadStated) return;

        stateCnt = 3.5f;
        distNeedToTakeAction = 21f;
        moveSpeed = moveSpeedNormal; // 移動速度を通常に戻す
        animator.SetTrigger("isWalking");
        InitNextStateInfo();
        
    }

    public void InvokePhrase2()
    {
        EventManager.EmitEvent("BossPhase2Start"); // フェーズ2開始イベントを発行
    }

    public void Move_Update()
    {
        if (isDeadStated) return;

        HomingPlayer();
        RotateTowardPlayer();
        FixY(0.35f);
        //FixRotXZ();

        if (distToPlayer < distNeedToTakeAction && stateCnt <= 0)
        {
            TransitionToNextState();

            //randAttackPattern = Random.Range(0, 7); // 0から3のランダムな整数を生成
            //if (randAttackPattern == preAttackPattern) // 前の攻撃パターンと同じ場合は再度ランダムに選択
            //{
            //    randAttackPattern = Random.Range(0, 7);
            //}
            
            preAttackPattern = randAttackPattern; // 前の攻撃パターンを保存

            if (eStatus.enemyHp < eStatus.enemyMaxHp / 2 && !isPhrase2)
            {
                isPhrase2 = true; // フェーズ2に入ったフラグを立てる
                Invoke(nameof(InvokePhrase2), 1.4f);
                ChangeToState(BossState.HanglingAttack);
                //SetNextPlannedState(BossState.HanglingAttack, 15f, 21f); // フェーズ2開始時にぶら下がり攻撃を計画
            }
            else if (isPhrase2 && !allDenDestroyed) ChangeToState(BossState.HanglingAttack);
            else if (randAttackPattern == 0) ChangeToWebBallAttack();
            else if (randAttackPattern == 1) ChangeToDashAttack();
            else if (randAttackPattern == 2) ChangeToState(BossState.ShotStickyWebAttack);
            else if (randAttackPattern == 3) ChangeToState(BossState.ShotFireAttack);
            else if (randAttackPattern == 4) ChangeToState(BossState.ShotFireAttack);
            else if (randAttackPattern == 5) ChangeToDashAttack();
            else
            {
                randAttackPattern = Random.Range(0, 6);
                 if (randAttackPattern == 0) ChangeToWebBallAttack();
            else if (randAttackPattern == 1) ChangeToDashAttack();
            else if (randAttackPattern == 2) ChangeToState(BossState.ShotStickyWebAttack);
            else if (randAttackPattern == 3) ChangeToState(BossState.ShotFireAttack);
            else if (randAttackPattern == 4) ChangeToDashAttack();
            else if (randAttackPattern == 5) ChangeToWebBallAttack();
            }

            randAttackPattern++;


        }
    }

    public void MagicBallAttack_Enter()
    {
        stateCnt = 2.5f;
        animator.SetTrigger("isIdle");
        SetInnerGlow(0.77f, 0.28f, 6); // 内側のグローを設定

        hasAttacked = false;



    }
    
    public void MagicBallAttack_Update()
    {

        RotateTowardPlayer();

        if (stateCnt > 0) return;

        AoeSpawnMagicAttck();
        Invoke(nameof(ChangeToMoveState), 2.8f);
        Invoke(nameof(DoShake), 0.5f);

    }


    public void JumpAttack_Enter()
    {
        //stateCnt = 4f;

        CirclePositionTracingTimer = CirclePositionTracingTimerMax; // 円の位置を追跡するタイマーをリセット
        isJumpAttackFilling = false;
        hasAttacked = false;

        animator.SetTrigger("isIdleWait");

        transform.DOMoveY(11f, 0.5f).SetEase(Ease.OutQuad);
        //Vector3 newPos = transform.position;
        //float jumpPointHeight = 11f; // ジャンプの高さ
        //newPos.y = jumpPointHeight; // Y座標をジャンプの高さに設定
        //transform.position = newPos; // ジャンプの高さに移動

        Vector3 circleAoePos = transform.position;
        circleAoePos.y = 0.1f; // Y座標を少し上に設定

        GameObject aoeObj = Instantiate(AoeJumpAttackCircle);
        activeJumpIndicator = aoeObj.GetComponent<AoeCircleDynamic>();

        //AoeSpawnJumpAttackCircle(circleAoePos, 7.7f, 2);

        FixRotXZ();


        //DoMove from 

    }

    public void JumpAttack_Update()
    {

        if (CirclePositionTracingTimer > 0)
        {
            CirclePositionTracingTimer -= Time.deltaTime;
            HomingPlayerXZ();

            Vector3 indicatorPos = playerTrans.position;
            indicatorPos.y = 0.1f; 
            float indicatorRadius = 7.7f; 
            activeJumpIndicator.UpdateTransform(indicatorPos, indicatorRadius);

        }

        else if (!isJumpAttackFilling)
        {
            isJumpAttackFilling = true; 

            if (activeJumpIndicator != null)
            {             
                float fillDuration = 1.5f;
                float attackDamage = 35f;
                activeJumpIndicator.BeginFill(fillDuration, attackDamage, 1.0f);
                StartCoroutine(FallAfterFill(fillDuration - 0.35f));
            }
        }
   
    }

    private IEnumerator FallAfterFill(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        if (activeJumpIndicator != null)
        {
            Vector3 jumpAttackTarget = activeJumpIndicator.transform.position;
            jumpAttackTarget.y = 0.3f; 

            transform.DOMove(jumpAttackTarget, 0.4f)
                .SetEase(Ease.InCubic) 
                .OnComplete(() =>
                {
                    PlayGroundHitParticle();
                    ShakeCamera(); 
                    SoundEffect.Instance.Play(SoundList.SpiderBossFallGroundSe);
                    Invoke(nameof(ChangeToMoveState), 1.0f);
                });
        }
        else
        {
            ChangeToState(BossState.HanglingAttack);
        }
    }


    public void DashAttack_Enter()
    {
        stateCnt = 1f;
        hasAttacked = false;

        StartCoroutine(DashAttackCoroutine()); // ダッシュ攻撃のコルーチンを開始

    }

    public void DashAttack_Update()
    {
        
    }

    public void Dizzy_Enter()
    {
        stateCnt = 10.9f;

        if (_activeIndicators != null)
        {
            foreach (var indicator in _activeIndicators)
            {
                if (indicator != null) Destroy(indicator.gameObject);
            }
            _activeIndicators.Clear();
        }
        if (activeJumpIndicator != null)
        {
            Destroy(activeJumpIndicator.gameObject);
        }

        if (HanglingThread != null) HanglingThread.SetActive(false);

        transform.rotation = Quaternion.Euler(0, 0, 180f);
        animator.SetTrigger("isMove");

        dizzyEffectObj.SetActive(true); // めまいエフェクトを有効にする
        CameraShake.Instance.StartShake(bigHitShake); // カメラを揺らす

         DOTween.Kill(transform, complete: true);
        StopAllCoroutines();
        CancelInvoke();

        SoundEffect.Instance.Play(SoundList.SpiderBossPhrase2Se);
        
    }

    public void Dizzy_Update()
    {
        if (stateCnt <= 0) ChangeToMoveState();
        
        FixPosYRotX(1.7f,0f);
    }

    public void Dizzy_Exit()
    {
        dizzyEffectObj.SetActive(false); // めまいエフェクトを無効にする
    }

    public void WebBallAttack_Enter()
    {
        FacePlayerNow();
        StartCoroutine(WebBallAttackCoroutine());

    }

    public void WebBallAttack_Update()
    {
        
          

    }

    public void Dead_Enter()
    {
        animator.SetTrigger("isDead");
        isDeadStated = true;

        SoundEffect.Instance.Play(SoundList.SpiderBossPhrase2Se);
        DOTween.Kill(transform, complete: true);
        StopAllCoroutines();
        CancelInvoke();

        if (_activeIndicators != null)
        {
            foreach (var indicator in _activeIndicators)
            {
                if (indicator != null) Destroy(indicator.gameObject);
            }
            _activeIndicators.Clear();
        }
        if (activeJumpIndicator != null)
        {
            Destroy(activeJumpIndicator.gameObject);
        }

        if (HanglingThread != null) HanglingThread.SetActive(false);
        if (dizzyEffectObj != null) dizzyEffectObj.SetActive(false);

    }

    private IEnumerator DashAttackCoroutine()
    {
        FacePlayerNow();
        SetInnerGlow(0.77f, 0.28f, 6);

        dashDirectionTracingTimer = dashDirectionTracingTimerMax; // ダッシュの方向をプレイヤーに追跡するタイマーを減少
        _activeIndicators.Clear();

        GameObject aoeRectObj = Instantiate(AoeAtkDashRectangle, transform.position, Quaternion.identity);
        _activeIndicators.Add(aoeRectObj.GetComponent<AoeRectIndicator>());

        //indicator Tracing
        while (dashDirectionTracingTimer > 0f)
        {
            RotateTowardPlayer();
            dashDirectionTracingTimer -= Time.deltaTime;

            Vector3 baseDirToPlayer = (playerTrans.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, playerTrans.position);

            Vector3 indicatorPosition = transform.position + baseDirToPlayer * (distanceToPlayer * 0.5f);
            Quaternion indicatorRotation = Quaternion.LookRotation(baseDirToPlayer, Vector3.up) * Quaternion.Euler(0f, -90f, 0f); //indicator rot with -90 rotY offset
            Vector2 indicatorSize = new Vector2(distanceToPlayer, 3f); 

            _activeIndicators[0].UpdateTransform(indicatorPosition, indicatorRotation, indicatorSize);

            yield return null;
        }

        //Indicator Fill & Dash Target Point Calculation
        float dashDist = distToPlayer; // ダッシュの距離
        dashTargetPoint = transform.position + playerDir * dashDist;
        _activeIndicators[0].BeginFill(indicatorFillTime, indicatorLingerTime, webAttackDamage, true);
        FacePlayerNow();

        yield return new WaitForSeconds(indicatorFillTime + indicatorLingerTime); // Wait for the animation to finish, indicatorLingerTime = 0.2f

        DoAttackAniTween();
        PlayDashParticle();
        SoundEffect.Instance.Play(SoundList.SpiderBossDashSe);
        hasAttacked = true;
        //animator.SetTrigger("isCloseAttack");
       
        transform.DOMove(dashTargetPoint, 0.5f).SetEase(Ease.Linear).OnComplete(() => {
            DoAttackAniTween();
            Invoke(nameof(TurnAround360Degree), 1f);
        });
 
    }


    private IEnumerator WebBallAttackCoroutine()
    {
        FacePlayerNow();
        animator.SetTrigger("isIdleWait");
        SetInnerGlow(0.77f, 0.28f, 6);

        projectileDirectionTracingTimer = projectileDirectionTracingTimerMax; // 糸の発射方向をプレイヤーに追跡するタイマーを減少
        _activeIndicators.Clear();

        for (int i = 0; i < webProjectileCount; i++)
        {
            GameObject aoeRectObj = Instantiate(AoeAtkDashRectangle, transform.position, Quaternion.identity);
            _activeIndicators.Add(aoeRectObj.GetComponent<AoeRectIndicator>());
        }

        //2.indicator Tracing
        while (projectileDirectionTracingTimer > 0f)
        {
            RotateTowardPlayer();
            projectileDirectionTracingTimer -= Time.deltaTime;

            Vector3 baseDirToPlayer = (playerTrans.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, playerTrans.position);

            float angleStep = (webProjectileCount > 1) ? webAttackSectorAngle / (webProjectileCount - 1) : 0f;
            float startAngle = -webAttackSectorAngle * 0.5f;

            for (int i = 0; i < _activeIndicators.Count; i++)
            {
                float currentAngle = startAngle + angleStep * i;
                Quaternion rotationOffset = Quaternion.Euler(0, currentAngle, 0);
                Vector3 indicatorDirection = rotationOffset * baseDirToPlayer;
                Vector3 indicatorPosition = transform.position + indicatorDirection * (distanceToPlayer * 0.5f);         
                Quaternion indicatorRotation = Quaternion.LookRotation(indicatorDirection, Vector3.up)* Quaternion.Euler(0f, -90f, 0f); //indicator rot with -90 rotY offset
                Vector2 indicatorSize = new Vector2(distanceToPlayer, 3f); //indicator size , y is fixed 3f
                _activeIndicators[i].UpdateTransform(indicatorPosition, indicatorRotation, indicatorSize);
            }

        yield return null;
        }

        //3.Indicator Fill
        foreach (var indicator in _activeIndicators)
        {        
            indicator.BeginFill(indicatorFillTime, indicatorLingerTime, webAttackDamage, true);
        }
        FacePlayerNow();

        //4.Fire Projectile
        yield return new WaitForSeconds(indicatorFillTime + indicatorLingerTime); // Wait for the animation to finish, indicatorLingerTime = 0.2f      
        DoAttackAniTween();          
        ShotWebProjectile3();
        Invoke(nameof(ShotWebProjectile5), 1.5f); // 0.5秒後に5本の糸弾を発射

        yield return new WaitForSeconds(2.0f);
        ChangeToMoveState();
    }

    //ShotFireAttack
    public void ShotFireAttack_Enter()
    {
        stateCnt = 4f;

        FacePlayerNow();
        animator.SetTrigger("isIdleWait");
        SetInnerGlow(1.77f, 0.28f, 6);

        AoeSpawnMagicSector();
        Invoke(nameof(RotateToShotFire),1.5f);
        Invoke(nameof(ShotFireMagicAttack),1.5f);
        Invoke(nameof(StopFireMagic), 3.3f);      
        Invoke(nameof(ChangeToMoveState), 3.5f); // 2秒後に移動状態に戻る
    }


    public void ShotStickyWebAttack_Enter()
    {
        Invoke(nameof(ShotCurveProjectilePlayer), 0.5f);
        Invoke(nameof(ShotCurveProjectilePlayer), 2.1f);
        Invoke(nameof(ShotCurveProjectilePlayer), 3.5f);
        Invoke(nameof(ChangeToMoveState), 3f); // 3秒後に移動状態に戻る

    }

    public void ShotFireAttack_Update()
    {
        
    }

    public void HanglingAttack_Enter()
    {
        stateCnt = 15f;
        animator.speed = 0; // アニメーションの再生を停止

        hanglingAttackTime = hanglingAttackTimeMax; // ぶら下がり攻撃の時間をリセット
        hanglingAttackCnt = hanglingAttackCntMax; // ぶら下がり攻撃のカウントダウンをリセット

        //change Tag to "EnemyBossSpiderHangling"
        gameObject.tag = "UnSelectableEnemy"; // タグを変更して、ぶら下がり状態を示す

        TurnFaceUp();
        hasMagicCrystalAtk = true;
    }

    public void HanglingAttack_Update()
    {
        hanglingAttackCnt -= Time.deltaTime; // ぶら下がり攻撃のカウントダウン
        if (hanglingAttackCnt <= 0)
        {
            hanglingAttackTime--; // ぶら下がり攻撃の時間を減少
            hanglingAttackCnt = 7; // カウントをリセット
            stateCnt = 5f;
            hasAttacked = false;
            SetInnerGlow(0.77f, 0.28f, 6);
            Invoke(nameof(DoShake), 0.5f);

            if(hasMagicCrystalAtk) 
            {
                Invoke(nameof(ShotCurveProjectilePlayer), 0.5f);
                Invoke(nameof(ShotCurveProjectilePlayer), 2f);
                Invoke(nameof(ShotCurveProjectilePlayer), 3.5f);
                hasMagicCrystalAtk = false; // 魔法クリスタル攻撃は1回のみ実行
            }
            else
            {
                AoeSpawnMagicAttck();
                hasMagicCrystalAtk = true; 
            }

        }

        if (stateCnt <= 0 && hanglingAttackTime <= 0)
        {          
            ChangeToJumpAttack();
        }
    }

    public void HanglingDownward()
    {
        RestoreShadows();
        transform.position = hanglingTrans.position; // ぶら下がり位置に移動
        transform.rotation = hanglingTrans.rotation; // ぶら下がり位置の回転に合わせる
        HanglingThread.SetActive(true); // ぶら下がり糸を表示する

        transform.DOMoveY(7f, 2f).SetEase(Ease.OutQuad).OnComplete(() => {
           

        });
    }

    public void HanglingAttack_Exit()
    {
        gameObject.tag = "Enemy"; // タグを元に戻す
        animator.speed = 1; // ぶら下がり状態からの脱出アニメーションを再生
        HanglingThread.SetActive(false); // ぶら下がり糸を非表示にする

    }

    //==============Utility============================================//
    void DoAttackAniTween()
    {
       transform.DOPunchRotation(punchRotationAmount,punchDuration,vibrato: punchVibrato,elasticity: punchElasticity).SetEase(Ease.OutQuad);
    }

    public void ShakeCamera()
    {
        //Camera.main.transform.DOShakePosition(1f, new Vector3(0.5f, 0.5f, 0.5f), 10, 90, false, true).SetEase(Ease.Linear);
        CameraShake.Instance.StartShake(bigHitShake);
    }

    public void ShakeCameraLight()
    {
        CameraShake.Instance.StartShake(footstepShake);
    }

    public void GetPlayerInfo()
    {
        if (playerTrans == null) return;
        playerDir = (playerTrans.position - transform.position).normalized; // プレイヤーの方向を計算
        distToPlayer = Vector3.Distance(transform.position, playerTrans.position); // プレイヤーとの距離を計算

    }

    public void FixPosYRotX(float posY, float rotX)
    {
        Vector3 pos = transform.position;
        pos.y = posY;
        transform.position = pos;

        Vector3 rotation = transform.rotation.eulerAngles;
        rotation.x = rotX;
        transform.rotation = Quaternion.Euler(rotation);

    }

    public void FixRotXZ()
    {
        Vector3 rotation = transform.rotation.eulerAngles;
        rotation.x = -180f; // X軸の回転をゼロにする
        rotation.z = 180f; // Z軸の回転をゼロにする
        transform.rotation = Quaternion.Euler(rotation);
    }

    public void FixY(float posY)
    {
        Vector3 pos = transform.position;
        pos.y = posY; // Y座標を指定の値に設定
        transform.position = pos; // 位置を更新
    }

    public void DisableShadows()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }
    }
    public void RestoreShadows()
    {
        if (enemyRenderer != null)
        {
             enemyRenderer.shadowCastingMode = ShadowCastingMode.On;
        }
    }

    public void HomingPlayer()
    {
        if (playerTrans == null) return;
        Vector3 direction = (playerTrans.position - transform.position).normalized;
        transform.position += direction * Time.deltaTime * moveSpeed; // 移動速度は調整可能
        //transform.LookAt(playerTrans); // プレイヤーの方向を向く
        


    }

    public void HomingPlayerXZ()
    {
        if (playerTrans == null) return;
        Vector3 direction = (playerTrans.position - transform.position).normalized;
        direction.y = 0; // Y軸の成分をゼロにしてXZ平面での移動にする
        transform.position += direction * Time.deltaTime * moveSpeed; // 移動速度は調整可能
        //transform.LookAt(playerTrans); // プレイヤーの方向を向く

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

    public void DoShake()
    {
        objectShaker.DoShake();
    }

    //==============Facing&Moving============================================//
    public void ClimbUpField()
    {
        float jumpHeight = 11f; // ジャンプの高さ
        float jumpDuration = 1.5f; // ジャンプの持続時間

        transform.DOMoveY(jumpHeight, 1.3f).OnComplete(() => {
            transform.DORotate(new Vector3(0, transform.rotation.eulerAngles.y, 0), 1f, RotateMode.Fast)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    SpiderShotThread.SetActive(false); // スパイダーショットの糸を非表示する
                    //SpiderShotThread.transform.position = spiderShotThreadOriginPos;
                    HanglingDownward();
                });

            //transform.DOMoveY(0.5f, 1); // ジャンプが完了したら、元の位置に戻る
        });
    }
    public void TurnFaceUp()
    {
        SoundEffect.Instance.Play(SoundList.SpiderBossPhrase2Se);
        DisableShadows();
        transform.DORotate(new Vector3(-90f, transform.rotation.eulerAngles.y, 0), 0.4f, RotateMode.Fast).SetEase(Ease.Linear)
            .OnComplete(() => {
                ShotThread();
                Invoke(nameof(ClimbUpField),0.4f);
            });

    }
    public void TurnAround360Degree()
    {
        transform.DORotate(new Vector3(0, transform.rotation.eulerAngles.y + 360f, 0), 1f,RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                // 回転が完了したら、元の状態に戻る
                //FixPosYRotX();
                SpawnMagicProjectile(10);
                Invoke(nameof(SpawnMagicProjectile20), 1f);
                Invoke(nameof(ChangeToMoveState),2f);
                
            });

    }


    public void RotateLeftAndRight(float degreesFromCenter, int loopCount, float timeCenterToExtreme)
    {
        
        float initialEulerX = transform.localEulerAngles.x;
        float initialEulerY = transform.localEulerAngles.y;
        float initialEulerZ = transform.localEulerAngles.z;

        Vector3 leftExtremeEuler = new Vector3(initialEulerX, initialEulerY + degreesFromCenter, initialEulerZ);
        Vector3 rightExtremeEuler = new Vector3(initialEulerX, initialEulerY - degreesFromCenter, initialEulerZ);

        float timeExtremeToExtreme = timeCenterToExtreme * 2.0f;

        Sequence s = DOTween.Sequence(); 
        s.Append(transform.DOLocalRotate(leftExtremeEuler, timeCenterToExtreme, RotateMode.Fast).SetEase(Ease.Linear));      
        Sequence oscillation = DOTween.Sequence(); //(Left -> Right) and then (Right -> Left) 
        oscillation.Append(transform.DOLocalRotate(rightExtremeEuler, timeExtremeToExtreme, RotateMode.Fast).SetEase(Ease.Linear)); // L -> R       
        oscillation.SetLoops(loopCount, LoopType.Yoyo);
        s.Append(oscillation);

        s.OnComplete(() => {
            
        });
    }

    public void RotateToShotFire()
    {
        RotateLeftAndRight(35, 2,0.35f);
    }

    //==============Particle Playing============================================//
    public void ShotFireMagicAttack()
    {
        spiderFireEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        spiderFireEffect.Play();
        BoxCollider boxCollider = spiderFireEffect.GetComponent<BoxCollider>();
        boxCollider.enabled = true; // 有効化して当たり判定を有効にする

        SoundEffect.Instance.Play(SoundList.SpiderBossFireAtKSe);

    }

    public void StopFireMagic()
    {
        spiderFireEffect.Stop();
        BoxCollider boxCollider = spiderFireEffect.GetComponent<BoxCollider>();
        boxCollider.enabled = false; // 無効化して当たり判定を無効にする
    }

    public void PlayDashParticle()
    {
        dashParticle.Play();
    }

    public void PlayGroundHitParticle()
    {
        GroundHitEffectObj.gameObject.SetActive(true); // Ensure the particle system is active
        GroundHitEffectObj.Play();  
    }

    //==============Aoe Spawning============================================//
    public void AoeSpawnMagicSector()
    {
        Vector3 spawnPos = transform.position + new Vector3(0,0.015f,0) ; // Y座標を0.5に固定
        GameObject aoeSector = Instantiate(AoeMagicSector, spawnPos, transform.rotation);

    }

    public void AoeSpawnJumpAttackCircle(Vector3 spawnPos, float circleScale, float circleDuration)
    {
        GameObject aoeObj = Instantiate(AoeJumpAttackCircle, spawnPos, Quaternion.identity);
         aoeObj.SetActive(true);

         AoeCircle aoeCircle = aoeObj.GetComponent<AoeCircle>();
         aoeCircle.InitCircle(circleScale, circleDuration, 10,circleScale / 10);  

    }

    public void AoeSpawnMagicAttck()
    {
        if(hasAttacked) return;
        hasAttacked = true;

        animator.SetTrigger("isShotAttack");

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
            GameObject aoeBall = Instantiate(AoeAtkMagicCircle, aoeSpawnPos, Quaternion.identity);
            
            aoeBall.SetActive(true);
            AoeCircle aoeCircle = aoeBall.GetComponent<AoeCircle>();
            aoeCircle.InitCircle(circleScale, circleDuration, 10,finalScale);           

        }
    }

    //==============Projectile Spawning============================================//
    public void ShotThread()
    {
        //if (threadMuzzle == null) return;
        SpiderShotThread.transform.position = transform.position + spiderShotThreadOriginPos;
        SpiderShotThread.SetActive(true);
        //Debug.Log("ShotThread: " + threadMuzzle.position + ", " + threadMuzzle.rotation);

        //GameObject thread = Instantiate(bossSpiderThread, threadMuzzle.position, threadMuzzle.rotation);
        //EffectMoveController mov = thread.GetComponent<EffectMoveController>();
        //if (mov != null)
        //{
        //    Vector3 dir;
        //    //move upward to the vector
        //    dir = Vector3.up;
        //
        //    mov.moveVec = dir * 10f; // 糸の移動速度を設定
        //}

    }

    public void ShotWebProjectile(int projectileCount, float sectorAngle)
    {       
        //int projectileCount = 3;
        //float sectorAngle = 45f;

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

           GameObject web = Instantiate(WebProjectileObj,origin,look * offset);
           var mov = web.GetComponent<EffectMoveController>();
           if (mov != null) mov.moveVec = dir;
                                   
        }

    } 

    public void ShotWebProjectile3()
    {
        ShotWebProjectile(3, 45f);
        SoundEffect.Instance.Play(SoundList.SpiderBossWebShotAtkSe);
    }

    public void ShotWebProjectile5()
    {
        ShotWebProjectile(5, 70f);
        SoundEffect.Instance.Play(SoundList.SpiderBossWebShotAtkSe);
    }

    public void SpawnMagicProjectile(int spawnNum)
    {
        //int spawnNum = 20;
        Vector3 spawnPos = transform.position;
        spawnPos.y = 0.5f; // Y座標を0.5に固定
        for (int i = 0; i < spawnNum; i++)
        {
            GameObject magicProjectile = Instantiate(MagicProjectileObj, spawnPos, Quaternion.identity);
            magicProjectile.SetActive(true);
            EffectMoveController projectile = magicProjectile.GetComponent<EffectMoveController>();

            //each projectile to move toward a circle with this.transform.position as center, with a radius of 10f, and each projectile will be evenly distributed around the circle , they all spawned from the center of circle
            float angle = i * (360f / spawnNum) * Mathf.Deg2Rad; // ラジアンに変換
            Vector3 targetPos = new Vector3(
                this.transform.position.x + Mathf.Cos(angle) * 10f,
                this.transform.position.y,
                this.transform.position.z + Mathf.Sin(angle) * 10f
            );
            projectile.moveVec = (targetPos - this.transform.position).normalized * 5f; // プレイヤーの位置をターゲットに設定
        }

        SoundEffect.Instance.Play(SoundList.SpiderBossThunderBallSe);
        //Debug.Log("Spawned " + spawnNum + " magic projectiles.");

    }

    public void SpawnMagicProjectile20()
    {
        SpawnMagicProjectile(20);
    }

    public void ShotACurveProjectileToTargetPoint(Vector3 targetPoint, float baseArriveTime = 1f)
    {

        GameObject web = Instantiate(WebProjectileObj,transform.position, Quaternion.identity);
        var mov = web.GetComponent<EffectMoveController>();

    }

    public void ShotCurveProjectile(Vector3 targetPoint,float flightTime  = 4f,float arcHeight   = 4f)
    {
        GameObject proj = Instantiate(WebProjectileObj, transform.position, Quaternion.identity);

        Vector3 start = transform.position;
        Vector3 end   = targetPoint;
        Vector3 apex  = (start + end) * 0.5f;   
        apex.y += arcHeight;  

        Vector3[] path = { start, apex, end };
        proj.transform.DOPath(path,flightTime,PathType.CatmullRom, PathMode.Full3D)       .SetEase(Ease.Linear).OnComplete(() => Destroy(proj));

    }

    public void ShotCurveProjectile(Vector3 targetPoint)
    {
        float dist = Vector3.Distance(transform.position, targetPoint);
        float t    = Mathf.Clamp(timeByDistance.Evaluate(dist), minFlightTime, maxFlightTime);
        float h    = Mathf.Min(heightByDistance.Evaluate(dist), maxArcHeight);
    
        GameObject proj = Instantiate(AoeAtkWebCircle, transform.position, Quaternion.identity);

        proj.transform.DOJump(targetPoint, h, 1, t,snapping: false).SetEase(Ease.Linear) //where it lands, how high above its start it goes, number of “jumps” (use 1 for a single arc), total flight time
        .OnComplete(() => 
        {
            Destroy(proj);
            SpawnWebGround(targetPoint);
        });

    }
    public void ShotCurveProjectilePlayer()
    {
        ShotCurveProjectile(playerTrans.position);
        SoundEffect.Instance.Play(SoundList.SpiderBossStickyWebSe);
    }

    public void SpawnWebGround(Vector3 spawnPos)
    {
        Instantiate(WebGroundObject, spawnPos, Quaternion.identity);     
    }

    public void Debug()
    {
        DebugMenu.Instance.ShowFloat("BossHp", eStatus.enemyHp);
    }
}

