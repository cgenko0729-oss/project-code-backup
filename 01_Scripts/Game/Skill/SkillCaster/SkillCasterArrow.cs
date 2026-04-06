using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillCasterArrow : SkillCasterBase
{
    
    

    protected override void CastSkill()
    {
         
        SoundEffect.Instance.Play(SoundList.ArrowSe);
     
        if(projectileNumFinal == 1)
        {

            Vector3 spawnPos = playerTran.position + castPosOffset + playerForwardVec * castDistance;
            Quaternion rot = Quaternion.LookRotation(playerForwardVec, Vector3.up) ;

            SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, rot);
            if (!isFinalSkill) sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal, isFinalSkill);
            else sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal * 1.14f, speedFinal, damageFinal* 1.21f, sizeFinal * 1.28f, isFinalSkill);
            sm.InitStatusSizeAndPosY();

            var arrow = sm.GetComponent<SkillForwardMove>();
            arrow.moveVec = playerForwardVec * speedFinal;

            sm.SetTrait(currentTrait);

            Vector3 arrowTargetPos = spawnPos + (playerForwardVec * speedFinal * (durationFinal));

            //SkillEffectManager.Instance.SpawnSkillWolfObj(spawnPos, arrowTargetPos,(durationFinal*2.1f));

        }
        else
        {
            float SECTOR_ANGLE = 45f;
            if(projectileNumFinal == 2) SECTOR_ANGLE = 14.9f;    
            else SECTOR_ANGLE = 45f;
            float halfAngle = SECTOR_ANGLE * 0.5f;
            float stepAngle = (projectileNumFinal > 1)? SECTOR_ANGLE / (projectileNumFinal - 1): 0f;
            Vector3 fwd = playerForwardVec.normalized;        

            for (int i = 0; i < projectileNumFinal; ++i)
            {
                
                float offset = -halfAngle + stepAngle * i; //direction 
                Vector3 dir  = Quaternion.AngleAxis(offset, Vector3.up) * fwd;
                Vector3 spawnPos = playerTran.position + castPosOffset + dir * castDistance;

                Quaternion yaw = Quaternion.LookRotation(dir, Vector3.up);
                Quaternion rot = yaw * prefabFix;

                SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, yaw);
                if(!isFinalSkill)sm.SetSkill(casterIdType,casterId,casterLevel,casterName,durationFinal,speedFinal,damageFinal,sizeFinal,isFinalSkill);
                else sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal* 1.4f, sizeFinal * 1.49f, isFinalSkill);
                sm.InitStatusSizeAndPosY();

                var mover = sm.GetComponent<SkillForwardMove>();
                mover.moveVec = dir * speedFinal;

                sm.SetTrait(currentTrait);
            }

        }

        if (isDoubleCast)
        {
            float chance = Random.Range(0f, 1f);
            if (chance > 0.5f && !SkillEffectManager.Instance.universalTrait.isFairJudge) return;

            ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);
            EventManager.EmitEvent("OnPlayDoubleCastAnim");

            Vector3 oppositeForwardVec = -playerForwardVec;
            float newCastDist = castDistance * 2.1f;

            if (projectileNumFinal == 1)
        {

            Vector3 spawnPos = playerTran.position + castPosOffset + oppositeForwardVec * newCastDist;
            Quaternion rot = Quaternion.LookRotation(oppositeForwardVec, Vector3.up) ;
            SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, rot);
            if(!isFinalSkill) sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal, isFinalSkill);
            else sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal * 1.49f, isFinalSkill);
            sm.InitStatusSizeAndPosY();
            sm.SetTrait(currentTrait);

            var arrow = sm.GetComponent<SkillForwardMove>();
            arrow.moveVec = oppositeForwardVec * speedFinal;

        }
        else
        {
            float SECTOR_ANGLE = 45f;
            if(projectileNumFinal == 2) SECTOR_ANGLE = 10f;    
            else SECTOR_ANGLE = 45f;
            float halfAngle = SECTOR_ANGLE * 0.5f;
            float stepAngle = (projectileNumFinal > 1)? SECTOR_ANGLE / (projectileNumFinal - 1): 0f;
            Vector3 fwd = oppositeForwardVec.normalized;        

            for (int i = 0; i < projectileNumFinal; ++i)
            {
                
                float offset = -halfAngle + stepAngle * i; //direction 
                Vector3 dir  = Quaternion.AngleAxis(offset, Vector3.up) * fwd;
                Vector3 spawnPos = playerTran.position + castPosOffset + dir * newCastDist;

                Quaternion yaw = Quaternion.LookRotation(dir, Vector3.up);
                Quaternion rot = yaw * prefabFix;

                SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, yaw);
                if(!isFinalSkill)sm.SetSkill(casterIdType,casterId,casterLevel,casterName,durationFinal,speedFinal,damageFinal,sizeFinal,isFinalSkill);
                else sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal * 1.49f, isFinalSkill);
                sm.InitStatusSizeAndPosY();
                sm.SetTrait(currentTrait);

                var mover = sm.GetComponent<SkillForwardMove>();
                mover.moveVec = dir * speedFinal;
            }

        }


        }


        


    }




}

