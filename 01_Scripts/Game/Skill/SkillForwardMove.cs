using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillForwardMove : MonoBehaviour
{
    public Vector3 moveVec;

    void Start()
    {
        
    }

    void Update()
    {
        if (moveVec != Vector3.zero)
        {
            transform.position += moveVec * Time.deltaTime;
        }

    }
}

