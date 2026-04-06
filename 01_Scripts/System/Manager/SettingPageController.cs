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

public class SettingPageController : MonoBehaviour
{

    public GameObject menuAch;
    public GameObject menuSetting;
    public GameObject menuShop;
    public bool isAllMenuReset = false;
    private float resetMenuCnt = 0.2f;

    public GameObject LanguageSettingPageObj;
    public GameObject GameSettingPageObj;
    public GameObject SoundSettingPageObj;
    public GameObject creditPageObj;

    public GameObject ExitPageObj;

    //Resolution
    public Button Btn1920;
    public Button Btn1080;

    public Button BtnFullScreen;
    public Button BtnNotFullScreen;
    //Fps
    public Button Btn60fps;
    public Button Btn120fps;
    public Button BtnUnlimited;

    //VSync
    public Button BtnVSyncOn;
    public Button BtnVSyncOff;

    //Language
    public Button JpButton;
    public Button EnButton;
    public Button ChSButton;
    public Button ChTButton;

    //Volume
    public Scrollbar allVolumeScrollbar;
    public Scrollbar bgmScrollbar;
    public Scrollbar soundEffectScrollbar;

    //(GameScene‚©‚ç‚Ì)Exit
    public Button exitButton;

    public Button BtnVibrationOff;
    public Button BtnVibrationWeak;
    public Button BtnVibrationStrong;

    public Color ButtonActiveColor = new Color(0.7f, 1f, 0.19f);
    public Color ButtonInactiveColor = Color.white;

    public bool isSetButtonColorAndPos = false;

    public float waitScreenResolutionCnt = 1f;

    public void Start()
    {
        //AchievementManager.Instance.RetrieveCharaMapUnlockData();


       DOVirtual.DelayedCall(0.4f, DelaySetSoundBarToValue);

    }

    void DelaySetSoundBarToValue()
    {
          float globalVol = AudioManager.Instance.globalVolume;
        float bgmVol = AudioManager.Instance.allBGMVolume;
        float seVol = SoundEffect.Instance.allSoundVolume;

        allVolumeScrollbar.value = globalVol;
        bgmScrollbar.value = bgmVol;
        soundEffectScrollbar.value = seVol;

        //Debug.Log($"[SettingPageController] Delayed SE Volume: {seVol}");
        //Debug.Log($"[SettingPageController] Delayed BGM Volume: {bgmVol}");
        //Debug.Log($"[SettingPageController] Delayed Global Volume: {globalVol}");

    }

    public void OpenOptionMenu()
    {
        LanguageSettingPageObj.SetActive(true);
        GameSettingPageObj.SetActive(false);
        SoundSettingPageObj.SetActive(false);
        creditPageObj.SetActive(false);

        EventSystem.current.SetSelectedGameObject(JpButton.gameObject);
    }

    public void OpenOptionMenuGame()
    {
        SoundSettingPageObj.SetActive(true);
        GameSettingPageObj.SetActive(false);
        ExitPageObj.SetActive(false);

        EventSystem.current.SetSelectedGameObject(allVolumeScrollbar.gameObject);
    }

    public void ToNextPageInGame()
    {
        if (SoundSettingPageObj.activeSelf)
        {
            GameSettingPageObj.SetActive(true);
            SoundSettingPageObj.SetActive(false);
            ExitPageObj.SetActive(false);

            EventSystem.current.SetSelectedGameObject(Btn1920.gameObject);

            return;
        }
        if (GameSettingPageObj.activeSelf)
        {
            ExitPageObj.SetActive(true);
            SoundSettingPageObj.SetActive(false);
            GameSettingPageObj.SetActive(false);

            EventSystem.current.SetSelectedGameObject(exitButton.gameObject);

            return;
        }
        if(ExitPageObj.activeSelf)
        {
            SoundSettingPageObj.SetActive(true);
            ExitPageObj.SetActive(false);
            GameSettingPageObj.SetActive(false);

            EventSystem.current.SetSelectedGameObject(allVolumeScrollbar.gameObject);

            return;
        }
    }

    public void ToPreviousPageGame()
    {
        if (ExitPageObj.activeSelf)
        {
            GameSettingPageObj.SetActive(true);
            ExitPageObj.SetActive(false);
            SoundSettingPageObj.SetActive(false);

            EventSystem.current.SetSelectedGameObject(Btn1920.gameObject);

            return;
        }
        if (GameSettingPageObj.activeSelf)
        {
            SoundSettingPageObj.SetActive(true);
            GameSettingPageObj.SetActive(false);
            ExitPageObj.SetActive(false);

            EventSystem.current.SetSelectedGameObject(allVolumeScrollbar.gameObject);

            return;
        }
        if (SoundSettingPageObj.activeSelf)
        {
            ExitPageObj.SetActive(true);
            SoundSettingPageObj.SetActive(false);
            GameSettingPageObj.SetActive(false);

            EventSystem.current.SetSelectedGameObject(exitButton.gameObject);

            return;
        }
    }

    public void ToNextPage()
    {
        if(LanguageSettingPageObj.activeSelf)
        {
            GameSettingPageObj.SetActive(true);
            LanguageSettingPageObj.SetActive(false);
            SoundSettingPageObj.SetActive(false);
            creditPageObj.SetActive(false);


            EventSystem.current.SetSelectedGameObject(Btn1920.gameObject);
            
            return;
        }
        if (GameSettingPageObj.activeSelf)
        {
            SoundSettingPageObj.SetActive(true);
            creditPageObj.SetActive(false);
            GameSettingPageObj.SetActive(false);
            LanguageSettingPageObj.SetActive(false);
            

            EventSystem.current.SetSelectedGameObject(allVolumeScrollbar.gameObject);

            return;
        }
        if (SoundSettingPageObj.activeSelf)
        {
            creditPageObj.SetActive(true);
            LanguageSettingPageObj.SetActive(false);
            SoundSettingPageObj.SetActive(false);
            GameSettingPageObj.SetActive(false);
            


            EventSystem.current.SetSelectedGameObject(null);

            return;

        }
        if (creditPageObj.activeSelf)
        {
            LanguageSettingPageObj.SetActive(true);
            creditPageObj.SetActive(false);
            SoundSettingPageObj.SetActive(false);
            GameSettingPageObj.SetActive(false);

            EventSystem.current.SetSelectedGameObject(JpButton.gameObject);
            return;

        }



    }

    public void ToPreviousPage()
    {
        if (SoundSettingPageObj.activeSelf)
        {
            GameSettingPageObj.SetActive(true);
            SoundSettingPageObj.SetActive(false);
            creditPageObj.SetActive(false);
            LanguageSettingPageObj.SetActive(false);


            EventSystem.current.SetSelectedGameObject(Btn1920.gameObject);

            return;
        }
        if (GameSettingPageObj.activeSelf)
        {
            LanguageSettingPageObj.SetActive(true);
            GameSettingPageObj.SetActive(false);
            creditPageObj.SetActive(false);
            SoundSettingPageObj.SetActive(false);


            EventSystem.current.SetSelectedGameObject(JpButton.gameObject);

            return;
        }
        if (LanguageSettingPageObj.activeSelf)
        {
            creditPageObj.SetActive(true);
            SoundSettingPageObj.SetActive(false);
            GameSettingPageObj.SetActive(false);
            LanguageSettingPageObj.SetActive(false);
            

            EventSystem.current.SetSelectedGameObject(null);

            return;

        }
        if (creditPageObj.activeSelf)
        {
            SoundSettingPageObj.SetActive(true);
            creditPageObj.SetActive(false);
            GameSettingPageObj.SetActive(false);
            LanguageSettingPageObj.SetActive(false);
            EventSystem.current.SetSelectedGameObject(allVolumeScrollbar.gameObject);
            return;
        }

     }

    

    public void SetJapanese()
    {
        SettingManager.Instance.SetJapaneseLanguage();


        if (!JpButton || !EnButton || !ChSButton || !ChTButton) return;
        JpButton.GetComponent<Image>().color = ButtonActiveColor;
        ChSButton.GetComponent<Image>().color = ButtonInactiveColor;
        ChTButton.GetComponent<Image>().color = ButtonInactiveColor;
        EnButton.GetComponent<Image>().color = ButtonInactiveColor;

    }

    public void SetEnglish()
    {
        SettingManager.Instance.SetEnglishLanguage();

        if (!JpButton || !EnButton || !ChSButton || !ChTButton) return;
        EnButton.GetComponent<Image>().color = ButtonActiveColor;
        JpButton.GetComponent<Image>().color = ButtonInactiveColor;
        ChSButton.GetComponent<Image>().color = ButtonInactiveColor;
        ChTButton.GetComponent<Image>().color = ButtonInactiveColor;


    }
    public void SetChineseS()
    {
        SettingManager.Instance.SetChineseSimLanguage();

        if (!JpButton || !EnButton || !ChSButton || !ChTButton) return;
        ChSButton.GetComponent<Image>().color = ButtonActiveColor;
        JpButton.GetComponent<Image>().color = ButtonInactiveColor;
        ChTButton.GetComponent<Image>().color = ButtonInactiveColor;
        EnButton.GetComponent<Image>().color = ButtonInactiveColor;

    }

    public void SetChineseT()
    {
        SettingManager.Instance.SetChineseTradLanguage();

        if (!JpButton || !EnButton || !ChSButton || !ChTButton) return;
        ChTButton.GetComponent<Image>().color = ButtonActiveColor;
        JpButton.GetComponent<Image>().color = ButtonInactiveColor;
        ChSButton.GetComponent<Image>().color = ButtonInactiveColor;
        EnButton.GetComponent<Image>().color = ButtonInactiveColor;

    }

    public void SetGlobalVolumeOption(float vol)
    {
        AudioManager.Instance.GlobalVolumeOption(vol);
    }

    public void SetAllBGMVolumeOption(float vol)
    {
        AudioManager.Instance.AllBGMVolumeOption(vol);

    }

    public void SetSoundEffectListSoundEffectVolume(float vol)
    {
        SoundEffect.Instance.SetSoundEffectVolume(vol);
    }

    public void Set1980()
    {

        //if (Screen.fullScreen)
        //{
        //    SetFullScreenYes();
        //}
        //else
        //{
        //    SettingManager.Instance.Set1920X1080Window();
        //}

        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow || Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
        {
            // If so, apply the new resolution directly in fullscreen mode
            SettingManager.Instance.SetBorderlessFullscreen(1920, 1080);
        }
        else
        {
            // Otherwise, just set the windowed resolution
            SettingManager.Instance.Set1920X1080Window();
        }


        if (!Btn1920 || !Btn1080) return; 
        Btn1920.GetComponent<Image>().color = ButtonActiveColor;
        Btn1080.GetComponent<Image>().color = ButtonInactiveColor;


    }

    public void Set1280()
    {
        //if (Screen.fullScreen)
        //{
        //    SetFullScreenYes();
        //}
        //else
        //{
        //    SettingManager.Instance.Set1080X720Window();
        //}

        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow || Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
        {
            SettingManager.Instance.SetBorderlessFullscreen(1280, 720);
        }
        else
        {
            SettingManager.Instance.Set1080X720Window();
        }
        

        if (!Btn1920 || !Btn1080) return; 
        Btn1080.GetComponent<Image>().color = ButtonActiveColor;
        Btn1920.GetComponent<Image>().color = ButtonInactiveColor;

    }

    public void SetFullScreenYes()
    {
        SettingManager.Instance.SetBorderlessFullscreen();
    }

    public void SetFullScreen(bool isFull)
    {
        //SettingManager.Instance.SetExclusiveFullscreenNative();
   
        if (isFull)
        {
            SettingManager.Instance.SetBorderlessFullscreen();
            BtnFullScreen.GetComponent<Image>().color = ButtonActiveColor;
            BtnNotFullScreen.GetComponent<Image>().color = ButtonInactiveColor;
        }
        else
        {
            SettingManager.Instance.SetNotBorderlessFullscreen();
            BtnNotFullScreen.GetComponent<Image>().color = ButtonActiveColor;
            BtnFullScreen.GetComponent<Image>().color = ButtonInactiveColor;
        }

    }

    public void SetFps60()
    {
        SettingManager.Instance.SetTargetFPS(60);

        if (!Btn60fps || !Btn120fps || !BtnUnlimited) return;
        Btn60fps.GetComponent<Image>().color = ButtonActiveColor;
        Btn120fps.GetComponent<Image>().color = ButtonInactiveColor;
        BtnUnlimited.GetComponent<Image>().color = ButtonInactiveColor;
    }

    public void SetFps120()
    {
        SettingManager.Instance.SetTargetFPS(120);

        if (!Btn60fps || !Btn120fps || !BtnUnlimited) return;
         Btn120fps.GetComponent<Image>().color = ButtonActiveColor;
         Btn60fps.GetComponent<Image>().color = ButtonInactiveColor;
         BtnUnlimited.GetComponent<Image>().color = ButtonInactiveColor;
    }

    public void SetFpsUnlimited()
    {
        SettingManager.Instance.SetTargetFPS(-1);

        if (!Btn60fps || !Btn120fps || !BtnUnlimited) return;
        BtnUnlimited.GetComponent<Image>().color = ButtonActiveColor;
        Btn60fps.GetComponent<Image>().color = ButtonInactiveColor;
        Btn120fps.GetComponent<Image>().color = ButtonInactiveColor;
    }

    public void SetVsyncOn()
    {
        SettingManager.Instance.SetVSyncOn();

        if (!BtnVSyncOn || !BtnVSyncOff) return;
        BtnVSyncOn.GetComponent<Image>().color = ButtonActiveColor;
        BtnVSyncOff.GetComponent<Image>().color = ButtonInactiveColor;
    }

    public void SetVsyncOff()
    {
        SettingManager.Instance.SetVSyncOff();
        if (!BtnVSyncOn || !BtnVSyncOff) return;
        BtnVSyncOff.GetComponent<Image>().color = ButtonActiveColor;
        BtnVSyncOn.GetComponent<Image>().color = ButtonInactiveColor;
    }

    public void SetVibrationOff()
    {
        SoundEffect.Instance.controllerVibrationType = 0;

        if (!BtnVibrationOff || !BtnVibrationWeak || !BtnVibrationStrong) return;
        BtnVibrationOff.GetComponent<Image>().color = ButtonActiveColor;
        BtnVibrationWeak.GetComponent<Image>().color = ButtonInactiveColor;
        BtnVibrationStrong.GetComponent<Image>().color = ButtonInactiveColor;

    }

    public void SetVibrationWeak()
    {
        SoundEffect.Instance.controllerVibrationType = 1;

        if (!BtnVibrationOff || !BtnVibrationWeak || !BtnVibrationStrong) return;
        BtnVibrationWeak.GetComponent<Image>().color = ButtonActiveColor;
        BtnVibrationOff.GetComponent<Image>().color = ButtonInactiveColor;
        BtnVibrationStrong.GetComponent<Image>().color = ButtonInactiveColor;

    }

    public void SetVibrationStrong()
    {
        SoundEffect.Instance.controllerVibrationType = 2;

        if (!BtnVibrationOff || !BtnVibrationWeak || !BtnVibrationStrong) return;
        BtnVibrationStrong.GetComponent<Image>().color = ButtonActiveColor;
        BtnVibrationOff.GetComponent<Image>().color = ButtonInactiveColor;
        BtnVibrationWeak.GetComponent<Image>().color = ButtonInactiveColor;

    }




    void SetButtonColorAndPos()
    {
        int fps = Application.targetFrameRate;
        bool isVsync = QualitySettings.vSyncCount > 0;
        bool isFullScreen = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;

        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        int vibrationType = SoundEffect.Instance.controllerVibrationType;

        if (fps == 60)
        {
            Btn60fps.GetComponent<Image>().color = ButtonActiveColor;
            Btn120fps.GetComponent<Image>().color = ButtonInactiveColor;
            BtnUnlimited.GetComponent<Image>().color = ButtonInactiveColor;
        }
        else if (fps == 120)
        {
            Btn120fps.GetComponent<Image>().color = ButtonActiveColor;
            Btn60fps.GetComponent<Image>().color = ButtonInactiveColor;
            BtnUnlimited.GetComponent<Image>().color = ButtonInactiveColor;
        }
        else
        {
            BtnUnlimited.GetComponent<Image>().color = ButtonActiveColor;
            Btn60fps.GetComponent<Image>().color = ButtonInactiveColor;
            Btn120fps.GetComponent<Image>().color = ButtonInactiveColor;
        }

        if (isVsync)
        {
            BtnVSyncOn.GetComponent<Image>().color = ButtonActiveColor;
            BtnVSyncOff.GetComponent<Image>().color = ButtonInactiveColor;
        }
        else
        {
            BtnVSyncOff.GetComponent<Image>().color = ButtonActiveColor;
            BtnVSyncOn.GetComponent<Image>().color = ButtonInactiveColor;
        }

        if (isFullScreen)
        {
            BtnFullScreen.GetComponent<Image>().color = ButtonActiveColor;
            BtnNotFullScreen.GetComponent<Image>().color = ButtonInactiveColor;
        }
        else
        {
            BtnNotFullScreen.GetComponent<Image>().color = ButtonActiveColor;
            BtnFullScreen.GetComponent<Image>().color = ButtonInactiveColor;
        }

        if(screenWidth == 1920 && screenHeight == 1080)
        {
            Btn1920.GetComponent<Image>().color = ButtonActiveColor;
            Btn1080.GetComponent<Image>().color = ButtonInactiveColor;
        }
        else if (screenWidth == 1280 && screenHeight == 720)
        {
            Btn1080.GetComponent<Image>().color = ButtonActiveColor;
            Btn1920.GetComponent<Image>().color = ButtonInactiveColor;
        }

        float globalVol = AudioManager.Instance.globalVolume;
        float bgmVol = AudioManager.Instance.allBGMVolume;
        float seVol = SoundEffect.Instance.allSoundVolume;

        allVolumeScrollbar.value = globalVol;
        bgmScrollbar.value = bgmVol;
        soundEffectScrollbar.value = seVol;

        //debug all seVol 
        //Debug.Log($"[SettingPageController] SE Volume: {seVol}");
        //Debug.Log($"[SettingPageController] BGM Volume: {bgmVol}");
        //Debug.Log($"[SettingPageController] Global Volume: {globalVol}");

        GameLanguage loadedLanguage = LocalizationManager.Instance.currentLanguage;

        if (JpButton == null || EnButton == null || ChSButton == null || ChTButton == null) return;
        
            if (loadedLanguage.ToString() == "Japanese")
            {
                JpButton.GetComponent<Image>().color = ButtonActiveColor;
                ChSButton.GetComponent<Image>().color = ButtonInactiveColor;
                ChTButton.GetComponent<Image>().color = ButtonInactiveColor;
                EnButton.GetComponent<Image>().color = ButtonInactiveColor;
            }
            else if (loadedLanguage.ToString() == "English")
            {
                EnButton.GetComponent<Image>().color = ButtonActiveColor;
                JpButton.GetComponent<Image>().color = ButtonInactiveColor;
                ChSButton.GetComponent<Image>().color = ButtonInactiveColor;
                ChTButton.GetComponent<Image>().color = ButtonInactiveColor;
            }
            else if (loadedLanguage.ToString() == "ChineseSimple")
            {
                ChSButton.GetComponent<Image>().color = ButtonActiveColor;
                JpButton.GetComponent<Image>().color = ButtonInactiveColor;
                ChTButton.GetComponent<Image>().color = ButtonInactiveColor;
                EnButton.GetComponent<Image>().color = ButtonInactiveColor;
            }
            else if (loadedLanguage.ToString() == "ChineseTrad")
            {
                ChTButton.GetComponent<Image>().color = ButtonActiveColor;
                JpButton.GetComponent<Image>().color = ButtonInactiveColor;
                ChSButton.GetComponent<Image>().color = ButtonInactiveColor;
                EnButton.GetComponent<Image>().color = ButtonInactiveColor;
            }

            if(BtnVibrationOff != null && BtnVibrationWeak != null && BtnVibrationStrong != null)
            {
                if (vibrationType == 0)
                {
                    BtnVibrationOff.GetComponent<Image>().color = ButtonActiveColor;
                    BtnVibrationWeak.GetComponent<Image>().color = ButtonInactiveColor;
                    BtnVibrationStrong.GetComponent<Image>().color = ButtonInactiveColor;
                }
                else if (vibrationType == 1)
                {
                    BtnVibrationWeak.GetComponent<Image>().color = ButtonActiveColor;
                    BtnVibrationOff.GetComponent<Image>().color = ButtonInactiveColor;
                    BtnVibrationStrong.GetComponent<Image>().color = ButtonInactiveColor;
                }
                else if (vibrationType == 2)
                {
                    BtnVibrationStrong.GetComponent<Image>().color = ButtonActiveColor;
                    BtnVibrationOff.GetComponent<Image>().color = ButtonInactiveColor;
                    BtnVibrationWeak.GetComponent<Image>().color = ButtonInactiveColor;
                }
            }

        


    }

    private void Update()
    {
        waitScreenResolutionCnt -= Time.deltaTime;
        if (waitScreenResolutionCnt > 0f) return;

        if (!isSetButtonColorAndPos)
        {
            isSetButtonColorAndPos = true;
            SetButtonColorAndPos();
        }

        resetMenuCnt -= Time.deltaTime;

        if(resetMenuCnt <= 0 && !isAllMenuReset)
        {
            isAllMenuReset = true;
            if (menuAch == null || menuSetting == null || menuShop == null) return;
            menuAch.SetActive(false);
            menuSetting.SetActive(false);
            menuShop.SetActive(false);
        }

    }



}

