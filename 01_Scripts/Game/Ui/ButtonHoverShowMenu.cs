using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager
using UnityEngine.EventSystems;

public class ButtonHoverShowMenu : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{

   

    public CanvasGroup detailMenu;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        detailMenu.alpha = 1;

      
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        detailMenu.alpha = 0;
    }

  

}

