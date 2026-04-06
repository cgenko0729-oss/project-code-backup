using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillCasterBombShark : SkillCasterBase
{
    protected override void CastSkill()
    {

        //SoundEffect.Instance.Play(SoundList.ArrowSe);

        for (int i = 0; i < projectileNumFinal; i++)
        {
            Vector3 spawnCenterPos = playerTran.position;

            Vector3 spawnPos = Vector3.zero;
            
            spawnPos = new Vector3(Random.Range(spawnCenterPos.x - 4.2f, spawnCenterPos.x + 4.2f), 0, Random.Range(spawnCenterPos.z - 4.2f, spawnCenterPos.z + 4.2f));

            while (Vector3.Distance(spawnPos, spawnCenterPos) < 2.8f)
            {
                spawnPos = new Vector3(Random.Range(spawnCenterPos.x - 4.2f, spawnCenterPos.x + 4.2f), 0, Random.Range(spawnCenterPos.z - 4.2f, spawnCenterPos.z + 4.2f));
            }

            SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, Quaternion.identity);
            sm.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal, damageFinal,sizeFinal, isFinalSkill);
            sm.SetTrait(currentTrait);
            sm.InitStatusSizeAndPosY();
        }

        


        if (isDoubleCast)
        {
            float chance = Random.Range(0f, 1f);
            if (chance > 0.5f && !SkillEffectManager.Instance.universalTrait.isFairJudge) return;
            Invoke("EmitDoubleCast", 0.35f);
        }

    }
    void EmitDoubleCast()
    {
        EventManager.EmitEvent("OnPlayDoubleCastAnim");
        ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);

        Vector3 spawnCenterPos = playerTran.position;

        Vector3 spawnPos = Vector3.zero;
        
        spawnPos = new Vector3(
            Random.Range(spawnCenterPos.x - 4.2f, spawnCenterPos.x + 4.2f),
            spawnCenterPos.y,
            Random.Range(spawnCenterPos.z - 4.2f, spawnCenterPos.z + 4.2f)
        );

        while (Vector3.Distance(spawnPos, spawnCenterPos) < 2.8f)
        {
            spawnPos = new Vector3(
                Random.Range(spawnCenterPos.x - 4.2f, spawnCenterPos.x + 4.2f),
                spawnCenterPos.y,
                Random.Range(spawnCenterPos.z - 4.2f, spawnCenterPos.z + 4.2f)
            );
        }


        SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos+ new Vector3(0,-0.49f,0), Quaternion.identity);

        sm.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal, damageFinal,sizeFinal, isFinalSkill);
        sm.SetTrait(currentTrait);
        sm.InitStatusSizeAndPosY();

    }



}

