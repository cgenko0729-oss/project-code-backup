using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class TraitManager : Singleton<TraitManager>
{
    public float turnGobalDamage = 0f;
    public float turnGobalCooldown = 0f;
    public float turnGobalDuration = 0f;
    public float turnGobalSpeed = 0f;
    public float turnGobalSize = 0f;
    public int turnGobalProjectileNum = 0;

    public float turnCritChanceAdd = 0f;

    public TraitData universalTrait;
    
    public void SetTrait(TraitData traitData)
    {


    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

