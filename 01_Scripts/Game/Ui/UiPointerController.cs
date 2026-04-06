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

public class UiPointerController : MonoBehaviour
{

    public ObjectRepeatMoveController oscillatingObject;
    public Vector2[] targetPositions;
    private int currentIndex = 0;
    private RectTransform rectTransform;


    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

