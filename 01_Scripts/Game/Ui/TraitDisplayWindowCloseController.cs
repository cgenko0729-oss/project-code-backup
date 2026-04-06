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

public class TraitDisplayWindowCloseController : MonoBehaviour
{
    MenuOpenAnimator menuOpenAnimator;

    private void OnEnable()
    {
        EventManager.StartListening("SelectedTraitSlot", CloseDisplayWindow);
    }

    private void OnDisable()
    {
        EventManager.StopListening("SelectedTraitSlot", CloseDisplayWindow);
    }

    void CloseDisplayWindow()
    {
        menuOpenAnimator.PlayeMenuAni(show:false);

    }

    void Start()
    {
        menuOpenAnimator = GetComponent<MenuOpenAnimator>();

    }

    void Update()
    {
        
    }
}

