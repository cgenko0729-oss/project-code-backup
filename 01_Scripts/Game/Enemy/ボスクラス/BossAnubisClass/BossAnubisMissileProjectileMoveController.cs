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

public class BossAnubisMissileProjectileMoveController : MonoBehaviour
{

    public float lifeCnt = 4f;

    

    void Start()
    {
        
    }

    void Update()
    {
        lifeCnt -= Time.deltaTime;
        if (lifeCnt <= 0f)
        {
            Destroy(this.gameObject);
        }



    }
}

