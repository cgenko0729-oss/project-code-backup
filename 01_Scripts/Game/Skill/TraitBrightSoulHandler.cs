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

public class TraitBrightSoulHandler : MonoBehaviour
{
    public ParticleSystem soulPs;
    public ObjectPool soulPool;

    public float lifeCnt = 3f;

    private void OnEnable()
    {
        lifeCnt = 3f;
        soulPs.Play();
    }

    void Start()
    {

    }

    void Update()
    {
        lifeCnt -= Time.deltaTime;

        if(lifeCnt <= 0)
        {
            soulPs.Stop();
            soulPool.Release(this.gameObject);

        }

    }
}

