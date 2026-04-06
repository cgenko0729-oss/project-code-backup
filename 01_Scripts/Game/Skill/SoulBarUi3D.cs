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

public class SoulBarUi3D : MonoBehaviour
{
    public Camera mainCam;

    public Transform headTran;
    public Vector3 headOffset = new Vector3(-2, 2f, 0);

    public Image soulBarImage;

    public Image soulBarFrame;


    void Start()
    {
        mainCam = Camera.main;
        
    }

    void Update()
    {
        Vector3 worldPos = headTran.position;
        worldPos += headOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

        transform.position = screenPos;

        float soulRatio = BrightSoullController.Instance.birghtSoulCount / BrightSoullController.Instance.maxBrightSoulCount;
        soulBarImage.fillAmount = soulRatio;
        
    }
}

