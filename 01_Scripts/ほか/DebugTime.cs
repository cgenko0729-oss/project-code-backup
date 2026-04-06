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

public class DebugTime : MonoBehaviour
{

    public float nowTimeScale = 1f;

    void Start()
    {
        Time.timeScale = 1f;

    }

    void Update()
    {
        nowTimeScale = Time.timeScale;

        //if (Input.GetKeyDown(KeyCode.B))
        //{
        //    Time.timeScale = 1f;
        //}

    }
}

