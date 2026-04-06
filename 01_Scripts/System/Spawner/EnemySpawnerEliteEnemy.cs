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

public class EnemySpawnerEliteEnemy : EnemySpawnerBase
{
    protected override void Spawn()
    {

        float radius = Random.Range(minSpawnDist, maxSpawnDist);
        Vector3 dir = new Vector3(1, 0f, 1);
        Vector3 pos = playerTrans.position + dir * radius;

        GameObject enemyObj = enemyPool.GetObject(pos);

        enemyManager.AddEnemyCount(enemyType);
    }

    protected override void DifficultyHandler()
    {

    }
}

