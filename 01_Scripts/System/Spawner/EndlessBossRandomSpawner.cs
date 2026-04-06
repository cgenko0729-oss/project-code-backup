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

public class EndlessBossRandomSpawner : MonoBehaviour
{
    public List<GameObject> bossObjList;

    public float spawnCnt = 7f;

    public Transform playerTrans;

    public int lastSpawnIndex = -1;

    public int spawnTime = 0;

    void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;

    }

    void Update()
    {
        if (TimeManager.Instance.isEndlessTimeStarted)
        {
            spawnCnt -= Time.deltaTime;

            if(spawnCnt <= 0)
            {
                SpawnBoss();
                spawnCnt = 91f;
            }

        }
        
    }

    void SpawnBoss()
    {
        if (spawnTime >= 9) return;
        spawnTime++;
        int randomIndex = Random.Range(0, bossObjList.Count);

        while(randomIndex == lastSpawnIndex)
        {
            randomIndex = Random.Range(0, bossObjList.Count);
        }

        lastSpawnIndex = randomIndex;

        GameObject bossObj = bossObjList[randomIndex];

        Vector3 spawnPos;
        float distWithPlayer = 14f;
        spawnPos = playerTrans.position + (Vector3)(Random.insideUnitCircle.normalized * distWithPlayer);

        spawnPos.y = 0.14f; //ínñ ÇÃçÇÇ≥Ç…çáÇÌÇπÇÈ

        GameObject newBoss = Instantiate(bossObj, spawnPos, Quaternion.identity);
        EffectManager.Instance.DisplayMidBossWarning();
    }

}

