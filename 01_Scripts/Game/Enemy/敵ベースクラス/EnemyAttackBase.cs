using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool

public class EnemyAttackBase : MonoBehaviour
{

    [Header("Attack Settings")]
    [SerializeField] private float attackDistNeeded = 1.0f;

    [Header("Cooldowns")]
    [SerializeField] private float attackCooldownMax = 3f;
    [SerializeField] private float attackTurnInterval = 0.7f;

    [Header("Burst")]
    [SerializeField] private int attackTurnCount = 1;

    [Header("Pooling")]
    [SerializeField] private ObjectPool attackObjPool;

    // internal state
    private float _attackCooldownTimer;
    private bool  _isAttacking;
    private WaitForSeconds _turnDelayYield;


    void Awake()
    {
        _attackCooldownTimer = attackCooldownMax;
        _turnDelayYield       = new WaitForSeconds(attackTurnInterval);
    }

    void Start()
    {
         
        

    }

    void Update()
    {
        if (_isAttacking) return;

        _attackCooldownTimer -= Time.deltaTime;

        if (_attackCooldownTimer <= 0f)
        {
            StartCoroutine(AttackSequence());
        }
    }

    private IEnumerator AttackSequence()
    {
        _isAttacking = true;

        for (int i = 0; i < attackTurnCount; i++)
        {
            TrySpawnAttack();
            yield return _turnDelayYield;
        }

        // reset full-sequence cooldown
        _attackCooldownTimer = attackCooldownMax;
        _isAttacking          = false;
    }

    private void TrySpawnAttack()
    {
        // if (Vector3.Distance(transform.position, target.position) > attackDistNeeded) return;

        Quaternion rot = Quaternion.LookRotation(transform.forward);
        //attackObjPool.GetObject(transform.position, rot);
        attackObjPool.GetObjectComponent<EffectMoveController>(transform.position, rot).moveVec = transform.forward;
    }

}

