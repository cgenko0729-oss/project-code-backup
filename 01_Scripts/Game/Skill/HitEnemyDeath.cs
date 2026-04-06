using UnityEngine;

public class HitEnemyDeath : MonoBehaviour
{
    [Header("敵が即死する確率")]
    [Range(0, 100)] public int deathProbability = 0;

    private float DamageAmount = 200f;

    private void OnTriggerEnter(Collider col)
    {
        if (!col.CompareTag("Enemy")) return;

        //敵のステータスコンポーネントを取得する
        EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();

        //コンポーネントが取得できなければ、処理を終了
        if (enemyStat == null) return;

        int enemyLayer = enemyStat.gameObject.layer;
     
        int rand = Random.Range(0, 100);
        if (rand <= deathProbability)
        {
            //敵のレイヤーを調べる
            if(enemyLayer == LayerMask.NameToLayer("EnemyBossSpider") || enemyLayer == LayerMask.NameToLayer("EnemyDragon"))
            {
                enemyStat.TakeDamage(DamageAmount, isShowDamageNumber: true, isCritical:true);
            }
            else
            {
                enemyStat.Dead(LastAttackType.Other);
            }
        }
    }
}
