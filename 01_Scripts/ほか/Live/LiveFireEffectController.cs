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

public class LiveFireEffectController : MonoBehaviour
{

    public ParticleSystem fire1;
    public ParticleSystem fire2;
    public ParticleSystem fire3;
    public ParticleSystem fire4;

    void Start()
    {
        
    }

    void Update()
    {
        //if press F key , play all fire effects ,and stop them after 1 second 
        if (Input.GetKeyDown(KeyCode.F))
        {
            fire1.Play();
            fire2.Play();
            fire3.Play();
            fire4.Play();
           DOVirtual.DelayedCall(1f, () => {
               fire1.Stop();
               fire2.Stop();
               fire3.Stop();
               fire4.Stop();
           });

        }

    }
}

