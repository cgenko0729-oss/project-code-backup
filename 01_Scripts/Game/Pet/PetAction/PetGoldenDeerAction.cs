using UnityEngine;

public class PetGoldenDeerAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    //基礎倍率
    private float baseMultiplier = 1f;

    [Header("経験値効率の上昇倍率(%)")]
    public float ExpUpRate = 10f;

    [Header("幸運ステータス上昇倍率(%)")]
    public float LuckUpRate = 10f;

    #endregion --------------------------------------------

    protected override void Start()
    {
        base.Start();
        ExpRateANDLackUp();
    }

    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();

    //固有能力：経験値効率＆幸運ステータスアップ
    private void ExpRateANDLackUp()
    {
        BuffManager.Instance.gobalExpGain += ExpUpRate;
        SkillManager.Instance.luck += LuckUpRate;
    }
}

