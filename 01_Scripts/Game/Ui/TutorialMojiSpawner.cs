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
using DamageNumbersPro;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;

public class TutorialMojiSpawner : MonoBehaviour
{
    public DamageNumber tutorPopup;
    public DamageNumber tutorText;

    /*
    WASD移動
Spaceキーでダッシュ
ESCキーで設定メニュー
TABキーでポーズメニュー 

     */

    void Start()
    {
        string tutorString = "";
       if(Gamepad.current != null)   tutorString  = LocalizationSettings.StringDatabase.GetLocalizedString("Enums", "Game.KeyToturial");
        else tutorString = LocalizationSettings.StringDatabase.GetLocalizedString("Enums", "Game.KeyToturialKeyBoard");
        
        tutorPopup =tutorText.Spawn(Vector3.zero,tutorString); 
        
    }

    void Update()
    {
        
    }
}

