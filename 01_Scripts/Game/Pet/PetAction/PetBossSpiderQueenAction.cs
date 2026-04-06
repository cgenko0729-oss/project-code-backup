using QFSW.MOP2;
using System;
using System.Collections;
using TigerForge;
using UnityEngine;

public class PetBossSpiderQueenAction : PetNagaAction
{
    [Header("扇形の範囲設定")]
    [SerializeField] private int projectilesPerVolley = 5;  //1回の斉射で発射する弾の数
    [SerializeField] private float spreadAngle = 130f;       //扇形の合計角度

    [Header("繭オブジェクト")]
    public GameObject cocoonObj;

    [Header("繭の生成開始高度")]
    [SerializeField] private float summonHeight = 10f;

    protected override void PetAttackAction()
    {
        //nullチェック
        if (enemyTransform == null) { return; }

        Vector3 baseDirection = (enemyTransform.position - petSkillPoint.transform.position).normalized;
        baseDirection.y = 0;

        lookingAtEnemies = false;

        for (int i = 0; i < projectilesPerVolley; i++)
        {
            float angle = 0f;
            if (projectilesPerVolley > 1)
            {
                float startAngle = -spreadAngle * 0.5f;
                float angleStep = spreadAngle / (projectilesPerVolley - 1);
                angle = startAngle + angleStep * i;
            }

            Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);
            Vector3 fireDirection = rotation * baseDirection;

            GameObject skill = skillObjPool.GetObject();
            skill.transform.position = petSkillPoint.transform.position;
            skill.transform.rotation = Quaternion.LookRotation(fireDirection);

            PetSkillData proj = skill.GetComponent<PetSkillData>();
            if (proj != null)
            {
                SetupProjectile(proj);
                proj.SetDirection(fireDirection);
            }
        }

        PlayAttackSounds();
    }

    protected override void Start()
    {
        base.Start();
        sm.ChangeState(PetActionStates.ActiveSkillMotion);
    }

    #region ステート上書き---------------------------------------

    protected override void ActiveSkillMotion_Update()
    {
        //アニメーション変更
        AnimationEndChange("ActiveSkillMotion");
    }

    //IdleMotion------------------------------------------
    protected override void IdleMotion_Enter()
    {
        petAnimator.SetBool("isIdle", false);
        petAnimator.SetTrigger("IdleMotion");
    }

    protected override void IdleMotion_Update()
    {
        if (playerToDist < runDist)
        {
            if (nearestEnemy != null)
            {
                if (petData.petRoles == null || !petData.petRoles.Contains(PetRole.NoAttack))
                {
                    float dist = Vector3.Distance(transform.position, nearestEnemy.transform.position);
                    if (dist <= attackRange)
                    {
                        // 近くの敵が攻撃範囲内なら敵追従状態に遷移
                        sm.ChangeState(PetActionStates.Attack);
                        return;
                    }
                }
            }
        }
        if (playerToDist > runDist)
        {
            // プレイヤーとの距離がrunDist以上なら走る
            sm.ChangeState(PetActionStates.Run);
            return;
        }
        if (playerToDist > walkDist)
        {
            // プレイヤーとの距離がwalkDist以上なら歩く
            sm.ChangeState(PetActionStates.Walk);
            return;
        }
    }
    //-----------------------------------------------

    #endregion --------------------------------------------

    private void SummonCocoon()
    {
        if(!SoundEffect.Instance.IsPlaying(SoundList.SpiderBossPhrase2Se))
        {
            SoundEffect.Instance.Play(SoundList.SpiderBossPhrase2Se);
        } 

        float spawnY = petSkillPoint.transform.position.y - summonHeight;

        Vector3 spawnPosition = new Vector3(petSkillPoint.transform.position.x, spawnY, petSkillPoint.transform.position.z);

        GameObject cocoon = Instantiate(cocoonObj, spawnPosition, Quaternion.identity);      
    }
}
