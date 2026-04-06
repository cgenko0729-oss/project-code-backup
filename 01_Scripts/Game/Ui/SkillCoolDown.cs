using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using System;


public class SkillCoolDown : MonoBehaviour
{
    public SkillCasterBase SkillCT;
    public Image SkillCTImage;
    public Image SkillNowImage;
    public bool isUsed = false; 

    float MaxCT;
    float NowCT;

    public int SkillSlotID;

     public float CTTime = 3.0f;
    void Start()
    {
    }

    void Update()
    {

        if (!isUsed)
        {
            NowCT = 1;
            MaxCT = 1;
        }
        else
        {
            //動的処理スキルクールダウンの処理
            NowCT = SkillManager.Instance.activeSkillCasterCollections[SkillSlotID].castCoolDown;
            MaxCT = SkillManager.Instance.activeSkillCasterCollections[SkillSlotID].castCoolDownFinal;      
        }

        
        //動的処理画像選択順に変更
        if(isUsed)SkillNowImage.sprite = 
            SkillManager.Instance.activeSkillCasterCollections[SkillSlotID].casterSpriteImage;

        //スキルクールタイム白背景の部分の処理
        if (CTTime <= 0)
        {
            CTTime = 3.0f;
        }

        CTTime -= Time.deltaTime;

        SkillCTImage.fillAmount = (float)(NowCT / MaxCT);
    }
}

