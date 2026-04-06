using UnityEngine;

public class PetGoldenSpiderAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    //基礎倍率
    private float baseMultiplier = 1f;

    [Header("コイン獲得量の上昇倍率(%)")]
    public float GoldUpRate = 10f;

    [Header("経験値効率の上昇倍率(%)")]
    public float ExpUpRate = 10f;

    [Header("プレイヤーの攻撃上昇倍率(%)")]
    public float PlayerAttackPowerUpRate = 10f;

    #endregion --------------------------------------------

    protected override void Start()
    {
        base.Start();
        ApplyGoldAuraBuff();
    }

    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();

    //コイン獲得量＆経験値効率アップ
    //プレイヤーとペットの攻撃力アップ
    private void ApplyGoldAuraBuff()
    {
        BuffManager.Instance.gobalGoldGain += GoldUpRate;
        BuffManager.Instance.gobalExpGain += ExpUpRate;
        BuffManager.Instance.gobalDamageAdd += PlayerAttackPowerUpRate;
    }
}
