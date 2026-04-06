using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class BossBallRotaion : MonoBehaviour
{

    public float rotSpeedY = 7f;
    public float rotSpeedX = 7f;
    public float rotSpeedZ = 7f;

    public bool isRotY = true;
    public bool isRotX = false;
    public bool isRotZ = false;

    void Start()
    {
        
    }

    void Update()
    {
        DoRotX();
        DoRotY();
        DoRotZ();

    }


    public void DoRotY()
    {
        if (!isRotY) return;
        transform.Rotate(0, rotSpeedY, 0);
        if (transform.rotation.eulerAngles.y > 360f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void DoRotX()
    {
        if (!isRotX) return;
        transform.Rotate(rotSpeedX, 0, 0);
        if (transform.rotation.eulerAngles.x > 360f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void DoRotZ()
    {
        if (!isRotZ) return;
        transform.Rotate(0, 0, rotSpeedZ);
        if (transform.rotation.eulerAngles.z > 360f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

}

