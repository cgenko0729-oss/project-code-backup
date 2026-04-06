using UnityEngine;

public class PetScorpionAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("スキル持続時間の増加時間(%)")]
    [SerializeField] private float SkillDurationIncreaseRate = 50f;

    [Header("スキル最大増加分身数(体)")]
    [SerializeField] private int PetCloneRate = 2;

    #endregion --------------------------------------------

    protected override void Start()
    {
        base.Start();
        Amplification();
    }

    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();

    //ドーピング
    private void Amplification()
    {
        //持続時間アップ
        ActivePetManager.Instance.PetSkillDuration += SkillDurationIncreaseRate / 100;

        //分身数アップ
        ActivePetManager.Instance.PetCloneCount += PetCloneRate;
    }
}
