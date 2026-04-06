using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool


public class EffectCollisionController : MonoBehaviour
{

    public float collisionStartTime;
    public float collisionEndTime;

    void Start()
    {
        Invoke("EnableCollision", collisionStartTime);
        Invoke("DisableCollision", collisionEndTime);

    }


    private void OnEnable()
    {
        Invoke("EnableCollision", collisionStartTime);
        Invoke("DisableCollision", collisionEndTime);
    }

    public void EnableCollision()
    {
        gameObject.GetComponent<Collider>().enabled = true;
    }

    public void DisableCollision()
    {
        gameObject.GetComponent<Collider>().enabled = false;
    }



}

