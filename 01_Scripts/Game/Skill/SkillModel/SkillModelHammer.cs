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
using System.Collections;

public class SkillModelHammer : SkillModelBase
{
    protected override void HandleSkillInit()
    {
        if (ps != null)
        {
            ps.gameObject.SetActive(true);
            ps.Play();
        }

        EnableCollision();

        StartCoroutine(DefaultCollisionProcess());
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

    IEnumerator DefaultCollisionProcess()
    {
        yield return new WaitForSeconds(0.1f);

        gameObject.GetComponent<SphereCollider>().enabled = false;
    }
}

