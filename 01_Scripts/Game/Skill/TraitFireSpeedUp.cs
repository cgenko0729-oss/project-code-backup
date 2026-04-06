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

public class TraitFireSpeedUp : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {

        if(!SkillEffectManager.Instance.universalTrait.isFireWalker) return;

        if (other.CompareTag("Skill"))
        {
            SkillModelBase skill = other.GetComponent<SkillModelBase>();
            if(skill != null)
            {
                if (skill.isWalkFire)
                {
                    SkillEffectManager.Instance.ApplyFireSpeedUp();
                }
            }

            
            
        }
    }

}

