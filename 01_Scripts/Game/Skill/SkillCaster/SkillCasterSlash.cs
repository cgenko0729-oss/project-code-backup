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

public class SkillCasterSlash : SkillCasterBase
{
    protected override async void CastSkill()
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

         // ――― スラッシュ発射位置 & 向き計算 ―――
            Vector3 spawnPos = playerTran.position + castPosOffset + playerForwardVec * castDistance;

            Vector3 dir = new Vector3(playerTran.forward.x, 0f, playerTran.forward.z).normalized;
            if (dir.sqrMagnitude < 0.0001f) dir = playerTran.forward;

            Quaternion yaw = Quaternion.LookRotation(dir, Vector3.up);
            Quaternion rot = yaw * prefabFix;

            if (!isFinalSkill)
            {
                durationFinal *= 0.35f;
            }

            // ――― プレハブ生成 ―――
            SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos, rot);
            sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal, isFinalSkill);
            sm.SetTrait(currentTrait);
            sm.InitStatusSizeAndPosY();

            if (isFinalSkill)
            {
                SkillForwardMove mv = sm.GetComponent<SkillForwardMove>();
                mv.moveVec = playerForwardVec * speedFinal;
            }

            // ――― サウンド ―――
            SoundEffect.Instance.Play(SoundList.Slash);

        if (isDoubleCast)
        {
            float chance = Random.Range(0f, 1f);
            if (chance > 0.5f && !SkillEffectManager.Instance.universalTrait.isFairJudge) return;

            Invoke("EmitDoubleCast", 0.21f);
        }
    }


        
        void EmitDoubleCast()
        {
            //Debug.Log("Slash Double Cast");
            EventManager.EmitEvent("OnPlayDoubleCastAnim");
            ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);

            Vector3 spawnPos2 = playerTran.position+ castPosOffset+ (-playerForwardVec * castDistance);

            Vector3 dir2 = new Vector3(playerTran.forward.x, 0f, playerTran.forward.z).normalized;
            if (dir2.sqrMagnitude < 0.0001f) dir2 = playerTran.forward;

            Quaternion yaw2 = Quaternion.LookRotation(-dir2, Vector3.up);
            Quaternion rot2 = yaw2 * prefabFix;

            if (!isFinalSkill)
            {
                durationFinal *= 0.35f;
            }

            // ――― プレハブ生成 ―――
            SkillModelBase sm2 = skillObjPool.GetObjectComponent<SkillModelBase>(spawnPos2, rot2);
            sm2.SetSkill(casterIdType, casterId, casterLevel, casterName,durationFinal, speedFinal, damageFinal, sizeFinal, isFinalSkill);
            sm2.SetTrait(currentTrait);
            sm2.InitStatusSizeAndPosY();

            if (isFinalSkill)
            {
                SkillForwardMove mv2 = sm2.GetComponent<SkillForwardMove>();
                mv2.moveVec = -playerForwardVec * speedFinal;
            }
        }



    }



