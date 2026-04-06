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

public class SkillModelBIrd : SkillModelBase
{

    public Transform playerTrans;
    public float distWithPlayer = 10f;

    public SkillFollowMouseMove skillMove;

    public bool isFlyBack = false;

    public GameObject endEffect;

    public float OnHitActionCnt = 0.3f;

    public SkillFollowMouseMove mouseMove;

    protected override void HandleSkillInit()
    {
        if (!mouseMove) mouseMove = GetComponent<SkillFollowMouseMove>();

        playerTrans = GameObject.FindWithTag("Player").transform;

        ps.Play();

        isEndActionFinished = false;

        if(!skillMove) skillMove = GetComponent<SkillFollowMouseMove>();
        skillMove.enabled = true;

        isFlyBack = false;

    }

    protected override void HandleSkillEndAction()
    {

        if (isFinalSkill)
        {

           

            if (!isFlyBack)
            {
                 skillMove.enabled = false;

                isFlyBack = true;

                Vector3 dir = (playerTrans.position - transform.position).normalized;
                Quaternion lookRot = Quaternion.LookRotation(dir);
                transform.DORotateQuaternion(lookRot, 0.2f).SetEase(Ease.InSine);

                transform.DOMove(playerTrans.position, 1.4f).SetEase(Ease.InSine).OnComplete(() => {
                    isEndActionFinished = true;
                });

            }

            //rotate towardplayer
            
            

            //distWithPlayer = Vector3.Distance(transform.position, playerTrans.position);
            //if (distWithPlayer > 2.1f)
            //{
                
            //}
        }
        else
        {
            Instantiate(endEffect, transform.position, Quaternion.identity);
            isEndActionFinished = true;
        }

        



    }

    protected override void HandleSkillOnHitAction(Collider col)
    {
        isOnHitActionFinished = false;

        if (OnHitActionCnt <= 0f)
        {

            mouseMove.canRotate = false;

            isOnHitActionFinished = true;
            OnHitActionCnt = 0.5f;
            EnemyAnimUtil.PunchRotation(transform, new Vector3(30,0,0));
            //EventManager.EmitEvent("BirdHitEnemy");
            
            transform.DOPunchRotation(new Vector3(42,0,0),0.35f,vibrato: 7,elasticity: 0.35f).SetEase(Ease.OutQuad).onComplete = () => {
            mouseMove.canRotate = true;
        };
            
            
            Debug.Log("Bird Hit Enemy");
        }
       
    }

    protected override void HandleSkillUpdateAction()
    {
        OnHitActionCnt -= Time.deltaTime;

    }



}

