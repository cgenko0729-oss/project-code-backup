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

public class SkillModelIce : SkillModelBase
{

    protected override void HandleSkillInit()
    {

        if (ps != null)
        {                
            ps.gameObject.SetActive(true);
            ps.Play();
        }

        if(subPs != null)
        {
            subPs.gameObject.SetActive(true);
            subPs.Play();
        }


    }

    protected override void HandleSkillEndAction()
    {
        
    }

    protected override void HandleSkillOnHitAction(Collider col)
    {
        //EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();
        //enemyStat.ApplyIceDebuff();
        
    }

    protected override void HandleSkillUpdateAction()
    {
        
    }

    
}

