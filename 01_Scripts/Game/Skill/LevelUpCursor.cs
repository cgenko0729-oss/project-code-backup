using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class LevelUpCursor : MonoBehaviour
{
    public Image cursor;
    public bool isStillExchangeMode = false;

    public Image slotFrameImage;

    private void OnEnable()
    {
        EventManager.StartListening("HideSlotFrame", HideFrame);

        EventManager.StartListening("ShowSlotFrame", ShowFrame);
    }

    private void OnDisable()
    {
        EventManager.StopListening("HideSlotFrame", HideFrame);

        EventManager.StopListening("ShowSlotFrame", ShowFrame);

    }

    void ShowFrame()
    {
        if(slotFrameImage)slotFrameImage.gameObject.SetActive(true);

    }

    void HideFrame()
    {
        if(slotFrameImage)slotFrameImage.gameObject.SetActive(false);

    }

    void Start()
    {
        cursor = GetComponentInChildren<Image>();
        DeactivateCursor();
        HideFrame();
    }

    void Update()
    {
        if(isStillExchangeMode && !SkillManager.Instance.isExchangeMode)
        {
            DeactivateCursor();
            isStillExchangeMode = false;
        }
        
    }

    public void ActivateCursor()
    {
        cursor.gameObject.SetActive(true);
        isStillExchangeMode = true;
    }


    public void DeactivateCursor()
    {
        cursor.gameObject.SetActive(false);
    }

}

