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

public class SkillCasterDamageField : SkillCasterBase
{
    

    public Vector3 previousFireSpawnPos= Vector3.zero;
    private float spawnDistNeed = 1.4f;
    public float spawnInterval = 0.5f;

    public float walkDistUpdateInterval = 0.1f;

    protected override void CastSkill()
    {
        

       //playerWalkedDistAmount += Vector3.Distance(playerTrans.position, previousePlayerPos);
          
        float distFromLastSpawn = Vector3.Distance(playerTran.position, previousFireSpawnPos);
        if (distFromLastSpawn < spawnDistNeed) return;

        previousFireSpawnPos = playerTran.position;


        SoundEffect.Instance.Play(SoundList.ArrowSe);

        Vector3 spawnPos = playerTran.position + castPosOffset;

        SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos);
        sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal, isFinalSkill);
        sm.InitStatusSizeAndPosY();
        sm.SetTrait(currentTrait);

        if (isDoubleCast)
        {
            float chance = Random.Range(0f, 1f);
            if (chance > 0.5f && !SkillEffectManager.Instance.universalTrait.isFairJudge) return;

            ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);
            EventManager.EmitEvent("OnPlayDoubleCastAnim");

            Vector3 oppositeForwardVec = -playerForwardVec;
            float newCastDist = castDistance * 2.1f;
             Vector3 spawnPos2 = playerTran.position + castPosOffset + oppositeForwardVec * newCastDist;
            SkillModelBase sm2 = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos2);
            sm2.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal, isFinalSkill);
            sm2.InitStatusSizeAndPosY();
            sm2.SetTrait(currentTrait);
        }

    }
}

