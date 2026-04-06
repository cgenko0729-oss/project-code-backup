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
using System.Linq;

public class SkillModelMonsterFlower : SkillModelBase
{

    public Animator animator;
    public float attackCnt = 1.4f;
    public float attackCntMax = 1.4f;
    public ObjectPool flowerBulletPool;

    public float enemySearchRadius = 7f;

    public float skillDurationStart = 1f;
    public bool isDurationInit = false;
    public bool isFinalEndPs = false;

    public ParticleSystem poisonCloudPs;
    public float poisonCloudAttackCnt = 1.4f;
    public float poisonCloudAttackCntMax = 2.1f;

    protected override void HandleSkillInit()
    {
        DisableCollision();

        ps.Play();

        isDurationInit = false;

        isEndActionFinished = false;
        isFinalEndPs = false;



    }


    protected override void HandleSkillEndAction()
    {
        //if (!isFinalEndPs)
        //{
        //    isFinalEndPs = true;
        //    subPs.Play();
        //}

        //Invoke("FinishEndAction", 0.5f);
        
    }

    void FinishEndAction()
    {
        isEndActionFinished = true;
    }

    protected override void HandleSkillOnHitAction(Collider col)
    {
       
    }

    void DisableSkillCollision()
    {
        skillCollider.enabled = false;

    }

    protected override void HandleSkillUpdateAction()
    {

        if (isFinalSkill)
        {
            poisonCloudAttackCnt -= Time.deltaTime;
            if (poisonCloudAttackCnt <= 0f)
            {
                EnableCollision();
                poisonCloudAttackCnt = poisonCloudAttackCntMax;
                poisonCloudPs.Play();
                skillCollider.enabled = true;
                Invoke("DisableSkillCollision", 0.42f);
            }
        }
        

        if (!isDurationInit)
        {
            isDurationInit = true;
            skillDurationStart = skillDuration;
        }

        attackCnt -= Time.deltaTime;

        if(attackCnt <= 0f)
        {
            attackCnt = attackCntMax;
            animator.SetTrigger("isAttack");

            int enemyMask = LayerMask.GetMask("EnemySpider","EnemyMage","EnemyDragon","EnemyBossSpider","EnemyMushroom");
            Collider[] hits = Physics.OverlapSphere(transform.position, enemySearchRadius, enemyMask);
            if (hits.Length == 0) return;

            Collider nearest = hits.OrderBy(h => (h.transform.position - transform.position).sqrMagnitude).First();

            //face to enemy
            Vector3 dir = (nearest.transform.position - transform.position).normalized;
            dir.y = 0;
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            Vector3 spawnPos = transform.position + dir * 1.4f + new Vector3(0, 0.075f, 0);
            SkillModelBase sm = flowerBulletPool.GetObjectComponent<SkillModelBase>(spawnPos, Quaternion.LookRotation(dir, Vector3.up));
            sm.SetSkill(skillIdType, skillId, skillLevel, skillName, 1.49f, skillSpeed, skillDamage * 0.7f, skillSize, isFinalSkill);
            sm.InitStatusSizeAndPosY();
            sm.SetTraitManual(isAfterDashCast, isAfterDashEnhanced, isHpChangeCast, isHpChangeEnhanced, isFinishCastExplosion, isFinishCastSplit, isKillEnemyExplosion, isKillEnemySkullSoul, isKillEnemyBrightSoul, isSkillMovingDropFire, isSkillMovingDropSpike, isDoubleCast, isEnchantFire, isEnchantIce, isEnchantPoison, isEnchantLightning, isPushback, isGetItemCast, isGetItemEnhanced, skillCriticalChance);

            SkillForwardMove move = sm.GetComponent<SkillForwardMove>();
            move.moveVec = dir * skillSpeed;



            //flowerBulletPool.GetO

        }

    }

}

