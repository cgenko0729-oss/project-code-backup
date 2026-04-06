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
using UnityEngine.InputSystem;

public class SelectedIconController : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        var InputDevice = InputDeviceManager.Instance.GetLastUsedDevice();
        Color color = Color.white;
        if (InputDevice is Mouse || InputDevice is Keyboard)
        {
            color.a = 0;
            GetComponent<Image>().color = color;
            return;
        }

        var selectedObj = EventSystem.current.currentSelectedGameObject;
        Selectable comp = selectedObj?.GetComponent<Selectable>();
        
        if (comp != null || comp is Scrollbar)
        {
            RectTransform rect = selectedObj.GetComponent<RectTransform>();
            Vector3 position = selectedObj.transform.position;
            position.x += (rect.sizeDelta.x * rect.localScale.x) / 2;
            position.y -= (rect.sizeDelta.y * rect.localScale.y) / 2;
            transform.position = position;
            transform.localScale = Vector3.one;
        }
        else
        {
            color.a = 0;
        }

        GetComponent<Image>().color = color;
    }
}

