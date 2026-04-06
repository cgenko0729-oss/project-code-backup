using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class SettingManager : SingletonA<SettingManager>
{
    // PlayerPrefs keys
    const string PP_RES_W = "pp_res_w";
    const string PP_RES_H = "pp_res_h";
    const string PP_FS_MODE = "pp_fs_mode"; // int of FullScreenMode
    const string PP_VSYNC = "pp_vsync";     // 0/1/2
    const string PP_FPS = "pp_fps";         // -1 = Unlimited
    const string PP_QUALITY = "pp_quality"; // index

    readonly int[] fpsOptions = new[] { 30, 60, -1 }; // -1 = Unlimited
    int fpsIndex = 0;

    //Resolution
    public Button Btn1920;
    public Button Btn1080;

    public Button BtnFullScreen;
    public Button BtnNotFullScreen;

    public bool isFullScreen = false;

    //Fps
    public Button Btn60fps;
    public Button Btn120fps;
    public Button BtnUnlimited;

    //VSync
    public Button BtnVSyncOn;
    public Button BtnVSyncOff;

    public Button CnSButton;
    public Button CnTButton;
    public Button JpButton;
    public Button EnButton;

    public float applySettingsDelay = 1.4f; // seconds to wait before saving settings to file
    public bool isApplySettingsDelay = false;

    public Scrollbar allVolumeScrollbar;
    public Scrollbar bgmScrollbar;
    public Scrollbar soundEffectScrollbar;

    protected override void OnAwake()
    {
        ApplySavedSettingsOrDefaults();
    }

 

    void Update()
    {

        applySettingsDelay -= Time.unscaledDeltaTime;
        if (applySettingsDelay <= 0 && !isApplySettingsDelay)
        {
            ApplySavedSettingsOrDefaults();
            isApplySettingsDelay = true;
            //applySettingsDelay = 1.4f;
        }

        // ---- Hotkeys (works with both old/new input systems) ----
        //#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        //        var kb = UnityEngine.InputSystem.Keyboard.current;
        //        if (kb == null) return;

        //        if (kb.digit1Key.wasPressedThisFrame)  SetResolutionWindowed(1280, 720);
        //        if (kb.digit2Key.wasPressedThisFrame)  SetResolutionWindowed(1920, 1080);
        //        if (kb.digit3Key.wasPressedThisFrame)  ToggleBorderlessFullscreen();
        //        if (kb.digit4Key.wasPressedThisFrame)  SetExclusiveFullscreenNative();

        //        if (kb.digit5Key.wasPressedThisFrame)  ToggleVSync();
        //        if (kb.digit6Key.wasPressedThisFrame)  CycleTargetFps();

        //        if (kb.f1Key.wasPressedThisFrame)      SetQualityLevelByNameOrIndex("Low", 0);
        //        if (kb.f2Key.wasPressedThisFrame)      SetQualityLevelByNameOrIndex("Medium", 1);
        //        if (kb.f3Key.wasPressedThisFrame)      SetQualityLevelByNameOrIndex("High", 2);
        //#else


        //Key Debug
        //if (Input.GetKeyDown(KeyCode.Alpha1))  SetResolutionWindowed(1280, 720);
        //if (Input.GetKeyDown(KeyCode.Alpha2))  SetResolutionWindowed(1920, 1080);
        //if (Input.GetKeyDown(KeyCode.Alpha3))  ToggleBorderlessFullscreen();
        //if (Input.GetKeyDown(KeyCode.Alpha4))  SetExclusiveFullscreenNative();

        //if (Input.GetKeyDown(KeyCode.Alpha5))  ToggleVSync();
        //if (Input.GetKeyDown(KeyCode.Alpha6))  CycleTargetFps();

        //if (Input.GetKeyDown(KeyCode.F1))      SetQualityLevelByNameOrIndex("Low", 0);
        //if (Input.GetKeyDown(KeyCode.F2))      SetQualityLevelByNameOrIndex("Medium", 1);
        //if (Input.GetKeyDown(KeyCode.F3))      SetQualityLevelByNameOrIndex("High", 2);
    }

    // ---------- Public APIs (wire these to UI buttons / dropdowns) ----------

    public void SaveSettingsNow()
    {
        SettingsSaveSystem.SaveFromRuntime();   // writes Screen/Quality/VSYNC/FPS to .../settings.dat
    }

    public void SetResolutionWindowed(int width, int height, int refreshHz = 0)
    {
        var rr = GetBestRefreshRate(refreshHz);
        Screen.SetResolution(width, height, FullScreenMode.Windowed, rr);
        SaveResolution(width, height, FullScreenMode.Windowed);
        Debug.Log($"[Settings] Windowed {width}x{height}@{rr.value}Hz");

        SaveSettingsNow(); //Delay this saving for 2 second may help update the correct resolution
    }

    public void Set1920X1080Window()
    {

        
            SetResolutionWindowed(1920, 1080);
        
       
        
       
    }

    public void Set1080X720Window()
    {
        
            SetResolutionWindowed(1280, 720);
        

        
       

    }

    public void SetFullScreenB(int width, int height, int refreshHz = 0)
    {
        var rr = GetBestRefreshRate(refreshHz);
        Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow, rr);
        SaveResolution(width, height, FullScreenMode.FullScreenWindow);
        SaveSettingsNow();
    }
    
    public void SetBorderlessFullscreen(int width, int height)
    {
        var rr = GetBestRefreshRate(0);
        Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow, rr);
        SaveResolution(width, height, FullScreenMode.FullScreenWindow);
        Debug.Log($"[Settings] Borderless set to {width}x{height}@{rr.value}Hz");
        SaveSettingsNow();
        isFullScreen = true;
    }

    public void SetBorderlessFullscreen()
    {

        //if(!BtnNotFullScreen || !BtnFullScreen) return;

        int width = Screen.width;
        int height = Screen.height; 
        

        var rr = GetBestRefreshRate(0);
        Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow, rr);
        SaveResolution(width, height, FullScreenMode.FullScreenWindow);
        Debug.Log($"[Settings] Borderless {width}x{height}@{rr.value}Hz");

        SaveSettingsNow();
        isFullScreen = true;
    }

    public void SetNotBorderlessFullscreen()
    {
       
        SetResolutionWindowed(Screen.width, Screen.height);
        SaveResolution(Screen.width, Screen.height, FullScreenMode.Windowed);

        SaveSettingsNow();
        isFullScreen = false;
    }

    public void SetExclusiveFullscreenNative(int refreshHz = 0)
    {

        //SetBorderlessFullscreen(Screen.width, Screen.height);

       

        // Use the current displayfs native resolution
//        var native = Screen.currentResolution;
//#if UNITY_2022_2_OR_NEWER
//        var rr = refreshHz > 0 ? new RefreshRate { numerator = (uint)(refreshHz), denominator = 1 } : native.refreshRateRatio;
//        Screen.SetResolution(native.width, native.height, FullScreenMode.ExclusiveFullScreen, rr);
//        Debug.Log($"[Settings] Exclusive {native.width}x{native.height}@{rr.value}Hz");
//#else
//        int rr = (refreshHz > 0) ? refreshHz : native.refreshRate;
//        Screen.SetResolution(native.width, native.height, true, rr);
//        Debug.Log($"[Settings] Exclusive {native.width}x{native.height}@{rr}Hz");
//#endif
//        SaveResolution(native.width, native.height, FullScreenMode.ExclusiveFullScreen);


        SaveSettingsNow();

      
    }

    public void ToggleBorderlessFullscreen()
    {
        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
        {
            // go back to previous windowed size or a sane default
            int w = PlayerPrefs.GetInt(PP_RES_W, 1920);
            int h = PlayerPrefs.GetInt(PP_RES_H, 1080);
            SetResolutionWindowed(w, h);
        }
        else
        {
            // set to current display's native size in borderless mode
            var native = Screen.currentResolution;
#if UNITY_2022_2_OR_NEWER
            Screen.SetResolution(native.width, native.height, FullScreenMode.FullScreenWindow, native.refreshRateRatio);
#else
            Screen.SetResolution(native.width, native.height, FullScreenMode.FullScreenWindow, native.refreshRate);
#endif
            SaveResolution(native.width, native.height, FullScreenMode.FullScreenWindow);
            Debug.Log($"[Settings] Toggled to Borderless {native.width}x{native.height}");
        }
    }

    public void SetQualityLevel(int index, bool applyExpensiveChanges = true)
    {
        index = Mathf.Clamp(index, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(index, applyExpensiveChanges);
        PlayerPrefs.SetInt(PP_QUALITY, index);
        Debug.Log($"[Settings] Quality: {QualitySettings.names[index]} ({index})");
    }

    public void SetQualityLevelByNameOrIndex(string nameFallback, int indexFallback)
    {
        int idx = System.Array.IndexOf(QualitySettings.names, nameFallback);
        if (idx < 0) idx = Mathf.Clamp(indexFallback, 0, QualitySettings.names.Length - 1);
        SetQualityLevel(idx);
    }

    public void SetVSync(int vSyncCount) // 0=Off, 1=Every VBlank, 2=Every 2nd VBlank
    {
        vSyncCount = Mathf.Clamp(vSyncCount, 0, 2);
        QualitySettings.vSyncCount = vSyncCount;
        PlayerPrefs.SetInt(PP_VSYNC, vSyncCount);
        Debug.Log($"[Settings] VSync: {vSyncCount}");
    }

    public void SetVSyncOn()
    {
        SetVSync(1);
 
        SaveSettingsNow();

      
    }

    public void SetVSyncOff()
    {
        SetVSync(0);
        
        SaveSettingsNow();

     
    }

    public void ToggleVSync()
    {
        int next = (QualitySettings.vSyncCount == 0) ? 1 : 0;
        SetVSync(next);
    }

    public void SetTargetFPS(int fps) // -1 = unlimited
    {
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt(PP_FPS, fps);
        Debug.Log($"[Settings] Target FPS: {(fps < 0 ? "Unlimited" : fps.ToString())}");

        SaveSettingsNow();

       


        
    }

    

    public void CycleTargetFps()
    {
        fpsIndex = (fpsIndex + 1) % fpsOptions.Length;
        SetTargetFPS(fpsOptions[fpsIndex]);
    }

    // ---------- Helpers & persistence ----------

#if UNITY_2022_2_OR_NEWER
    static RefreshRate GetBestRefreshRate(int preferredHz)
    {
        if (preferredHz > 0) return new RefreshRate { numerator = (uint)preferredHz, denominator = 1 };
        return Screen.currentResolution.refreshRateRatio; // safest default
    }
#else
    static int GetBestRefreshRate(int preferredHz)
    {
        return (preferredHz > 0) ? preferredHz : Screen.currentResolution.refreshRate;
    }
#endif

    void SaveResolution(int w, int h, FullScreenMode mode)
    {
        PlayerPrefs.SetInt(PP_RES_W, w);
        PlayerPrefs.SetInt(PP_RES_H, h);
        PlayerPrefs.SetInt(PP_FS_MODE, (int)mode);
        PlayerPrefs.Save();
    }

    public void SetChineseSimLanguage()
    {
        
        LocalizationManager.Instance.ChangeToChineseS();

        //i also want to save this langauge setting so the next time the game open it will be the same language
        SaveSettingsNow();

        if(!CnSButton || !CnTButton || !JpButton || !EnButton) return; //safety check (in case the button is not assigned in the inspector

       
    }

    public void SetChineseTradLanguage()
    {
        
        LocalizationManager.Instance.ChangeToChineseT();

        SaveSettingsNow();

       
    }

    public void SetJapaneseLanguage()
    {
        
        LocalizationManager.Instance.ChangeToJapanese();
        SaveSettingsNow();

       
    }

    public void SetEnglishLanguage()
    {
        
        LocalizationManager.Instance.ChangeToEnglish();

        SaveSettingsNow();

       
    }

   


    void ApplySavedSettingsOrDefaults()
{
    // 1) Load values from the file. If file missing, we get sane defaults back.
    bool loaded = SettingsSaveSystem.Load(
        out int w,
        out int h,
        out FullScreenMode mode,
        out int vSync,
        out int fps,
        out int quality,
        out int lang,
        out float globalVolume,
        out float bgmVolume,
        out float seVolume,
        out int vibrateType
    );

    // 2) Apply them to the runtime
#if UNITY_2022_2_OR_NEWER
    var rr = Screen.currentResolution.refreshRateRatio;
    Screen.SetResolution(w, h, mode, rr);
#else
    int rr = Screen.currentResolution.refreshRate;
    if (mode == FullScreenMode.ExclusiveFullScreen)
        Screen.SetResolution(w, h, true, rr);
    else
        Screen.SetResolution(w, h, mode, rr);
#endif

    QualitySettings.SetQualityLevel(quality, true);
    QualitySettings.vSyncCount = Mathf.Clamp(vSync, 0, 2);
    Application.targetFrameRate = fps;

        // 3) Update any UI highlight states (optional: implement based on your button scheme)
        // Example:
        //BtnVSyncOn.GetComponent<Image>().color = (vSync > 0) ? ButtonActiveColor : ButtonInactiveColor;
        //BtnVSyncOff.GetComponent<Image>().color = (vSync == 0) ? ButtonActiveColor : ButtonInactiveColor;

        //if (fps == 60)
        //{
        //    Btn60fps.GetComponent<Image>().color = ButtonActiveColor;
        //    Btn120fps.GetComponent<Image>().color = ButtonInactiveColor;
        //    BtnUnlimited.GetComponent<Image>().color = ButtonInactiveColor;
        //}
        //else if (fps == 120)
        //{
        //    Btn120fps.GetComponent<Image>().color = ButtonActiveColor;
        //    Btn60fps.GetComponent<Image>().color = ButtonInactiveColor;
        //    BtnUnlimited.GetComponent<Image>().color = ButtonInactiveColor;
        //}
        //else
        //{
        //    BtnUnlimited.GetComponent<Image>().color = ButtonActiveColor;
        //    Btn60fps.GetComponent<Image>().color = ButtonInactiveColor;
        //    Btn120fps.GetComponent<Image>().color = ButtonInactiveColor;
        //}

        //// Resolution button highlights
        //if (mode == FullScreenMode.Windowed && w == 1920 && h == 1080)
        //{
        //    Btn1920.GetComponent<Image>().color = ButtonActiveColor;
        //    Btn1080.GetComponent<Image>().color = ButtonInactiveColor;
        //    BtnFullScreen.GetComponent<Image>().color = ButtonInactiveColor;
        //}
        //else if (mode == FullScreenMode.Windowed && w == 1280 && h == 720)
        //{
        //    Btn1080.GetComponent<Image>().color = ButtonActiveColor;
        //    Btn1920.GetComponent<Image>().color = ButtonInactiveColor;
        //    BtnFullScreen.GetComponent<Image>().color = ButtonInactiveColor;
        //}
        //else if (mode == FullScreenMode.FullScreenWindow || mode == FullScreenMode.ExclusiveFullScreen)
        //{
        //    BtnFullScreen.GetComponent<Image>().color = ButtonActiveColor;
        //    Btn1920.GetComponent<Image>().color = ButtonInactiveColor;
        //    Btn1080.GetComponent<Image>().color = ButtonInactiveColor;
        //}

        //Load Language
        GameLanguage loadedLanguage = (GameLanguage)lang;
        switch (loadedLanguage)
        {
            case GameLanguage.English:
                SetEnglishLanguage();
                break;
            case GameLanguage.Japanese:
                SetJapaneseLanguage();
                break;
            case GameLanguage.ChineseSimple:
                SetChineseSimLanguage();
                break;
            case GameLanguage.ChineseTrad:
                SetChineseTradLanguage();
                break;
        }

        //Debug orginal sound value:
       //Debug.Log("Original Global Volume: " + AudioManager.Instance.globalVolume);
       //Debug.Log("Original BGM Volume: " + AudioManager.Instance.allBGMVolume);
       //Debug.Log("Original SE Volume: " + SoundEffect.Instance.allSoundVolume);

        AudioManager.Instance.globalVolume = globalVolume;
        AudioManager.Instance.allBGMVolume = bgmVolume;
        SoundEffect.Instance.allSoundVolume = seVolume;

        //Debug all sound value
        //Debug.Log("Load Global Volume: " + globalVolume);
        //Debug.Log("Load BGM Volume: " + bgmVolume);
        //Debug.Log("Load SE Volume: " + seVolume);

        SoundEffect.Instance.controllerVibrationType = vibrateType;

        // 4) If this was the very first run (no file), persist the defaults to create the file now.
        if (!loaded)
        SaveSettingsNow();

        if (bgmScrollbar != null)
        {
            bgmScrollbar.value = bgmVolume;
        }
        if (soundEffectScrollbar != null)
        {
            soundEffectScrollbar.value = seVolume;
        }
        if (allVolumeScrollbar != null)
        {
            allVolumeScrollbar.value = globalVolume;
        }

        Debug.Log($"[Settings] Applied file settings: {w}x{h} {mode}, VSync={vSync}, FPS={(fps < 0 ? "Unlimited" : fps.ToString())}, Quality={QualitySettings.names[quality]}, Language={loadedLanguage}, GlobalVol={globalVolume}, BGMVol={bgmVolume}, SEVol={seVolume}");
    }


}

