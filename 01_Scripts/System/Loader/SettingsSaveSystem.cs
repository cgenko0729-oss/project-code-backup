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

public class SettingsSaveSystem : MonoBehaviour
{
    private const string FILE = "settings"; // .../Application.persistentDataPath/settings.dat C:\Users\<User>\AppData\LocalLow\<CompanyName>\<ProductName>\settings.dat

    // Write current runtime values
    public static void SaveFromRuntime()
    {
        var f = new EasyFileSave(FILE);
        f.Add("w", Screen.width);
        f.Add("h", Screen.height);
        f.Add("mode", (int)Screen.fullScreenMode);
        f.Add("vSync", QualitySettings.vSyncCount);
        f.Add("fps", Application.targetFrameRate);
        f.Add("quality", QualitySettings.GetQualityLevel());

        if (LocalizationManager.Instance != null)
        {
            f.Add("lang", (int)LocalizationManager.Instance.currentLanguage);
        }

        f.Add("globalVolume", AudioManager.Instance.globalVolume);
        f.Add("bgmVolume", AudioManager.Instance.allBGMVolume);
        f.Add("seVolume", SoundEffect.Instance.allSoundVolume);

        //Debug all sound value
        //Debug.Log("Save Global Volume: " + AudioManager.Instance.globalVolume);
        //Debug.Log("Save BGM Volume: " + AudioManager.Instance.allBGMVolume);
        //Debug.Log("Save SE Volume: " + SoundEffect.Instance.allSoundVolume);

        f.Add("controllerVibrationType", SoundEffect.Instance.controllerVibrationType);

        f.Save();
    }

    // Load values (returns false if file missing)
    public static bool Load(out int w, out int h, out FullScreenMode mode, out int vSync, out int fps, out int quality, out int lang,out float globalVolume, out float bgmVolume, out float seVolume, out int vibrationType )
    {
        var f = new EasyFileSave(FILE);
        if (!f.Load())
        {
            w = 1920; h = 1080; mode = FullScreenMode.Windowed; vSync = 1; fps = -1; quality = QualitySettings.GetQualityLevel();
            
            SystemLanguage systemLang = Application.systemLanguage;
            
            // 2. Default to English (Safe fallback)
            lang = (int)GameLanguage.English;

            // 3. Check for specific languages
            switch (systemLang)
            {
                case SystemLanguage.Japanese:
                    lang = (int)GameLanguage.Japanese;
                    break;

                // Unity might return "Chinese" for simplified generally, 
                // or specific "ChineseSimplified"
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    lang = (int)GameLanguage.ChineseSimple;
                    break;

                case SystemLanguage.ChineseTraditional:
                    lang = (int)GameLanguage.ChineseTrad;
                    break;
            }
                
                // If the system is English or any other language (German, French, etc.),
                // it remains (int)GameLanguage.English as defined above.

            globalVolume = 1.0f;
            bgmVolume = 1.0f;
            seVolume = 1.0f;
            vibrationType = 0;

            return false;
        }

        w       = f.GetInt("w", 1920);
        h       = f.GetInt("h", 1080);
        mode    = (FullScreenMode)f.GetInt("mode", (int)FullScreenMode.Windowed);
        vSync   = f.GetInt("vSync", 1);
        fps     = f.GetInt("fps", -1);
        quality = f.GetInt("quality", Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, QualitySettings.names.Length-1));

        lang = f.GetInt("lang", (int)GameLanguage.English);
            

        globalVolume = f.GetFloat("globalVolume", 1.0f);
        bgmVolume    = f.GetFloat("bgmVolume", 1.0f);
        seVolume     = f.GetFloat("seVolume", 1.0f);
        vibrationType = f.GetInt("controllerVibrationType", 0);

        //Debug all sound value
        //Debug.Log("Load Global Volume: " + globalVolume);
        //Debug.Log("Load BGM Volume: " + bgmVolume);
        //Debug.Log("Load SE Volume: " + seVolume);

        f.Dispose();
        return true;
    }

}

