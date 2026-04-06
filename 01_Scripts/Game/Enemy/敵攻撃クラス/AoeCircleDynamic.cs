using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class AoeCircleDynamic : MonoBehaviour
{
    
   [Header("References")]
    public GameObject AoeEffectObj;     // Prefab for the effect played on completion
    public Transform outterCicleTran;   // The static outer ring showing max size
    public Transform innerCircleTrans;  // The inner circle that fills up

    [Header("Default AOE Settings")]
    public float startScale = 0.1f;
    public float endScale = 4.0f;     // Default max size
    public float duration = 2f;       // Default fill time
    public float aoeDamage = 10f;     // Default damage

    [Header("Damage & Effect Toggles")]
    public bool isDamageEnemy = true;
    public bool isDamagePlayer = true;
    public bool isSpawnEffect = true;
    public float controlScale = 1.0f; // Scale multiplier for the spawned effect

    // --- Private State ---
    private SphereCollider col;
    private bool isFilling = false; // Controls whether the attack animation is running

     public bool isBlockMove = false;
    public bool isBlockDash = false;
    public bool isBlockCast = false;

    public int effectStyle = 0;
    public GameObject attackEffect1;

    void Awake()
    {
        col = GetComponent<SphereCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
        }
        col.enabled = false;
    }

    void OnEnable()
    {
        // Reset the state each time the object is activated (important for pooling)
        isFilling = false;
        innerCircleTrans.DOKill(); // Stop any leftover animations
        innerCircleTrans.localScale = new Vector3(startScale, startScale, startScale);
        outterCicleTran.localScale = Vector3.one; // Will be properly sized by UpdateTransform
        col.enabled = false;
    }

    // „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ NEW PUBLIC METHODS FOR BOSS SCRIPT „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ

    /// <summary>
    /// PHASE 1: Called continuously during tracking.
    /// Updates the indicator's position and final radius without starting the animation.
    /// </summary>
    /// <param name="position">The world-space position for the center of the AOE.</param>
    /// <param name="radius">The final radius the circle will expand to.</param>
    public void UpdateTransform(Vector3 position, float radius)
    {
        if (isFilling) return; // Do not allow changes once the fill has begun

        transform.position = position;
        this.endScale = radius;

        // Immediately set the outer circle's scale to show the maximum attack range
        outterCicleTran.localScale = new Vector3(endScale, endScale, endScale);
    }

    public void UpdateTransform(Vector3 position)
    {
        //if (isFilling) return; // Do not allow changes once the fill has begun

        transform.position = position;
       
    }

    /// <summary>
    /// PHASE 2: Called once tracking is finished.
    /// Locks the indicator in place and begins the fill/growth animation.
    /// </summary>
    /// <param name="fillDuration">How long the fill animation should take.</param>
    /// <param name="damage">The damage to be dealt on completion.</param>
    /// <param name="effectScale">Optional scale multiplier for the spawned particle effect.</param>
    public void BeginFill(float fillDuration, float damage, float effectScale = 1.0f)
    {
        if (isFilling) return; // Prevent this from being called more than once

        // 1. Set final properties
        this.duration = fillDuration;
        this.aoeDamage = damage;
        this.controlScale = effectScale;
        isFilling = true;

        // 2. Ensure visuals are correct at the start of the fill
        innerCircleTrans.localScale = new Vector3(startScale, startScale, startScale);

        // 3. Start the main animation
        innerCircleTrans.DOScale(endScale, duration)
            .SetEase(Ease.OutQuad) // Added an ease for a smoother look
            .OnComplete(OnFillComplete);
    }

    // „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ Private Logic „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ

    /// <summary>
    /// This function is called by DOTween when the inner circle's animation is complete.
    /// </summary>
    private void OnFillComplete()
    {
        col.enabled = true;
        col.radius = endScale / 2f; // SphereCollider's radius is half of its scale/diameter

        SoundEffect.Instance.Play(SoundList.TraitAfterSkillExplosionSe);
        CheckHitWithTargets();

        if (isSpawnEffect && AoeEffectObj != null)
        {
            // Spawn from a pool if available, otherwise Instantiate
            var effect = ObjectPool.Instantiate(AoeEffectObj, transform.position, Quaternion.identity);

            // You may need a simple script on your effect prefab to handle this
            if (effect.TryGetComponent<EffectScaleControlloer>(out var es))
            {
                es.effectScale = controlScale;
            }
        }

        // Deactivate or destroy the indicator after a short delay
        ObjectPool.Destroy(gameObject, 0.35f);
    }

    /// <summary>
    /// Performs an OverlapSphere check to find and damage valid targets.
    /// </summary>
    private void CheckHitWithTargets()
    {
        Collider[] hitColliders = new Collider[20]; // Using NonAlloc to avoid garbage
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, endScale / 2f, hitColliders);

        if(effectStyle != 0)Instantiate(attackEffect1, transform.position, Quaternion.Euler(0f, 0f, 0f));

        for (int i = 0; i < numColliders; i++)
        {
            var hitCollider = hitColliders[i];

            if (isDamagePlayer && hitCollider.CompareTag("Player"))
            {
                

                if (isBlockMove)
                {
                    CameraShake.Instance.StartShake();
                    EnemyManager.Instance.ApplyCannotMove();
                }
                else if (isBlockDash)
                {
                    CameraShake.Instance.StartShake();
                    EnemyManager.Instance.ApplyCannotDash();
                }
                else if (isBlockCast)
                {
                    CameraShake.Instance.StartShake();
                    EnemyManager.Instance.ApplyCannotCast();
                }

                EventManager.EmitEventData("ChangePlayerHp", -aoeDamage);
            }

            if (isDamageEnemy && hitCollider.CompareTag("Enemy"))
            {
                if (hitCollider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(aoeDamage);
                }
            }
        }
    }

    void OnDisable()
    {
        // Crucial for preventing errors if the object is disabled mid-animation
        innerCircleTrans.DOKill();
    }



    
}

