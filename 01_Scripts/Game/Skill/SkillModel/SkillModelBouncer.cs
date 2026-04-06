using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillModelBouncer : SkillModelBase
{
    protected override void HandleSkillInit()
    {
        if (ps != null)
        {
            ps.gameObject.SetActive(true);
            ps.Play();
        }
        EnableCollision();
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

