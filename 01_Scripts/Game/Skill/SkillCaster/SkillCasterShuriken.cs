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

public class SkillCasterShuriken : SkillCasterBase
{
    protected override async void CastSkill()
    {
        //SoundEffect.Instance.Play(SoundList.ArrowSe);
       
        //if (autoMode)
        //{
        //    Transform target = GetNearestEnemy();

        //    if (target != null)
        //    {
        //        // Calculate direction to target
        //        Vector3 directionToTarget = target.position - playerTran.position;

        //        // Flatten Y so the player doesn't tilt physically
        //        Vector3 flatDir = directionToTarget;
        //        flatDir.y = 0; 

        //        if (flatDir != Vector3.zero)
        //        {
        //            playerTran.rotation = Quaternion.LookRotation(flatDir);
        //            playerForwardVec = flatDir.normalized; 
        //        }
        //    }
        //}

        for(int i = 0; i < projectileNumFinal; i++)
        {
            CastOnce();

            try
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(0.3f), cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            catch (System.OperationCanceledException)
            {              
                break;
            }


        }

        if (isDoubleCast)
        {
            float chance = Random.Range(0f, 1f);
            if (chance > 0.5f && !SkillEffectManager.Instance.universalTrait.isFairJudge) return;
            EventManager.EmitEvent("OnPlayDoubleCastAnim");
            ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);

            for(int i = 0; i < projectileNumFinal; i++)
            {
                CastOnce();

                try
                {
                    await UniTask.Delay(System.TimeSpan.FromSeconds(0.3f), cancellationToken: this.GetCancellationTokenOnDestroy());
                }
                catch (System.OperationCanceledException)
                {              
                    break;
                }


            }

        }


    }

    void CastOnce()
    {
        Vector3 spawnPos = playerTran.position + castPosOffset + playerForwardVec * castDistance;
        Quaternion rot = Quaternion.LookRotation(playerForwardVec, Vector3.up) ;

        SkillModelShuriken sm = skillObjPool.GetObjectComponent<SkillModelShuriken>(spawnPos, rot);
        if (!isFinalSkill) sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal, isFinalSkill);
        else sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal * 1.14f, speedFinal, damageFinal* 1.4f, sizeFinal * 1.4f, isFinalSkill);
        sm.InitStatusSizeAndPosY();
        

        var arrow = sm.GetComponent<SkillForwardMove>();
        arrow.moveVec = playerForwardVec * speedFinal;

        sm.SetTrait(currentTrait);

        sm.sMove = arrow;
    }

   



}

