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

public class PlayerMageShadowController : MonoBehaviour
{
    public Animator animator;

    public float lifeCnt = 3f;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play("Idle_Battle");
        animator.SetTrigger("Attack");
    }



    void Update()
    {
        lifeCnt -= Time.deltaTime;
        if (lifeCnt <= 0f)
        {
            Destroy(this.gameObject);
        }

    }

    [ContextMenu("PlayAttackAni")]
    public void PlayAttackAni()
    {
        animator.SetTrigger("Attack");
    }

}

