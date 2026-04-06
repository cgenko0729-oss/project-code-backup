using System.Collections.Generic;
using UnityEngine;

public class PetCactasHitBody : MonoBehaviour
{
    [Header("与えるダメージ")]
    [SerializeField] private float takeDamages = 10f;

    [Header("当たった時の音声")]
    [SerializeField] private SoundList hitSound;

    [Header("金を落とさせるか")]
    [SerializeField] private bool makeEnemyDropMoney = true;

    [Header("吹き飛ばす量")]
    [SerializeField] private float pushBackPower = 2.1f;

    private void OnTriggerEnter(Collider col)
    {
        if (!col.CompareTag("Enemy")) return;

        //敵のステータスコンポーネントを取得する
        EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();

        //コンポーネントが取得できなければ、処理を終了
        if (enemyStat == null) return;

        if (takeDamages > 0)
        {
            //敵にダメージを与える
            enemyStat.TakeDamage(takeDamages);
        }

        //敵をプレイヤーから遠ざける
        enemyStat.DynamicPushBackEnemyItselfAwayFromPlayer(pushBackPower);

        //ヒットエフェクトを再生する
        if (!SoundEffect.Instance.IsPlaying(hitSound))
        {
            SoundEffect.Instance.Play(hitSound);
        }

        //金をドロップさせる
        if (makeEnemyDropMoney)
        { 
            int rand = Random.Range(0, 100);
            if (rand <= ActivePetManager.Instance.PetDropMoneyChance)
            {
                //金をドロップさせる
                SoundEffect.Instance.Play(SoundList.DropCoin);
                CoinSpawner.Instance.SpawnCoin(enemyStat.transform.position, 1, 1.4f); // コインをドロップ
            }
        }
    }
}
