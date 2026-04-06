using UnityEngine;

public class PetCocconAction : MonoBehaviour
{
    [Header("鈍足デバフの量")]
    public float debuffSpeedDownAmount = 0.50f;

    [Header("鈍足デバフの時間")]
    public float debuffSpeedDownDuration = 5f;

    private void OnTriggerEnter(Collider col)
    {
        if (!col.CompareTag("Enemy")) return;

        //敵のステータスコンポーネントを取得する
        EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();

        //コンポーネントが取得できなければ、処理を終了
        if (enemyStat == null) return;

        enemyStat.ApplySpeedDownDebuff(debuffSpeedDownAmount,debuffSpeedDownDuration);
    }
}
