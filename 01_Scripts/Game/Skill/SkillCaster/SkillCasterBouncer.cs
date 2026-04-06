using DG.Tweening;
using Hellmade.Sound; //SoundManager
using MonsterLove.StateMachine; //StateMachine
using QFSW.MOP2;                //Object Pool
using System.Collections.Generic;
using TigerForge;               //EventManager
using TMPro;    
using UnityEngine;
using UnityEngine.UI;

public class SkillCasterBouncer : SkillCasterBase
{
    protected override void CastSkill()
    {
        float totalAngle = 45f; // 全体に開く角度（例：±22.5度）
        int num = projectileNumFinal;

        Vector3 spawnPos = playerTran.position + castPosOffset + playerForwardVec * castDistance;

        for (int i = 0; i < num; i++) //1
        {
            // -half → 0 → +half に角度をずらしていく
            float angle = 0f;
            if (num > 1) //0
            {
                float startAngle = -totalAngle * 0.5f;
                float angleStep = totalAngle / (num);
                angle = startAngle + angleStep * i;
            }

            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 dir = rotation * playerForwardVec;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, rot);
            sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal, isFinalSkill);
            sm.SetTrait(currentTrait);
            sm.InitStatusSizeAndPosY();

            var bouncer = sm.GetComponent<SkillBounceMove>();
            bouncer.moveVec = dir.normalized * speedFinal;

            if (isFinalSkill)
            {
                bouncer.isFinalSkill = true;
            }
        }


        if (isDoubleCast)
        {
            float chance = Random.Range(0f, 1f);
            if (chance > 0.5f && !SkillEffectManager.Instance.universalTrait.isFairJudge) return;
            Invoke("EmitDoubleCast", 0.28f);
        }

    }

    void EmitDoubleCast()
    {
        TigerForge.EventManager.EmitEvent("OnPlayDoubleCastAnim");
        ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);

        float totalAngle = 45f; // 全体に開く角度（例：±22.5度）
        int num = projectileNumFinal/2;

        Vector3 spawnPos = playerTran.position + castPosOffset + playerForwardVec * castDistance;

        for (int i = 0; i < num; i++) //1
        {
            // -half → 0 → +half に角度をずらしていく
            float angle = 0f;
            if (num > 1) //0
            {
                float startAngle = -totalAngle * 0.5f;
                float angleStep = totalAngle / (num);
                angle = startAngle + angleStep * i;
            }

            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 dir = rotation * playerForwardVec;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, rot);
            sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal, isFinalSkill);
            sm.SetTrait(currentTrait);
            sm.InitStatusSizeAndPosY();

            var bouncer = sm.GetComponent<SkillBounceMove>();
            bouncer.moveVec = dir.normalized * speedFinal;

            if (isFinalSkill)
            {
                bouncer.isFinalSkill = true;
            }
        }

    }


}

