using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class EnemyBatAction : EnemyActionBase
{

    public float distToTargetPoint = 10f;


    private void OnEnable()
    {
        
    }

    protected override void Start()
    {
        InitSeperationInfo();
        GetPlayerInfo();

        FaceTargetPointNow();
        

    }

    protected override void Update()
    {
    

        EnemyFollowTargetPoint();
        EnemySeparation();
        //EnemyRotationTargetPoint();

        if (isResetRotation)
        {
            isResetRotation = false;
            FaceTargetPointNow();
        }

        if(enemyStatus.enemyLifeCnt < 3f)
        {
            distToTargetPoint = Vector3.Distance(transform.position, targetPoint);
            if (distToTargetPoint < 3f) enemyStatus.DeadNoExp();
        }
        

    }
}

