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

public class QuestLaserController : MonoBehaviour
{

    public float laserLength = 14f;

    public Rigidbody rb;

    public bool isActivated = false;

    public float addProgressAmount = 51f;

    public AudioClip finishSE;

    void Start()
    {
        //rb = GetComponent<Rigidbody>();

    }

    void Update()
    {

        Debug.DrawRay(transform.position, transform.forward * laserLength, Color.red);

        if (isActivated) return;

        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit))
        {
            if (hit.collider.CompareTag("QuestTargetObj"))
            {
                rb.isKinematic = true;
                isActivated = true;
                EventManager.EmitEventData("AddQuestProgress", addProgressAmount);
                SoundEffect.Instance.PlayOneSound(finishSE,2.1f);
            }


        }

      

    }

}

