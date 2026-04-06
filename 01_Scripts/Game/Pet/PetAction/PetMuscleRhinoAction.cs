using UnityEngine;

public class PetMuscleRhinoAction : PetNagaAction
{
    protected override void PetAttackAction()
    {
        if (enemyTransform == null) { return; }
        Vector3 direction = (enemyTransform.position - petSkillPoint.transform.position).normalized;
        direction.y = 0;

        lookingAtEnemies = false;
    
        GameObject skill = skillObjPool.GetObject();

        //スキルの位置を敵の位置に設定
        Vector3 targetPos = enemyTransform.position;
        targetPos.y = 0; // Y軸の位置を固定
        skill.transform.position = targetPos;

        if (direction != Vector3.zero)
        {
            skill.transform.rotation = Quaternion.LookRotation(direction);
        }
        else
        {
            skill.transform.rotation = Quaternion.identity;
        }

        PetSkillHitEffectData proj = skill.GetComponent<PetSkillHitEffectData>();

        if (proj != null)
        {
            proj.SetPool(skillObjPool);
            // 元々の初期化処理
            proj.Initialize(takeDamages, this.gameObject);
        }

        PlayAttackSound();  
    }
}
