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

public class SkillCasterBird : SkillCasterBase
{
    protected override void CastSkill()
    {
        SoundEffect.Instance.Play(SoundList.ArrowSe);

        Vector3 spawnPos = playerTran.position + castPosOffset + playerForwardVec * castDistance;
        Quaternion rot = Quaternion.LookRotation(playerForwardVec, Vector3.up) ;


        SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, rot);
        if (!isFinalSkill) sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal, isFinalSkill);
        else sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal * 1.14f, speedFinal, damageFinal* 1.4f, sizeFinal * 1.4f, isFinalSkill);
        sm.InitStatusSizeAndPosY();

        SkillFollowMouseMove move = sm.GetComponent<SkillFollowMouseMove>();
        move.homingSpeed = speedFinal;

        if (isDoubleCast)
        {
            float chance = Random.Range(0f, 1f);
            if (chance > 0.5f && !SkillEffectManager.Instance.universalTrait.isFairJudge) return;
            Invoke("EmitDoubleCast", 0.28f);
        }



    }

    void EmitDoubleCast()
    {

        EventManager.EmitEvent("OnPlayDoubleCastAnim");
        ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);

        Vector3 spawnPos = playerTran.position + castPosOffset + playerForwardVec * castDistance;
        Quaternion rot = Quaternion.LookRotation(playerForwardVec, Vector3.up) ;


        SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, rot);
        if (!isFinalSkill) sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal, isFinalSkill);
        else sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal * 1.14f, speedFinal, damageFinal* 1.4f, sizeFinal * 1.4f, isFinalSkill);
        sm.InitStatusSizeAndPosY();

        SkillFollowMouseMove move = sm.GetComponent<SkillFollowMouseMove>();
        move.homingSpeed = speedFinal;
    }



}

