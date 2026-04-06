using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillCasterCircleShield : SkillCasterBase
{
    public float circlingRadius = 2.89f;

    protected override void CastSkill()
    {
        float slice = 360f / projectileNumFinal;

        for (int i = 0; i < projectileNumFinal; i++)
        {
            float phase = slice * i * Mathf.Deg2Rad;

            SkillModelBase mdl = skillObjPool.GetObjectComponent<SkillModelBase>(playerTran.position, Quaternion.identity);

            if (!isFinalSkill)
                mdl.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal, damageFinal,sizeFinal, isFinalSkill);
            else
                mdl.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal * 0.77f, damageFinal,sizeFinal * 1.4f, isFinalSkill);

            mdl.InitStatusSizeAndPosY();
            mdl.UpdateSkillSize();
            mdl.SetTrait(currentTrait);

            var mover = mdl.GetComponent<SkillCircleMove>();
            mover.Init(playerTran, phase, speedFinal, circlingRadius);

           



        }


        if (isDoubleCast)
        {
            float chance = Random.Range(0f, 1f);
            if (chance > 0.5f && !SkillEffectManager.Instance.universalTrait.isFairJudge) return;
            EventManager.EmitEvent("OnPlayDoubleCastAnim");
            ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);

            for (int i = 0; i < projectileNumFinal; i++)
            {
                float phase = slice * i * Mathf.Deg2Rad;

                SkillModelBase mdl = skillObjPool.GetObjectComponent<SkillModelBase>(playerTran.position, Quaternion.identity);

                if (!isFinalSkill)
                    mdl.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal, damageFinal,sizeFinal, isFinalSkill);
                else
                    mdl.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal * 0.77f, damageFinal,sizeFinal * 1.4f, isFinalSkill);

                mdl.InitStatusSizeAndPosY();
                mdl.UpdateSkillSize();
                mdl.SetTrait(currentTrait);

                var mover = mdl.GetComponent<SkillCircleMove>();

               GameObject cloneObj = Instantiate(SkillEffectManager.Instance.playerCloneObj,playerTran.position, Quaternion.identity); //need optimize
                mover.Init(cloneObj.transform, phase, speedFinal, circlingRadius);


            }
        }


        SoundEffect.Instance.Play(SoundList.CircleBall);

    }

}

