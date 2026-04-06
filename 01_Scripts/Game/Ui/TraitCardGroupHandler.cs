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

public class TraitCardGroupHandler : MonoBehaviour
{
    public List<TraitUiDisplay> traitCardDisplays = new List<TraitUiDisplay>();

    [ContextMenu("Update All UI Display")]
    private void UpdateAllTraitCards()
    {
        foreach (var traitCard in traitCardDisplays)
        {
            if (traitCard.traitData != null)
            {
                traitCard.traitName.text = L.TraitName(traitCard.traitData.traitType);
                traitCard.traitDescription.text = L.TraitDesc(traitCard.traitData.traitType);
                traitCard.traitIcon.sprite = traitCard.traitData.icon;
                traitCard.traitType = traitCard.traitData.traitType;
            }
        }
    }


}

