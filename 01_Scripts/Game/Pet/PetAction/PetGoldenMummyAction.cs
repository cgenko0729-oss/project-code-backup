using UnityEngine;

public class PetGoldenMummyAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("金色デバフの持続時間")]
    [SerializeField] private float goldDebuffDuration = 5f;

    #endregion --------------------------------------------

    protected override void Start()
    {
        base.Start();

        //金ミイラがいることをActivePetManagerに伝える
        ActivePetManager.Instance.isGoldMummyInGame = true;
    }

    public override void PerformAttack() => PetAttackAction();
    protected override void PetAttackAction() => base.PetAttackAction();

    protected override void OnHitEnemy(EnemyStatusBase enemyStat)
    {
        //敵を金色デバフにする
        enemyStat.ApplyGoldDebuff(goldDebuffDuration);
    }
}
