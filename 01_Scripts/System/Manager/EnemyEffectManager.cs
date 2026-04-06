using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class EnemyEffectManager : Singleton<EnemyEffectManager>
{

    public GameObject aoeCircleObj;

    public ObjectPool aoeCirclePool;

    public void SpawnAoeCircle(Vector3 pos,float radiuSize, float fillDuration,float damage = 15f, bool isBlockMove = false, bool isBlockDash = false, bool isBlockCast = false)
    {
        AoeCircle aoe = aoeCirclePool.GetObjectComponent<AoeCircle>(pos);
        aoe.InitCircle(radiuSize, fillDuration, damage);

        if(isBlockMove) aoe.isBlockMove = true;
        if (isBlockDash) aoe.isBlockDash = true;
        if (isBlockCast) aoe.isBlockCast = true;

        //AoeCircle circle = Instantiate(aoeCircleObj, pos, Quaternion.Euler(0f, 0f, 0f)).GetComponent<AoeCircle>();
        //circle.InitCircle(radiuSize, fillDuration, damage);

    }

   

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

