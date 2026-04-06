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

public class QuestDisplayEffectController : MonoBehaviour
{
    public Vector3 startPos = new Vector3(0, 0, 0);

    public Vector3 endPos = new Vector3(600, 300, 0);

    void Start()
    {
        
    }

    void Update()
    {

        //if press R key , dotween move from startPos to endPos in 2.1 second:
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.localPosition = startPos;
            transform.DOLocalMove(endPos, 2.1f).SetEase(Ease.OutExpo);
        }



    }
}

