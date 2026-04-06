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

public class EnemySpawnerMover : EnemySpawnerBase
{
     protected override void Spawn()
    {
        if(enemyType == EnemyType.Mover && TimeManager.Instance.gameTimeLeft < 350 && !GameManager.Instance.isBossFight)
        {
            float targetNumber = enemyManager.GetSpawnerValuesForEnemy(enemyType, TimeManager.Instance.gamePhrase).MaxNum;
            float currentNumber = enemyManager.moverNum;
            float ratio = (targetNumber > 0) ? (currentNumber / targetNumber) : 0;
            spawnCooldownScaler =  ratio;
            spawnNumberScaler = 1f - ratio;
            spawnCoolDown = spawnCooldownMax * spawnCooldownScaler;
        }

        if (isFixCooldown) spawnCoolDown = fixCooldown;

        float countf = Random.Range(minEnemies, maxEnemies + 1);
        int count = (int)(countf * (1 + spawnNumberScaler));

        float angleStep = 360f / count;
        float startAngle = Random.Range(0f, angleStep);
        float jitter = angleStep * 0.2f;

      
        int timePhase = TimeManager.Instance.gamePhrase;
        //debug cooldown, count + Time Phase: {timePhase}, also minenemies, maxenemies
        if (enemyType == EnemyType.Bomber || enemyType == EnemyType.Caster)Debug.Log($"Spawn Cooldown: {spawnCoolDown}, Count: {count}, Time Phase: {timePhase}, MinEnemies: {minEnemies}, MaxEnemies: {maxEnemies}, Scaler: {spawnNumberScaler}");


        for (int i = 0; i < count; ++i)
        {
            float angleDeg = startAngle + i * angleStep + Random.Range(-jitter, jitter);
            float angleRad = angleDeg * Mathf.Deg2Rad;
            float radius = Random.Range(minSpawnDist, maxSpawnDist);

            Vector3 dir = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad));
            Vector3 pos = playerTrans.position + dir * radius;

            GameObject enemyObj = enemyPool.GetObject(pos);

            if (enemyType == EnemyType.Mover)
            {
                //EnemyStatusBase enemyStatus = enemyObj.GetComponent<EnemyStatusBase>();
                //enemyStatus.enemyMaxHp = enemySpawnHp;
            }
            
            enemyManager.AddEnemyCount(enemyType);

            if(enemyType == EnemyType.Bomber || enemyType == EnemyType.Caster) Debug.Log($"Spawned {enemyType} at {pos}");
            

        }

    }

    protected override void DifficultyHandler()
    {
        
        //if (type == EnemyType.Boss || type == EnemyType.MidBoss || type == EnemyType.NoEnemy || type == EnemyType.Slime) return;

        SpawnerValues currentValues = enemyManager.GetSpawnerValuesForEnemy(enemyType, TimeManager.Instance.gamePhrase);

        // Update the spawner's local variables with the data from the ScriptableObject
        this.minEnemies = currentValues.MinEnemiesToSpawn;
        this.maxEnemies = currentValues.MaxEnemiesToSpawn;
        this.spawnCooldownMax = currentValues.SpawnCooldown;


        


    }

}

