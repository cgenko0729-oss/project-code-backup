using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class EnemySpiderDenBlock : MonoBehaviour
{

    public int blockDenId; // スパイダーデンのID
    BossSpiderBattleField bossBattleField;

    public void OnEnable()
    {
        EventManager.StartListening("SpiderDenDestroy", DisableBlockWall);
    }
    public void OnDisable()
    {
        EventManager.StopListening("SpiderDenDestroy", DisableBlockWall);
    }

    public void Awake()
    {
        bossBattleField = GetComponentInParent<BossSpiderBattleField>();
    }

    public void DisableBlockWall()
    {
        int denId = EventManager.GetInt("SpiderDenDestroy");
        if(denId == blockDenId)
        {
            Destroy(gameObject);
            
        }

    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

