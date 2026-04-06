using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class Dash : MonoBehaviour
{
    private PlayerState dashCT;
    public Image DashCTImage;
    [SerializeField] private TextMeshProUGUI chargeCountText;
    private PlayerController playerController;

    float MaxCT;
    float NowCT;

    public float CTTime = 2.0f;

    void Start()
    {
        dashCT = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();

        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

    }

    void Update()
    {
        //;
        //MaxCT = dashCT.DashCoolDownTime;
        //NowCT = dashCT.DashCoolDownCnt;

        //if(CTTime <= 0)
        //{
        //    CTTime = 0;
        //}

        //CTTime -= Time.deltaTime;

        //DashCTImage.fillAmount = (float)(NowCT / MaxCT);

        // If the reference to the player is lost, do nothing.
        if (playerController == null) return;

        // Get the latest dash data from the PlayerController.
        int availableCharges = playerController.CurrentDashCharges;
        int maxCharges = playerController.maxDashCharges;
        float rechargeTimer = playerController.dashRechargeTimer;
        float maxRechargeTime = playerController.dashRechargeTime;

        // --- 1. Update the Charge Count Text ---
        // Set the text to show the number of currently available dashes.
        if (maxCharges >= 2)
        {
            if(availableCharges > 0) chargeCountText.text = availableCharges.ToString();
            else chargeCountText.text = "";

        }
        else chargeCountText.text = "";

        // --- 2. Update the Cooldown Image ---
        // If the player has all charges, the image should be completely full.
        if (availableCharges == maxCharges)
        {
            DashCTImage.fillAmount = 0f;
        }
        else
        {
            // If a charge is recharging, calculate and display the progress.
            // The timer counts down, so we subtract its progress from 1 to show the image filling up.
            if (maxRechargeTime > 0)
            {
                DashCTImage.fillAmount = (rechargeTimer / maxRechargeTime);
            }
        }

    }
}

