using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class LevelUpCardShineEffect : MonoBehaviour
{
    public float shineCnt;
    public float shineCntMax = 3.5f;

    UIFXController uiFXController;

    public void Start()
    {
        uiFXController = GetComponent<UIFXController>();
    }
    public void Update()
    {
        shineCnt -= Time.unscaledDeltaTime;
        if (shineCnt <= 0)
        {
            shineCnt= shineCntMax;
            uiFXController.TriggerShine();

        }

    }


}

