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

public class PlayerDirectionIndicator : MonoBehaviour
{
   
    Transform playerTrans;

    Vector3 playerForwardDirection;

    private void Start()
    {
       playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
       

    }

     void Update()
    {
        playerForwardDirection = playerTrans.forward;

        //rotate this rotation to player forward direction
        transform.rotation = Quaternion.LookRotation(playerForwardDirection, Vector3.up);
        //transform.rotation = Quaternion.Euler(0, playerTrans.eulerAngles.y, 0); // これでもOK

        transform.position = playerTrans.position + new Vector3(0, 0.5f, 0); // プレイヤーの位置に少し上げて表示

    }



}

