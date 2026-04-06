using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillCasterPoisonField : SkillCasterBase
{
    protected override void CastSkill()
    {
        Vector3 spawnPos = playerTran.position;

        SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, Quaternion.identity);

        if (!isFinalSkill)sm.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal, damageFinal,sizeFinal, isFinalSkill);
        else sm.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal, damageFinal * 1.28f,sizeFinal, isFinalSkill);
        sm.SetTrait(currentTrait);
        sm.InitStatusSizeAndPosY();

        ObjectFollow objFollow = sm.GetComponent<ObjectFollow>();
        objFollow.isFollowPlayer = true;

        SoundEffect.Instance.Play(SoundList.CircleBall);


        if (isDoubleCast)
        {
            float chance = Random.Range(0f, 1f);
            if (chance > 0.5f && !SkillEffectManager.Instance.universalTrait.isFairJudge) return;

            EventManager.EmitEvent("OnPlayDoubleCastAnim");
            ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);

            Quaternion rotStaff = Quaternion.Euler(-90f, 0f, 0f);
            GameObject cloneObj = Instantiate(SkillEffectManager.Instance.playerCloneObj,playerTran.position, rotStaff); //need optimize


            Vector3 spawnPos2 = playerTran.position;

            SkillModelBase sm2 = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos2, Quaternion.identity);

            sm2.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal, damageFinal,sizeFinal, isFinalSkill);
            sm2.SetTrait(currentTrait);
            sm2.InitStatusSizeAndPosY();

            ObjectFollow objFollow2 = sm2.GetComponent<ObjectFollow>();
            objFollow2.isFollowPlayer = false;

        }

    }

}

