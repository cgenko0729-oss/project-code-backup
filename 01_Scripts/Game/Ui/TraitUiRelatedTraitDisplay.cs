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

public class TraitUiRelatedTraitDisplay : MonoBehaviour
{
    public int displaySlotIndex = 0;

    public TextMeshProUGUI traitName;
    public Image traitIcon;

    public CanvasGroup cg;

    void Start()
    {
        //traitIcon.GetComponentInChildren<Image>();
        //traitName.GetComponentInChildren<TextMeshProUGUI>();
        //cg = GetComponent<CanvasGroup>();

    }

    void Update()
    {
        
    }

    public void SetUpRelatedTraitInfo(TraitData traitData)
    {
        if(traitData.relatedTraitList.Count <= displaySlotIndex)
        {
            cg.alpha = 0f;
            return;
        }
        cg.alpha = 1f;
        TraitData targetTrait = traitData.relatedTraitList[displaySlotIndex];

        traitName.text = L.TraitName(targetTrait.traitType);
        traitIcon.sprite = targetTrait.icon;


    }

}

