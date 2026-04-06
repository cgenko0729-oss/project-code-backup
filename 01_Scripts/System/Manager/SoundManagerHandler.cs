using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine

using Hellmade.Sound;


public class SoundManagerHandler : Singleton<SoundManagerHandler>
{

    public float globalAudioVolume;
    public float globalSoundEffectVolume;
    public float globalMusicVolume;
    public float globalUiSoundVolume;

    void Start()
    {
        
    }

    void Update()
    {
        globalAudioVolume = EazySoundManager.GlobalVolume;
        globalSoundEffectVolume = EazySoundManager.GlobalSoundsVolume;
        globalMusicVolume = EazySoundManager.GlobalMusicVolume;
        globalUiSoundVolume = EazySoundManager.GlobalUISoundsVolume;

    }
}

