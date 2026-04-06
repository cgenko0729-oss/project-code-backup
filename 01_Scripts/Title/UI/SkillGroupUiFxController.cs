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

public class SkillGroupUiFxController : MonoBehaviour,ISelectHandler, IDeselectHandler
{
    public UIFXController uifx;

    void Start()
    {
        
    }

    void Update()
    {
        
    }


    public void OnSelect(BaseEventData eventData)
    {
        //debug.log

        if (uifx)
        {
            
            //uifx.SetInnerOutline(true,true,7,7,Color.blue);
            Debug.Log($"{gameObject.name} OnSelect {Time.time}");

            uifx.ToggleOverlayUiFX(true);

           // UIManager.Instance.OnOffScrollViewSkillWindow();
        }
       
    }

    public void OnDeselect(BaseEventData eventData)
    {

        //debug.log

        if (uifx)
        {
             uifx.ToggleOverlayUiFX(false);
            //uifx.SetInnerOutline(false);
            Debug.Log($"{gameObject.name} OnDeselect {Time.time}");
        }
        
    }


}

