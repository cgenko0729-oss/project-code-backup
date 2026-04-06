using UnityEngine;

public class PetGravityAura : MonoBehaviour
{
    [Header("鈍足デバフの量")]
    public float debuffSpeedDownAmount = 0.75f;

    private void OnTriggerEnter(Collider col)
    {
        if (!col.CompareTag("Enemy")) return;

        //敵のステータスコンポーネントを取得する
        EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();

        //コンポーネントが取得できなければ、処理を終了
        if (enemyStat == null) return;

        enemyStat.SpdDownRate = debuffSpeedDownAmount;
    }

    private void OnTriggerExit(Collider col)
    {
        if (!col.CompareTag("Enemy")) return;

        //敵のステータスコンポーネントを取得する
        EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();

        //コンポーネントが取得できなければ、処理を終了
        if (enemyStat == null) return;

        enemyStat.SpdDownRate = 1f;
    }
}
