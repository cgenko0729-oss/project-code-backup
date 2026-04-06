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

public class EnemySpawnerFlyer : EnemySpawnerBase
{

    public float slashDistToPlayer = 25f;
    public float slashSpacing = 2.5f;

    public AudioClip spawnFlyerSe;

    protected override void Spawn()
    {
        if (historySpawnTime % 5 == 0 && historySpawnTime != 0)
        {
            slashSpacing = 2.8f;
        }
        else slashSpacing = 0f;

        Vector3 playerPos = playerTrans.position;
        Vector3 topLeftDir = (-playerTrans.right + playerTrans.forward).normalized;
        Vector3 spawnOrigin = playerPos + topLeftDir * slashDistToPlayer;
        Vector3 bottomRightDir = (playerTrans.right - playerTrans.forward).normalized;
        Vector3 moveTarget = playerPos + bottomRightDir * slashDistToPlayer;
        
        int count = Random.Range(minEnemies, maxEnemies + 1);
        Vector3 rightwardOffset = Vector3.Cross(Vector3.up, (moveTarget - spawnOrigin).normalized);

        if(spawnFlyerSe) SoundEffect.Instance.PlayOneSound(spawnFlyerSe,0.42f);

        for (int i = 0; i < count; ++i)
        {
           
            Vector3 offset = rightwardOffset * (i - (count - 1) / 2f) * slashSpacing;
            Vector3 spawnPos = spawnOrigin + offset;

            GameObject enemyObj = enemyPool.GetObject(spawnPos);
            
            EnemyActionBase enemyMove = enemyObj.GetComponent<EnemyActionBase>();
            enemyMove.targetPoint = moveTarget;
            enemyMove.isResetRotation = true;

            //EnemyStatusBase enemyStatus = enemyObj.GetComponent<EnemyStatusBase>();
            //enemyStatus.enemyMaxHp = enemySpawnHp;

            enemyManager.AddEnemyCount(enemyType);
        }

    }

    protected override void DifficultyHandler()
    {

        //if (enemyType == EnemyType.Flyer)
        //{
        //    if(TimeManager.Instance.gamePhrase >= 5)
        //    {
        //        minEnemies = 42;
        //        maxEnemies = 42;
        //    }

        //}

        SpawnerValues currentValues = enemyManager.GetSpawnerValuesForEnemy(enemyType, TimeManager.Instance.gamePhrase);

        // Update the spawner's local variables with the data from the ScriptableObject
        this.minEnemies = currentValues.MinEnemiesToSpawn;
        this.maxEnemies = currentValues.MaxEnemiesToSpawn;
        this.spawnCooldownMax = currentValues.SpawnCooldown;

        
    } 


}

