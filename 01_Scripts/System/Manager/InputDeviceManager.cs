using Cysharp.Threading.Tasks;
using DG.Tweening;
using MonsterLove.StateMachine; //StateMachine
using QFSW.MOP2;                //Object Pool
using System;
using System.Collections.Generic;
using TigerForge;               //EventManager
using TMPro;    
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;     

public class InputDeviceManager : Singleton<InputDeviceManager>
{
    public event Action<InputDevice> OnDeviceChanged;
    [SerializeField] private InputDevice lastDevice;

    private GameObject lastSelctedUiObj;
    public InputDevice GetLastUsedDevice() => lastDevice;

    // [FIX 1] Store the subscription to fix OnDisable errors
    private IDisposable _inputSubscription;

    //private void OnEnable()
    //{
    //    InputSystem.onAnyButtonPress.Call(OnAnyButtonPress);
    //}


    //private void OnDisable()
    //{
    //    //InputSystem.onAnyButtonPress.RemoveListener(OnAnyButtonPress);    }
    //}

    private void OnEnable()
    {
        // [FIX 1] Save the subscription token
        _inputSubscription = InputSystem.onAnyButtonPress.Call(OnAnyButtonPress);
    }

    private void OnDisable()
    {
        // [FIX 1] Dispose properly to prevent memory leaks
        _inputSubscription?.Dispose(); 
    }

    void Awake()
    {
        lastDevice = Keyboard.current;
    }

    private void Update()
    {
        if(EventSystem.current == null) { return; }

        if(EventSystem.current.alreadySelecting == true)
        {
            lastSelctedUiObj = EventSystem.current.currentSelectedGameObject;
        }
        

        if (Gamepad.current != null && lastDevice != Gamepad.current)
        {
            // Check if sticks are moving (using sqrMagnitude is faster)
            // 0.04 is a small deadzone to prevent drift from switching devices
            float leftStick = Gamepad.current.leftStick.ReadValue().sqrMagnitude;
            float rightStick = Gamepad.current.rightStick.ReadValue().sqrMagnitude;

            if (leftStick > 0.04f || rightStick > 0.04f)
            {
                // Manually trigger the switch logic
                SetNewDevice(Gamepad.current);
            }
        }

        // If the mouse moves significantly, show the cursor and switch to Keyboard
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            // Use a threshold (e.g. 4.0) to avoid switching on tiny accidental jitters
            if (mouseDelta.sqrMagnitude > 4.0f) 
            {
                SetNewDevice(Keyboard.current);
            }
        }

    }

    private void SetNewDevice(InputDevice device)
    {
        // Treat Mouse as Keyboard
        if (device is Mouse || Gamepad.current == null)
        {
            device = Keyboard.current;
        }

        if (lastDevice == device) return;

        lastDevice = device;
        OnDeviceChanged?.Invoke(device);

        if (device is Gamepad)
        {
            Cursor.visible = false;
            // Optional: Lock cursor to center so it doesn't click outside window
            Cursor.lockState = CursorLockMode.Locked; 
            
            // Restore UI selection for controller navigation
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(lastSelctedUiObj);
            }
        }
        else // Keyboard / Mouse
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        Debug.Log($"[InputDeviceManager] デバイス切り替え: {device.displayName}");
    }

    private void OnAnyButtonPress(InputControl control)
    {
        var device = control.device;
        // 入力したデバイスがマウスの場合はキーボードの入力としてみなす
        if (device is Mouse ||
            Gamepad.current == null)
        {
            device = Keyboard.current;
        }
        if (lastDevice == device) return;

        lastDevice = device;
        OnDeviceChanged?.Invoke(device);

        if(EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelctedUiObj);
        }

        Debug.Log($"[InputDeviceManager] デバイス切り替え: {device.displayName}");
    }
}

