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

public abstract class EnemySpawnerBase : MonoBehaviour
{
    public ObjectPool enemyPool; 
    public EnemyType enemyType; 
    public SpawnMethod spawnMethod;

    public float spawnCoolDown;
    public float spawnCooldownMax = 1f;
    public float spawnCooldownScaler = 100f;

    public int minEnemies = 2;
    public int maxEnemies = 6;
    public float spawnNumberScaler = 1f;

    public float minSpawnDist = 21f;
    public float maxSpawnDist = 28f;

    public float enemySpawnHp = 100;

    protected Transform playerTrans;
    protected PlayerState playerStatus;
    protected EnemyManager enemyManager;
    protected GameManager gameManager;

    public bool isFixCooldown = false;
    public float fixCooldown = 280f;

    public int historySpawnTime = 0;

    protected virtual void Start()
    {
        if (playerTrans == null) playerTrans = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTrans != null) playerStatus = playerTrans.GetComponent<PlayerState>();
        enemyManager = EnemyManager.Instance;
        gameManager = GameManager.Instance;
    }

    protected virtual void Update()
    {
        if (playerTrans == null || playerStatus == null || enemyManager == null) return;

        spawnCoolDown -= Time.deltaTime;

        DifficultyHandler();

        if (spawnCoolDown <= 0f)
        {
            if (!enemyManager.CheckEnemyNumLimit(enemyType)) return;           
            spawnCoolDown = spawnCooldownMax;
            Spawn();
            historySpawnTime++;
        }

    }

    protected abstract void Spawn();

    protected abstract void DifficultyHandler();
    

}

/*
    protected override void Spawn()
    {
       

    }

    protected override void DifficultyHandler()
    {
        
    } 

 */

