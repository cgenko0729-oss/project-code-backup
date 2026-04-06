using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class PetMidBossWolfAction : ActivePetActionBase
{
    [Header("攻撃トレイルのプレハブ(複数)")]
    [SerializeField]private GameObject[] attackTrailPrefabs;

    [Header("覚醒したときのトレイルプレハブ(複数)")]
    [SerializeField]private GameObject[] awakenTrailPrefabs;

    [Header("攻撃判定用コライダー(覚醒)")]
    [SerializeField]private Collider petAttackCol_Awaken;

    private enum wolfType
    {
        Normal,   //普通
        Awakened  //覚醒
    }

    //現在のオオカミのタイプ
    private wolfType currentWolfType = wolfType.Normal;

    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("攻撃力の上昇倍率")]
    [SerializeField] private float attackPowerUpRate = 50f;

    #endregion --------------------------------------------

    public override void PerformAttack() => PetAttackAction();
    protected override void PetAttackAction()
    {
        base.PetAttackAction();

        if(currentWolfType == wolfType.Awakened)
        {
            if(petAttackCol_Awaken != null)
            {
                //覚醒時は攻撃判定用コライダーを有効にする
                petAttackCol_Awaken.isTrigger = true;
            }

            //元の攻撃判定用コライダーを無効にする
            petAttackCol.isTrigger = false;
        }
    }

    protected override void DisableAttackCollider()
    {
        if (currentWolfType == wolfType.Normal)
        {
            if (petAttackCol != null)
            {
                petAttackCol.isTrigger = false;
            }
        }
        if (currentWolfType == wolfType.Awakened)
        {
            if (petAttackCol_Awaken != null)
            {
                petAttackCol_Awaken.isTrigger = false;
            }
        }
    }

    protected override void Start()
    {
        base.Start();

        if (ActivePetManager.Instance.activePets != null)
        {
            CountActivePets();
        }
    }

    #region ステート上書き---------------------------------------


    protected override void HomingEnemy_Update()
    {
        // 追跡対象の敵がいない（nullになった）場合、プレイヤーを追う状態に戻る
        if (nearestEnemy == null || enemyTransform == null)
        {
            // プレイヤーとの距離に応じてWalkかRun状態に遷移する
            if (playerToDist > runDist)
            {
                sm.ChangeState(PetActionStates.Run);
            }
            else
            {
                sm.ChangeState(PetActionStates.Walk);
            }
            return; // 敵がいないので、以降の処理は行わない
        }

        //敵のTransformがnullでないことを確認
        if (enemyTransform)
        {
            Homing(enemyTransform, moveSpeed_Run);
        }

        float dist = Vector3.Distance(transform.position, nearestEnemy.transform.position);

        Vector3 myPosOnGround = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 enemyPosOnGround = new Vector3(enemyTransform.position.x, 0, enemyTransform.position.z);
        float horizontalDistance = Vector3.Distance(myPosOnGround, enemyPosOnGround);

        float verticalDistance = Mathf.Abs(transform.position.y - enemyTransform.position.y);

        if (horizontalDistance <= attackMoveRange && verticalDistance <= maxAttackHeightDifference)
        {
            //狼の状態で攻撃状態を変更
            if (currentWolfType == wolfType.Normal)
            {
                sm.ChangeState(PetActionStates.Attack);
                return;
            }
            if (currentWolfType == wolfType.Awakened)
            {
                sm.ChangeState(PetActionStates.ActiveSkillMotion);
                return;
            }
        }

        // 追跡中に敵が索敵範囲（attackRange）の外に出てしまった場合も、プレイヤーを追う状態に戻る
        if (dist > attackRange)
        {
            // プレイヤーとの距離に応じてWalkかRun状態に遷移する
            if (playerToDist > runDist)
            {
                sm.ChangeState(PetActionStates.Run);
            }
            else
            {
                sm.ChangeState(PetActionStates.Walk);
            }
            return;
        }
    }

    protected override void Attack_Enter()
    {
        base.Attack_Enter();

        //トレイルプレハブを無効にする
        if (currentWolfType == wolfType.Normal)
        {
            attackTrailPrefabs[0].SetActive(false);
        }
    }

    protected override void Attack_Exit()
    {
        base.Attack_Exit();

        //トレイルプレハブを無効にする
        if (currentWolfType == wolfType.Normal)
        {
            attackTrailPrefabs[0].SetActive(false);
        }
    }


    protected override void ActiveSkillMotion_Enter()
    {
        base.ActiveSkillMotion_Enter();

        if (currentWolfType == wolfType.Awakened)
        {
            foreach (var trail in attackTrailPrefabs)
            {
                trail.SetActive(false);
            }
        }
    }
    protected override void ActiveSkillMotion_Update()
    {
        AnimatorStateInfo stateInfo = petAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("ActiveSkillMotion") && stateInfo.normalizedTime >= 0.95f)
        {
            sm.ChangeState(PetActionStates.HomingEnemy);
        }
    }

    protected override void ActiveSkillMotion_Exit()
    {
        DisableAttackCollider();

        if (currentWolfType == wolfType.Awakened)
        {
            foreach (var trail in attackTrailPrefabs)
            {
                trail.SetActive(false);
            }
        }
    }

    #endregion --------------------------------------------------

    public void ActiveTrail()
    {
       //攻撃トレイルのプレハブ有効(最初の一つだけ)
       if(attackTrailPrefabs.Length > 0)
       {
          attackTrailPrefabs[0].SetActive(true);
       }
    }

    public void ActiveTrail_Awaken()
    {
       //攻撃トレイルのプレハブ有効(全て)
       foreach(var trail in attackTrailPrefabs)
       {
          trail.SetActive(true);
       }
    }

    private void CountActivePets()
    {
        //現在のペット数を調べる
        //狼分は引く
        int currentPetCount = ActivePetManager.Instance.activePets.Count - 1;

        //ペットの最大数を調べる
        //最大数も狼分は引く
        int maxPetCount = PetSelectDataManager.Instance.MaxPets - 1;

        //ペットが少なければ少ないほど攻撃力が上がる
        if (currentPetCount < maxPetCount)
        {
            float attackPowerIncreaseRate = 1f + (maxPetCount - currentPetCount) * attackPowerUpRate;
            takeDamages *= attackPowerIncreaseRate;
        }

        //連れているペットがこのペットだけなら覚醒扱いにする
        if (currentPetCount == 0)
        {
            //覚醒時のトレイルプレハブを有効にする
            foreach (var trail in awakenTrailPrefabs)
            {
                trail.SetActive(true);
            }

            //オオカミのタイプを覚醒に変更
            currentWolfType = wolfType.Awakened;

        }
    }
}
