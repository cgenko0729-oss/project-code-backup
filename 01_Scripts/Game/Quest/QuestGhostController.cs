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

public class QuestGhostController : MonoBehaviour
{

    public GameObject deadPurifyEffect;

    public float lifeCnt = 10f;

    void Start()
    {
        
    }

    void Update()
    {

        //dotween move up and down
        transform.DOMoveY(Mathf.Sin(Time.time) * 0.2f + 1.5f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

        lifeCnt -= Time.deltaTime;
        if (lifeCnt < 0)
        {
            Destroy(gameObject);
            Instantiate(deadPurifyEffect, transform.position, Quaternion.identity);
        }

    }

    //OntriggerEnter with player tag , play purify effect and destroy this ghost
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.EmitEvent("QuestGhostGetPurify");
            Instantiate(deadPurifyEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

}

