using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine



public class EXPControl : MonoBehaviour
{
    private GameObject Player;
    public Image Exp;

    float playerNextLvExp;
    float playerExp;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
         playerExp = Player.GetComponent<PlayerState>().NowExp;
        playerNextLvExp = Player.GetComponent<PlayerState>().NextLvExp;

        Exp.fillAmount = (float)(playerExp / playerNextLvExp);
    }
}

