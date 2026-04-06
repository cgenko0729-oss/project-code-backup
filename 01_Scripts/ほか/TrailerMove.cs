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

public class TrailerMove : MonoBehaviour
{

    public float moveSpeed = 5f; // 移動速度

    public float waitTime = 2f; // 待機時間
    public bool isActive = true; // 移動中かどうかのフラグ
    void Start()
    {
        
    }

    void Update()
    {
        //if press T key, toggle isActive
        if (Input.GetKeyDown(KeyCode.T))
        {
            isActive = true;
        }


        //keep move toward right
       if(isActive) transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

    }
}

