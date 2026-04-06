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

public class SkillMoveTracePlayer : MonoBehaviour
{
    Transform playerTrans;

    public float homingSpeed = 5.6f;

    public float distToPlayer = 10;

    SkillModelBase skillModel;

    void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;

        skillModel = GetComponent<SkillModelBase>();

        //ENemyManager also have playerTrans
    }

    void Update()
    {

        Vector3 direction = (playerTrans.position - transform.position);
        direction.y = 0; // Keep the Y position unchanged
        direction.Normalize();

        transform.position += direction * homingSpeed * Time.deltaTime;

        distToPlayer = Vector3.Distance(playerTrans.position, transform.position);
        if (distToPlayer < 1.49f)
        {
            skillModel.DestroySkill();
        }


    }
}

