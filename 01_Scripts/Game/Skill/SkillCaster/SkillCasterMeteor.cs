using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillCasterMeteor : SkillCasterBase
{
    protected override void CastSkill()
    {
        float slice = 360f / projectileNumFinal;

        if (isFinalSkill)
        {
            durationFinal = 7000f;
            castCoolDown  = 7000f;
        }

        for (int i = 0; i < projectileNumFinal; i++)
        {
            float phase = slice * i * Mathf.Deg2Rad;

            SkillModel mdl = skillObjPool.GetObjectComponent<SkillModel>(
                                 playerTran.position, Quaternion.identity);

            if (!isFinalSkill)
                mdl.SetSkill(casterIdType, casterId, casterLevel, casterName,
                             durationFinal, speedFinal, damageFinal,
                             sizeFinal, isFinalSkill);
            else
                mdl.SetSkill(casterIdType, casterId, casterLevel, casterName,
                             durationFinal, speedFinal * 0.77f, damageFinal,
                             sizeFinal * 1.4f, isFinalSkill);

            mdl.UpdateSkillSize();

            var mover = mdl.GetComponent<SkillCircleMove>();
            mover.Init(playerTran, phase, speedFinal, 3.5f);


        }

        SoundEffect.Instance.Play(SoundList.CircleBall);


        Debug.Log($"SkillCasterCircleBall1: Casted skill {casterIdType} with ID {casterId} at level {casterLevel} named {casterName}.");

    }
    
}

