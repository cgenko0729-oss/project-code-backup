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

public class BossAnubisThunderPillarController : MonoBehaviour
{

    public float enablEffectCnt = 2.8f;

    public ParticleSystem thunderCircleEffect;
    public ParticleSystem dustEffect;

    public SphereCollider col;

    

    void Start()
    {
        thunderCircleEffect.Stop();

        col = GetComponent<SphereCollider>();

        //dotweenMove posY to 0 in 0.3f
        transform.DOMoveY(0.21f, 0.21f).SetEase(Ease.OutBounce).onComplete = () => {
            dustEffect.Play();
            //HESoundManager.Play("ThunderPillar_Land");
        };


    }

    void Update()
    {
        enablEffectCnt -= Time.deltaTime;
        if (enablEffectCnt <= 0f)
        {
            //thunderCircleEffect.Stop();
            //thunderCircleEffect.Play();
            thunderCircleEffect.gameObject.SetActive(true);

            col.enabled = true;
        }

    }
}

