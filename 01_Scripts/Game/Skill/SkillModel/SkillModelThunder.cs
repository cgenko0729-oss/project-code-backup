using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillModelThunder : SkillModelBase
{
    protected override void HandleSkillInit()
    {
        if (ps != null)
        {                
            ps.gameObject.SetActive(true);
            ps.Play();
        }

        gameObject.GetComponent<BoxCollider>().enabled = false; // スラッシュスキルではないのでBoxColliderを無効化
        gameObject.GetComponent<SphereCollider>().enabled = false; // SphereColliderも無効化

        if (!isFinalSkill)
        {
            gameObject.GetComponent<BoxCollider>().enabled = true;

        }
        else
        {
            gameObject.GetComponent<SphereCollider>().enabled = true; // 最終スキルならSphereColliderを有効化
            //spawn effectAfterMathObj at transform.position , rotation = -90 ,0,0
            Vector3 spawnPos = transform.position + new Vector3(0, -0.56f, 0); // Yオフセットを適用
            GameObject afterMath = Instantiate(effectAfterMathObj, spawnPos, Quaternion.Euler(-90, 0, 0));
            //afterMath.transform.localScale = new Vector3(skillBaseSize.x * skillSize, skillBaseSize.y * skillSize, skillBaseSize.z * skillSize);
            Destroy(afterMath,0.77f);
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

