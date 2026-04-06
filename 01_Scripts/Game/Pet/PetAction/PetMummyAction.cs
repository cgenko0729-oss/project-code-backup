using UnityEngine;

public class PetMummyAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("召喚するマミー(ダミー)のプレハブ")]
    [SerializeField] private PetData shadowMummyData;

    [Tooltip("召喚する間隔（秒）")]
    [SerializeField] private float summonInterval = 120f;

    [Tooltip("最大召喚数")]
    [SerializeField] private int maxSummonCountMax = 5;

    private float summonTimer;
    private int maxSummon;

    #endregion --------------------------------------------

    protected override void Start()
    {
        base.Start();
      
        summonTimer = summonInterval;
    }

    protected override void Update()
    {
        base.Update();

        summonTimer -= Time.deltaTime;

        if (summonTimer <= 0)
        {
            SummonShadowMummy();

            summonTimer = summonInterval;
        }
    }

    private void SummonShadowMummy()
    {
        if (shadowMummyData == null || shadowMummyData.petPrefab == null)return;

        //最大召喚数に達している場合は終了
        if (maxSummon >= maxSummonCountMax) return;

        //分身の上昇量を取得
        int extraCount = ActivePetManager.Instance.PetCloneCount;

        //現在の召喚数
        int summonBatchCount = 1 + extraCount;

        //最終的な召喚数
        int FinalCloneCount = maxSummonCountMax + extraCount;

        //効果音がもう再生されていたら、スルー
        if (!SoundEffect.Instance.IsPlaying(skillEffectSound))
        {
            SoundEffect.Instance.Play(skillEffectSound);
        }

        //召喚数をカウントアップ
        maxSummon += summonBatchCount;

        for (int i = 0; i < summonBatchCount; i++)
        {
            
            //プレイヤーの位置を取得
            var activePets = ActivePetManager.Instance.activePets;

            //カウントに応じて円形に配置
            int newTeamSize = activePets.Count + 1;

            //プレイヤーのTransformを取得
            int myIndexInTeam = activePets.Count;

            float angle = myIndexInTeam * (360f / newTeamSize);
            Vector3 offset = Quaternion.Euler(0, angle, 0) * (Vector3.forward * 2f);

            Vector3 randomSpawn = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));

            Vector3 spawnPosition = playerTransform.position + (offset + randomSpawn);

            GameObject shadowInstance = Instantiate(shadowMummyData.petPrefab, spawnPosition, transform.localRotation);

            ActivePetActionBase shadowStatus = shadowInstance.GetComponent<ActivePetActionBase>();
            if (shadowStatus != null)
            {
                //配置位置のオフセットを設定してアクティブスキルモーションに移行
                shadowStatus.SetFormationOffset(offset + randomSpawn);
                shadowStatus.ForceChangeState(PetActionStates.ActiveSkillMotion);
            }

            //アクティブペットマネージャーに登録(意味がある)
            ActivePetManager.Instance.RegisterActivePetList(shadowInstance);
        }
    }

    public override void PerformAttack() => PetAttackAction();
    protected override void PetAttackAction() => base.PetAttackAction();
}
