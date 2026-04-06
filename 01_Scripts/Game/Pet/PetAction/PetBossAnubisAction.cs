using QFSW.MOP2;
using System.Collections;
using TigerForge;
using UnityEngine;

public class PetBossAnubisAction : PetNagaAction
{
    [Space]

    [Header("召喚する間隔（秒）")]
    [SerializeField] private float summonInterval = 120f;

    [Header("最大召喚数")]
    [SerializeField] private int maxSummonCountMax = 5;

    [Header("召喚柱のプレハブプール")]
    [SerializeField] private ObjectPool summonPillarPrefabPool;

    [Header("召喚柱の設定")]
    [SerializeField] private float summonPillarInterval = 0.15f;

    [Header("扇形の範囲設定")]
    [SerializeField] private int projectilesPerVolley = 3;  //1回の斉射で発射する弾の数
    [SerializeField] private float spreadAngle = 60f;       //扇形の合計角度

    [Header("召喚柱を置く範囲")]
    [SerializeField] private float summonRange = 5f;

    [Header("召喚柱の落下開始高度")]
    [SerializeField] private float summonHeight = 10f;

    [Header("攻撃力上昇量")]
    [SerializeField] private float attackUp = 0.1f; //ペット1体ごとに攻撃力が上昇する量

    private float summonTimer;
    private int maxSummon;
    private float baseDamage;

    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.StartListening("ChangePetList", ModifyAttackSpeedWithPetCount);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.StopListening("ChangePetList", ModifyAttackSpeedWithPetCount);
    }

    protected override void PetAttackAction()
    {
        //nullチェック
        if (enemyTransform == null) { return; }

        Vector3 baseDirection = (enemyTransform.position - petSkillPoint.transform.position).normalized;
        baseDirection.y = 0;

        lookingAtEnemies = false;

        for (int i = 0; i < projectilesPerVolley; i++)
        {
            float angle = 0f;
            if (projectilesPerVolley > 1)
            {
                float startAngle = -spreadAngle * 0.5f;
                float angleStep = spreadAngle / (projectilesPerVolley - 1);
                angle = startAngle + angleStep * i;
            }

            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 fireDirection = rotation * baseDirection;

            GameObject skill = skillObjPool.GetObject();
            skill.transform.position = petSkillPoint.transform.position;
            skill.transform.rotation = Quaternion.LookRotation(fireDirection);

            PetSkillData proj = skill.GetComponent<PetSkillData>();
            if (proj != null)
            {
                SetupProjectile(proj);
                proj.SetDirection(fireDirection);
            }
        }

        PlayAttackSounds();
    }

    protected override void Start()
    {
        base.Start();
        summonTimer = summonInterval;
        baseDamage = this.takeDamages;
    }

    protected override void Update()
    {
        base.Update();

        summonTimer -= Time.deltaTime;

        if (summonTimer <= 0)
        {
            sm.ChangeState(PetActionStates.ActiveSkillMotion);

            summonTimer = summonInterval;
        }
    }

    #region ステート上書き---------------------------------------

    protected override void ActiveSkillMotion_Update()
    {
        //アニメーション変更
        AnimationEndChange("ActiveSkillMotion");
    }

    //IdleMotion------------------------------------------
    protected override void IdleMotion_Enter()
    {
        petAnimator.SetBool("isIdle", false);
        petAnimator.SetTrigger("IdleMotion");
    }

    protected override void IdleMotion_Update()
    {
        if (playerToDist < runDist)
        {
            if (nearestEnemy != null)
            {
                if (petData.petRoles == null || !petData.petRoles.Contains(PetRole.NoAttack))
                {
                    float dist = Vector3.Distance(transform.position, nearestEnemy.transform.position);
                    if (dist <= attackRange)
                    {
                        // 近くの敵が攻撃範囲内なら敵追従状態に遷移
                        sm.ChangeState(PetActionStates.Attack);
                        return;
                    }
                }
            }
        }
        if (playerToDist > runDist)
        {
            // プレイヤーとの距離がrunDist以上なら走る
            sm.ChangeState(PetActionStates.Run);
            return;
        }
        if (playerToDist > walkDist)
        {
            // プレイヤーとの距離がwalkDist以上なら歩く
            sm.ChangeState(PetActionStates.Walk);
            return;
        }
    }
    //-----------------------------------------------

    #endregion --------------------------------------------

    private void PutSummonPillars()
    {
        StartCoroutine(SpawnSummonPillars());
    }

    private IEnumerator SpawnSummonPillars()
    {     
        //現在のペットリストを取得
        var petList = ActivePetManager.Instance.activePets;

        maxSummon = petList.Count;

        Vector3 centerPoint = transform.position;

        for (int i = 0; i < maxSummon; i++)
        {
            //iが最大召喚数を超えていたら終了
            if (i > maxSummonCountMax) yield break;

            Vector2 randomCircle = Random.insideUnitCircle * summonRange;

            float spawnX = centerPoint.x + randomCircle.x;
            float spawnZ = centerPoint.z + randomCircle.y;

            float spawnY = centerPoint.y + summonHeight;

            Vector3 spawnPosition = new Vector3(spawnX, spawnY, spawnZ);

            CameraShake.Instance.StartShake(0.1f, 1.1f, 3f, 3f);

            GameObject summonPillars = summonPillarPrefabPool.GetObject();
            summonPillars.transform.position = spawnPosition;

            SummonPillarAction pillarAction = summonPillars.GetComponent<SummonPillarAction>();
            if (pillarAction != null)
            {
                pillarAction.Initialize(this.takeDamages * 0.5f);
                pillarAction.SetPool(summonPillarPrefabPool);
            }

            if(whoIam == PetCloneType.Original)
            {
                SoundEffect.Instance.Play(skillEffectSound);
            }

            yield return new WaitForSeconds(summonPillarInterval);
        }
    }
    private void ModifyAttackSpeedWithPetCount()
    {
        //現在のペットリストを取得
        var petList = ActivePetManager.Instance.activePets;

        //攻撃力上昇量に今のペットのスポーン数分を掛ける
        float petCount = petList.Count;
        float changeAtk = attackUp * petCount;

        this.takeDamages = baseDamage * (1.0f + changeAtk);
    }
}
