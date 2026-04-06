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

public class MapEndlessReturnPortal : MonoBehaviour
{
   

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Enter Return Portal");
            EventManager.EmitEvent("isGameClear");
        }
    }

    void Start()
    {

        this.gameObject.SetActive(true); 

        ParticleSystem ps = GetComponent<ParticleSystem>();
        ps.Play();
        
    }

    void Update()
    {
        
    }
}

