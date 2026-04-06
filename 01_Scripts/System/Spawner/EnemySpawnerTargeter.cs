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

public class EnemySpawnerTargeter : EnemySpawnerBase
{
    protected override void Spawn()
    {
        //GameObject enemy = enemyPool.GetObject(targetPoint);
        //EnemyManager.Instance.AddEnemyCount(enemyType);
    }

    protected override void DifficultyHandler()
    {
        
    } 


}

