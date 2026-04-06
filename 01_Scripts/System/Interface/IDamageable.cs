using System;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage,bool isShowDamageNumber = true, SkillIdType _skillType = SkillIdType.None, bool isCritical = false, LastAttackType LastAttack = LastAttackType.Other);

   
}
