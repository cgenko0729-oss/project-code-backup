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

public class TraitIceExplosion : MonoBehaviour
{
    Transform playerTrans;
    public float RANGE = 7f;

    void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;

        IceExplosion();

    }

    void Update()
    {
        
    }

    void IceExplosion()
    {
        int enemyMask = LayerMask.GetMask("EnemySpider","EnemyMage","EnemyDragon","EnemyMushroom","EnemyBat");

        Collider[] hits = Physics.OverlapSphere(playerTrans.position, RANGE, enemyMask);
        if (hits.Length == 0) return;

        //loop all enemies in range
        foreach (Collider hit in hits)
        {
            EnemyStatusBase enemy = hit.GetComponent<EnemyStatusBase>();
            if (enemy != null)
            {
                enemy.ApplyIceDebuff();
            }
        }



    }

}

