using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class LevelUpUI : MonoBehaviour
{
    public TextMeshProUGUI Lv;
    
    PlayerState player;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerState>();
    }

    void Update()
    {
        Lv.text = player.NowLv.ToString();
    }
}

