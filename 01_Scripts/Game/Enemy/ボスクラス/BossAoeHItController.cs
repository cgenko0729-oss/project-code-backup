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

public class BossAoeHItController : MonoBehaviour
{

    public float projectileDamage = 10f;
    public float aoeDamageInterval = 2f;

    void Start()
    {
        
    }

    void Update()
    {
        aoeDamageInterval -= Time.deltaTime;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (aoeDamageInterval <= 0f)
            {
                EventManager.EmitEventData("ChangePlayerHp", -projectileDamage);
                aoeDamageInterval = 2f; 
                
            }


        }

    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (aoeDamageInterval <= 0f)
            {
                EventManager.EmitEventData("ChangePlayerHp", -projectileDamage);
                aoeDamageInterval = 2f;

            }
        }
    }

}

