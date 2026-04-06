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

public class EndlessMidBossSpawner : MonoBehaviour
{

    public List<GameObject> allPossibleMidBossPrefabs;

    public float spawnCnt = 7f;

    void Start()
    {
        
    }

    void Update()
    {
        if(StageManager.Instance.isEndlessMode == false) return;

        if (TimeManager.Instance.isEndlessFinalPhaseStarted)
        {
            spawnCnt -= Time.deltaTime;
            if (spawnCnt <= 0f)
            {
                SpawnRandomMidBoss();
                spawnCnt = 120f; // Reset the spawn count
            }
        }
        
    }

    public void SpawnRandomMidBoss()
    {
        if (allPossibleMidBossPrefabs.Count == 0) return;
        int randomIndex = Random.Range(0, allPossibleMidBossPrefabs.Count);
        GameObject midBossPrefab = allPossibleMidBossPrefabs[randomIndex];
        Vector3 spawnPosition = new Vector3(Random.Range(-50f, 50f), 0f, Random.Range(-50f, 50f));
        Instantiate(midBossPrefab, spawnPosition, Quaternion.identity);
        EffectManager.Instance.DisplayMidBossWarning();

    }
}

