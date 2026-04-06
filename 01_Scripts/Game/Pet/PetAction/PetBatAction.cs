using UnityEngine;
using TigerForge;               //EventManager

public class PetBatAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("攻撃吸収量")]
    public float lifeSteelAmount = 1;

    [Header("基礎回復確率")]
    public int baseHealChance = 30;

    #endregion --------------------------------------------

    //攻撃メゾット
    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();

    //敵に当たったとき
    protected override void OnHitEnemy(EnemyStatusBase enemyStat)
    {
        //確率で回復
        int randomValue = Random.Range(0, 100); // 1から100までのランダムな整数を生成
        if (randomValue <= baseHealChance)
        {
            EventManager.EmitEventData("ChangePlayerHp", lifeSteelAmount);
        }
    }
}

