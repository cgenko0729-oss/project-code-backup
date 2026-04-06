using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class ObjectColliderSwitcher : MonoBehaviour
{
    Collider collder;

    public float disableColliderInterval = 0.3f; // Time in seconds to disable the collider
    public float disableColliderTimer = 1f;
    public float disableColliderTimerMax = 1f;
    void Start()
    {
        collder = GetComponent<Collider>();

    }

    void Update()
    {
        disableColliderTimer -= Time.deltaTime;
        if (disableColliderTimer <= 0)
        {
            DisableAndEnableCollider();
            disableColliderTimer = disableColliderTimerMax; // Reset the timer
        }

    }

    public void DisableAndEnableCollider()
    {
        collder.enabled = false;

        Invoke("EnableCollider", disableColliderInterval);

    }

    public void EnableCollider()
    {
        collder.enabled = true;
    }


}

