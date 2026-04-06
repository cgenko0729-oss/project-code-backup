using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillModelSlash : SkillModelBase
{
    protected override void HandleSkillInit()
    {
        // ------ パーティクル切替 ------
        if (!isFinalSkill)
        {
            if (ps != null)
            {
                ps.gameObject.SetActive(true);
                ps.Play();
            }
        }
        else
        {
            if (ps != null)      ps.gameObject.SetActive(false);
            if (finalPs != null)
            {
                finalPs.gameObject.SetActive(true);
                finalPs.Play();
            }
        }

        // ------ コライダー有効／無効タイミング ------
        if (isFinalSkill)
        {
            EnableCollision();            
        }
        else
        {
            Invoke(nameof(EnableCollision),  collisionStartTime);
            Invoke(nameof(DisableCollision), collisionEndTime);
        }

        // 攻撃でのバフを発動する
        if(BuffManager.Instance.isIncreaseDamagePerAttackEnabled == true)
        {
            UpgradeEffectManager.Instance.Active(BuffType.IncreaseDamagePerAttack);
        }
    }

    protected override void HandleSkillEndAction()
    {
        
    }

    protected override void HandleSkillOnHitAction(Collider col)
    {
        float pushDistance = 3f;   // how far to push
        float pushTime     = 0.2f;
        Transform enemyT = col.transform;            // push this transform
        Vector3 dir = enemyT.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward; // fallback
        dir.Normalize();
        float finalDistance = pushDistance;
        DOTween.Kill(enemyT, complete: false);

        Vector3 targetPos = enemyT.position + dir * finalDistance;
        enemyT.DOMove(targetPos, pushTime).SetEase(Ease.OutQuad).SetId(enemyT); 
        
    }

    protected override void HandleSkillUpdateAction()
    {
        
    }

}

