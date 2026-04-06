using UnityEngine;

public class PetDragonAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("スキル発動時のハイライトエフェクト")]
    [Tooltip("有効化するハイライトエフェクトのコンポーネント")]
    [SerializeField] private Behaviour highlightEffect;

    [Header("プレイヤーのHPの割合(その１)")]
    [SerializeField] private float playerHpRateThreshold_First = 0.5f;


    [Header("プレイヤーのHPの割合(その２)")]
    [SerializeField]private float  playerHpRateThreshold_Secound = 0.5f;

    //基礎倍率
    private float baseAttackPower = 1f;
    private float baseAttackSpeed = 1f;

    [Header("攻撃力の上昇倍率(%)")]
    public float attackPowerUpRate = 50f;

    [Header("攻撃速度の上昇倍率(%)")]
    public float attackSpeedUpRate = 20f;

    private enum DragonMode
    {
        Normal,
        SortSpot,
        HighSortSpot
    }

    private DragonMode currentMode = DragonMode.Normal;

    private float baseRate = 1f;

    #endregion --------------------------------------------

    protected override void Update()
    {
        base.Update();

        //プレイヤーの現在HPを取得
        float playerCurrentHP = playerStatus.NowHp;
        //プレイヤーの最大HPを取得
        float playerMaxHP = playerStatus.MaxHp;

        //プレイヤーのHP割合を計算
        float playerHpRate = playerCurrentHP / playerMaxHP;

        //プレイヤーのHPの割合が第二の閾値以下ならさらに攻撃力アップ
        if (playerHpRate <= playerHpRateThreshold_Secound)
        {
            if (currentMode != DragonMode.HighSortSpot)
            {
                HighSortSpot();
            }
        }
        //またはプレイヤーのHPの割合が閾値以下なら攻撃力アップ
        else if (playerHpRate <= playerHpRateThreshold_First)
        {
            if (currentMode != DragonMode.SortSpot)
            {
                SortSpot();   
            }
        }
        else
        {
            if (currentMode != DragonMode.Normal)
            {
                ResetSortSpot();
            }
        }
    }

    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();

    private void SortSpot()
    {
        //攻撃力アップ
        baseAttackPower = baseRate + (attackPowerUpRate / 100f);

        //攻撃速度アップ
        baseAttackSpeed = baseRate + (attackSpeedUpRate / 100f);
        SetAttackSpeed(baseAttackSpeed);

        //ハイライトエフェクトを有効化
        if (highlightEffect != null)
        {
            highlightEffect.enabled = true;
        }

        //攻撃音を噛みつく(強)音にする
        attackSound = SoundList.Bite_StrongSe;

        currentMode = DragonMode.SortSpot;
    }

    private void HighSortSpot()
    {
        //二倍にする
        float doubleUpRate =  2f;

        //攻撃力アップ
        baseAttackPower = baseRate + (attackPowerUpRate / 100f) * doubleUpRate;

        //攻撃速度アップ
        baseAttackSpeed = baseRate + (attackSpeedUpRate / 100f) * doubleUpRate;
        SetAttackSpeed(baseAttackSpeed);

        SoundEffect.Instance.Play(SoundList.TurnipaBossPlaceAoeSe);

        currentMode = DragonMode.HighSortSpot;
    }

    private void ResetSortSpot()
    {
        //攻撃力を元に戻す
        baseAttackPower = baseRate;

        //攻撃速度を元に戻す
        baseAttackSpeed = baseRate;
        SetAttackSpeed(baseAttackSpeed);

        //ハイライトエフェクトを無効化
        if (highlightEffect != null)
        {
            highlightEffect.enabled = false;
        }

        //攻撃音を戻す
        attackSound = SoundList.Bite_WeakSe;

        currentMode = DragonMode.Normal;
    }

    protected override void OnTakeDamegesEnemy(EnemyStatusBase enemyStat)
    {
        //最終ダメージ計算
        float finalDamage = takeDamages * baseAttackPower;

        //敵にダメージを与える
        FinalDanages(enemyStat, finalDamage);
    }
}
