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

// PetActionBase.cs
//using UnityEngine;
//using MonsterLove.StateMachine;
//using DG.Tweening;
//using Cysharp.Threading.Tasks;
using System;

public abstract class PetActionBase : MonoBehaviour
{
    [Header("--- Base Pet Settings ---")]
    [Header("Movement")]
    public float moveSpd = 2.8f;
    public float moveSpdRush = 7f;
    public float followSmooth = 6f;

    [Header("Ranges")]
    public float distToKeepBetweenPlayer = 2.8f;
    public float distTooFarRushToPlayer = 14f;
    public float distTooFarNeedTeleport = 28f;
    public float attackRange = 10f; // Åö NEW: For pets that can attack

    [Header("Animation Triggers")]
    public string animFollow = "isWalking";
    public string animRush = "isRunning";
    public string animWander = "isWalking";
    public string animIdle = "isIdleing";

    [Header("LayerMasks")]
    public LayerMask enemyLayer; // Åö NEW: To find targets

    // FSM: We add 'Attack' for pets that can use it.
    public enum PetState { Follow, RushToPlayer, Teleport, Wander, Attack, CollectExp }
    public StateMachine<PetState> fsm;
    public string stateName;

    protected Transform playerTrans;
    protected Animator animator;
    protected Vector3 selfPos;
    protected Transform attackTarget; // Åö NEW: The current enemy to attack

    // Distance caches
    float keepDistSqr, rushDistSqr, tpDistSqr;

    // Search cache
    protected readonly Collider[] hitCache = new Collider[16];
    
    [Header("Wander Settings")]
    public float wanderRadiusSlack = 0.6f;
    public float wanderIntervalMin = 0.8f;
    public float wanderIntervalMax = 1.5f;
    public float wanderWaitTime = 1.5f;
    public float wanderArrivalDistance = 0.14f;
    private Vector3 wanderTarget;
    private bool isWaitingAtWanderPoint = false;

    protected virtual void Awake()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();

        keepDistSqr = distToKeepBetweenPlayer * distToKeepBetweenPlayer;
        rushDistSqr = distTooFarRushToPlayer * distTooFarRushToPlayer;
        tpDistSqr = distTooFarNeedTeleport * distTooFarNeedTeleport;

        fsm = StateMachine<PetState>.Initialize(this);
    }

    protected virtual void Start()
    {
        // Default starting state
        fsm.ChangeState(PetState.Follow);
    }

    void Update()
    {
        if (playerTrans == null) return;
        selfPos = transform.position;
        stateName = fsm.State.ToString(); // For debugging
    }

    // ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü Follow ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü
    protected virtual void Follow_Enter()
    {
        if (animator) animator.SetBool(animFollow, true);
    }
    protected virtual void Follow_Update()
    {
        Vector3 playerPos = playerTrans.position;
        Vector3 toPlayer = playerPos - selfPos;
        float distSq = toPlayer.sqrMagnitude;

        // Priority 1: Distance management
        if (distSq > tpDistSqr) { fsm.ChangeState(PetState.Teleport); return; }
        if (distSq > rushDistSqr) { fsm.ChangeState(PetState.RushToPlayer); return; }
        
        // Priority 2: Check for enemies to attack (if this pet can attack)
        if (TryFindNearestEnemy(out attackTarget))
        {
            fsm.ChangeState(PetState.Attack);
            return;
        }

        // Priority 3: Wander if close enough to player
        if (distSq <= keepDistSqr) { fsm.ChangeState(PetState.Wander); return; }

        // Default: Follow player at a set distance
        Vector3 desired = playerPos - toPlayer.normalized * distToKeepBetweenPlayer;
        MoveTowards(desired, moveSpd);
        Face(playerPos);
    }
    protected virtual void Follow_Exit()
    {
        if (animator) animator.SetBool(animFollow, false);
    }

    // ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü RushToPlayer ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü
    void RushToPlayer_Enter()
    {
        if (animator)
        {
            animator.SetBool(animFollow, false);
            animator.SetBool(animRush, true);
        }
    }
    void RushToPlayer_Update()
    {
        Vector3 playerPos = playerTrans.position;
        float distSq = (playerPos - selfPos).sqrMagnitude;

        if (distSq < keepDistSqr) { fsm.ChangeState(PetState.Follow); return; }
        if (distSq > tpDistSqr) { fsm.ChangeState(PetState.Teleport); return; }
        
        MoveTowards(playerPos, moveSpdRush);
        Face(playerPos);
    }
    void RushToPlayer_Exit()
    {
        if (animator) animator.SetBool(animRush, false);
    }

    // ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü Teleport ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü
    void Teleport_Enter()
    {
        Vector3 rndOffset = UnityEngine.Random.insideUnitCircle.normalized * distToKeepBetweenPlayer;
        rndOffset.y = 0f;

        transform.DOScale(0, 0.2f).SetEase(Ease.InBack)
           .OnComplete(() => {
               transform.position = playerTrans.position + rndOffset;
               selfPos = transform.position;
               transform.DOScale(1, 0.25f).SetEase(Ease.OutBack)
                        .OnComplete(() => fsm.ChangeState(PetState.Follow));
           });
    }

    // ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü Wander ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü
    void Wander_Enter()
    {
        isWaitingAtWanderPoint = false;
        if (animator)
        {
            animator.SetBool(animRush, false);
            animator.SetBool(animWander, true);
        }
        PickNextWanderPoint();
    }
    void Wander_Update()
    {
        float distSqToPlayer = (playerTrans.position - selfPos).sqrMagnitude;
        if (distSqToPlayer > keepDistSqr + wanderRadiusSlack * wanderRadiusSlack) { fsm.ChangeState(PetState.Follow); return; }
        if (distSqToPlayer > rushDistSqr) { fsm.ChangeState(PetState.RushToPlayer); return; }

        // Åö Check for enemies even while wandering
        if (TryFindNearestEnemy(out attackTarget))
        {
            fsm.ChangeState(PetState.Attack);
            return;
        }

        if (isWaitingAtWanderPoint) return;

        MoveTowards(wanderTarget, moveSpd);
        Face(wanderTarget);

        if ((wanderTarget - selfPos).sqrMagnitude < wanderArrivalDistance * wanderArrivalDistance)
        {
            WaitAndPickNextPointAsync().Forget();
        }
    }
    void Wander_Exit()
    {
        isWaitingAtWanderPoint = false;
        if (animator) animator.SetBool(animWander, false);
    }

    // --- Wander Async Helper ---
    async UniTask WaitAndPickNextPointAsync()
    {
        isWaitingAtWanderPoint = true;
        if (animator) animator.SetTrigger(animIdle);

        await UniTask.Delay(TimeSpan.FromSeconds(wanderWaitTime), cancellationToken: this.GetCancellationTokenOnDestroy());

        if (fsm.State != PetState.Wander) return;

        if (animator) animator.SetBool(animWander, true);
        PickNextWanderPoint();
        isWaitingAtWanderPoint = false;
    }
    void PickNextWanderPoint()
    {
        Vector2 rnd = UnityEngine.Random.insideUnitCircle.normalized * distToKeepBetweenPlayer * 0.8f;
        wanderTarget = playerTrans.position + new Vector3(rnd.x, 0f, rnd.y);
    }

    // ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü Shared Utilities ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü
    
    // Åö NEW: Finds enemies. Returns false if no enemies are in range.
    protected virtual bool TryFindNearestEnemy(out Transform result)
    {
        result = null;
        // If the enemyLayer isn't set, this pet can't attack.
        if (enemyLayer.value == 0) return false;

        int hits = Physics.OverlapSphereNonAlloc(selfPos, attackRange, hitCache, enemyLayer);
        if (hits == 0) return false;
        
        float bestSqDist = float.MaxValue;
        for (int i = 0; i < hits; i++)
        {
            float sqDist = (hitCache[i].transform.position - selfPos).sqrMagnitude;
            if (sqDist < bestSqDist)
            {
                bestSqDist = sqDist;
                result = hitCache[i].transform;
            }
        }
        return result != null;
    }
    
    protected void MoveTowards(Vector3 target, float spd)
    {
        Vector3 dir = (target - selfPos);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            transform.position += dir.normalized * spd * Time.deltaTime;
        }
    }

    protected void Face(Vector3 target)
    {
        Vector3 dir = target - selfPos;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 12f * Time.deltaTime);
    }
}
