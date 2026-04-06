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

public class InputDeviceIconController : MonoBehaviour
{
    public enum InputDevice
    {
        Keyboard,
        Gamepad,
    }

    [Header("このアイコン画像が使われるときのデバイスの種類")]
    [SerializeField] private InputDevice inputDevice;
    private Image iconImage;

    [Header("シーン内のUIオブジェクトのIntractableによって表示を切り替えるための情報")]
    [Tooltip("Interactableを判定に使うかどうか")]
    [SerializeField] private bool useObjInteractable = false;
    [Tooltip("判定に使われるUIオブジェクト(選ぶことのできるオブジェクトのみ)")]
    [SerializeField] private Selectable useInteractableUiObject;

    [Header("シーン内のUIオブジェクトのActiveフラグによって表示を切り替えるための情報")]
    [Tooltip("Activeフラグを判定に使うかどうか")]
    [SerializeField] private bool useObjActiveFlg = false;
    [Tooltip("判定に使われるUIオブジェクト(選ぶことのできるオブジェクトのみ)")]
    [SerializeField] private GameObject useActiveUiObject;

    void Start()
    {
        iconImage = GetComponent<Image>();
        ChangeInputDeviceIcon();
    }

    void Update()
    {
        ChangeInputDeviceIcon();
    }

    private void ChangeInputDeviceIcon()
    {
        Color iconColor = new Color(1, 1, 1, 0);

        // 選んだObjectのInteractableがFalseならAlphaを0のまま早期リターン
        if (useObjInteractable == true && useInteractableUiObject != null)
        {
            if (useInteractableUiObject.interactable == false)
            {
                return;
            }
        }
        // 選んだObjectのActiveフラグがFalseならAlphaを0のまま早期リターン
        else if(useObjActiveFlg == true && useActiveUiObject != null)
        {
            if (useActiveUiObject.activeSelf == false)
            {
                return;
            }
        }

        switch (inputDevice)
            {
                case InputDevice.Keyboard:
                    if (InputDeviceManager.Instance.GetLastUsedDevice() is Keyboard)
                    {
                        iconColor.a = 1;
                    }
                    break;
                case InputDevice.Gamepad:
                    if (InputDeviceManager.Instance.GetLastUsedDevice() is Gamepad)
                    {
                        iconColor.a = 1;
                    }
                    break;
            }

        iconImage.color = iconColor;
    }
}

