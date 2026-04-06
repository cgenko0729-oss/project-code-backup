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

public class EnemyMidBossSpawner : MonoBehaviour
{
    public GameObject midBossObject;

    public float spawnCnt = 210f;
    public float spawnCntMax = 210f;

    public float spawnDistanceAwayFromPlayer = 35f;
    public Transform playerTrans;

    public bool isBossSpawned = false;
    public bool canMultiSpawn = false;

    public bool hasWarning = true;

    private void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        spawnCnt -= Time.deltaTime;

        if(isBossSpawned && !canMultiSpawn) return;

        if (spawnCnt <= 0f)
        {
            isBossSpawned = true;
            SpawnMidBoss();
            spawnCnt = spawnCntMax; // Reset the spawn count
        }
    }

    //spawn mid boss around player around 35f dist 
    private void SpawnMidBoss()
    {
        Vector3 spawnPosition = playerTrans.position + (Random.onUnitSphere * spawnDistanceAwayFromPlayer);
        spawnPosition.y = 0f; // Ensure the mid boss spawns at ground level

        GameObject midBoss = Instantiate(midBossObject, spawnPosition, Quaternion.identity);

        if(hasWarning)EffectManager.Instance.DisplayMidBossWarning();

    }



}

