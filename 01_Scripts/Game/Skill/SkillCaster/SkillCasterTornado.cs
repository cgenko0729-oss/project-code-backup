using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillCasterTornado : SkillCasterBase
{
   

    public float doubleCastOffset = 4f;


     protected override void CastSkill()
    {
        // 1. Setup Basic Numbers
        int tornadoShootNum = projectileNumFinal;
        float angleStep = (tornadoShootNum > 1) ? 360f / tornadoShootNum : 0f;

        SoundEffect.Instance.Play(SoundList.TornadoSe);
        
        // Standard Spawn Position (Center of Player)
        Vector3 spawnPos = playerTran.position + castPosOffset + playerForwardVec * castDistance;
        
        // ========================================================================
        // LOGIC 1: NORMAL SKILL
        // ========================================================================
        if (!isFinalSkill)
        {
            // --- Primary Cast (Forward from Player) ---
            for (int k = 0; k < tornadoShootNum; k++)
            {
                Quaternion dirRot = Quaternion.AngleAxis(angleStep * k, Vector3.up);
                Vector3 currentDir = dirRot * playerForwardVec;

                SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, Quaternion.identity);
                sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal, isFinalSkill);
                sm.SetTrait(currentTrait);
                sm.InitStatusSizeAndPosY();
                
                var tornado = sm.GetComponent<SkillForwardMove>();
                tornado.moveVec = currentDir * speedFinal;
            }

            // --- Double Cast (Backward from Offset) ---
            if (isDoubleCast)
            {
                float chance = Random.Range(0f, 1f);
                if (chance > 0.5f && !SkillEffectManager.Instance.universalTrait.isFairJudge) return;
                
                EventManager.EmitEvent("OnPlayDoubleCastAnim");
                ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);

                // [CHANGE] Calculate specific spawn position for Double Cast
                float distOffset = doubleCastOffset;
                Vector3 baseBackDir = -playerForwardVec; // Normalized direction (length 1)
                
                // Position is Player + (Backwards * Distance)
                Vector3 doubleSpawnPos = playerTran.position + castPosOffset + (baseBackDir * distOffset);

                for (int k = 0; k < tornadoShootNum; k++)
                {
                    // Rotate the DIRECTION, not the position
                    Quaternion dirRot = Quaternion.AngleAxis(angleStep * k, Vector3.up);
                    Vector3 currentDir = dirRot * baseBackDir; 

                    // [CHANGE] Spawn at doubleSpawnPos
                    SkillModelBase sm2 = skillObjPool.GetObjectComponent<SkillModelBase>(doubleSpawnPos, Quaternion.identity);
                    sm2.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal, isFinalSkill);
                    sm2.SetTrait(currentTrait);
                    sm2.InitStatusSizeAndPosY();
                    
                    var tornado2 = sm2.GetComponent<SkillForwardMove>();
                    tornado2.moveVec = currentDir * speedFinal;
                }
            }
        }
        // ========================================================================
        // LOGIC 2: FINAL SKILL (S-Move)
        // ========================================================================
        else
        {
            // --- Primary Cast (Forward from Player) ---
            for (int k = 0; k < tornadoShootNum; k++)
            {
                Quaternion dirRot = Quaternion.AngleAxis(angleStep * k, Vector3.up);
                Vector3 currentDir = dirRot * playerForwardVec;

                for(int i = 0; i < 2; i++)
                {
                    SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, Quaternion.identity);
                    ConfigureFinalSkillModel(sm, i); // Helper function logic moved below for readability

                    SkillSMove tornadoSMove = sm.GetComponent<SkillSMove>();
                    tornadoSMove.isActivated = true;

                    if (tornadoSMove != null)
                    {               
                        float amp = 1.4f * sizeFinal;   
                        float wave = 6f;                 
                        bool firstBendRight = (i == 0);

                        // Use 1.14f as per your latest snippet
                        tornadoSMove.Configure(currentDir, speedFinal * 1.0f, amp, wave, firstBendRight);
                    }
                }
            }

            // --- Double Cast (Backward from Offset) ---
            if (isDoubleCast)
            {
                float chance = Random.Range(0f, 1f);
                if (chance > 0.5f && !SkillEffectManager.Instance.universalTrait.isFairJudge) return;
                
                EventManager.EmitEvent("OnPlayDoubleCastAnim");
                ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);

                // [CHANGE] Calculate specific spawn position for Double Cast
                float distOffset = doubleCastOffset;
                Vector3 baseBackDir = -playerForwardVec; 
                Vector3 doubleSpawnPos = playerTran.position + castPosOffset + (baseBackDir * distOffset);

                for (int k = 0; k < tornadoShootNum; k++)
                {
                    Quaternion dirRot = Quaternion.AngleAxis(angleStep * k, Vector3.up);
                    Vector3 currentDir = dirRot * baseBackDir;

                    for(int i = 0; i < 2; i++)
                    {
                        // [CHANGE] Spawn at doubleSpawnPos
                        SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(doubleSpawnPos, Quaternion.identity);
                        ConfigureFinalSkillModel(sm, i);

                        SkillSMove tornadoSMove = sm.GetComponent<SkillSMove>();
                        tornadoSMove.isActivated = true;

                        if (tornadoSMove != null)
                        {               
                            float amp = 1.4f * sizeFinal;   
                            float wave = 6f;                 
                            bool firstBendRight = (i == 0);

                            tornadoSMove.Configure(currentDir, speedFinal * 1.0f, amp, wave, firstBendRight);
                        }
                    }
                }
            }
        }
    }

    // Helper to reduce code duplication in Final Skill Particle switching
    private void ConfigureFinalSkillModel(SkillModelBase sm, int index)
    {
        if(index == 0)
        {
            sm.ps.gameObject.SetActive(false);
            sm.subPs.gameObject.SetActive(false);
            sm.finalPs.gameObject.SetActive(true);
        }
        else
        {
            sm.ps.gameObject.SetActive(false);
            sm.finalPs.gameObject.SetActive(false);
            sm.subPs.gameObject.SetActive(true);
        }
        sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal, isFinalSkill);
        sm.SetTrait(currentTrait);
        sm.InitStatusSizeAndPosY();
    }



}

