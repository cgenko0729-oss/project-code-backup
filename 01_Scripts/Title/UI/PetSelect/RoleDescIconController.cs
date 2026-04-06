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

public class RoleDescIconController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject RolesDescPanel;
    public float changeTime;
    public float addScale;

    private float defaultScale;
    private RectTransform panelTrans;
    private bool isOpenPanel = false;

    void Start()
    {
        if (RolesDescPanel != null)
        {
            RolesDescPanel.SetActive(true);
            panelTrans = RolesDescPanel.GetComponent<RectTransform>();
            panelTrans.localScale = Vector3.zero;
        }

        defaultScale = transform.localScale.x;
        isOpenPanel = false;
    }

    private void OnEnable()
    {
        
    }

    private void Update()
    {
        if(InputDeviceManager.Instance.GetLastUsedDevice() is Gamepad)
        {
            if(Gamepad.current.leftShoulder.wasPressedThisFrame)
            {
                isOpenPanel = !isOpenPanel;

                if(isOpenPanel == true)
                {
                    OnPointerEnter(null);
                }
                else
                {
                    OnPointerExit(null);
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (RolesDescPanel == null) { return; }
        panelTrans.DOScale(Vector3.one, changeTime);
        transform.DOScale(defaultScale + addScale, changeTime);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (RolesDescPanel == null) { return; }
        panelTrans.DOScale(Vector3.zero, changeTime);
        transform.DOScale(defaultScale, changeTime);
    }
}

