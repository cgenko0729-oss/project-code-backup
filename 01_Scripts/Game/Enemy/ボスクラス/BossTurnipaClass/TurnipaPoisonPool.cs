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

public class TurnipaPoisonPool : MonoBehaviour
{

    public float poisonCnt = 0.2f;

    private void Update()
    {
        poisonCnt -= Time.deltaTime; // 毒のカウントダウン

    }

    public void OnCollisionEnter(Collision collision)
    {
        if(poisonCnt <= 0f)
        {
            if (collision.gameObject.CompareTag("Player"))
            {             
                poisonCnt = 1f;
                //EventManager.EmitEvent("PlayerHitWeb");
                ItemManager.Instance.pickUpSpdUpWing = true;
                ItemManager.Instance.spdUpAmount = 0.4f;
                ItemManager.Instance.spdUpTime = 3f;
                //EffectManager.Instance.CreatePlayerPoisonEffect();
                EffectManager.Instance.CreatePlayerWebSlowEffect();
            }

        }
        
    }
}

