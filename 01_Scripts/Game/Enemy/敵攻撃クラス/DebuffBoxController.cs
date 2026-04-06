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

public class DebuffBoxController : MonoBehaviour
{
    float explodeTimer = 1.4f;

    public GameObject explodeEffectObj;

    public ObjectFollow follow;

    float stopMoveTimer = 3.5f;
    public bool isStopMove = false;

    void Start()
    {
        explodeTimer = 3.5f;

        follow = GetComponent<ObjectFollow>();
    }

    void Update()
    {
        stopMoveTimer -= Time.deltaTime;
        explodeTimer -= Time.deltaTime;

        if(stopMoveTimer <= 0 && !isStopMove)
        {
            isStopMove = true;
            follow.enabled = false;
        }

        if (explodeTimer <= 0f)
        {

            transform.DOMoveY(0f, 1.4f).SetEase(Ease.Linear).OnComplete(() => {
                Instantiate(explodeEffectObj, transform.position,Quaternion.identity); //爆発エフェクト再生
                Destroy(gameObject);

            });

        }

    }
}

