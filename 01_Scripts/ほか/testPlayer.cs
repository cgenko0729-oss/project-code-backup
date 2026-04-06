using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool






public class testPlayer : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {

        if(Input.GetKeyDown(KeyCode.E)) {
            EventManager.EmitEvent("HpChange","tag:Player");
            Debug.Log("HpChangeというイベントを発動するよ！");
        }

        EventManager.EmitEvent("HpChange2","tag:Player");

    }
}

