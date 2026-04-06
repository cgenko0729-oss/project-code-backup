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


public class BossHomingBullet : MonoBehaviour
{

    private Transform playerTarget;

    private float speed = 4f;
    [SerializeField] private float turnSpeed = 5f;

    public float distToPlayer;
    public bool closeToPlayer = false;
    public float closeHomingTimer = 3.5f;

    public GameObject endExplosionObj;

    public void SetSpeed(float newSpeed) => speed = newSpeed; 

    private void Awake()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public async void LaunchSequence(Vector3 formationPosition, float formationMoveTime, float postFormationDelay, float homingDuration)
    {
        try
        {
            await transform.DOMove(formationPosition, formationMoveTime).SetEase(Ease.OutQuad).ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

            await UniTask.Delay(System.TimeSpan.FromSeconds(postFormationDelay), cancellationToken: this.GetCancellationTokenOnDestroy());

            float homingTimer = homingDuration;
            while (homingTimer > 0)
            {
                Vector3 directionToTarget = ((playerTarget.position + new Vector3(0,1.5f,0)) - transform.position).normalized;

                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

                transform.position += transform.forward * speed * Time.deltaTime;

                homingTimer -= Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: this.GetCancellationTokenOnDestroy());

            }

            Vector3 finalDirection = transform.forward;
            float nonHomingDuration = 2.1f;
            float nonHomingTimer = nonHomingDuration;

            while (nonHomingTimer > 0)
            {
                transform.position += finalDirection * speed * Time.deltaTime;
                nonHomingTimer -= Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            Deactivate();
        }
        catch (OperationCanceledException)
        {
            
        }
        

    }

    private void Deactivate()
    {

        //cancel all unitask 
       

        transform.DOKill();
        Destroy(gameObject);
        
        GameObject explosion = Instantiate(endExplosionObj, transform.position, Quaternion.identity);
        Destroy(explosion, 2f); // 爆発エフェクトを2秒後に削除
        
    }

    private void Update()
    {
        distToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        if (distToPlayer < 4.9f) closeToPlayer = true;
    }

    //OnTriggerEnter with player tag
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Deactivate();
            EventManager.EmitEventData(GameEvent.ChangePlayerHp, -20f);
        }
    }


}

