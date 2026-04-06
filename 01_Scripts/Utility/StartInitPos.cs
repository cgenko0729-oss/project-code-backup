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

public class StartInitPos : MonoBehaviour
{

    public float initPosY = 0.077f;

    void Start()
    {
        Vector3 pos = transform.position;
        pos.y = initPosY;
        transform.position = pos;

    }

    void Update()
    {
        
    }
}

