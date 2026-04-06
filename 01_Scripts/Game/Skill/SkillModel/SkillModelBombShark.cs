using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillModelBombShark : SkillModelBase
{

    public ObjectPool bombExplosionSkillPool;

    public ObjectPool bombExplosionMuzzlePool;

    private bool hasExploded = false;


    protected override void HandleSkillInit()
    {
        if (ps != null)
        {
            ps.gameObject.SetActive(true);
            ps.Play();
            subPs.Play();
        }
        EnableCollision();

        hasExploded = false;
        isEndActionFinished = false;
        
    }

    protected override void HandleSkillEndAction()
    {
        if (hasExploded)
        {
            isEndActionFinished = true;
            return;
        }
        hasExploded = true;

        Vector3 spawnPos = transform.position;
         spawnPos.y = 0.35f; //height fixed with 1
        
        SkillModelBase sm = bombExplosionSkillPool.GetObjectComponent<SkillModelBase>(spawnPos, Quaternion.identity);
        sm.SetSkill(skillIdType, skillId, skillLevel, skillName, 0.77f, skillSpeed, skillDamage, skillSize, isFinalSkill);

        sm.InitStatusSizeAndPosY();

        sm.SetTraitManual(isAfterDashCast, isAfterDashEnhanced, isHpChangeCast, isHpChangeEnhanced, isFinishCastExplosion, isFinishCastSplit, isKillEnemyExplosion, isKillEnemySkullSoul, isKillEnemyBrightSoul, isSkillMovingDropFire, isSkillMovingDropSpike, isDoubleCast, isEnchantFire, isEnchantIce, isEnchantPoison, isEnchantLightning, isPushback, isGetItemCast, isGetItemEnhanced, skillCriticalChance);

        SoundEffect.Instance.PlayBombExplosionSound();


        if (isFinalSkill)
        {
            Vector3 spawnPos2 = transform.position + new Vector3(0, -0.56f, 0);

            SkillModelBase sm2 = bombExplosionMuzzlePool.GetObjectComponent<SkillModelBase>(spawnPos2, Quaternion.identity);
            sm2.SetSkill(skillIdType, skillId, skillLevel, skillName, 4.2f, skillSpeed, skillDamage * 0.28f, skillSize, isFinalSkill);
            sm2.InitStatusSizeAndPosY();
            sm2.SetTraitManual(false, false, false, false, false, isFinishCastSplit, isKillEnemyExplosion, isKillEnemySkullSoul, isKillEnemyBrightSoul, isSkillMovingDropFire, false, false, isEnchantFire, isEnchantIce, isEnchantPoison, isEnchantLightning, false, false, false, skillCriticalChance);
        }


        subPs.Stop();

        skillDamage = 0f; //bomb itself does no damage, only explosion does
        isEndActionFinished = true;
        //skillDuration = 0.1f; //short duration to despawn itself
    }

    protected override void HandleSkillOnHitAction(Collider col)
    {
        if(hasExploded) return;
        hasExploded = true;

        Vector3 spawnPos = transform.position;
         spawnPos.y = 0.35f; //height fixed with 1
        
        SkillModelBase sm = bombExplosionSkillPool.GetObjectComponent<SkillModelBase>(spawnPos, Quaternion.identity);
        sm.SetSkill(skillIdType, skillId, skillLevel, skillName, 0.77f, skillSpeed, skillDamage, skillSize, isFinalSkill);

        sm.InitStatusSizeAndPosY();

        sm.SetTraitManual(isAfterDashCast, isAfterDashEnhanced, isHpChangeCast, isHpChangeEnhanced, isFinishCastExplosion, isFinishCastSplit, isKillEnemyExplosion, isKillEnemySkullSoul, isKillEnemyBrightSoul, isSkillMovingDropFire, isSkillMovingDropSpike, isDoubleCast, isEnchantFire, isEnchantIce, isEnchantPoison, isEnchantLightning, isPushback, isGetItemCast, isGetItemEnhanced, skillCriticalChance);

        SoundEffect.Instance.PlayBombExplosionSound();


        if (isFinalSkill)
        {
            Vector3 spawnPos2 = transform.position + new Vector3(0, -0.56f, 0);

            SkillModelBase sm2 = bombExplosionMuzzlePool.GetObjectComponent<SkillModelBase>(spawnPos2, Quaternion.identity);
            sm2.SetSkill(skillIdType, skillId, skillLevel, skillName, 4.2f, skillSpeed, skillDamage * 0.28f, skillSize, isFinalSkill);
            sm2.InitStatusSizeAndPosY();
            sm2.SetTraitManual(false, false, false, false, false, isFinishCastSplit, isKillEnemyExplosion, isKillEnemySkullSoul, isKillEnemyBrightSoul, isSkillMovingDropFire, false, false, isEnchantFire, isEnchantIce, isEnchantPoison, isEnchantLightning, false, false, false, skillCriticalChance);
        }


        subPs.Stop();

        skillDamage = 0f; //bomb itself does no damage, only explosion does
        skillDuration = 0.1f; //short duration to despawn itself

        

    }

    protected override void HandleSkillUpdateAction()
    {
        
    }

}

