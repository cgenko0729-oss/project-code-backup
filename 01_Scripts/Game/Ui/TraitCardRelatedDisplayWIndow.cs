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
using UnityEngine.EventSystems;

public class TraitCardRelatedDisplayWIndow : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    public int traitCardSlotId = -1;

    void Start()
    {
        
    }

    void Update()
    {
        
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!SkillManager.Instance.isGetNewTrait) return; //only show when get new trait

        var allRelatedTraits =  SkillManager.Instance.traitOptionToGet[traitCardSlotId].relatedTraitList;
        if(allRelatedTraits.Count == 0) return; //no related trait to show



        SkillEffectManager.Instance.ShowRelatedTraitWindow(allRelatedTraits,traitCardSlotId);

        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
         if(!SkillManager.Instance.isGetNewTrait) return;
        var allRelatedTraits =  SkillManager.Instance.traitOptionToGet[traitCardSlotId].relatedTraitList;
        if(allRelatedTraits.Count == 0) return;

        SkillEffectManager.Instance.HideRelatedTraitWindow();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SkillEffectManager.Instance.CloseRelatedTraitWindow();
    }

}

