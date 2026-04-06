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

public class PetSpiderAction : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpd = 2.8f;
    public float moveSpdRush = 7f;

    [Header("Ranges")]
    public float distToKeepBetweenPlayer = 2.8f;
    public float distTooFarRushToPlayer = 14f;
    public float distTooFarNeedTeleport = 28f;
    [SerializeField] private float attackRange = 12f; // Åö How close an enemy must be to trigger an attack

    [Header("Juice / Animation Trigger")]
    public string animFollow = "isWalking";
    public string animRush = "isRunning";
    public string animWander = "isWalking";
    public string animIdle = "isIdleing";
    public string animAttack = "isShotAttack"; // Åö Animation for the web shot

    [Header("LayerMasks")]
    public LayerMask enemyLayer; // Åö LayerMask to identify enemies

    [Header("Spider Attack Settings")]
    public GameObject webProjectilePrefab; // Åö The projectile to shoot (your WebProjectileObj)
    public GameObject webGroundDecalPrefab; // Åö The sticky web decal for the ground (your WebGroundObject)
    public float attackCooldown = 3.5f; // Åö Time between attacks
    private float attackTimer;

    [Header("Curved Shot Parameters")] // Åö Copied from EnemyBossSpiderAction
    [SerializeField] private AnimationCurve timeByDistance = AnimationCurve.Linear(0, 0.4f, 20, 1.4f);
    [SerializeField] private AnimationCurve heightByDistance = AnimationCurve.Linear(0, 3f, 20, 8f);
    [SerializeField] private float minFlightTime = 0.25f;
    [SerializeField] private float maxFlightTime = 2.0f;
    [SerializeField] private float maxArcHeight = 10f;

    // --- State Machine ---
    public enum PetState { Follow, RushToPlayer, Teleport, Wander, Attack } // Åö Added Attack state
    public StateMachine<PetState> fsm;
    public string stateName; // For debugging

    // --- Internal References & Caches ---
    Transform playerTrans;
    Animator animator;
    Vector3 selfPos;
    Transform attackTarget; // Åö The current enemy target
    float keepDistSqr, rushDistSqr, tpDistSqr, attackRangeSqr;
    readonly Collider[] hitCache = new Collider[16];

    // --- Wander State Variables ---
    [Header("Wander Settings")]
    public float wanderRadiusSlack = 0.6f;
    public float wanderWaitTime = 1.5f;
    public float wanderArrivalDistance = 0.14f;
    private bool isWaitingAtWanderPoint = false;
    private Vector3 wanderTarget;

    // =================================================================
    // SECTION 2: CORE UNITY METHODS (Start, Update)
    // =================================================================

    void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();

        // Cache squared distances for performance
        keepDistSqr = distToKeepBetweenPlayer * distToKeepBetweenPlayer;
        rushDistSqr = distTooFarRushToPlayer * distTooFarRushToPlayer;
        tpDistSqr = distTooFarNeedTeleport * distTooFarNeedTeleport;
        attackRangeSqr = attackRange * attackRange;

        fsm = StateMachine<PetState>.Initialize(this);
        fsm.ChangeState(PetState.Follow);
        attackTimer = 0f;
    }

    void Update()
    {
        selfPos = transform.position; // Update self position every frame
        stateName = fsm.State.ToString(); // Update state name for debugging

        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }


    // =================================================================
    // SECTION 3: STATE MACHINE IMPLEMENTATIONS
    // =================================================================

    // ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü Follow ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü
    void Follow_Enter() => animator?.SetBool(animFollow, true);
    void Follow_Exit() => animator?.SetBool(animFollow, false);
    void Follow_Update()
    {
        Vector3 toPlayer = playerTrans.position - selfPos;
        float distSq = toPlayer.sqrMagnitude;

        // Priority 1: Distance management
        if (distSq > tpDistSqr) { fsm.ChangeState(PetState.Teleport); return; }
        if (distSq > rushDistSqr) { fsm.ChangeState(PetState.RushToPlayer); return; }

        // Åö Priority 2: Check for enemies to attack
        if (TryFindNearestEnemy(out attackTarget))
        {
            fsm.ChangeState(PetState.Attack);
            return;
        }

        // Priority 3: Wander if close enough
        if (distSq <= keepDistSqr) { fsm.ChangeState(PetState.Wander); return; }

        // Default: Follow player at a set distance
        Vector3 desired = playerTrans.position - toPlayer.normalized * distToKeepBetweenPlayer;
        MoveTowards(desired, moveSpd);
        Face(playerTrans.position);
    }

    // ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü RushToPlayer ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü
    void RushToPlayer_Enter() => animator?.SetBool(animRush, true);
    void RushToPlayer_Exit() => animator?.SetBool(animRush, false);
    void RushToPlayer_Update()
    {
        Vector3 playerPos = playerTrans.position;
        float distSq = (playerPos - selfPos).sqrMagnitude;

        if (distSq < keepDistSqr) { fsm.ChangeState(PetState.Follow); return; }
        if (distSq > tpDistSqr) { fsm.ChangeState(PetState.Teleport); return; }

        MoveTowards(playerPos, moveSpdRush);
        Face(playerPos);
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
        animator?.SetBool(animWander, true);
        PickNextWanderPoint();
    }
    void Wander_Exit()
    {
        isWaitingAtWanderPoint = false;
        animator?.SetBool(animWander, false);
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
    async UniTask WaitAndPickNextPointAsync()
    {
        isWaitingAtWanderPoint = true;
        animator?.SetTrigger(animIdle);
        await UniTask.Delay(TimeSpan.FromSeconds(wanderWaitTime), cancellationToken: this.GetCancellationTokenOnDestroy());
        if (fsm.State != PetState.Wander) return;
        animator?.SetBool(animWander, true);
        PickNextWanderPoint();
        isWaitingAtWanderPoint = false;
    }
    void PickNextWanderPoint()
    {
        Vector2 rnd = UnityEngine.Random.insideUnitCircle.normalized * (distToKeepBetweenPlayer * 0.8f);
        wanderTarget = playerTrans.position + new Vector3(rnd.x, 0f, rnd.y);
    }

    // ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü Attack (Åö NEW STATE) ÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑüÑü
    void Attack_Enter()
    {
        animator?.SetTrigger(animAttack);
        if (attackTarget != null) Face(attackTarget.position);

        if (attackTimer <= 0)
        {
            ShotCurveProjectileAtTarget();
            attackTimer = attackCooldown;
        }
    }
    void Attack_Update()
    {
        // If target is dead/gone or too far away, go back to following
        if (attackTarget == null || (attackTarget.position - selfPos).sqrMagnitude > attackRangeSqr)
        {
            fsm.ChangeState(PetState.Follow);
            return;
        }

        Face(attackTarget.position); // Keep facing the target

        // Once the attack is fired (cooldown started), we can return to following.
        // The pet will re-enter Attack state on the next frame if the enemy is still in range.
        if (attackTimer > 0)
        {
            fsm.ChangeState(PetState.Follow);
        }
    }

    // =================================================================
    // SECTION 4: UTILITY & ATTACK METHODS
    // =================================================================

    // --- Movement Utilities ---
    void MoveTowards(Vector3 target, float spd)
    {
        Vector3 dir = (target - selfPos);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            transform.position += dir.normalized * spd * Time.deltaTime;
        }
    }
    void Face(Vector3 target)
    {
        Vector3 dir = target - selfPos;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 12f * Time.deltaTime);
    }

    // --- Search Utility (Åö NEW) ---
    bool TryFindNearestEnemy(out Transform result)
    {
        result = null;
        if (enemyLayer.value == 0) return false;

        int hits = Physics.OverlapSphereNonAlloc(selfPos, attackRange, hitCache, enemyLayer);
        if (hits == 0) return false;

        float bestSqDist = float.MaxValue;
        for (int i = 0; i < hits; i++)
        {
            // Optional: Add a check to ensure the enemy is not obstructed by a wall
            float sqDist = (hitCache[i].transform.position - selfPos).sqrMagnitude;
            if (sqDist < bestSqDist)
            {
                bestSqDist = sqDist;
                result = hitCache[i].transform;
            }
        }
        return result != null;
    }

    // --- Attack Logic (Åö Copied & Adapted from EnemyBossSpiderAction) ---
    void ShotCurveProjectileAtTarget()
    {
        if (attackTarget == null) return;
        ShotCurveProjectile(attackTarget.position);
        // Add sound effect here if you want
        // SoundEffect.Instance.Play(SoundList.SpiderBossStickyWebSe);
    }

    void ShotCurveProjectile(Vector3 targetPoint)
    {
        if (webProjectilePrefab == null) return;

        float dist = Vector3.Distance(transform.position, targetPoint);
        float flightTime = Mathf.Clamp(timeByDistance.Evaluate(dist), minFlightTime, maxFlightTime);
        float arcHeight = Mathf.Min(heightByDistance.Evaluate(dist), maxArcHeight);

        // Consider using an object pool (like QFSW.MOP2) for better performance
        GameObject proj = Instantiate(webProjectilePrefab, transform.position, Quaternion.identity);

        proj.transform.DOJump(targetPoint, arcHeight, 1, flightTime)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                Destroy(proj); // Or release to pool
                SpawnWebGround(targetPoint);
            });
    }

    void SpawnWebGround(Vector3 spawnPos)
    {
        if (webGroundDecalPrefab == null) return;
        Instantiate(webGroundDecalPrefab, spawnPos, Quaternion.identity);
    }










}

