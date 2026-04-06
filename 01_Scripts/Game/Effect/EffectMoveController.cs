using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool

public class EffectMoveController : MonoBehaviour
{
    public Vector3 moveVec;
    public float moveSpd;

    public bool isFixedY = false;
    public float fixedYPos;

    public bool isFixRotX = false;
    public float fixedRotX = 0f;

    public bool isFixRotZ = false;
    public float fixedRotZ = 0f;

    public bool isInitRotX = false;
    public float initRotX = 0f;

    public bool isInitRotY = false;
    public float initRotY = 0f;

    public bool isInitRotZ = false;
    public float initRotZ = 0f;


    void Start()
    {
        
    }

    void Update()
    {
        transform.position += moveVec * moveSpd * Time.deltaTime;

        if(isFixedY)
        {
            Vector3 newPos  = transform.position;
            newPos.y =  fixedYPos;
            transform.position = newPos;
        }

        if (isFixRotX)
        {
            Vector3 newRot = transform.rotation.eulerAngles;
            newRot.x = fixedRotX;
            transform.rotation = Quaternion.Euler(newRot);
        }

        if (isFixRotZ)
        {
            Vector3 newRot = transform.rotation.eulerAngles;
            newRot.z = fixedRotZ;
            transform.rotation = Quaternion.Euler(newRot);
        }

        if (isInitRotX)
        {
            isInitRotX = false;
            Vector3 newRot = transform.rotation.eulerAngles;
            newRot.x = fixedRotX;
            transform.rotation = Quaternion.Euler(newRot);
        }

        if (isInitRotY)
        {
            isInitRotY = false;
            Vector3 newRot = transform.rotation.eulerAngles;
            newRot.y = initRotY;
            transform.rotation = Quaternion.Euler(newRot);
        }

        if (isInitRotZ)
        {
            isInitRotZ = false;
            Vector3 newRot = transform.rotation.eulerAngles;
            newRot.z = initRotZ;
            transform.rotation = Quaternion.Euler(newRot);
        }

    }
}

