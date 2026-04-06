using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class SkillCasterThunder : SkillCasterBase
{
    const float RANGE = 14f;
   
    protected override void CastSkill()
    {
        int enemyMask = LayerMask.GetMask("EnemySpider","EnemyMage","EnemyDragon","EnemyBossSpider","EnemyMushroom","EnemyBat");

        Collider[] hits = Physics.OverlapSphere(playerTran.position, RANGE, enemyMask);
        if (hits.Length == 0) return;

        int strikeCount = Mathf.Max(1, projectileNumFinal);
        Collider nearest = hits.OrderBy(h => (h.transform.position - playerTran.position).sqrMagnitude).First();

        SpawnThunderAt(nearest.transform.position);

        List<Collider> cand = hits.ToList();
        cand.Remove(nearest);

        for (int i = 1; i < strikeCount && cand.Count > 0; ++i)
        {
            int rnd = Random.Range(0, cand.Count);
            SpawnThunderAt(cand[rnd].transform.position);
            cand.RemoveAt(rnd);
        }

        if (isDoubleCast)
        {
            float chance = Random.Range(0f, 1f);
            if (chance > 0.5f && !SkillEffectManager.Instance.universalTrait.isFairJudge) return;
            Invoke("CastOneMOre", 0.7f);
        }


        SoundEffect.Instance.Play(SoundList.Thunder);
    }

    void CastOneMOre()
    {
        EventManager.EmitEvent("OnPlayDoubleCastAnim");
        ActiveBuffManager.Instance.AddStack(TraitType.DoubleCast);

        int enemyMask = LayerMask.GetMask("EnemySpider","EnemyMage","EnemyDragon","EnemyBossSpider","EnemyMushroom","EnemyBat");

        Collider[] hits = Physics.OverlapSphere(playerTran.position, RANGE, enemyMask);
        if (hits.Length == 0) return;

        int strikeCount = Mathf.Max(1, projectileNumFinal);
        Collider nearest = hits.OrderBy(h => (h.transform.position - playerTran.position).sqrMagnitude).First();

        SpawnThunderAt(nearest.transform.position);

        List<Collider> cand = hits.ToList();
        cand.Remove(nearest);

        for (int i = 1; i < strikeCount && cand.Count > 0; ++i)
        {
            int rnd = Random.Range(0, cand.Count);
            SpawnThunderAt(cand[rnd].transform.position);
            cand.RemoveAt(rnd);
        }

         SoundEffect.Instance.Play(SoundList.Thunder);

    }

    void SpawnThunderAt(Vector3 pos)
    {
        pos.y = 0.7f;
        SkillModelBase sm = skillObjPool.GetObjectComponent<SkillModelBase>(pos, Quaternion.identity);
        sm.SetSkill(casterIdType, casterId, casterLevel, casterName, durationFinal, speedFinal, damageFinal, sizeFinal, isFinalSkill);
        sm.SetTrait(currentTrait);
        sm.InitStatusSizeAndPosY();
    }


}

