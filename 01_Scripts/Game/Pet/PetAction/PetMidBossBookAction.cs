using QFSW.MOP2;
using System.Collections.Generic;
using System.Linq;
using TigerForge;
using UnityEngine;

public class PetMidBossBookAction : PetNagaAction
{
    [Header("ターゲットマークのプール")]
    public ObjectPool targetMarkPool;

    [Header("攻撃速度上昇量")]
    [SerializeField] private float attackSpeedIncrease = 0.2f;

    [Header("スキル設置のSE")]
    [SerializeField] private SoundList putSkillSound;

    private List<EnemyStatusBase> markedEnemies = new List<EnemyStatusBase>();

    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.StartListening("ChangePetList", ChangeAttackSpeedInPetList);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.StopListening("ChangePetList", ChangeAttackSpeedInPetList);
    }

    protected override void PetAttackAction()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, attackRange);

        // 3. 取得したコライダーの中から、EnemyStatusBaseを持つものをリストアップ
        List<EnemyStatusBase> potentialTargets = new List<EnemyStatusBase>();
        foreach (var col in enemiesInRange)
        {
            if (col.CompareTag("Enemy"))
            {
                EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();
                if (enemyStat != null && enemyStat.isAlive)
                {
                    potentialTargets.Add(enemyStat);
                }
            }
        }

        int maxTargets = 1;

        //既にマークされている敵を除外
        var selectedTargets = potentialTargets.OrderBy(enemy => Random.value).Take(maxTargets);

        //ターゲットに対してスキルオブジェクトを生成し、初期化処理を行う
        foreach (var target in selectedTargets)
        {
            markedEnemies.Add(target);

            GameObject targetMarker = targetMarkPool.GetObject();


            //設置音再生
            if (!SoundEffect.Instance.IsPlaying(putSkillSound))
            {
                SoundEffect.Instance.Play(putSkillSound);
            }

            // スキルの位置を設定
            targetMarker.transform.position = target.transform.position;

            PetTargetMarkData tgm = targetMarker.GetComponent<PetTargetMarkData>();

            tgm.Initialize(target.transform, targetMarkPool);
        }
    }

    public void SpellBoom()
    {
        ActivePetManager.Instance.spellBomb = true;

        //攻撃音再生
        if (!SoundEffect.Instance.IsPlaying(skillEffectSound))
        {
            SoundEffect.Instance.Play(skillEffectSound);
        }

        foreach (var enemy in markedEnemies)
        {
            if (enemy != null && enemy.isAlive)
            {
                GameObject skill = skillObjPool.GetObject();

                // スキルの位置を設定
                skill.transform.position = enemy.transform.position;

                PetSkillData proj = skill.GetComponent<PetSkillData>();

                SetupProjectile(proj);
            }
        }
        //マークリストをクリア
        markedEnemies.Clear();
    }
    protected override void Attack_Update()
    {
        if (stateCnt <= stateZeroCnt)
        {
            //petAttackColが有効なら無効化
            if (petAttackCol != null)
            {
                DisableAttackCollider();
            }

            if (playerToDist > walkDist)
            {
                // プレイヤーとの距離がwalkDist以上なら歩く
                sm.ChangeState(PetActionStates.Walk);
                return;
            }
            if (playerToDist <= idleDist)
            {
                // プレイヤーとの距離がidleDist以下なら待機状態に遷移
                sm.ChangeState(PetActionStates.Idle);
                return;
            }
            if (playerToDist > runDist)
            {
                // プレイヤーとの距離がrunDist以上なら走る
                sm.ChangeState(PetActionStates.Run);
                return;
            }

            ActivePetManager.Instance.spellBomb = false;
            lookingAtEnemies = true; // 敵を見ている状態にする
            stateCnt = attackCooldown; // 再度クールダウンタイマーをリセット
        }
    }

    protected override void Attack_Exit()
    {
        base.Attack_Exit();

        ActivePetManager.Instance.spellBomb = false;
    }

    public void PetBiteAttack()
    {
        //nullチェック
        if (enemyTransform == null) { return; }

        //敵を見ていない状態にする
        lookingAtEnemies = false;

        //自身のコライダーを消す
        if (petAttackCol != null)
        {
            petAttackCol.enabled = true; // コライダーを付ける
        }
    }

    protected override void DisableAttackCollider()
    {
        if (petAttackCol != null)
        {
            petAttackCol.enabled = false;
        }
    }

    public void SpellStart()
    {
        petSkillPoint.SetActive(true);
    }

    public void SpellEnd()
    {
        petSkillPoint.SetActive(false);
    }

    protected override void SetupProjectile(PetSkillData proj)
    {
        proj.SetPool(skillObjPool);
        // 元々の初期化処理
        proj.Initialize(takeDamages, this.gameObject);
    }

    private void ChangeAttackSpeedInPetList()
    {
        //現在のペットリストを取得
        var petList = ActivePetManager.Instance.activePets;

        //攻撃速度上昇量に今のペットのスポーン数分を掛ける
        float petCount = petList.Count;
        float changeAtkSpd = attackSpeedIncrease * petCount;

        float totalAtkSpd = 1.0f + changeAtkSpd;

        SetAttackSpeed(totalAtkSpd);
    }
}
