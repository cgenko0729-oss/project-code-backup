using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class AoeIndicator : MonoBehaviour
{
   [Header("Timing")]
    [SerializeField] private float windUpSeconds = 2f;      // visible warning phase
    [SerializeField] private float lingerSeconds = 0.3f;    // small linger after hit

    [Header("Damage")]
    [SerializeField] private float damageRadius = 5f;
    [SerializeField] private int damageAmount = 25;
    [SerializeField] private LayerMask playerLayer;         // layer of hurtables
    [SerializeField] private bool useNonAlloc = true;

    [Header("References")]
    [SerializeField] private MeshRenderer innerFillRenderer;
    private Material innerMatInstance;

    // Cache – non-alloc overlap
    private static readonly Collider[] hitBuffer = new Collider[8];

    float timer;

    void Awake()
    {
        // Always clone material so each instance has its own _Progress value
        innerMatInstance = innerFillRenderer.material;
        timer = windUpSeconds + lingerSeconds;
    }

    void OnEnable()
    {
        timer = windUpSeconds + lingerSeconds;
        SetProgress(0f);           // start empty
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer > lingerSeconds)               // still charging
        {
            float progress = 1f - (timer - lingerSeconds) / windUpSeconds;
            SetProgress(progress);
        }
        else if (timer + Time.deltaTime >= lingerSeconds)   // JUST hit
        {
            Hit();
            SetProgress(1f);   // full, optional flash
        }
        else if (timer <= 0f)                           // finished
        {
            Recycle();                                  // return to pool
        }
    }

    void SetProgress(float value)
    {
        // Shader uses _Progress (0-1) to radial-wipe
        innerMatInstance.SetFloat("_Progress", value);
    }

    void Hit()
    {
        int hits;
        if (useNonAlloc)
        {
            hits = Physics.OverlapSphereNonAlloc(transform.position, damageRadius, hitBuffer, playerLayer);
            for (int i = 0; i < hits; i++)
                hitBuffer[i].GetComponent<IDamageable>()?.TakeDamage(damageAmount);
        }
        else
        {
            foreach (var col in Physics.OverlapSphere(transform.position, damageRadius, playerLayer))
                col.GetComponent<IDamageable>()?.TakeDamage(damageAmount);
        }
    }

    void Recycle()
    {
        // If using a pooling lib, replace with Pool.Release(this);
        gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    // Nice gizmo for designers
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
#endif
}

