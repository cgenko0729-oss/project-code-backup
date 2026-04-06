using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine

public class EnemyRabbitAction : EnemyStateActionBase
{

    [Header("Rabbit Behavior Distances")]
    [Tooltip("If player is closer than this, the rabbit will run away.")]
    public float escapeDistance = 10f;

    [Tooltip("If player is between escapeDistance and this, the rabbit wanders around the player.")]
    public float wanderDistance = 21f;

    [Tooltip("The radius of the circle the rabbit wanders on around the player.")]
    public float wanderCircleRadius = 20f;

    // The target point for the current wander movement
    private Vector3 wanderTargetPoint;

    protected override void Awake()
    {
        // Use more descriptive states from the Enum for clarity
         stateMachine = StateMachine<EnemyState>.Initialize(this);
        stateMachine.ChangeState(EnemyState.Wander);

        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        InitSeperationInfo();
        GetPlayerInfo();
        GetAnimator();
    }

    protected override void Update()
    {
        // Base class updates
        base.Update(); // This calls the empty Update in the base, but it's good practice

        CalDistToPlayer();

        // --- This is the "Brain" of the rabbit, deciding which state to be in ---
        // It constantly checks distances and overrides the current state if needed.
        if (distToPlayer < escapeDistance)
        {
            // Player is too close! Escape!
            // We use ChangeState which only transitions if we aren't already in the state.
            stateMachine.ChangeState(EnemyState.RunAway);
        }
        else if (distToPlayer < wanderDistance)
        {
            // Player is in the "medium" range, let's wander around them.
            stateMachine.ChangeState(EnemyState.Wander);
        }
        else
        {
            // Player is too far away, we can relax.
            // Let's use the Idle state for this.
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }

    #region Idle State
    // A new state for when the player is too far away to be a threat.
    void Idle_Enter()
    {
        //if (animator) animator.SetTrigger("isIdle");
    }

    void Idle_Update()
    {
        // In this state, the rabbit does nothing but wait.
        // The main Update() loop will handle transitioning out if the player gets closer.
    }
    #endregion

    #region RunAway State (Previously Move)
    void RunAway_Enter()
    {
        //if (animator) animator.SetTrigger("isMove"); // Use a running animation
    }

    void RunAway_Update()
    {
        MoveAwayFromPlayer();
        EnemyFaceAwayPlayer();



        //EnemySeparation();
    }
    #endregion

    #region Wander State
    void Wander_Enter()
    {
        // Debug.Log("Rabbit: Entering Wander state!");
        //if (animator) animator.SetTrigger("isMove"); // Use a walking/hopping animation
        PickNewWanderPoint();
    }

    void Wander_Update()
    {
        // Move towards the chosen point on the circle
        WanderToTargetPoint();

        // Always apply separation to avoid clumping with other enemies
        //EnemySeparation();

        // Make the rabbit face the direction it's moving
        if (wanderTargetPoint != transform.position)
        {
            // We set faceVector so the existing base class method can work.
            // We want to face TOWARDS the target, so isOpposite should be false.
            faceVector = wanderTargetPoint;
            EnemyFaceTargetPoint(wanderTargetPoint); 
        }
    }

    /// <summary>
    /// Moves the rabbit towards its current wanderTargetPoint and picks a new one upon arrival.
    /// </summary>
    void WanderToTargetPoint()
    {
        // If we are very close to the target, pick a new one.
        if (Vector3.Distance(transform.position, wanderTargetPoint) < 1.5f)
        {
            PickNewWanderPoint();
        }

        // Move towards the target
        transform.position = Vector3.MoveTowards(transform.position, wanderTargetPoint, moveSpd * Time.deltaTime);
    }

    /// <summary>
    /// This is the core logic for your request. It picks a random point on a circle around the player.
    /// </summary>
    void PickNewWanderPoint()
    {
        // Get the player's position, but on the same horizontal plane as the rabbit.
        Vector3 playerCenterPos = new Vector3(playerTrans.position.x, transform.position.y, playerTrans.position.z);

        // Get a random angle in radians (0 to 360 degrees)
        float randomAngle = Random.Range(0f, 2f * Mathf.PI);

        // Calculate the position on the circle using trigonometry
        // x = cx + r * cos(angle)
        // z = cz + r * sin(angle)
        float x = playerCenterPos.x + wanderCircleRadius * Mathf.Cos(randomAngle);
        float z = playerCenterPos.z + wanderCircleRadius * Mathf.Sin(randomAngle);

        wanderTargetPoint = new Vector3(x, transform.position.y, z);

        // Optional: Draw a debug line to see the chosen path
        // Debug.DrawLine(transform.position, wanderTargetPoint, Color.cyan, 3f);
    }
    #endregion

    #region Movement Methods
    /// <summary>
    /// Moves directly away from the player.
    /// </summary>
    public void MoveAwayFromPlayer()
    {
        if (playerTrans == null) return;

        // Direction is from player to rabbit, to move away
        Vector3 moveDir = (transform.position - playerTrans.position).normalized;
        moveDir.y = 0; // Ensure movement is only on the horizontal plane

        transform.position += moveDir * moveSpd * Time.deltaTime;
    }
    #endregion



}

