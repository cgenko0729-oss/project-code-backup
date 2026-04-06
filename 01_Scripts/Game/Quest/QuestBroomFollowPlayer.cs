using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager
using Cysharp.Threading.Tasks;

public class QuestBroomFollowPlayer : MonoBehaviour
{

    public float distNeedToStartFollow = 10f;
    public Vector3 broomPosStart = Vector3.zero;

    public Vector3 followOffset = new Vector3(0, 2, 0);

    public Transform playerTrans;
 
    void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        broomPosStart = transform.position;

    }

    void Update()
    {
        float distToPlayer = Vector3.Distance(broomPosStart, playerTrans.position);
        if (distToPlayer < distNeedToStartFollow)
        {
            //Home to player with MoveTowards
            Vector3 targetPos = playerTrans.position + followOffset;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 5f * Time.deltaTime);


            //RotationX use dotween to loop moving between -70 to -140
            float rotX = Mathf.PingPong(Time.time * 230f, 70f) - 140f;
            Quaternion targetRot = Quaternion.Euler(rotX, 90, 90);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 7.7f * Time.deltaTime);



        }
        else
        {
            //Go back to start position
            transform.position = Vector3.MoveTowards(transform.position, broomPosStart, 5f * Time.deltaTime);

            //fix rotation to -90 90 90
            Quaternion targetRot = Quaternion.Euler(-90, 90, 90);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 7.7f * Time.deltaTime);


        }


    }
}

