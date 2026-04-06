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

public class SkillModelBlackHole : SkillModelBase
{

    public ObjectColliderSwitcher colliderSwitcher;

    public bool isBlackHoleInit = false;
    SkillBlackHoleAbsorb skillBlackHoleAbsorb;

    public ObjectPool endExplosionPool;

    protected override void HandleSkillInit()
    {
        //if(!colliderSwitcher)colliderSwitcher = GetComponent<ObjectColliderSwitcher>();

        isEndActionFinished = false;

        if (!isBlackHoleInit)
        {
            skillBlackHoleAbsorb = GetComponent<SkillBlackHoleAbsorb>();
        }

        skillBlackHoleAbsorb.absorbCnt = 0.3f;

        EnableCollision();

        if (!isFinalSkill)
        {
            if (ps != null)
            {
                ps.gameObject.SetActive(true);
                ps.Play();
            }

           // colliderSwitcher.disableColliderInterval = 0.219f;

        }

        if (isFinalSkill)
        {
            hasEndAction = true;
        }
        else
        {
            hasEndAction = false;
        }


    }

    void SpawnFinalExplosion()
    {
        //endExplosionPool.GetObject(transform.position, Quaternion.identity);

        SkillModelBase sm = endExplosionPool.GetObjectComponent<SkillModelBase>(transform.position, Quaternion.identity);
        sm.SetSkill(skillIdType, skillId, skillLevel, skillName, 0.77f, skillSpeed, skillDamage * 2, skillSize, isFinalSkill);
        sm.InitStatusSizeAndPosY();
        sm.SetTraitManual(isAfterDashCast, isAfterDashEnhanced, isHpChangeCast, isHpChangeEnhanced, isFinishCastExplosion, isFinishCastSplit, isKillEnemyExplosion, isKillEnemySkullSoul, isKillEnemyBrightSoul, isSkillMovingDropFire, isSkillMovingDropSpike, isDoubleCast, isEnchantFire, isEnchantIce, isEnchantPoison, isEnchantLightning, isPushback, isGetItemCast, isGetItemEnhanced, skillCriticalChance);
    }

    protected override void HandleSkillEndAction()
    {
        if (isFinalSkill & !isEndActionFinished)
        {
            isEndActionFinished = true;
            SpawnFinalExplosion();

        }
        
    }

    protected override void HandleSkillOnHitAction(Collider col)
    {
        
    }

    protected override void HandleSkillUpdateAction()
    {
        
    }

}

