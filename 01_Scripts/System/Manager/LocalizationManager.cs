using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public enum GameLanguage
{
    English,
    Japanese,
    ChineseSimple,
    ChineseTrad,

}

public class LocalizationManager : SingletonA<LocalizationManager>
{

     // A flag to ensure we don't try to switch languages before the system is ready.
    private bool isLocalizationInitialized = false;

    public GameLanguage currentLanguage = GameLanguage.Japanese;

    // The Start method is changed to a Coroutine to wait for initialization.
    IEnumerator Start()
    {
        // Wait until the LocalizationSettings have been initialized.
        yield return LocalizationSettings.InitializationOperation;

        // Now that it's initialized, we can safely switch locales.
        isLocalizationInitialized = true;

        //Debug.Log("Localization Initialized. Press J, E, or C to switch languages.");

        //ChangeLocale("ja");
        //currentLanguage = GameLanguage.Japanese;

        //change locale to currentLanguage
        switch (currentLanguage)
        {
            case GameLanguage.English:
                ChangeLocale("en");
                break;
            case GameLanguage.Japanese:
                ChangeLocale("ja");
                break;
            case GameLanguage.ChineseSimple:
                ChangeLocale("zh-Hans");
                break;
            case GameLanguage.ChineseTrad:
                ChangeLocale("zh-TW");
                break;
            default:
                ChangeLocale("ja");
                break;
        }

    }

    void Update()
    {
        // Don't process any input if the localization system isn't ready.
        if (!isLocalizationInitialized)
        {
            return;
        }

        //// Check for key presses. GetKeyDown fires only once per press.
        //if (Input.GetKeyDown(KeyCode.J))
        //{
        //    // "ja" is the standard code for Japanese.
        //    ChangeLocale("ja"); 
        //    currentLanguage = GameLanguage.Japanese;
        //}
        //else if (Input.GetKeyDown(KeyCode.E))
        //{
        //    // "en" is the standard code for English.
        //    ChangeLocale("en");
        //    currentLanguage = GameLanguage.English;
        //}
        //else if (Input.GetKeyDown(KeyCode.C))
        //{
        //    // "zh-Hans" is the code for Chinese (Simplified) from your screenshot.
        //    ChangeLocale("zh-Hans");
        //    currentLanguage = GameLanguage.ChineseSimple;
        //}
        //else if (Input.GetKeyDown(KeyCode.T))
        //{
        //    // "zh-Hant" is the code for Chinese (Traditional).
        //    ChangeLocale("zh-Hant");
        //    currentLanguage = GameLanguage.ChineseTrad;
        //}
    }

    public void ChangeToChineseS()
    {     
        ChangeLocale("zh-Hans");
        currentLanguage = GameLanguage.ChineseSimple;
        EventManager.EmitEvent("LanguageChanged");  
    }
    public void ChangeToChineseT()
    {
        ChangeLocale("zh-TW");
        currentLanguage = GameLanguage.ChineseTrad;
        EventManager.EmitEvent("LanguageChanged");
    }

    public void ChangeToEnglish()
    {
        ChangeLocale("en");
        currentLanguage = GameLanguage.English;
        EventManager.EmitEvent("LanguageChanged");  
    }

    public void ChangeToJapanese()
    {
        ChangeLocale("ja");
        currentLanguage = GameLanguage.Japanese;
        EventManager.EmitEvent("LanguageChanged");  
    }


    /// <summary>
    /// Changes the active locale to the one specified by the locale code.
    /// </summary>
    /// <param name="localeCode">The standard code for the locale (e.g., "en", "ja", "zh-Hans").</param>
    private void ChangeLocale(string localeCode)
    {
        // Find the target locale from the list of available locales.
        Locale targetLocale = null;
        var availableLocales = LocalizationSettings.AvailableLocales.Locales;

        foreach (var locale in availableLocales)
        {
            // The Identifier contains the locale's code.
            if (locale.Identifier.Code == localeCode)
            {
                targetLocale = locale;
                break; // We found our locale, so we can exit the loop.
            }
        }
        
        // Check if we found a matching locale.
        if (targetLocale != null)
        {
            // Set the found locale as the selected one.
            LocalizationSettings.SelectedLocale = targetLocale;
            //Debug.Log($"Language successfully changed to: {targetLocale.LocaleName}");
        }
        else
        {
            Debug.LogWarning($"Could not find a locale with the code '{localeCode}'. " +
                             "Check if it's added to your project's Localization Settings.");
        }
    }

}

