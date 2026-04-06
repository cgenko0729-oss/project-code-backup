using DG.Tweening;
using UnityEngine;
using static ActivePetActionBase;

public class CocconSpawner : MonoBehaviour
{
    [Header("召喚するクモ")]
    [SerializeField] private GameObject spiderPrefab;

    [Header("最大召喚数")]
    [SerializeField] private int maxSummonCountMax = 10;

    [Header("上昇設定")]
    [SerializeField] private float riseDuration = 3f;

    [Header("召喚する間隔（秒）")]
    [SerializeField] private float summonInterval = 120f;

    [Header("有効化のハイライトエフェクト")]
    [SerializeField] private Behaviour highlightEffect;

    [Header("召喚したクモの攻撃力倍率")]
    [SerializeField] private float attackMultiplier = 2.5f;

    [Header("召喚したクモに適用するマテリアル")]
    [SerializeField] private Material spawnSpiderMaterial;

    private float summonTimer;
    private int maxSummon;

    void Start()
    {
        summonTimer = summonInterval;

        transform.DOMoveY(0, riseDuration).SetEase(Ease.InQuad).OnComplete(() =>
        {
            if (highlightEffect != null)
            {
                highlightEffect.enabled = true;
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        summonTimer -= Time.deltaTime;

        if (summonTimer <= 0)
        {
            SpawnSpider();

            summonTimer = summonInterval;
        }
    }

    private void SpawnSpider()
    {
        if (maxSummon >= maxSummonCountMax) { return; }
        GameObject spider = Instantiate(spiderPrefab, transform.position, Quaternion.identity);

        if (spawnSpiderMaterial != null)
        {           
            Renderer[] renderers = spider.GetComponentsInChildren<Renderer>();

            foreach (var r in renderers)
            {
                //マテリアルを差し替える
                r.material = spawnSpiderMaterial;
            }
        }

        spider.transform.localScale = Vector3.zero;

        //繭の周囲に少し散らばるように着地地点を決める
        Vector3 landPos = transform.position + new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));

        //DOTweenで動きをつける
        Sequence spawnSeq = DOTween.Sequence();
        spawnSeq.Append(spider.transform.DOJump(landPos, 2.0f, 1, 0.5f));
        spawnSeq.Join(spider.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));

        if(!SoundEffect.Instance.IsPlaying(SoundList.SpiderBossWebShotAtkSe))
        {
           SoundEffect.Instance.Play(SoundList.SpiderBossWebShotAtkSe);
        }

        ActivePetActionBase spiderSpawnerStatus = spider.GetComponent<ActivePetActionBase>();
        if (spiderSpawnerStatus != null)
        {
            Vector2 randomPoint = Random.insideUnitCircle.normalized * Random.Range(2f, 4f);
            Vector3 finalOffset = new Vector3(randomPoint.x, 0, randomPoint.y);

            //編成位置を設定
            spiderSpawnerStatus.SetFormationOffset(finalOffset);

            //君はクローンですと告げる
            spiderSpawnerStatus.Doppelganger();

            //召喚したクモの攻撃力を上げる
            spiderSpawnerStatus.SetAttackMaltiplier(attackMultiplier);
        }
        ActivePetManager.Instance.RegisterActivePetList(spider);
        maxSummon++;
    }
}
