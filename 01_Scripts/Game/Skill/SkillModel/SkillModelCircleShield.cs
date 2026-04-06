using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillModelCircleShield : SkillModelBase
{

    public bool hasInitEndAction = false; // スキル終了アクションを実行するかどうか
    public float endActionDuration = 1f;
    public float endAcionDurationMax = 0.4f;

    public float endActionMoveSpeed = 35f;

    public bool isFinalSkillEndActionFinished = false;
    public float finalSkillEndActionCnt = 3.5f;

    float originalDamage; 

    protected override void HandleSkillInit()
    {
        if (ps != null)
        {
            ps.gameObject.SetActive(true);
            ps.Play();
        }
        EnableCollision();

        hasInitEndAction = false;
        isEndActionFinished = false;
        isFinalSkillEndActionFinished = false;
        finalSkillEndActionCnt = 3.5f;

        SkillCircleMove sm = GetComponent<SkillCircleMove>();
        sm.isActivated = true; 

        SkillForwardMove sfm= GetComponent<SkillForwardMove>();
        sfm.moveVec = Vector3.zero;

    }

    protected override void HandleSkillEndAction()
    {   
        endActionDuration -= Time.deltaTime;

        if ((!hasInitEndAction))
        {
            endActionDuration = endAcionDurationMax;
            hasInitEndAction = true; 
            SkillCircleMove sm = GetComponent<SkillCircleMove>();
            sm.isActivated = false; // スキルの移動を停止

            SkillForwardMove sfm= GetComponent<SkillForwardMove>();
            sfm.moveVec = sm.radialDir * skillSpeed * endActionMoveSpeed;

            originalDamage = skillDamage;
            skillDamage *= 2f;
            
        }
        
       if(endActionDuration <= 0)
        {
            

            if(!isFinalSkill)isEndActionFinished = true;

            else
            {
                if(!isFinalSkillEndActionFinished)
                {
                    isFinalSkillEndActionFinished = true;
                    finalSkillEndActionCnt = 3.5f;
                    SkillForwardMove sfm= GetComponent<SkillForwardMove>();
                    sfm.moveVec = Vector3.zero;
                    GetComponent<SkillCircleMove>().enabled = false;

                    skillDamage = originalDamage;
                }
                else
                {
                    finalSkillEndActionCnt -= Time.deltaTime;
                    //Object rotationZ keep rotating 
                    gameObject.transform.Rotate(90f * Time.deltaTime * 7, 0f,0f );

                    if (finalSkillEndActionCnt <= 0)
                    {
                        GetComponent<SkillCircleMove>().enabled = true;
                        isEndActionFinished = true;
                        isFinalSkillEndActionFinished = false; // Reset for next use
                    }
                }

            }



            

        }

    }


    protected override void HandleSkillOnHitAction(Collider col)
    {
        
    }

    protected override void HandleSkillUpdateAction()
    {
        
    }


}

