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

public class QuestGhostSPawner : MonoBehaviour
{

    public GameObject ghostObj;

    public float distWithPlayerToStartSpawning = 7.7f;
    public float distWithPlayer = 10f;

    public float spawnCnt = 3f;
    public float spawnRadius = 7f;
    public float spawnCntMax = 2.8f;

    public float distWithPlayerRequired = 4.2f;
    public float distWithPreviousGhostRequired = 4.2f;
    public Vector3 previousGhostPos= Vector3.zero;

    public Transform playerTrans;

    void Start()
    {
       playerTrans = GameObject.FindGameObjectWithTag("Player").transform;

    }

    void Update()
    {

        distWithPlayer = Vector3.Distance(transform.position, playerTrans.position);
        if (distWithPlayer > distWithPlayerToStartSpawning) return;
      

        spawnCnt -= Time.deltaTime;

        if (spawnCnt < 0 && spawnCntMax > 0)
        {
            Vector3 spawnPos = transform.position;
            int tryCnt = 0;
            do
            {
                float angle = Random.Range(0, Mathf.PI * 2);
                float radius = Random.Range(0, spawnRadius);
                spawnPos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius) + transform.position;
                tryCnt++;
                if (tryCnt > 70)
                {
                    break;
                }
            } while (Vector3.Distance(spawnPos, playerTrans.position) < distWithPlayerRequired
            || Vector3.Distance(spawnPos, previousGhostPos) < distWithPreviousGhostRequired);

            Instantiate(ghostObj, spawnPos, Quaternion.Euler(0, 0, 0));
            previousGhostPos = spawnPos;
            spawnCnt = spawnCntMax;
            
        }

    }
}

