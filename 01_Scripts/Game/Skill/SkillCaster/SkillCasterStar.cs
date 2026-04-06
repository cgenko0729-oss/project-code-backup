using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillCasterStar : SkillCasterBase
{

    

    protected override void CastSkill()
    {

        
        Vector3 spawnPos = playerTran.position + castPosOffset + playerForwardVec * castDistance;

        Quaternion rot = Quaternion.LookRotation(playerForwardVec, Vector3.up)* prefabFix;

        SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, Quaternion.identity);

        sm.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal, damageFinal,sizeFinal, isFinalSkill);
        sm.SetTrait(currentTrait);
        sm.InitStatusSizeAndPosY();

        var tornado = sm.GetComponent<SkillForwardMove>();
        tornado.moveVec = playerForwardVec * speedFinal;

        SoundEffect.Instance.Play(SoundList.MagicSe);


        if (isDoubleCast)
        {
            float chance = Random.Range(0f, 1f);
            if (chance > 0.5f && !SkillEffectManager.Instance.universalTrait.isFairJudge) return;

            EventManager.EmitEvent("OnPlayDoubleCastAnim");
            ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);

            Vector3 spawnPos2 = playerTran.position + castPosOffset + playerForwardVec * castDistance;

            Quaternion rot2 = Quaternion.LookRotation(playerForwardVec, Vector3.up)* prefabFix;

            SkillModelBase sm2 = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos2, Quaternion.identity);

            sm2.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal, damageFinal,sizeFinal, isFinalSkill);
            sm2.SetTrait(currentTrait);
            sm2.InitStatusSizeAndPosY();

            var tornado2 = sm2.GetComponent<SkillForwardMove>();
            tornado2.moveVec = -playerForwardVec * speedFinal;

        }

    }


}

