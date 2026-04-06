using TigerForge;
using UnityEngine;

public class PetPummyAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("攻撃したときのステータスの上昇倍率(%)")]
    [SerializeField] private float statusUpRate_Attack = 1f;

    [Header("攻撃したときのステータスの最大倍率(%)")]
    [SerializeField] private float maxStatusUpRate_Attack = 100f;

    [Space]

    [Header("倒したときのステータスの上昇倍率(%)")]
    [SerializeField] private float statusUpRate_LastAttack = 5f;

    [Header("倒したときのステータスの最大倍率(%)")]
    [SerializeField] private float maxStatusUpRate_LastAttack = 50f;

    [Header("エフェクトの位置オブジェクト")]
    [SerializeField] private GameObject effectPosObject;

    //倍率ベース
    private float baseRate = 0f;

    //倒したときの倍率ベース
    private float lastBaseRate = 0f;

    //パーセントで分けるための定数
    private const float percentDivisor = 100f;

    #endregion --------------------------------------------

    protected override void Update()
    {
        base.Update(); 

        float totalMultiplier = (1f + baseRate) * (1f + lastBaseRate);

        this.takeDamages = petData.attackPower * totalMultiplier;

        SetAttackSpeed(totalMultiplier);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.StartListening(GameEvent.LastAttack_Pummy, HandleLastAttack);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.StopListening(GameEvent.LastAttack_Pummy, HandleLastAttack);
    }

    protected override void OnTakeDamegesEnemy(EnemyStatusBase enemyStat)
    {
        //敵にダメージを与える
        FinalDanages(enemyStat, takeDamages, LastAttackType.Pummy);
    }

    //敵を倒したとき
    private void HandleLastAttack()
    {
        //最大値チェック
        if (lastBaseRate >= maxStatusUpRate_LastAttack / percentDivisor)
        {
            lastBaseRate = maxStatusUpRate_LastAttack / percentDivisor;
            return;
        }
        //ステータスアップ
        lastBaseRate += statusUpRate_LastAttack / percentDivisor;

        Vector3 effectPos   = new Vector3(0, 0, 0);
        Vector3 effectScale = new Vector3(0.3f, 0.3f, 0.3f);

        //魂吸収エフェクト再生
        ItemManager.Instance.PlayTimeParticleInObject(effectPosObject,
                                                      ItemManager.Instance.itemKeepTimePs.SoulEatPs,
                                                      effectPos, effectScale);

        if(!SoundEffect.Instance.IsPlaying(skillEffectSound))
        {
            SoundEffect.Instance.Play(skillEffectSound);
        }
    }
    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction()
    {
        //nullチェック
        if (enemyTransform == null) { return; }

        //敵を見ている状態にする
        lookingAtEnemies = false;

        //自身のコライダーを消す
        if (petAttackCol != null)
        {
            petAttackCol.isTrigger = true; // コライダーを付ける
        }

        //攻撃する度にステータスアップ
        //最大値チェック
        if (baseRate >= maxStatusUpRate_Attack / percentDivisor)
        {
            baseRate = maxStatusUpRate_Attack / percentDivisor;
            return;
        }

        //ステータスアップ
        baseRate += statusUpRate_Attack / percentDivisor;

    }
}
