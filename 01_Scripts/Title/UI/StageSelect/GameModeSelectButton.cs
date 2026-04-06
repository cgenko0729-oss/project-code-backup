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

public class GameModeSelectButton : MonoBehaviour
{
    public enum Mode
    { 
        Normal = 0,
        Endless = 1,
    }

    [Header("選択したモードの表示に必要なデータ")]
    [SerializeField] public Image checkImage;
    [SerializeField] public Mode mode;

    void Update()
    {
        if(mode == Mode.Normal && StageManager.Instance.isEndlessMode == false)
        {
            checkImage.enabled = true;
        }
        else if(mode == Mode.Endless&&StageManager.Instance.isEndlessMode == true)
        {
            checkImage.enabled = true;
        }
        else
        {
            checkImage.enabled = false;
        }
    }

    public void ChangeMode()
    {
        if (mode == Mode.Normal)
        {
            StageManager.Instance.isEndlessMode = false;
        }
        else
        {
            StageManager.Instance.isEndlessMode = true;
        }
    }
}

