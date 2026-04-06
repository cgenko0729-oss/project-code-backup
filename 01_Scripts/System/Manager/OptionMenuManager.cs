using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

public class OptionMenuManager : Singleton<OptionMenuManager>
{

    public GameObject optionMenu;
   
    public GameObject statusMenu;

    public SettingPageController settingPageController;

    public bool isOptionMenuOpen = false;



    void Start()
    {
        
    }

    void Update()
    {
        if(ResultMenuController.Instance.isResultMenuOpen) return;

        // レベルアップメニューが表示されている場合は入力を無視する
        if (SkillManager.Instance.waitingForPlayer == false)
        {
            //if press esc key , set option menu active
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EventManager.EmitEvent(GameEvent.isOptionMenu);
                if (optionMenu.activeSelf)
                {
                    CloseOptionMenu();
                }
                else
                {
                    OpenOptionMenu();
                }
            }

            if (Gamepad.current != null)
            {
                if (Gamepad.current.selectButton.wasPressedThisFrame)
                {
                    EventManager.EmitEvent(GameEvent.isOptionMenu);
                if (optionMenu.activeSelf)
                {
                    CloseOptionMenu();
                }
                else
                {
                    OpenOptionMenu();
                }
                }

                //if press LeftShoulder button :
                if (Gamepad.current.leftShoulder.wasPressedThisFrame && isOptionMenuOpen)
                {
                    settingPageController.ToPreviousPageGame(); 
                }
                //if press RightShoulder button :
                if (Gamepad.current.rightShoulder.wasPressedThisFrame && isOptionMenuOpen)
                {
                    settingPageController.ToNextPageInGame();
                }


                }

        }
    }

    public void OnOffOptionMenu()
    {
        EventManager.EmitEvent(GameEvent.isOptionMenu);
        if (optionMenu.activeSelf)
        {
            CloseOptionMenu();
        }
        else
        {
            OpenOptionMenu();
        }
    }

    public void OpenOptionMenu()
    {
        if (SkillManager.Instance.waitingForPlayer) return;

        CameraViewManager.Instance.ShowAndUnlockCursor();

        isOptionMenuOpen = true;

        settingPageController.OpenOptionMenuGame();

        statusMenu.SetActive(false);
        optionMenu.SetActive(true);

        MenuOpenAnimator menuAni = optionMenu.GetComponent<MenuOpenAnimator>();
        menuAni.PlayeMenuAni(true);


        if (!SkillManager.Instance.waitingForPlayer && !SkillManager.Instance.isGetNewTrait)
        {
            Time.timeScale = 0f; // Pause the game
            GameManager.Instance.isGamePause = true;
        }
        

        //AudioManager.Instance.PlayPauseBGM();
    }

    public void CloseOptionMenu()
    {

        if(CameraViewManager.Instance.currentMode == CameraViewManager.CameraMode.CloseView)
        {
            CameraViewManager.Instance.HideAndLockCursor();
        }

        //optionMenu.SetActive(false);
        MenuOpenAnimator menuAni = optionMenu.GetComponent<MenuOpenAnimator>();
        menuAni.PlayeMenuAni(false);

        isOptionMenuOpen = false;

        if (!SkillManager.Instance.waitingForPlayer && !SkillManager.Instance.isGetNewTrait)
        {
            Time.timeScale = 1f; // Resume the game
            GameManager.Instance.isGamePause = false;
        }

        TutorMessageManager.Instance.CloseTutorMenu();
        //AudioManager.Instance.StopPauseBGM();
    }

  
}

