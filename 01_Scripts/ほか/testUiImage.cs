using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool


public class testUiImage : MonoBehaviour
{

    public Image img;

    void Start()
    {
        
    }

    void Update()
    {
        
    }


    //OnMouse系の関数はColliderを付ける必要があります。MouseがオブジェクトのColliderに触れると実行されます。
    void OnMouseDown() //マウスがこの3Dオブジェクトに当たったときに実行される
    {
        Debug.Log("OnMouseDown");
    }

    void OnMouseUp() //マウスがこの3Dオブジェクトから離れたときに実行される
    {
        Debug.Log("OnMouseUp");
    }
    void OnMouseDrag() //マウスがこの3Dオブジェクトをドラッグしているときに実行される
    {
        Debug.Log("OnMouseDrag");
    }

    void OnMouseEnter() //マウスがこの3Dオブジェクトに入ったときに実行される
    {
        Debug.Log("OnMouseEnter");
    }

    void OnMouseExit() //マウスがこの3Dオブジェクトから出たときに実行される
    {
        Debug.Log("OnMouseExit");
    }

    void OnMouseOver() //マウスがこの3Dオブジェクトの上にあるときに実行される
    {
        Debug.Log("OnMouseOver");
    }



}

