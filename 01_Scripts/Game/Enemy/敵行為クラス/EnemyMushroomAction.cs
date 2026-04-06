using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class EnemyMushroomAction : EnemyActionBase
{

    public float standStillCnt = 4f;
    public float stadnStillCntMax = 4f;
    public float standStillSpeed = 0.1f;
    public float homingMoveSpeed;

    public float spawnWallColliderCnt = 0.1f;
    public List<GameObject> surroundWallCollideObjList = new List<GameObject>();

    protected override void Start()
    {
        InitSeperationInfo();
        GetPlayerInfo();
        standStillCnt = stadnStillCntMax;

    }

    public void OnEnable()
    {
        standStillCnt = stadnStillCntMax;

        //FaceTargetPointNow();
    }

    public void OnDisable()
    {
        foreach (var obj in surroundWallCollideObjList)
        {
            if(obj!= null && obj.activeInHierarchy)
            {
                ObjectLifeController life = obj.GetComponent<ObjectLifeController>();
                if(life != null)life.ReleaseToObjectPool();
            }
        }
        if(surroundWallCollideObjList != null)surroundWallCollideObjList.Clear();
    }

    protected override void Update()
    {
        standStillCnt -= Time.deltaTime;

        if (standStillCnt <= 0f) enemyStatus.enemyMoveSpd = standStillSpeed;
        else enemyStatus.enemyMoveSpd = homingMoveSpeed;

        EnemyFollowTargetPoint();       
        EnemySeparation();
        
        //EnemyRotationTargetPoint();
        
        if (isResetRotation)
        {
            isResetRotation = false;
            FaceTargetPointNow();
        }

        spawnWallColliderCnt -= Time.deltaTime;
        if(spawnWallColliderCnt <= 0f)
        {
            spawnWallColliderCnt = 2.1f;
            GameObject obj = EnemyManager.Instance.SpawnSurroundWallObj(transform.position, transform.rotation);
            surroundWallCollideObjList.Add(obj);
        }

    }


}

