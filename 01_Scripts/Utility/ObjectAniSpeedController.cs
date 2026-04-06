using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class ObjectAniSpeedController : MonoBehaviour
{
    public Animator animator;
    public float aniSpeed = 1f;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = aniSpeed;
    }

    void Update()
    {
        
    }
}

