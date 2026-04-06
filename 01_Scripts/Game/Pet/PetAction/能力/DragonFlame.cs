using System.Collections;
using UnityEngine;

public class DragonFlame : MonoBehaviour
{
    [Header("Xスケールの拡大量")]
    [SerializeField] private float xScaleIncrease = 5.0f;

    public float spawnFireCnt = 3f;
    public bool isSpawnedFire = false;

    private void OnEnable()
    {
        isSpawnedFire = false;
    }

    private void Start()
    {
        //rand spawnFireCnt between 2 and 5
        spawnFireCnt = Random.Range(0.1f, 0.21f);
        isSpawnedFire = false;
    }

    private void Update()
    {
        // Xスケールを増やす
        Vector3 newScale = transform.localScale;
        newScale.x += xScaleIncrease * Time.deltaTime;
        transform.localScale = newScale;

        spawnFireCnt -= Time.deltaTime;
        if (spawnFireCnt <= 0f && !isSpawnedFire)
        {
            isSpawnedFire = true;

            //rand a bit of position x and z between -0.5 and 0.5
            Vector3 spawnPos = transform.position;
            spawnPos.x += Random.Range(-0.5f, 0.5f);
            spawnPos.z += Random.Range(-0.5f, 0.5f);

            SkillEffectManager.Instance.SpawnSkillFireObj(spawnPos,SkillIdType.arrow);
        }
    }
    private void OnTriggerEnter(Collider col)
    {
        if (!col.CompareTag("Enemy")) return;

        //敵のステータスコンポーネントを取得する
        EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();

        //コンポーネントが取得できなければ、処理を終了
        if (enemyStat == null) return;

        //敵を燃やす
        enemyStat.ApplyFireDebuff();
    }
}
