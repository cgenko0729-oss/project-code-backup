using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class ParticleSpeedController : MonoBehaviour
{

    public ParticleSystem targetParticleSystem;
     [Range(0.0f, 10.0f)]
    public float simulationSpeed = 1.0f;

    void Start()
    {
        targetParticleSystem = GetComponent<ParticleSystem>();

    }

    void Update()
    {
        var mainModule = targetParticleSystem.main;
        mainModule.simulationSpeed = this.simulationSpeed;
    }

    public void SetSimulationSpeed(float newSpeed)
    {
        this.simulationSpeed = Mathf.Max(0, newSpeed);
    }

}

