using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool






public class testUi : MonoBehaviour
{
    void Start()
    {
        EventManager.StartListening("HpChange", ChangeHpBar);
        

    }

    public void ChangeHpBar()
    {
        Image img = GetComponent<Image>();
        img.color = Color.red;
        Debug.Log("イベント参加しました。HpBarの色を変更しました");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            ChangeHpBar();
        }

    }
}

