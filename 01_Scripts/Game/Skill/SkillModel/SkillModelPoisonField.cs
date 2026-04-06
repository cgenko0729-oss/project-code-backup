using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillModelPoisonField : SkillModelBase
{

    public ObjectColliderSwitcher colliderSwitcher;

    protected override void HandleSkillInit()
    {

        if(!colliderSwitcher)colliderSwitcher = GetComponent<ObjectColliderSwitcher>();

        if (!isFinalSkill)
        {
            if (ps != null)
            {
                ps.gameObject.SetActive(true);
                ps.Play();
            }

            colliderSwitcher.disableColliderInterval = 0.219f;

        }
        else
        {
            ps.Stop();
            ps.gameObject.SetActive(false);

            finalPs.gameObject.SetActive(true);
            finalPs.Play();

            hasOnHitAction = true;

            colliderSwitcher.disableColliderInterval = 0.14f;

            //PoisonFieldHItController hitController = GetComponent<PoisonFieldHItController>();
            //hitController.gameObject.SetActive(true);

        }

        EnableCollision();

    }

    protected override void HandleSkillEndAction()
    {
        
    }

    protected override void HandleSkillOnHitAction(Collider col)
    {
        SkillModelBase sm = GetComponent<SkillModelBase>();
            if (!sm.isFinalSkill) return;

            //EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();
            //enemyStat.ApplySpeedDownDebuff(0.49f,1f);
    }

    protected override void HandleSkillUpdateAction()
    {
        
    }

}

