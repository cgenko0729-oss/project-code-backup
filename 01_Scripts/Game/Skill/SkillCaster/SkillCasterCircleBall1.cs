using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillCasterCircleBall1 : SkillCasterBase
{
    public float circlingRadius = 3.59f;

    public Transform targetTrans;

    protected override void CastSkill()
    {
        float slice = 360f / projectileNumFinal;

        //if (isFinalSkill)
        //{
        //    durationFinal = 7000f;
        //    castCoolDown  = 7000f;
        //}

        //targetTrans = find obect with tag "PlayerClone" and is Active in the scene
        //targetTrans = GameObject.FindGameObjectWithTag("PlayerClone").transform;



        for (int i = 0; i < projectileNumFinal; i++)
        {
            float phase = slice * i * Mathf.Deg2Rad;

            SkillModelCircleBall mdl = skillObjPool.GetObjectComponent<SkillModelCircleBall>(playerTran.position, Quaternion.identity);

            if (!isFinalSkill)
                mdl.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal, damageFinal,sizeFinal, isFinalSkill);
            else
                mdl.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal * 1f, damageFinal* 1.35f,sizeFinal * 1.35f, isFinalSkill);

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

            ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);
            EventManager.EmitEvent("OnPlayDoubleCastAnim");
            //make quternion -180 ,0 ,0
            Quaternion rotStaff = Quaternion.Euler(-90f, 0f, 0f);
            GameObject cloneObj = Instantiate(SkillEffectManager.Instance.playerCloneObj,playerTran.position, rotStaff); //need optimize

            for (int i = 0; i < projectileNumFinal; i++)
            {
                float phase = slice * i * Mathf.Deg2Rad;

                SkillModelCircleBall mdl = skillObjPool.GetObjectComponent<SkillModelCircleBall>(playerTran.position, Quaternion.identity);

                if (!isFinalSkill)
                    mdl.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal, damageFinal,sizeFinal, isFinalSkill);
                else
                    mdl.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal * 1f, damageFinal* 1.35f,sizeFinal * 1.35f, isFinalSkill);

                mdl.InitStatusSizeAndPosY();
                mdl.UpdateSkillSize();
                mdl.SetTrait(currentTrait);

                var mover = mdl.GetComponent<SkillCircleMove>();

                

                mover.Init(cloneObj.transform, phase, speedFinal, circlingRadius);


            }
        }


        SoundEffect.Instance.Play(SoundList.CircleBall);

    }

}

