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

public class EnemySpawnerCircleSurrounder : EnemySpawnerBase
{

    public int circleMinEnemies = 8;
    public int circleMaxEnemies = 16;
    public float circleRadius = 11f;
    public float circleJitter = 0f;
    public bool circleRandomYaw = true;
    
    protected override void Spawn()
    {

        int count = Random.Range(circleMinEnemies, circleMaxEnemies + 1);
        float angleStep = 360f / count;
        float startYaw = circleRandomYaw ? Random.Range(0f, 360f) : 0f;

        for (int i = 0; i < count; ++i)
        {
            float angleDeg = startYaw + i * angleStep;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            float radius = circleRadius + Random.Range(-circleJitter, circleJitter);

            Vector3 dir = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad));
            Vector3 pos = playerTrans.position + dir * radius;
            
            GameObject enemyObj = enemyPool.GetObject(pos);
            EnemyActionBase enemyMove = enemyObj.GetComponent<EnemyActionBase>();
            enemyMove.targetPoint = playerTrans.position;
            enemyMove.isResetRotation = true;
            enemyManager.AddEnemyCount(enemyType);
        }


    }

    protected override void DifficultyHandler()
    {

        //if (enemyType == EnemyType.Surrounder)
        //{
        //    if (TimeManager.Instance.gamePhrase >= 5)
        //    {
        //        spawnCooldownMax = 21f;
        //        return;
        //    }

        //}

        SpawnerValues currentValues = enemyManager.GetSpawnerValuesForEnemy(enemyType, TimeManager.Instance.gamePhrase);

        // Update the spawner's local variables with the data from the ScriptableObject
        this.minEnemies = currentValues.MinEnemiesToSpawn;
        this.maxEnemies = currentValues.MaxEnemiesToSpawn;
        this.spawnCooldownMax = currentValues.SpawnCooldown;

        
    }



}



