using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using UnityEngine.EventSystems;   // for IPointerClickHandler


public class LevelUpFieldRelay : MonoBehaviour, IPointerClickHandler
{
     [Tooltip("0 = A, 1 = B, 2 = C")]
    public int slot;

    [Tooltip("0 = Rarity, 1 = Stat, 2 = Skill")]
    public int fieldKind;

     Graphic g; 

    void Awake()
    {
        g = GetComponent<Graphic>();
    }

    void Update()    
    {
        bool wantRaycast = SkillManager.Instance.isExchangeMode;
        if (g && g.raycastTarget != wantRaycast)
            g.raycastTarget = wantRaycast;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!SkillManager.Instance.isExchangeMode) return;
        SkillManager.Instance.OnOptionFieldClicked(slot, fieldKind);
        eventData.Use();  
    }
}

