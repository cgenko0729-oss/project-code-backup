using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using System.Data;

public class UIcontrol : MonoBehaviour
{
    //public Slider Hp;
    private GameObject Player;
    public Image Heart;


    float PlayerMaxHp;
    float PlayerHp;

    private Material heartMaterialInstance;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");

        if (Heart != null)
        {
            heartMaterialInstance = Heart.material;
        }
        else
        {
            Debug.Log("The 'Heart' Image has not been assigned in the Inspector for UIcontrol.cs!");
        }
    }

    void Update()
    {
        PlayerHp = Player.GetComponent<PlayerState>().NowHp;
        PlayerMaxHp = Player.GetComponent<PlayerState>().MaxHp;


        Heart.fillAmount =(float)(PlayerHp / PlayerMaxHp);

        float fillRatio = 0f;
        if (PlayerMaxHp > 0) fillRatio = Mathf.Clamp01(PlayerHp / PlayerMaxHp);
        heartMaterialInstance.SetFloat("_FillAmount", fillRatio);
    }
}

