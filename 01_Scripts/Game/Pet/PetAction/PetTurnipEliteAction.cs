using UnityEngine;

public class PetTurnipEliteAction : ActivePetActionBase
{
    [Header("生成する重圧フィールド")]
    public GameObject heavyFieldPrefab;

    protected override void Start()
    {
        base.Start();

        Vector3 EffectPos = new (0,1,0);

        //重力フィールドを子オブジェクトとして生成
        if (heavyFieldPrefab != null)
        {
            GameObject heavyField = Instantiate(heavyFieldPrefab, transform.position+EffectPos, transform.localRotation);
            heavyField.transform.SetParent(transform);
        }
    }

    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();
}
