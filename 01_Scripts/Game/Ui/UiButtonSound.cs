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

public class UiButtonSound : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    public bool hasClickSound = true;
    public bool hasHoverSound = false;

    public bool isDefaultClickSound = true;

    public AudioClip clickSound;
    public AudioClip hoverSound;

    public float clickVolume = 1f;
    public float hoverVolume = 1f;

    void Start()
    {
        
    }

    void Update()
    {
        
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hasHoverSound)
        {
            if(hoverSound)SoundEffect.Instance.PlayOneSound(hoverSound, hoverVolume);
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        

    }

    public void OnPointerClick(PointerEventData eventData)
    {
       if (hasClickSound)
        {
            if(isDefaultClickSound)
            {
                SoundEffect.Instance.Play(SoundList.ShopOpenSe);
            }
            else if
                (clickSound)SoundEffect.Instance.PlayOneSound(clickSound,clickVolume);
        }
    }

}

