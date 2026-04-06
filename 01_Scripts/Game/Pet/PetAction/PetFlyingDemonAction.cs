using TigerForge;
using UnityEngine;

public class PetFlyingDemonAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("攻撃力の上昇倍率(%)")]
    [SerializeField] private float attackPowerUpRate = 25f;

    [Header("1キルあたりのスケール増加倍率")]
    [SerializeField] private float scaleIncreaseMultiplier = 0.1f;

    [Header("スケールの最大倍率")]
    [SerializeField] private float maxScaleMultiplier = 2.0f;

    //攻撃力の倍率ベース
    private float baseAttackPower = 1f;

    //魂吸収エフェクトの位置補正
    private Vector3 newEffectPos = new Vector3(0f, 4.5f, 0f);

    //現在の倍率と、元のサイズを記憶
    private float currentScaleMultiplier = 1.0f;
    private Vector3 originalScale;

    #endregion --------------------------------------------
    protected override void Start()
    {
        base.Start();

        //最大スケールを計算
        this.originalScale = transform.localScale;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.StartListening(GameEvent.LastAttack_FlyingDemon, AttackUp);   
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.StopListening(GameEvent.LastAttack_FlyingDemon, AttackUp);
    }


    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();

    protected override void OnTakeDamegesEnemy(EnemyStatusBase enemyStat)
    {
        //最終ダメージ計算
        float finalDamage = takeDamages * baseAttackPower;

        //敵にダメージを与える
        FinalDanages(enemyStat, finalDamage, LastAttackType.FlyingDemon);
    }

    protected override GameObject FindNearestEnemy()
    {
        // 索敵したいレイヤーをまとめたレイヤーマスクを作成
        int enemyMask = LayerMask.GetMask("EnemySpider", "EnemyMage", "EnemyDragon", "EnemyBossSpider", "EnemyMushroom");

        // ペットの位置からattackRangeの範囲内にある、指定したレイヤーのコライダーを全て取得
        Collider[] enemiesInAttackRange = Physics.OverlapSphere(transform.position, attackRange, enemyMask);

        // 範囲内に敵が一人もいなければ、何もせずに処理を終了
        if (enemiesInAttackRange.Length == 0)
        {
            enemyTransform = null;
            return null;
        }

        GameObject closestEnemy = null;        // 最適なターゲットを保持する変数
        float lowestHp = Mathf.Infinity;     // 今まで見つけた敵の最低HP
        float minSqrDistance = Mathf.Infinity; // 最低HPの敵との距離

        foreach (Collider enemyCollider in enemiesInAttackRange)
        {
            EnemyStatusBase enemyStat = enemyCollider.GetComponent<EnemyStatusBase>();

            if (enemyStat != null && enemyStat.enemyHp > 0)
            {
                // 現在チェックしている敵のHP
                float currentHp = enemyStat.enemyHp;

                if (currentHp < lowestHp)
                {
                    lowestHp = currentHp;
                    closestEnemy = enemyCollider.gameObject;
                    // このターゲットとの距離も記録しておく
                    minSqrDistance = (transform.position - enemyCollider.transform.position).sqrMagnitude;
                }
                else if (currentHp == lowestHp)
                {
                    //HPが同じなら、距離を比較する
                    float sqrDist = (transform.position - enemyCollider.transform.position).sqrMagnitude;

                    //もし、今までの最短距離よりも近ければ
                    if (sqrDist < minSqrDistance)
                    {
                        //ターゲットをこちらに乗り換える
                        closestEnemy = enemyCollider.gameObject;
                        minSqrDistance = sqrDist;
                    }
                }
            }
        }

        // 最も近い敵をenemyTransformに設定
        enemyTransform = closestEnemy?.transform;
        // 最も近い敵を返す
        return closestEnemy;
    }

    //固有能力：攻撃力アップ
    private void AttackUp()
    {
        //スケールアップ
        if (currentScaleMultiplier < maxScaleMultiplier)
        {
            currentScaleMultiplier += scaleIncreaseMultiplier;

            currentScaleMultiplier = Mathf.Min(currentScaleMultiplier, maxScaleMultiplier);

            transform.localScale = originalScale * currentScaleMultiplier;
        }

        //攻撃力アップ(永続的にかかる)
        baseAttackPower +=  attackPowerUpRate / 100f;

        //魂吸収エフェクト再生
        ItemManager.Instance.PlayTimeParticleInObject(this.gameObject,
                                                      ItemManager.Instance.itemKeepTimePs.SoulEatPs,
                                                      newEffectPos);

        //魂吸収音再生
        if (!SoundEffect.Instance.IsPlaying(skillEffectSound))
        {
            SoundEffect.Instance.Play(skillEffectSound);
        }
    }
}

