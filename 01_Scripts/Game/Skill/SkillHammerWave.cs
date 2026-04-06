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

public class SkillHammerWave : MonoBehaviour
{
    public GameObject waveEffect;
    public GameObject waveCollider;
    public float colLifeTime = 3.0f;
    public Vector3 defaultScale = Vector3.zero;
    public Vector3 finalScale = Vector3.zero;
    public Vector3 waveScaleOffset = Vector3.zero;
    public bool isFinalSkill = false;
    public float elapsedTime = 0;

    private ParticleSystem wavePs;

    public void Init()
    {
        elapsedTime = 0;
        if (isFinalSkill == true)
        {
            waveEffect.SetActive(true);
            wavePs = waveEffect.GetComponent<ParticleSystem>();
            if (wavePs != null)
            {
                wavePs.Play();
            }

            waveCollider.transform.localScale = Vector3.zero;
            waveCollider.SetActive(true);
        }
    }

    void Update()
    {
        if(isFinalSkill == false) { return; }

        elapsedTime += Time.deltaTime;
        if(elapsedTime >= colLifeTime)
        {
            waveEffect.SetActive(false);
            elapsedTime = 0;
            return;
        }

        float rate = elapsedTime / colLifeTime;
        Vector3 scale = Vector3.Lerp(defaultScale, finalScale, rate);
        waveCollider.transform.localScale = scale;

        if(wavePs != null)
        {
            waveCollider.SetActive(wavePs.isPlaying);
        }
    }
}

