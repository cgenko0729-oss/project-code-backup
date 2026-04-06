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

public class TitleSettingController : MonoBehaviour
{

    public Texture2D cursorTexNormal;
    public Vector2 hotspot = Vector2.zero;


    void Start()
    {
        Cursor.visible = true;
       
        Cursor.SetCursor(cursorTexNormal, hotspot, CursorMode.Auto);
    }

    void Update()
    {
        
    }
}

