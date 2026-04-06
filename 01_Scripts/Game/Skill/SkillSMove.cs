using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillSMove : MonoBehaviour
{
    [Header("Path Settings")]
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float amplitude    = 2f;
    [SerializeField] private float waveLength   = 5f;
    [SerializeField] private bool  startToRight = true;

    private float angularFreq;
    private float distanceTravel;
    private Vector3 startPos;
    public bool isActivated = false;

    private void OnEnable()
    {
        distanceTravel = 0f;     
        startPos       = transform.position;
    }

    public void Configure(Vector3 moveDir, float speed, float amp,float waveLen, bool firstBendRight)
    {
        transform.rotation = Quaternion.LookRotation(moveDir.normalized, Vector3.up);

        forwardSpeed = speed;
        amplitude    = amp;
        waveLength   = Mathf.Max(0.01f, waveLen);   // avoid div-by-zero
        startToRight = firstBendRight;

        angularFreq  = 2f * Mathf.PI / waveLength;

        startPos     = transform.position;
        distanceTravel = 0f;
    }
    private void Update()
    {
        if (!isActivated) return;

        float d = forwardSpeed * Time.deltaTime;
        distanceTravel += d;

        Vector3 forwardOffset = transform.forward * distanceTravel;

        float phase   = startToRight ? 0f : Mathf.PI;
        float side    = amplitude * Mathf.Sin(angularFreq * distanceTravel + phase);
        Vector3 sideOffset = transform.right * side;

        transform.position = startPos + forwardOffset + sideOffset;

        // orient mesh along velocity        
        //Vector3 vel = transform.forward * forwardSpeed + transform.right  * (amplitude * angularFreq *Mathf.Cos(angularFreq * distanceTravel + phase) *forwardSpeed);
        //transform.rotation = Quaternion.LookRotation(vel, Vector3.up);
        
    }

}

