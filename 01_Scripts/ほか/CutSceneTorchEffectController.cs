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

public class CutSceneTorchEffectController : MonoBehaviour
{
    public GameObject fireObj;

    public float setActiveTimer = 0.5f;

    public float disableTimer = 3.5f;

    public bool isSet = false;

    public AudioClip torchSe;

    void Start()
    {
        fireObj.SetActive(false);

    }

    void Update()
    {
        setActiveTimer -= Time.unscaledDeltaTime;
        if (setActiveTimer <= 0f && !isSet)
        {
            isSet = true;
            fireObj.SetActive(true);
            if(torchSe)SoundEffect.Instance.PlayOneSound(torchSe, 0.21f);
        }

        disableTimer -= Time.unscaledDeltaTime;
        if (disableTimer <= 0f)
        {
            fireObj.SetActive(false);
            this.enabled = false;
        }

    }
}

