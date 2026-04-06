using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class DebugManager : SingletonA<DebugManager>
{

    public bool isDebugMode = true;

    void Start()
    {
        
    }

    void Update()
    {
        if (!isDebugMode) return;

        //if (Input.GetKeyDown(KeyCode.Z))
        //{
        //    SoundEffect.Instance.Play(SoundList.DebugkeySe);

        //    PetGetChanceManager.Instance.UnlockAllPets();
        //}
        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    SoundEffect.Instance.Play(SoundList.DebugkeySe);
        //    PetGetChanceManager.Instance.ResetPets();
        //}
        
    }
}

