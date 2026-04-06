using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool






public class ObjectFollow : MonoBehaviour
{
    public Transform targetPos;
     
    public float distTokeep = 2.8f;
    public float speed = 4f;

    public bool isMoveByInput = false;
    public bool isFollowPlayer = false;
    public Vector3 followOffset = new Vector3(0,0,0);

    void Start()
    {
        //if(isFollowPlayer) targetPos = GameObject.FindGameObjectWithTag("Player").transform;
        targetPos = GameObject.FindGameObjectWithTag("Player").transform;

    }

    // Update is called once per frame
    void Update()
    {
        if (isFollowPlayer && targetPos != null)
        {
            transform.position = targetPos.position + followOffset;
        }
        else if (isMoveByInput) {
            MoveByInput();
        }else {
            HomingTarget();
        }
        
    }

    void MoveByInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;    
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    void HomingTarget()
    {
        //homing to target until distance is less than distTokeep

        if(!isFollowPlayer || targetPos == null) return;

        if (Vector3.Distance(transform.position, targetPos.position) > distTokeep) {
            Vector3 direction = (targetPos.position - transform.position).normalized;
            direction.y = 0;
            transform.position += direction * Time.deltaTime * speed;


        }

    }

}

