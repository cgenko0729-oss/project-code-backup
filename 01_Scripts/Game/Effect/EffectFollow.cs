using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine

public class EffectFollow : MonoBehaviour
{
   
    public float yOffset = 1.0f; //above player

    ParticleSystem ps;
    Transform playerTrans;

    void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        ps          = GetComponent<ParticleSystem>();

        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        transform.SetParent(playerTrans);
        transform.localRotation = Quaternion.Euler(0, 180f, 0);
        transform.localPosition = new Vector3(0, yOffset, 0);
    }

    void LateUpdate()
    {
        transform.localPosition = new Vector3(0, yOffset, 0);
    }
}

