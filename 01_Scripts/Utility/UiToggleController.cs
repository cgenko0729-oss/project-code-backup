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

public class UiToggleController : MonoBehaviour
{
    public Toggle toggle;

    void Start()
    {
        toggle = GetComponent<Toggle>();

        ToggleOn();

    }

    public void ToggleOn()
    {
        DOVirtual.DelayedCall(0.1f, () =>
        {
            toggle.isOn = true;

            //trigger value change event
                toggle.onValueChanged.Invoke(toggle.isOn);

        });
    }

    void Update()
    {
        
    }
}

