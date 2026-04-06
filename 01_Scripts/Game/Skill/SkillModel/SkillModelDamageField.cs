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

public class SkillModelDamageField : SkillModelBase
{
    protected override void HandleSkillInit()
    {

        if (ps != null)
        {

            ps.gameObject.SetActive(true);
            ps.Play();
        }

        if (isFinalSkill)
        {
            Invoke("DelayEnchantFire", 0.14f);
        }

    }

    public void DelayEnchantFire()
    {
        isEnchantFire = true;
        if (SkillEffectManager.Instance.universalTrait.isFireEnhanced)
        {
            skillDamage *= 1.5f;

        }
    }


    protected override void HandleSkillEndAction()
    {
        
    }

    protected override void HandleSkillOnHitAction(Collider col)
    {
       
    }

    protected override void HandleSkillUpdateAction()
    {
        
    }
}

