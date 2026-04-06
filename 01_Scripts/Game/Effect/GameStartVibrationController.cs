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

public class GameStartVibrationController : MonoBehaviour
{
    void Start()
    {
        Invoke("PulseShake", 0.7f);
        
    }

    void Update()
    {
        
    }

    void PulseShake()
    {
        ControllerVibrationManager.Instance.VibratePulse();
    }

}

