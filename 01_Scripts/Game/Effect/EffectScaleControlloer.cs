using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class EffectScaleControlloer : MonoBehaviour
{

    //ParticleSystem ps;
    public Vector3 baseScale = Vector3.one; // 基本のスケール
    public float effectScale = 0f; // エフェクトのスケール

    void Start()
    {
        baseScale = transform.localScale;

        Vector3 newScale = new Vector3(baseScale.x * (1 + effectScale),baseScale.y * (1 + effectScale),baseScale.z * (1 + effectScale));
        transform.localScale = newScale;
        
        
    }

    void Update()
    {
        
    }

   

}

