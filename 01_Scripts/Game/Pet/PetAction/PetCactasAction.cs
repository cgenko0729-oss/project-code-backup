using UnityEngine;

public class PetCactasAction :  ActivePetActionBase
{
    public override void PerformAttack() => PetAttackAction();
    protected override void PetAttackAction() => base.PetAttackAction();
}