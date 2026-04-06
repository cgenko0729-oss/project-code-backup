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

public class EnemySpawnerSideSurrounder : EnemySpawnerBase
{

    public int sideSpawnCount = 21;
    public float sideSpawnDist = 14f;
    public float sideSpawnSpacing = 2.0f;

    

    protected override void Spawn()
    {
        Vector3 playerPos = playerTrans.position;
        Vector3 worldRight = Vector3.right;   
        Vector3 worldForward = Vector3.forward;

        Vector3 leftLineCenter = playerPos - worldRight * sideSpawnDist;
        Vector3 rightLineCenter = playerPos + worldRight * sideSpawnDist;

        for (int i = 0; i < sideSpawnCount; i++)
        {
            float offsetMagnitude = (i - (sideSpawnCount - 1) / 2.0f) * sideSpawnSpacing;
            Vector3 spawnOffset = worldForward * offsetMagnitude;

            Vector3 leftSpawnPos = leftLineCenter + spawnOffset;
            Vector3 rightSpawnPos = rightLineCenter + spawnOffset;

            Vector3 leftEnemyTarget = rightSpawnPos;
            Vector3 rightEnemyTarget = leftSpawnPos;

            GameObject leftEnemyObj = enemyPool.GetObject(leftSpawnPos);
            SetupSpawnedEnemy(leftEnemyObj, leftEnemyTarget);

            GameObject rightEnemyObj = enemyPool.GetObject(rightSpawnPos);
            SetupSpawnedEnemy(rightEnemyObj, rightEnemyTarget);
        }

    }

    private void SetupSpawnedEnemy(GameObject enemyObj, Vector3 targetPoint)
    {
        EnemyActionBase enemyMove = enemyObj.GetComponent<EnemyActionBase>();
        enemyMove.targetPoint = targetPoint;
        enemyMove.isResetRotation = true;

        //EnemyStatusBase enemyStatus = enemyObj.GetComponent<EnemyStatusBase>();
        //enemyStatus.enemyMaxHp = enemySpawnHp;
        
        enemyManager.AddEnemyCount(enemyType);
    }

    protected override void DifficultyHandler()
    {
        
    } 



}

