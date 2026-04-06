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

public class RelatedTraitUIItem : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;

    // Populates the UI item with data from a TraitData object and makes it visible.
    public void SetData(TraitData data)
    {
        if (data != null)
        {
            iconImage.sprite = data.icon;
            nameText.text = L.TraitName(data.traitType);
            gameObject.SetActive(true); // Show this item
        }
        else
        {
            // Hide if data is null, just in case.
            Hide();
        }
    }

    // Hides this UI item.
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

