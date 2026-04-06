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


public class PetKappaAction : MonoBehaviour
{
    public string petName = "カッパ助"; //or カッパ太郎

    [Header("Movement")]
    public float moveSpd          = 2.8f;
    public float moveSpdRush      = 7f;
    public float moveSpdCollect   = 4.2f;
    public float followSmooth     = 6f;
    
    [Header("Ranges")]
    public float distToKeepBetweenPlayer = 2.8f;
    public float distTooFarRushToPlayer  = 14f;
    public float distTooFarNeedTeleport  = 28f;
    public float expCollectRange         = 7f;

    [Header("Juice / Animation Trigger")]
    public string animFollow      = "isWalking";
    public string animCollect     = "CollectExp";
    public string animRush        = "isRunning";
    public string animWander = "isWalking";
    public string animIdle = "isIdleing";

    [Header("LayerMask")]
    public LayerMask expLayer;

    public enum PetState { Follow, CollectExp, RushToPlayer, Teleport, Wander }
    public StateMachine<PetState> fsm;
    public string stateName;
    public int stateCnt;


    Transform  playerTrans;
    Animator   animator;
    Transform  targetExp;                               // ★ 追尾中Exp
    Vector3    selfPos;

    // 距離²キャッシュ
    public float keepDistSqr, rushDistSqr, tpDistSqr;

    // Exp 検索用キャッシュ
    readonly Collider[] hitCache = new Collider[16];

    [Header("Wander")]
    public float wanderRadiusSlack   = 0.6f;       
    public float wanderIntervalMin   = 0.8f;       
    public float wanderIntervalMax   = 1.5f;       
    public float wanderTimer;                             
    Vector3 wanderTarget;  
    
    public float wanderWaitTime = 1.5f;
    public float wanderArrivalDistance = 0.14f;
    public bool isWaitingAtWanderPoint = false;

    public bool canPickExp = true;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            //プレイヤーのTransformを取得
            playerTrans = playerObj.transform;
        }
        else
        {
            this.enabled = false;
            return;
        }

        animator    = GetComponent<Animator>();

        keepDistSqr = distToKeepBetweenPlayer * distToKeepBetweenPlayer;
        rushDistSqr = distTooFarRushToPlayer  * distTooFarRushToPlayer;
        tpDistSqr   = distTooFarNeedTeleport  * distTooFarNeedTeleport;

        fsm = StateMachine<PetState>.Initialize(this);
        fsm.ChangeState(PetState.Follow);
    }
    //void OnEnable()  => fsm.ChangeState(PetState.Follow);
    void Update()
    {
        selfPos = transform.position;   // 汎用で使うので毎フレーム更新

        UpdateCurrentStateInfo();
    }

    // ───────────── Follow ─────────────
    void Follow_Enter()
    {
        if (this == null || animator == null) return;
        animator.SetBool("isWalking", true);
    }
    void Follow_Update()
    {
        Vector3 playerPos = playerTrans.position;
        Vector3 toPlayer  = playerPos - selfPos;
        float   distSq    = toPlayer.sqrMagnitude;

        // A) Teleport / Rush 判定
        if (distSq > tpDistSqr)  { fsm.ChangeState(PetState.Teleport);   return; }
        if (distSq > rushDistSqr){ fsm.ChangeState(PetState.RushToPlayer); return; }

        // B) Exp 判定
        if (TryFindNearestExp(out targetExp))
        {  fsm.ChangeState(PetState.CollectExp);  return; }

        // C) Wander 判定
        if (distSq <= keepDistSqr)                       // プレイヤー十分近い
        {  fsm.ChangeState(PetState.Wander);  return; }

        // D) 近過ぎでも遠過ぎでもない通常追従
        Vector3 desired = playerPos - toPlayer.normalized * distToKeepBetweenPlayer;
        MoveTowards(desired, moveSpd);
        Face(playerPos);
    }

    // ───────────── CollectExp ─────────────
    void CollectExp_Enter()
    {
        //animator.SetTrigger(animCollect);
    }
    void CollectExp_Update()
    {
        if (!IsValidExp(targetExp))
        {              
            targetExp = null;
        }

        if (!targetExp) { fsm.ChangeState(PetState.Follow); return; }
        Vector3 targetPos = targetExp.position;
        
        // プレイヤーから離れすぎたら中断
        float distPlayerSq = (playerTrans.position - selfPos).sqrMagnitude;
        if (distPlayerSq > rushDistSqr) { fsm.ChangeState(PetState.RushToPlayer); return; }

        // 移動
        MoveTowards(targetPos, moveSpdCollect);
        Face(targetPos);

        // 到達チェック
        if ((targetPos - selfPos).sqrMagnitude < .1f || targetExp == null)
        {
            // 取得演出をここに書く（例：targetExp.GetComponent<ExpMove>().Collect()）
            //if (targetExp) Destroy(targetExp.gameObject);
            targetExp = null;
            fsm.ChangeState(PetState.Follow);
        }
    }

    bool IsValidExp(Transform t)
    {
        return t                     // reference still exists
            && t.gameObject.activeInHierarchy;
    }

    public void ChangeToThisState(PetState nextState)
    {
        fsm.ChangeState(nextState); // 次の状態に変更

    }

    public void ChangeToFollowState()
    {
        //if is !collectExpState return
        if (fsm.State != PetState.CollectExp) return; // CollectExp状態なら無視

        fsm.ChangeState(PetState.Follow); // Follow状態に変更

    }

    // ───────────── RushToPlayer ─────────────
    void RushToPlayer_Enter()
    {
        if (this == null || animator == null) return;
        animator.SetBool("isWalking", false); 
        animator.SetBool("isRunning", true);

        
        // 残像演出
        DOTween.Sequence()
               .AppendCallback(()=>SpawnAfterImage())
               .AppendInterval(.15f)
               .SetLoops(3);
    }
    void RushToPlayer_Update()
    {
        Vector3 playerPos = playerTrans.position;
        float   distSq    = (playerPos - selfPos).sqrMagnitude;

        if (distSq < keepDistSqr)
        {
            fsm.ChangeState(PetState.Follow);
            return;
        }
        if (distSq > tpDistSqr)
        {
            fsm.ChangeState(PetState.Teleport);
            return;
        }
        MoveTowards(playerPos, moveSpdRush);
        Face(playerPos);
    }

    // ───────────── Teleport ─────────────
    void Teleport_Enter()
    {
        const float shrinkT = .2f, growT = .25f;
        Vector3 rndOffset = UnityEngine.Random.insideUnitCircle.normalized * distToKeepBetweenPlayer;
              rndOffset.y = 0f;
        
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(0.56f, shrinkT).SetEase(Ease.InBack))
           .AppendCallback(()=> {
                transform.position = playerTrans.position + rndOffset;
                selfPos = transform.position;
           })
           
           .OnComplete(()=>fsm.ChangeState(PetState.Follow));
    }
    
    void Wander_Enter()
    {
        if (this == null || animator == null) return;
        isWaitingAtWanderPoint = false; // Ensure we start by moving
        animator.SetBool("isWalking",true);
        animator.SetBool("isRunning", false);
        PickNextWanderPoint();
    }

   void Wander_Update()
    {
        // 1) First, check for higher-priority state changes. This is always important.
        float distSqToPlayer = (playerTrans.position - selfPos).sqrMagnitude;
        if (distSqToPlayer > keepDistSqr + wanderRadiusSlack * wanderRadiusSlack)
        {
            fsm.ChangeState(PetState.Follow);
            return;
        }
        if (distSqToPlayer > rushDistSqr)
        {
            fsm.ChangeState(PetState.RushToPlayer);
            return;
        }
        if (TryFindNearestExp(out targetExp))
        {
            fsm.ChangeState(PetState.CollectExp);
            return;
        }
        
        // 2) If we are in the "waiting" phase, do nothing else.
        if (isWaitingAtWanderPoint)
        {
            return;
        }

        // 3) If we are not waiting, perform the movement.
        MoveTowards(wanderTarget, moveSpd);
        Face(wanderTarget);

        // 4) Check for arrival. If we've arrived, start the waiting sequence.
        if ((wanderTarget - selfPos).sqrMagnitude < wanderArrivalDistance * wanderArrivalDistance)
        {
            // Don't call PickNextWanderPoint directly. Instead, start the async wait task.
            // The ".Forget()" call is from UniTask, indicating we are intentionally not awaiting the result here.
            WaitAndPickNextPointAsync().Forget();
        }
    }

    void Wander_Exit() // ★ NEW: Good practice to reset flags when exiting a state
    {
        isWaitingAtWanderPoint = false;
    }

    async UniTask WaitAndPickNextPointAsync() // ★ NEW: The async method for the wait sequence
    {
        // 1. Start waiting
        if (this == null || animator == null) return;
        isWaitingAtWanderPoint = true;
        animator.SetTrigger("isIdleing");

        // 2. Wait for the specified time
        // UniTask.Delay is more efficient than WaitForSeconds. We use TimeSpan.FromSeconds.
        await UniTask.Delay(TimeSpan.FromSeconds(wanderWaitTime), ignoreTimeScale: false,cancellationToken: this.GetCancellationTokenOnDestroy()); //Cancel the task automatically when the object dies

        // 3. IMPORTANT: After the wait, check if we are still in the Wander state.
        // If the player moved and the state changed, we must abort this sequence.
        if (fsm.State != PetState.Wander)
        {
            return; // Exit the async method early.
        }

        if (this == null || animator == null) return;
        animator.SetBool("isWalking",true);
        PickNextWanderPoint();
        isWaitingAtWanderPoint = false; // Allow movement to resume in Wander_Update
    }

    void PickNextWanderPoint()
    {
        // This function is now simpler, it only picks a point.
        Vector2 rnd = UnityEngine.Random.insideUnitCircle.normalized * distToKeepBetweenPlayer * 0.8f; // Slightly closer
        wanderTarget = playerTrans.position + new Vector3(rnd.x, 0f, rnd.y);
    }



    // ───────────── 汎用ユーティリティ ─────────────
    bool TryFindNearestExp(out Transform result)
    {

        if (!canPickExp) { result = null; return false; }

        result = null;
        int hits = Physics.OverlapSphereNonAlloc(
            selfPos, expCollectRange, hitCache, expLayer);

        float bestSq = float.MaxValue;
        for (int i = 0; i < hits; i++)
        {
            Transform t = hitCache[i].transform;
            // 自分からも / プレイヤーからも遠すぎない？
            float sqSelf   = (t.position - selfPos).sqrMagnitude;
            float sqPlayer = (t.position - playerTrans.position).sqrMagnitude;
            if (sqSelf < bestSq && sqPlayer < expCollectRange * expCollectRange)
            {
                bestSq = sqSelf;
                result = t;
            }
        }
        return result;
    }

    void MoveTowards(Vector3 target, float spd)
    {
        Vector3 dir = (target - selfPos);
        dir.y = 0f;
        float mag = dir.magnitude;
        if (mag < .001f) return;
        selfPos += dir / mag * spd * Time.deltaTime;
        transform.position = selfPos;
    }

    void Face(Vector3 target)
    {
        Vector3 dir = target - selfPos;
        dir.y = 0;
        if (dir.sqrMagnitude > .001f)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                12f * Time.deltaTime);
    }

    void BobbingAnimation()
    {
        float y = Mathf.Sin(Time.time * 4f) * .08f + 0.5f;
        Vector3 p = selfPos; p.y = y; transform.position = p;
    }
    void SpawnAfterImage()
    {
        // 任意：ポストプロシェーダや半透明メッシュをInstantiate
    }

    public void UpdateCurrentStateInfo()
    {
        //PetState currentState = fsm.State;
        //stateName = currentState.ToString();
    }




}

