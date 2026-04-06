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
using Cysharp.Threading.Tasks.Triggers;
using System.Collections;

public class SkillCasterHammer : SkillCasterBase
{
    [Header("ハンマーの進化先で使用する情報")]
    public float hammerWaveDuration = 0;
    public float hammerWaveLifeTime = 0;

    protected override void CastSkill()
    {

        //if (autoMode)
        //{
        //    Transform target = GetNearestEnemy();

        //    if (target != null)
        //    {
        //        Vector3 directionToTarget = target.position - playerTran.position;
        //        Vector3 flatDir = directionToTarget;
        //        flatDir.y = 0; 

        //        if (flatDir != Vector3.zero)
        //        {
        //            playerTran.rotation = Quaternion.LookRotation(flatDir);                   
        //            playerForwardVec = flatDir.normalized; 
        //        }
        //    }
        //}

        Vector3 spawnPos = playerTran.position + castPosOffset + playerForwardVec * castDistance;
        spawnPos.y = 0;
        SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, Quaternion.identity);

        SoundEffect.Instance.Play(SoundList.HammerSe);

        float duration = 0;
        if (isFinalSkill == true)
        {
            duration = durationFinal + hammerWaveDuration + hammerWaveLifeTime;
        }
        else
        {
            duration = durationFinal;
        }

        sm.SetSkill(casterIdType, casterId, casterLevel, casterName,duration, speedFinal, damageFinal, sizeFinal, isFinalSkill);
        sm.SetTrait(currentTrait);
        sm.InitStatusSizeAndPosY();

        var hammerWave = sm.GetComponent<SkillHammerWave>();
        hammerWave.isFinalSkill = isFinalSkill;
        hammerWave.colLifeTime = hammerWaveLifeTime;
        hammerWave.Init();

        if (isDoubleCast)
        {
            float chance = Random.Range(0f, 1f);
            if (chance > 0.5f && !SkillEffectManager.Instance.universalTrait.isFairJudge) return;
            
            Invoke("EmitDoubleCast", 0.21f);

        }
    }


    void EmitDoubleCast()
    {
            EventManager.EmitEvent("OnPlayDoubleCastAnim");
            ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);

            Vector3 spawnPos2 = playerTran.position + castPosOffset - playerForwardVec * castDistance;

            SkillModelBase sm2 = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos2, Quaternion.identity);

            float duration2 = 0;
            if (isFinalSkill == true)
            {
                duration2 = durationFinal + hammerWaveDuration + hammerWaveLifeTime;
            }
            else
            {
                duration2 = durationFinal;
            }

            sm2.SetSkill(casterIdType, casterId, casterLevel, casterName,duration2, speedFinal, damageFinal, sizeFinal, isFinalSkill);
            sm2.SetTrait(currentTrait);
            sm2.InitStatusSizeAndPosY();

            var hammerWave2 = sm2.GetComponent<SkillHammerWave>();
            hammerWave2.isFinalSkill = isFinalSkill;
            hammerWave2.colLifeTime = hammerWaveLifeTime;
            hammerWave2.Init();
    }


}

