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

public class ObjectFacePlayer : MonoBehaviour
{
    Transform playerTrans;

    public Vector3 faceRotationOffset = new Vector3(-90, 0, 0);

    private void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        
    }

    private void Update()
    {
        //Vector3 direction = (playerTrans.position - transform.position).normalized;
        //Quaternion lookRotation = Quaternion.LookRotation(direction);
        //transform.rotation = lookRotation;

        //rotation + faceRotationOffset
        Vector3 direction = (playerTrans.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation * Quaternion.Euler(faceRotationOffset);


    }

}

