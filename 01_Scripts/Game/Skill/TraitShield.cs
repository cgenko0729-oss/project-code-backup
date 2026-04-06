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

public class TraitShield : MonoBehaviour
{

    public float lifeTime = 7f;

    void Start()
    {
        
    }

    void Update()
    {
        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0)
        {
            
            SkillEffectManager.Instance.isPlayerShieldActive = false;
            Destroy(gameObject);
        }

    }

    public void ResetShieldTime()
    {
        lifeTime = 2.8f;
    }

    public void BreakShield()
    {
        SkillEffectManager.Instance.isPlayerShieldActive = false ;
        Destroy(gameObject);
    }

}

