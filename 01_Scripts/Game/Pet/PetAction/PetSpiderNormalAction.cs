using UnityEngine;

public class PetSpiderNormalAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------

    [Header("特定の敵の特効レイヤー")]
    [Tooltip("このレイヤーを持つ敵に対してのダメージが増加します\n(いらないのなら【なし】でOK!)")]
    public LayerMask targetEnemyLayerMask;

    [Header("特定の敵に対するダメージ倍率")]
    [Tooltip("特定の敵に対するダメージ倍率を設定します\n(1.0で通常ダメージ、2.0で2倍ダメージ)")]
    public float damageMultiplier = 1.5f;

    #endregion --------------------------------------------

    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();

    protected override void OnTakeDamegesEnemy(EnemyStatusBase enemyStat)
    {
        //最終ダメージ
        float finalDamage = takeDamages;

        //接触した敵のレイヤー番号を取得する
        int enemyLayer = enemyStat.gameObject.layer;

        if (((1 << enemyLayer) & targetEnemyLayerMask) != 0)
        {
            //特定のレイヤーが入るとダメージ倍率が変わる
            finalDamage *= damageMultiplier;
        }

        //敵にダメージを与える
        FinalDanages(enemyStat, finalDamage);

        DisableAttackCollider();
    }

}

