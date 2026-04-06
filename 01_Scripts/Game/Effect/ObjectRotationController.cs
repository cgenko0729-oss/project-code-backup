using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class ObjectRotationController : MonoBehaviour
{
    public bool isInitRotation = false;
    public Vector3 startRotation;

    public float fixedRotationX;
    public float fixedRotationY;
    public float fixedRotationZ;

    public bool isFixedRotationX = false;
    public bool isFixedRotationY = false;
    public bool isFixedRotationZ = false;

    public bool isFixedRotation = false;
    public Vector3 fixedRotation;

    void Start()
    {
        if (isInitRotation)
        {
            startRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(startRotation);
        }
        

        

    }

    void Update()
    {
        if(isFixedRotation) transform.rotation = Quaternion.Euler(fixedRotation);
        else
        {
            Vector3 currentRotation = transform.rotation.eulerAngles;

            if (isFixedRotationX) currentRotation.x = fixedRotationX;
            if (isFixedRotationY) currentRotation.y = fixedRotationY;
            if (isFixedRotationZ) currentRotation.z = fixedRotationZ;

            transform.rotation = Quaternion.Euler(currentRotation);
        }

    }
}

