using HighlightPlus;
using UnityEngine;

public class PetMummyDummyAction : ActivePetActionBase
{
    protected override void Start()
    {
        if (petData != null)
        {
            this.takeDamages = petData.attackPower;
        }
        else
        {
            Debug.LogWarning("PetDataが設定されていません！", this.gameObject);
        }
    }

    #region --ステート上書き----------------------------------

    protected override void ActiveSkillMotion_Update()
    {
        //アニメーション変更
        AnimationEndChange("ActiveSkillMotion");
    }

    #endregion --ステート上書き終了--

    public override void PerformAttack() => PetAttackAction();
    protected override void PetAttackAction() => base.PetAttackAction();
}
