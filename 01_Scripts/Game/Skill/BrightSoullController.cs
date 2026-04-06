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

public class BrightSoullController : Singleton<BrightSoullController>
{

    public GameObject soulfairObj;

    public GameObject soulbarObject;

    public GameObject canvasBottomTrans;

    public int birghtSoulCount = 0;
    public int maxBrightSoulCount = 4;

    void Start()
    {
      canvasBottomTrans = GameObject.FindGameObjectWithTag("UiCanvasBottom");

        

    }

    void Update()
    {

        //if press Q key, 
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SoulBarUi3D soulBar = Instantiate(soulbarObject, canvasBottomTrans.transform).GetComponent<SoulBarUi3D>();
            soulBar.headTran = soulfairObj.transform;
        }

       
        
    }
}

