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

public class SkillRotateMove : MonoBehaviour
{

    public float rotateSpeed = 180f;

    void Start()
    {
        
    }

    void Update()
    {
        //keep rotating Y axis
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

    }
}

