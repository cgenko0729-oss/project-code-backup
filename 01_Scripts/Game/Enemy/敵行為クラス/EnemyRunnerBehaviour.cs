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

public class EnemyRunnerBehaviour : MonoBehaviour
{
    public enum RunnerState
    {
        Wander,
        Escape,
    }

     public float stateCnt;                    // 各状態の継続時間などをカウントするタイマー
    public StateMachine<RunnerState> stateMachine;　　// 状態遷移を管理するステートマシン
    public string stateInfoText;

    public EnemyStatusBase enemyStat;
     private Animator animator;
    HighlightEffect highlightEffect;

    Transform playerTrans; // プレイヤーのTransform
    public float distToPlayer; // プレイヤーとの距離
    public Vector3 dirToPlayer;

    public float moveSpeed = 2.1f; // 移動速度
    public float moveSpeedWander = 2.1f;
    public float moveSpeedEscape = 4.5f;

    public float distNeedToEscape = 5.6f; // プレイヤーから逃げる距離
    public float distNeedToReturnWander = 14.9f;

    public float stateCntEscape = 10f;
    public float stateCntWander = 5f;

    private float wanderDistToFarFromPlayer = 9.8f;
    private float wanderDistToCloseToPlayer = 7f;

    Vector3  wanderTarget;                 // next point to walk toward
    const float arriveThreshold = 0.35f;  

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player) playerTrans = player.transform;

        animator = GetComponent<Animator>();
        //highlightEffect = GetComponent<HighlightEffect>();

        stateMachine = StateMachine<RunnerState>.Initialize(this);
        stateMachine.ChangeState(RunnerState.Wander);

        enemyStat = GetComponent<EnemyStatusBase>();
    }

    private void Update()
    {
        UpdatePlayerInfo();
        UpdateStateInfo();


    }

    private void UpdatePlayerInfo()
    {
       dirToPlayer = (playerTrans.position - transform.position).normalized; // プレイヤーの方向を計算
       distToPlayer = Vector3.Distance(transform.position, playerTrans.position); // プレイヤーとの距離を計算

    }

    private void UpdateStateInfo()
    {
       stateInfoText = stateMachine.State.ToString();
        stateCnt -= Time.deltaTime * enemyStat.iceDebuffFactor * enemyStat.poisonSpeedDownFactor;
    }


    public void Wander_Enter()
    {
        stateCnt = stateCntWander;
        animator.SetBool("isWandering", true);
        moveSpeed = moveSpeedWander;
    }

    public void Wander_Update()
    {
        
         // 1) bail out if the player got too close
        if (distToPlayer < distNeedToEscape && stateCnt <= 0)
        {
            stateMachine.ChangeState(RunnerState.Escape);
            return;
        }

        // 2) move toward current wanderTarget
        Vector3 toTarget = wanderTarget - transform.position;

        // reached it? → pick another
        if (toTarget.sqrMagnitude < arriveThreshold * arriveThreshold)
        {
            PickNewWanderTarget();
            toTarget = wanderTarget - transform.position;
        }

        Vector3 dir = toTarget.normalized;
        transform.position += dir * moveSpeed * Time.deltaTime * enemyStat.iceDebuffFactor * enemyStat.poisonSpeedDownFactor;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    //────────────────────  E S C A P E  ────────────────────
    /* (unchanged – your original Escape_Enter / Escape_Update) */


    //────────────────────  H E L P E R  ────────────────────
    void PickNewWanderTarget()
    {
        // find a point that is 14 m – 21 m away from the player on the X-Z plane
        for (int i = 0; i < 8; i++)          // try up to 8 random samples
        {
            Vector2 dir2D = Random.insideUnitCircle.normalized;
            float    dist = Random.Range(wanderDistToCloseToPlayer, wanderDistToFarFromPlayer);

            Vector3 candidate = playerTrans.position +
                                new Vector3(dir2D.x, 0f, dir2D.y) * dist;

            // keep only points that are ALSO farther than 2 m from the runner,
            // so we don’t pick something right on top of ourselves.
            if ((candidate - transform.position).sqrMagnitude > 4f)
            {
                wanderTarget = candidate;
                return;
            }
        }

        // fallback – couldn’t find a good point this frame, stay put
        wanderTarget = transform.position;
    
        
    }

    public void Escape_Enter()
    {
        stateCnt = stateCntEscape;
        animator.SetBool("isWandering", false);
        moveSpeed = moveSpeedEscape;
    }

    public void Escape_Update()
    {
        // プレイヤーからの距離が逃げる距離を超えたらワンダリング状態に戻る
        if (distToPlayer >= distNeedToReturnWander && stateCnt <= 0)
        {
            stateMachine.ChangeState(RunnerState.Wander);
        }
        else
        {
            // プレイヤーから逃げる方向に移動
            Vector3 escapeDirection = -dirToPlayer.normalized;
            escapeDirection.y = 0;
            transform.position += escapeDirection * moveSpeed * Time.deltaTime * enemyStat.iceDebuffFactor * enemyStat.poisonSpeedDownFactor;
            transform.rotation = Quaternion.LookRotation(escapeDirection);
        }
    }



}

