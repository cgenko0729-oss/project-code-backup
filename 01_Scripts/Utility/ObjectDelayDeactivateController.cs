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
using System.Collections;

public class ObjectDelayDeactivateController : MonoBehaviour
{

    public float delayToCloseTime = 0.14f;

    private IEnumerator Start()
    {
        
        yield return new WaitForSeconds(delayToCloseTime);
        gameObject.SetActive(false);
    }

    void Update()
    {
        
    }
}

