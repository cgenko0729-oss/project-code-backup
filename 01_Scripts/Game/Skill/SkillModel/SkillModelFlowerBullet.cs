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

public class SkillModelFlowerBullet : SkillModelBase
{
    
    protected override void HandleSkillInit()
    {
        
        ps.Play();

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

