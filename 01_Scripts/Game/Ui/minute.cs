using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using System;


public class minute : MonoBehaviour
{
    public TextMeshProUGUI minute1;        //•ª
    public TextMeshProUGUI second1;        //•b

    //public float gameTime = 600f;
    

    void Start()
    {

        // compute minutes and seconds
        int mins = Mathf.FloorToInt(TimeManager.Instance.gameTimeLeft / 60f);
        int secs = Mathf.FloorToInt(TimeManager.Instance.gameTimeLeft % 60f);

        // format with leading zeros
        minute1.text = mins.ToString("D2");
        second1.text = secs.ToString("D2");
    }

    void Update()
    {
        //if(gameTime <= 0)
        //{
        //    gameTime = 0;
        //}

        //gameTime -= Time.deltaTime;

        if (StageManager.Instance.isEndlessMode) //Endless Mode
        {
            int mins = Mathf.FloorToInt(TimeManager.Instance.gameTimePassed / 60f);
            int secs = Mathf.FloorToInt(TimeManager.Instance.gameTimePassed % 60f);
            minute1.text = mins.ToString("D2");
            second1.text = secs.ToString("D2");
        }
        else //Normal Mode
        {
            int mins = Mathf.FloorToInt(TimeManager.Instance.gameTimeLeft / 60f);
            int secs = Mathf.FloorToInt(TimeManager.Instance.gameTimeLeft % 60f);
            // format with leading zeros
            minute1.text = mins.ToString("D2");
            second1.text = secs.ToString("D2");

            if (TimeManager.Instance.gameTimeLeft <= 0)
            {
                TimeManager.Instance.gameTimeLeft = 0;
                minute1.text  =  "00";
                second1.text =   "00";
            }

        }


        
    }
}

