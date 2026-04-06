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

public class PushTargetHandler : MonoBehaviour
{
    public Transform targetTrans;

    public float distToTarget = 100f;

    public float distNeedToArrive = 7f;

    public bool isTargetArrived = false;

    public Rigidbody rb;

    public AudioClip finishSe;
    public GameObject finishEffectObj;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (targetTrans == null) Debug.LogError("No target assigned for PushTargetHandler");

    }

    void Update()
    {

        distToTarget = Vector3.Distance(transform.position, targetTrans.position);

        if(distToTarget <= distNeedToArrive  && !isTargetArrived)
        {
            isTargetArrived = true;
            EventManager.EmitEvent(GameEvent.PushObjectArriveTarget);

            //disable rigidbody to prevent further movement
            Invoke("DisableRigidbody", 0.35f);

        }

    }

    void DisableRigidbody()
    {
        rb.isKinematic = true;

        if(finishSe)SoundEffect.Instance.PlayOneSound(finishSe, 2.8f);

        //instantiate finish effect
        if (finishEffectObj != null) Instantiate(finishEffectObj, transform.position, Quaternion.identity);

    }

}

