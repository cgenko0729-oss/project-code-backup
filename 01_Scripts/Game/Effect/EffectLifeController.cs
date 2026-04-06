using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool

public class EffectLifeController : MonoBehaviour
{
    public ObjectPool effectObjectPool;
    public ParticleSystem effectObj;

    public bool isPooled = false;       //プール化されているかどうか
    public bool isDeadOnFinish = true;  //完成したら消える
    public bool isDeadOnLife = false;　　//ライフcntが0になったら消える

    public float lifeCnt = 0f;
    public float lifeCntMax = 0f;

    void OnEnable()
    {
        if (isDeadOnFinish) effectObj.Play();
        
        if (isDeadOnLife) lifeCnt = lifeCntMax;
        
    }

    void Start()
    {
        InitEffectLife();
        InitEffectOnFinish();

    }

    void Update()
    {
        UpdateEffectLife();

    }

    void InitEffectLife()
    {
        if (isDeadOnLife) lifeCnt = lifeCntMax;
    }

    void InitEffectOnFinish()
    {
        if(!isDeadOnFinish) return;
        effectObj = GetComponentInChildren<ParticleSystem>();
        if(!effectObj) effectObj = GetComponent<ParticleSystem>();
        var main = effectObj.main;
        main.stopAction = ParticleSystemStopAction.Callback;

    }

    void UpdateEffectLife()
    {
        if (!isDeadOnLife) return;
        lifeCnt -= Time.deltaTime;

        if (lifeCnt <= 0) {
            if (isPooled) {
                lifeCnt = lifeCntMax;
                effectObjectPool.Release(gameObject);
            }
            else Destroy(gameObject);
                        
        }


    }

    void OnParticleSystemStopped()
    {
        effectObjectPool.Release(gameObject);
    }

}

