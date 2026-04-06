using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager
using System.Collections; //Coroutine

public class PetSpiderLiittleAction : MonoBehaviour
{

    public enum PetState { Follow, RushToPlayer, Teleport, Wander, Attack } // Åö Added Attack state
    public StateMachine<PetState> fsm;
    public string stateName;
    public float stateCnt = 3f;

    Transform playerTrans;
    Animator animator;
    Transform attackTarget;

    public float distToPlayer;
    public float dirToPlayer;

    public float moveSpeed = 2.1f;
    public float moveSpeedFollow = 2.1f;
    public float moveSpeedRush = 5.6f;

    public float distNeedToWander = 2.8f;
    public float distNeedToRush = 10f;
    public float distNeedToFollow = 4.9f;
    public float distToKeepBetweenPlayer = 1.4f;

    public float attackCooldown = 3f;
    public float attackCooldownMax = 10f;


      [Header("Wander Settings")]
    public int wanderMovesMax = 3; 
    public float wanderPauseTime = 1.0f;
    // Åö NEW: Controls the angle randomness when picking a point on the opposite side.
    [Tooltip("Controls the angle randomness when picking a wander point on the opposite side of the player.")]
    public float wanderAngleRange = 45f;
    
    private Coroutine wanderCoroutine;
    private int wanderMovesCurrent;
    private Vector3 wanderTargetPosition;

    public float wanderRadius = 5.6f;
    public float playerPersonalSpaceRadius = 2.1f;
    void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
    
        fsm = StateMachine<PetState>.Initialize(this);
        fsm.ChangeState(PetState.Follow);
    
    }

    void Update()
    {
        stateName = fsm.State.ToString();
        stateCnt -= Time.deltaTime;
        CalDistToPlayer();



    }

    public void CalDistToPlayer()
    {

        distToPlayer = Vector3.Distance(transform.position, playerTrans.position);
        dirToPlayer = Vector3.Dot((playerTrans.position - transform.position).normalized, transform.forward);
    }
       

    public void RotateToFaceTarget(Vector3 tagetPos)
    {
        Vector3 dir = tagetPos - transform.position;
        dir.y = 0; // Keep the rotation on the horizontal plane
        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    public void RotateFaceTargetNow(Vector3 tagetPos)
    {
        Vector3 dir = tagetPos - transform.position;
        dir.y = 0; // Keep the rotation on the horizontal plane
        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = targetRotation; // Instantly face the target
        }
    }

    public void HomingTowardPlayer()
    {
        if (playerTrans != null)
        {
            if (distToPlayer < distToKeepBetweenPlayer) return;
            Vector3 direction = (playerTrans.position - transform.position).normalized;
            transform.position += direction * Time.deltaTime * moveSpeed; // Adjust speed as needed
        }
    }

    void Follow_Enter()
    {
        animator.SetBool("isMoving",true);
        moveSpeed = moveSpeedFollow;

    }

    void Follow_Update()
    {
        RotateToFaceTarget(playerTrans.position);
        HomingTowardPlayer();

        if(distToPlayer > distNeedToRush)
        {
            ChangeState(PetState.RushToPlayer);
        }

        if (distToPlayer < distNeedToWander)
        {
            ChangeState(PetState.Wander);
        }

    }

    void Follow_Exit()
    {
        animator.SetBool("isMoving",false);
    }

    void RushToPlayer_Enter()
    {
        moveSpeed = moveSpeedRush;

    }

    void RushToPlayer_Update()
    {
        RotateToFaceTarget(playerTrans.position);
        HomingTowardPlayer();


        if(distToPlayer < distNeedToFollow)
        {
            ChangeState(PetState.Follow);
        }
      
    }

    void RushToPlayer_Exit()
    {

    }

    void Teleport_Enter()
    {

    }

    void Teleport_Update()
    {

    }

    void Teleport_Exit()
    {

    }

    void Wander_Enter()
    {
        stateCnt = 14f;

        wanderMovesCurrent = wanderMovesMax;
        wanderCoroutine = StartCoroutine(WanderSequence());
    }



    IEnumerator WanderSequence()
    {
        while (wanderMovesCurrent > 0)
        {
            // Step 1: Find a valid wander point that satisfies all rules.
            if (FindBestWanderPoint(out wanderTargetPosition))
            {
                // We found a good point.
                //Debug.DrawLine(transform.position, wanderTargetPosition, Color.green, 2f);
            }
            else
            {
                // Fallback: If no good point was found, do a simple safe maneuver.
                wanderTargetPosition = GetSafeFallbackPoint();
                //Debug.DrawLine(transform.position, wanderTargetPosition, Color.yellow, 2f);
            }

            // Step 2: Move towards the target (straight line movement).
            // animator.SetBool("canWalk", true);
            while (Vector3.Distance(transform.position, wanderTargetPosition) > 0.2f)
            {
                // Emergency stop if we get too close to the player while moving
                if (Vector3.Distance(transform.position, playerTrans.position) < playerPersonalSpaceRadius)
                {
                    break; 
                }
                RotateToFaceTarget(wanderTargetPosition);
                transform.position = Vector3.MoveTowards(transform.position, wanderTargetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            // Step 3: Pause.
             animator.SetBool("isIdling", true);
            yield return new WaitForSeconds(wanderPauseTime);

            wanderMovesCurrent--;
        }

        ChangeState(PetState.Follow);
    }

    // ÅöÅöÅö THIS IS THE FINAL, COMBINED POINT-PICKING FUNCTION ÅöÅöÅö
    bool FindBestWanderPoint(out Vector3 bestPoint)
    {
        Vector3 spiderPos = transform.position;
        Vector3 playerPos = playerTrans.position;
        bestPoint = spiderPos; // Default value

        // --- PRE-CALCULATIONS FOR SAFETY CHECK ---
        Vector3 dirToPlayerFromSpider = (playerPos - spiderPos).normalized;
        float distSpiderToPlayer = Vector3.Distance(spiderPos, playerPos);
        if (distSpiderToPlayer <= playerPersonalSpaceRadius) return false; // Too close to calculate, abort.
        float tangentAngleCos = Mathf.Sqrt(distSpiderToPlayer * distSpiderToPlayer - playerPersonalSpaceRadius * playerPersonalSpaceRadius) / distSpiderToPlayer;
        
        // --- RULE 1: DETERMINE TARGET SIDE (Cross-Over) ---
        Vector3 playerFwd = playerTrans.forward;
        playerFwd.y = 0;
        Vector3 dirToSpiderFromPlayer = (spiderPos - playerPos).normalized;
        
        float side = Vector3.Cross(playerFwd, dirToSpiderFromPlayer).y;
        Vector3 baseTargetDirection = (side < 0) ? playerTrans.right : -playerTrans.right;

        // --- RULE 2 & 3: SEARCH AND FILTER ---
        int maxAttempts = 30;
        for (int i = 0; i < maxAttempts; i++)
        {
            // 2a. Generate a potential point on the correct side and at the correct distance (Proximity Rule)
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(-wanderAngleRange, wanderAngleRange), 0);
            Vector3 finalTargetDirection = randomRotation * baseTargetDirection;
            Vector3 potentialPoint = playerPos + finalTargetDirection.normalized * distToKeepBetweenPlayer;

            // 3a. Check if the PATH to this point is safe (Safe Path Rule)
            Vector3 dirToPotentialPoint = (potentialPoint - spiderPos).normalized;
            if (Vector3.Dot(dirToPlayerFromSpider, dirToPotentialPoint) < tangentAngleCos)
            {
                // This is a valid point that satisfies all rules!
                bestPoint = potentialPoint;
                return true;
            }
        }

        // If the loop finishes without finding a point, it means no point in the target arc was "safe".
        return false;
    }

    // A simple, predictable, and always safe maneuver if the main logic fails.
    Vector3 GetSafeFallbackPoint()
    {
        Vector3 spiderPos = transform.position;
        Vector3 playerPos = playerTrans.position;
        
        // Get direction perpendicular to the player
        Vector3 dirToPlayer = (playerPos - spiderPos).normalized;
        Vector3 perpendicularDir = Vector3.Cross(dirToPlayer, Vector3.up).normalized;
        
        // Flip direction if it points towards a wall (requires raycast, optional)
        
        return spiderPos + perpendicularDir * 2f; // Move 2 units sideways
    }


    void Wander_Update()
    {




        if (stateCnt <= 0) ChangeState(PetState.Follow);
    }

    void Wander_Exit()
    {
        animator.SetBool("isIdling", false);

        if (wanderCoroutine != null)
        {
            StopCoroutine(wanderCoroutine);
        }

    }

    void Attack_Enter()
    {
        
    }

    void Attack_Update()
    {
       
    }

    void Attack_Exit()
    {

    }

    public void ChangeState(PetState newState)
    {
        fsm.ChangeState(newState);
    }

}

