using System.Collections;
using TigerForge;
using UnityEngine;
using QFSW.MOP2;

public class PetMuscleDuckAction : ActivePetActionBase
{
    [Header("雷のエフェクト（プレハブプール）")]
    [SerializeField] private ObjectPool thunderPrefabPool;

    [Header("雷の連撃設定")]
    [SerializeField] private int thunderCount = 5;       // 雷が出る回数
    [SerializeField] private float thunderInterval = 0.15f; // 次の雷が出るまでの時間
    [SerializeField] private float distanceStep = 1.5f;  // 雷ごとの距離間隔

    [Header("雷剣のプレハブ")]
    [SerializeField] private GameObject thunderSwordPrefab;

    [Header("雷剣のトレイル")]
    [SerializeField] private GameObject trailEffect;

    [Header("残像プレハブ")]
    [SerializeField] private GameObject afterImagePrefab;

    [Header("カウンターのクールダウン（秒）")]
    [SerializeField] private float counterCooldownMax = 30f;

    private SoundList counterSound = SoundList.ParrySe;

    public float currentCounterCd = 0f;

    //保存用のトランスフォーム
    //public Transform keepTruns;

    //保存用の敵の方向
    private Vector3 keepDirection = Vector3.zero;

    public bool TryPriorityCounter(Transform attackerTransform)
    {
        if (ActivePetManager.Instance.allDucks.Count == 0) return false;

        if (ActivePetManager.Instance.petSkillWaitTime > 0f) return false;

        PetMuscleDuckAction executor = null;

        foreach (var duck in ActivePetManager.Instance.allDucks)
        {
            if (duck.HasSword())
            {
                executor = duck;
                break; 
            }
        }

        if (executor != null)
        {
            executor.ExecuteCounter(attackerTransform);

            foreach (var duck in ActivePetManager.Instance.allDucks)
            {
                if (duck != executor)
                {
                    duck.StopActionForAllyCounter();
                }
            }
            return true;
        }

        return false;
    }

    public void StopActionForAllyCounter()
    {
        ActivePetManager.Instance.petSkillWaitTime = 1.0f;

        sm.ChangeState(PetActionStates.Idle);        
    }


    protected override void OnEnable()
    {
        base.OnEnable();

        if (!ActivePetManager.Instance.allDucks.Contains(this))
        {
            ActivePetManager.Instance.allDucks.Add(this);
        }

        ActivePetManager.Instance.allDucks.Sort((a, b) => {
            if (a.whoIam == PetCloneType.Original) return -1; // aがオリジナルなら前へ
            if (b.whoIam == PetCloneType.Original) return 1;  // bがオリジナルなら前へ
            return 0;
        });

        //EventManager.StartListening(GameEvent.CounterAllPetAttack,CloneCountor);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
      
        if (ActivePetManager.Instance.allDucks.Contains(this))
        {
            ActivePetManager.Instance.allDucks.Remove(this);
        }

        //EventManager.StopListening(GameEvent.CounterAllPetAttack,CloneCountor);
    }

    public bool HasSword()
    {
        return currentCounterCd <= 0f;
    }

    protected override void Start()
    {
        base.Start();

        //マッスルアヒルがいることをActivePetManagerに通知
        ActivePetManager.Instance.isMuscleDuckInGame = true;

        // 最初は剣を持っている
        EquipSword();
    }

    public void ExecuteCounter(Transform attackerTransform)
    {
        if (!this.HasSword()) return;

        //カウンターSE再生
        //音が再生中でなければ再生
        if (!SoundEffect.Instance.IsPlaying(counterSound))
        {
            SoundEffect.Instance.Play(counterSound);
        }

        //自分の位置に残像を生成
        if (afterImagePrefab != null)
        {
            GameObject afterImage = 
            Instantiate(afterImagePrefab, transform.position, transform.rotation);
        }

        currentCounterCd = counterCooldownMax;

        Vector3 playerPos = playerTransform.position;
        Vector3 enemyPos = attackerTransform.position;

        Vector3 guardPos = playerPos + (enemyPos - playerPos).normalized * 0.3f;

        guardPos.y = 0f;

        Vector3 direction = (attackerTransform.position - transform.position).normalized;
        direction.y = 0;

        //保存!
        keepDirection = direction;
        //keepTruns     = attackerTransform;

        transform.position = guardPos;
        transform.rotation = Quaternion.LookRotation(direction);

        //if(whoIam == PetCloneType.Original)
        //{
        //    EventManager.SetData(GameEvent.CounterAllPetAttack, keepTruns);
        //    EventManager.EmitEvent(GameEvent.CounterAllPetAttack);

        //    Debug.Log("オリジナルカウンター発動");
        //}

        //アニメーション変更
        sm.ChangeState(PetActionStates.ActiveSkillMotion);
    }

    #region ステート上書き---------------------------------------

    protected override void ActiveSkillMotion_Update()
    {
        //アニメーション変更
        AnimationEndChange("ActiveSkillMotion");
    }

    protected override void ActiveSkillMotion_Exit()
    {
        DisableAttackCollider();

        if (thunderSwordPrefab != null) thunderSwordPrefab.SetActive(false);
    }

    #endregion --------------------------------------------------

    public override void PerformAttack() => PetAttackAction();
    protected override void PetAttackAction() => base.PetAttackAction();

    protected override void Update()
    {
        base.Update();

        if (ActivePetManager.Instance.petSkillWaitTime > 0f)
        {
            ActivePetManager.Instance.petSkillWaitTime -= Time.deltaTime;

            return;
        }

        // クールダウン回復処理
        if (currentCounterCd > 0f)
        {
            currentCounterCd -= Time.deltaTime;

            // クールダウンが終わった瞬間
            if (currentCounterCd <= 0f)
            {
                currentCounterCd = 0f;
                EquipSword();
            }
        }
    }

    private void EquipSword()
    {
        if (thunderSwordPrefab != null)
        {
            thunderSwordPrefab.SetActive(true);
        }
    }

    public void ActiveTrail()
    {
        //雷剣がアクティブならトレイルエフェクトを有効化
        if (thunderSwordPrefab.activeSelf)
        {
            trailEffect.SetActive(true);
        }
    }

    public void EndTrail()
    {
        if (thunderSwordPrefab.activeSelf)
        {
            //トレイルエフェクトを無効化
            trailEffect.SetActive(false);
        }
    }

    private void CounterThunder()
    {
        StartCoroutine(SpawnSequentialThunder(keepDirection));
    }

    private IEnumerator SpawnSequentialThunder(Vector3 direction)
    {
        Vector3 startPos = transform.position + (direction * 1.0f);

        //スキル音出す
        //if (whoIam == PetCloneType.Original)
        //{
            SoundEffect.Instance.Play(skillEffectSound);
        //}

        for (int i = 0; i < thunderCount; i++)
        {
            //画面揺らす
            //if (whoIam == PetCloneType.Original)
            //{
                CameraShake.Instance.StartShake(0.1f, 1.1f, 3f, 3f);
            //}

            Vector3 spawnPos = startPos + (direction * (distanceStep * i));

            spawnPos.y = transform.position.y;

            GameObject thunder = thunderPrefabPool.GetObject();

            thunder.transform.position = spawnPos;
            thunder.transform.rotation = Quaternion.LookRotation(direction);

            PetSkillData skillData = thunder.GetComponent<PetSkillData>();
            if (skillData != null)
            {
                skillData.Initialize(takeDamages * 3, this.gameObject);
                skillData.SetPool(thunderPrefabPool);
                skillData.speed = 0f;
            }

            yield return new WaitForSeconds(thunderInterval);
        }
    }

    //private void CloneCountor()
    //{
    //    //自分がオリジナルか確認
    //    if (whoIam == PetCloneType.Original) return;

    //    var enemyData = EventManager.GetData(GameEvent.CounterAllPetAttack);

    //    //敵のトランスフォームを取得
    //    Transform targetTrans = (Transform)enemyData;

    //    //保存!
    //    ExecuteCounter(targetTrans);
    //}
}
