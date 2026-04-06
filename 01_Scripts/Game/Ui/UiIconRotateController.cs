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

public class UiIconRotateController : MonoBehaviour
{
    RectTransform rect;

    public float rotateSpeed = 42f;

    public bool canRotate = false;

    public GameObject controllerIconObj;
    public GameObject keyboardIconObj;

    public GameObject keyBoardRotCamIcon;
    public GameObject controllerRotCamIcon;

    private void OnEnable()
    {
        EventManager.StartListening(GameEvent.ChangeCameraMode, EnableRotate);

        EventManager.StartListening("ShowControllerIcons", OnOffControllerIcon);
    }

    private void OnDisable()
    {
        EventManager.StopListening(GameEvent.ChangeCameraMode, EnableRotate);

        EventManager.StopListening("ShowControllerIcons", OnOffControllerIcon);
    }

    void OnOffControllerIcon()
    {
        bool isController = EventManager.GetBool("ShowControllerIcons");
        controllerIconObj.SetActive(isController);
        keyboardIconObj.SetActive(!isController);
    }

    void EnableRotate()
    {
        bool isAutoMode = EventManager.GetBool(GameEvent.ChangeCameraMode);
        canRotate = isAutoMode;



        if (isAutoMode)
        {
            if (Gamepad.current != null)
            {
                controllerRotCamIcon.SetActive(true);
                keyBoardRotCamIcon.SetActive(false);
            }
            else
            {
                controllerRotCamIcon.SetActive(false);
                keyBoardRotCamIcon.SetActive(true);
            }

        }
        else
        {
            controllerRotCamIcon.SetActive(false);
            keyBoardRotCamIcon.SetActive(false);

            //reset rot z to 0
            rect.rotation = Quaternion.Euler(0, 0, 0);

        }

    }

    void Start()
    {
        rect = GetComponent<RectTransform>();

    }

    void Update()
    {

        if(canRotate)rect.Rotate(0, 0, rotateSpeed * Time.deltaTime);

    }
}

