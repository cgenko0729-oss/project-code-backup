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

public class TraitDashPushBackController : MonoBehaviour
{

    public PlayerController playerController;
    public Transform playerTrans;

    public float finalPushRange = 4.2f;
    public float finalPushDetectRange = 4.9f;
    public float finalPushDuration = 0.28f;

    public float onTheWayPushRange = 3.5f;
    public float onTheWayPushDuration = 0.14f;

    public bool iswayPushEnabled = false;
    public GameObject PushBackEffect;


    private void OnEnable()
    {
        EventManager.StartListening(GameEvent.PlayerDashEnd, PushBackAllEnemyWithinRange);
    }

    private void OnDisable()
    {
        EventManager.StopListening(GameEvent.PlayerDashEnd, PushBackAllEnemyWithinRange);
    }


    void Start()
    {
        playerTrans = GameObject.FindWithTag("Player").transform;
        playerController = playerTrans.GetComponent<PlayerController>();

    }

    void Update()
    {
        
    }

    public void PushBackAllEnemyWithinRange()
    {
        Instantiate(PushBackEffect, playerTrans.position, Quaternion.identity);

        //cast a sphere with 7.7 radius to find all enemies within range
        Collider[] hitColliders = Physics.OverlapSphere(playerTrans.position, finalPushDetectRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                EnemyStatusBase enemyStatusBase = hitCollider.GetComponent<EnemyStatusBase>();
                enemyStatusBase.DynamicPushBackEnemyItselfAwayFromPlayer(finalPushRange, finalPushDuration);
                enemyStatusBase.TakeDamage(30f);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if(playerController.isDashing == false) return;
            if (iswayPushEnabled == false) return;
            EnemyStatusBase enemyStatusBase = other.GetComponent<EnemyStatusBase>();
            enemyStatusBase.DynamicPushBackEnemyItselfAwayFromPlayer(onTheWayPushRange,onTheWayPushDuration);

        }
    }

}

