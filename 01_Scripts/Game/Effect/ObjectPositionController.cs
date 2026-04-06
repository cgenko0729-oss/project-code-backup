using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class ObjectPositionController : MonoBehaviour
{

    //like Object Rotation Controller

    public bool isInitPosition = false;
    public Vector3 startPosition;

    public float fixedPositionX;
    public float fixedPositionY;
    public float fixedPositionZ;

    public bool isFixedPositionX = false;
    public bool isFixedPositionY = false;
    public bool isFixedPositionZ = false;
    public bool isFixedPosition = false;

    public Vector3 fixedPosition;





    void Start()
    {
        if (isInitPosition)
        {
            startPosition = transform.position;
            transform.position = startPosition;
        }

        if (isFixedPosition) transform.position = fixedPosition;
        else
        {
            Vector3 currentPosition = transform.position;

            if (isFixedPositionX) currentPosition.x = fixedPositionX;
            if (isFixedPositionY) currentPosition.y = fixedPositionY;
            if (isFixedPositionZ) currentPosition.z = fixedPositionZ;

            transform.position = currentPosition;
        }

    }

    void Update()
    {

        if (isFixedPosition) transform.position = fixedPosition;
        else
        {
            Vector3 currentPosition = transform.position;

            if (isFixedPositionX) currentPosition.x = fixedPositionX;
            if (isFixedPositionY) currentPosition.y = fixedPositionY;
            if (isFixedPositionZ) currentPosition.z = fixedPositionZ;

            transform.position = currentPosition;
        }

    }
}

