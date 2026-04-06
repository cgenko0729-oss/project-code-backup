using DG.Tweening;
using Hellmade.Sound; //SoundManager
using MonsterLove.StateMachine; //StateMachine
using QFSW.MOP2;                //Object Pool
using System.Collections.Generic;
using TigerForge;               //EventManager
using TMPro;    
using UnityEngine;
using UnityEngine.UI;     
using System.Linq;

public class SkillModelStar : SkillModelBase
{

    public GameObject endExplosionObj;
    private Vector3 afterEffectOffset  = new Vector3(0f,1.21f, 0f);

    public ObjectPool skillAfterMagicPool;

    public bool isSecondExplode = false;
    public bool isThirdExplode = false;

    public GameObject playerShadowObj;

    protected override void HandleSkillInit()
    {
        if (ps != null)
        {
            ps.gameObject.SetActive(true);
            ps.Play();
        }

        if (subPs) subPs.Stop();

        EnableCollision();

        isEndActionFinished = false;
        isSecondExplode = false;
        isThirdExplode = false;
    }

    protected override void HandleSkillEndAction()
    {
        if (!endExplosionObj) return;
        GameObject explosion = Instantiate(endExplosionObj, transform.position + afterEffectOffset, Quaternion.identity);
        SKillAfterMathHitController am =explosion.GetComponent<SKillAfterMathHitController>();
        am.SetEffectStatus(skillDamage * 2.1f, skillSize);

        isEndActionFinished = true;

        if (isFinalSkill)
        {
            if (isThirdExplode) return;

            if (!isSecondExplode)
            {
               
                GameObject shadow = Instantiate(playerShadowObj, transform.position, Quaternion.identity);
                
                Transform playerTrans = GameObject.FindGameObjectWithTag("Player").transform; //playerShadow face player forward
                
               shadow.transform.position = new Vector3(shadow.transform.position.x, 0f, shadow.transform.position.z);

                //shadow.transform.forward = playerTrans.forward;d
                //Vector3 shadowRot = shadow.transform.forward;  //save the current rotation of playerShadowObj

               
                    float RANGE = 14f;
                    int enemyMask = LayerMask.GetMask("EnemySpider","EnemyMage","EnemyDragon","EnemyBossSpider","EnemyMushroom");
                    Collider[] hits = Physics.OverlapSphere(transform.position, RANGE, enemyMask);
                    if (hits.Length == 0) return;
                    Collider nearest = hits.OrderBy(h => (h.transform.position - transform.position).sqrMagnitude).First();
                    Vector3 dirToEnemy = (nearest.transform.position - transform.position).normalized;
                dirToEnemy.y = 0f;
                    shadow.transform.forward = dirToEnemy;

                DOVirtual.DelayedCall(0.35f, () => {


                    SkillModelStar sm = skillAfterMagicPool.GetObjectComponent<SkillModelStar>(transform.position, Quaternion.identity);
                sm.SetSkill(skillIdType, skillId, skillLevel, skillName,0.49f, skillSpeed, skillDamage,skillSize, isFinalSkill);
                sm.CopyTrait(isFinishCastExplosion, isKillEnemyExplosion, isKillEnemySkullSoul, isKillEnemyBrightSoul, isSkillMovingDropFire, isPushback, isEnchantFire, isEnchantIce, isEnchantPoison,isTreasureHunter);
                sm.InitStatusSizeAndPosY();
                sm.isSecondExplode = true;
                var starMove = sm.GetComponent<SkillForwardMove>();
                starMove.moveVec = dirToEnemy * skillSpeed;
                });


            }

            if (isSecondExplode)
            {

                 GameObject shadow = Instantiate(playerShadowObj, transform.position, Quaternion.identity);
                
                Transform playerTrans = GameObject.FindGameObjectWithTag("Player").transform; //playerShadow face player forward
                
               shadow.transform.position = new Vector3(shadow.transform.position.x, 0f, shadow.transform.position.z);

                //shadow.transform.forward = playerTrans.forward;d
                //Vector3 shadowRot = shadow.transform.forward;  //save the current rotation of playerShadowObj

               
                    float RANGE = 14f;
                    int enemyMask = LayerMask.GetMask("EnemySpider","EnemyMage","EnemyDragon","EnemyBossSpider","EnemyMushroom", "EnemyBat");
                    Collider[] hits = Physics.OverlapSphere(transform.position, RANGE, enemyMask);
                    if (hits.Length == 0) return;
                    Collider nearest = hits.OrderBy(h => (h.transform.position - transform.position).sqrMagnitude).First();
                    Vector3 dirToEnemy = (nearest.transform.position - transform.position).normalized;
                //set y of dirToEnemy is 0, so shadow won't fly up or down : 
                 
                dirToEnemy.y = 0f;

                shadow.transform.forward = dirToEnemy;

                DOVirtual.DelayedCall(0.35f, () => {


                    SkillModelStar sm = skillAfterMagicPool.GetObjectComponent<SkillModelStar>(transform.position, Quaternion.identity);
                sm.SetSkill(skillIdType, skillId, skillLevel, skillName,0.49f, skillSpeed, skillDamage,skillSize, isFinalSkill);
                sm.CopyTrait(isFinishCastExplosion, isKillEnemyExplosion, isKillEnemySkullSoul, isKillEnemyBrightSoul, isSkillMovingDropFire, isPushback, isEnchantFire, isEnchantIce, isEnchantPoison,isTreasureHunter);
                sm.InitStatusSizeAndPosY();
                sm.isThirdExplode = true;
                var starMove = sm.GetComponent<SkillForwardMove>();
                starMove.moveVec = dirToEnemy * skillSpeed;
                });
            }

            // 進化後の追加効果が発動したら、さらにキャラ強化の追加攻撃を行う
            float damage = skillDamage * 
                (1 + (BuffManager.Instance.gobalDamageAdd / 100));
            if(UpgradeEffectManager.Instance.Active(BuffType.AddAttackPerFinalSkill,
                gameObject.transform.position,damage))
            {

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

