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

public class TrailerEffectController : MonoBehaviour
{

    public ParticleSystem trailerSmokeEffect;

    public GameObject kappaActorObj;


    void Start()
    {
        trailerSmokeEffect.Stop();
        
    }

    void Update()
    {

        //if press Y key, play trailerSmokeEffect
        if (Input.GetKeyDown(KeyCode.Y))
        {
            trailerSmokeEffect.Play();
            Invoke("StopTrailerSmokeEffect", 2.1f); //2ïbå„Ç…StopTrailerSmokeEffectÇåƒÇ—èoÇ∑
        }

    }

    void StopTrailerSmokeEffect()
    {
        trailerSmokeEffect.Stop();

        Animator animator = kappaActorObj.GetComponent<Animator>();
        animator.SetTrigger("isDie");

    }

}

