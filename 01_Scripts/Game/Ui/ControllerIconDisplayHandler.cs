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
using UnityEngine.InputSystem;

public class ControllerIconDisplayHandler : MonoBehaviour
{

    public float detectInterval = 0.5f;

    public Image iconImage;

    public Sprite keyboardSprite;
    public Sprite gamepadSprite;

    private void OnEnable()
    {
        InputDeviceManager.Instance.OnDeviceChanged += UpdateIcon;
        UpdateIcon(InputDeviceManager.Instance.GetLastUsedDevice());
    }

    private void OnDisable()
    {
        // Unsubscribe when the object is disabled/destroyed
        if (InputDeviceManager.Instance != null)
        {
            InputDeviceManager.Instance.OnDeviceChanged -= UpdateIcon;
        }
    }

    private void UpdateIcon(InputDevice device)
    {
        if (iconImage == null) return;

        // Logic to determine which sprite to show
        if (device is Gamepad)
        {
            if(gamepadSprite)iconImage.sprite = gamepadSprite;
        }
        else // Covers Keyboard/Mouse
        {
            if(keyboardSprite)iconImage.sprite = keyboardSprite;
        }
    }

    void Start()
    {
        iconImage = GetComponent<Image>();

        if(Gamepad.current != null)
        {
            if(gamepadSprite)iconImage.sprite = gamepadSprite;
        }
        else
        {
            if(keyboardSprite)iconImage.sprite = keyboardSprite;
        }

    }

    void Update()
    {

        
    }



}

