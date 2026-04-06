using UnityEngine;

public class PetGoldenCactusAction : PetCactasAction
{
    protected override void OnHitEnemy(EnemyStatusBase enemyStat)
    {
        enemyStat.PushBackEnemyItselfAwayFromPlayer();

        int rand = Random.Range(0, 100);
        if (rand <= ActivePetManager.Instance.PetDropMoneyChance)
        {
            //金をドロップさせる
            SoundEffect.Instance.Play(SoundList.DropCoin);
            CoinSpawner.Instance.SpawnCoin(enemyStat.transform.position, 1, 1.4f); // コインをドロップ
        }     
    }
}
