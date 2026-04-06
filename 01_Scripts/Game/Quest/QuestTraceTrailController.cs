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

public class QuestTraceTrailController : MonoBehaviour
{

    public Vector3 questTargetPos = new Vector3(0, 0, 0);
    public Transform playerTrans;

    public float homingSpeed = 14.9f;

    public float lifeCnt = 14.9f;

    void Start()
    {
        playerTrans = GameObject.FindWithTag("Player").transform;
        //questTargetPos = GameObject.FindWithTag("QuestTarget").transform.position;
        transform.position = playerTrans.position + new Vector3(0, 1.5f, 0);

        //// LookAtÇ≈èÌÇ…ÉvÉåÉCÉÑÅ[ÇÃï˚Çå¸Ç≠
        //transform.LookAt(playerTrans);

    }

    void Update()
    {
        //homing toward quest target
        transform.position = Vector3.MoveTowards(transform.position, questTargetPos, homingSpeed * Time.deltaTime);


        lifeCnt -= Time.deltaTime;
        if (lifeCnt < 0)
        {
            Destroy(gameObject);
           
        }

    }
}

