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

public class SkillModelShuriken : SkillModelBase
{

    public SkillForwardMove sMove;

    protected override void HandleSkillInit()
    {
        isEndActionFinished = false;
        
        if (ps != null)
        {
            ps.Play();
        }
        EnableCollision();
    }

    protected override void HandleSkillEndAction()
    {
        if (isFinalSkill)
        {
            isEndActionFinished = true;

            const int projectileCount = 3;
            const float angleStep = 360f / projectileCount;

            for (int i = 0; i < 3; i++)
            {
                float currentAngle = i * angleStep;
                Vector3 dir = (Quaternion.AngleAxis(currentAngle, Vector3.up) * transform.forward).normalized;

                Vector3 spawnPos = transform.position;
                SkillModelBase sm = effectObjPool.GetObjectComponent<SkillModelBase>(spawnPos);
                sm.SetSkill(skillIdType, 1, 1, "", 0.7f, skillSpeed * 0.77f, skillDamage * 0.5f, skillSize * 0.56f, false);
                sm.InitStatusSizeAndPosY();
                sm.CopyTrait(isFinishCastExplosion, isKillEnemyExplosion, isKillEnemySkullSoul, isKillEnemyBrightSoul, isSkillMovingDropFire, isPushback, isEnchantFire, isEnchantIce, isEnchantPoison,isTreasureHunter);

                var skillMove = sm.GetComponent<SkillForwardMove>();
                skillMove.moveVec = dir * skillSpeed;
            }

                // move toward player 
                //Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
                //Vector3 dirToPlayer = (playerPos - transform.position).normalized;
                //Vector3 spawnPos = transform.position;

                //Vector3.MoveTowards(transform.position, playerPos, 21 * Time.deltaTime);
                //sMove.moveVec = Vector3.zero;

                //float distToPlayer = Vector3.Distance(transform.position, playerPos);
                //if (distToPlayer < 0.5f) isEndActionFinished = true;


        }
        else
        {
            isEndActionFinished = true;
        }
        
    }

    protected override void HandleSkillOnHitAction(Collider col)
    {
       
    }

    protected override void HandleSkillUpdateAction()
    {
        
    }


}

