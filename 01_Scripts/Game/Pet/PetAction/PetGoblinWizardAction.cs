using QFSW.MOP2;
using System.Collections.Generic;
using UnityEngine;


public class MapSpecificAttack
{
    public MapType mapType; // マップの種類（enum型だと管理しやすい）
    public GameObject bulletPrefab; // そのマップで使う弾のプレハブ
    public ObjectPool objectPool;   // その弾に対応するオブジェクトプール
}

public class PetGoblinWizardAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------
  
    [System.Serializable]
    // マップ固有の攻撃情報を格納するクラス
    public class MapSpecificAttack
    {
        [Header("マップの種類")]
        public MapType mapType; 

        [Header("弾のプレハブ")]
        public GameObject bulletPrefab; 

        [Header("オブジェクトプール")]
        public ObjectPool objectPool; 

        [Header("攻撃音")]
        public SoundList  attackSE;

        [Header("攻撃倍率")]
        public float attackMultiplier = 1.0f;

        [Header("攻撃速度倍率")]
        public float attackSpeedMultiplier = 1.0f;

        [Header("弾速")]
        public float bulletSpeed = 5.0f;
    }

    [Space]

    [Header("固有の能力")]

    [Header("スキルの撃つ位置")]
    public GameObject petSkillPoint;

    [Header("マップごとの攻撃設定")]
    public List<MapSpecificAttack> mapAttacks;

    //スキルの角度
    private Vector3 skillRot = new Vector3(0, 0, 0);

    #endregion --------------------------------------------

    #region 各ステート---------------------------------------

    protected override void Walk_Update() //ステート中ずっと実行
    {
        // プレイヤーとの距離を測定
        if (playerToDist > runDist)
        {
            // プレイヤーとの距離がrunDist以上なら走る
            sm.ChangeState(PetActionStates.Run);
            return;
        }
        if (playerToDist < runDist)
        {
            if (nearestEnemy != null)
            {
                float dist = Vector3.Distance(transform.position, nearestEnemy.transform.position);
                if (dist <= attackRange)
                {
                    // 近くの敵が攻撃範囲内なら攻撃状態に遷移
                    sm.ChangeState(PetActionStates.Attack);
                    return;
                }
            }
        }
        if (playerToDist > walkDist)
        {
            if (playerTransform)
            {
                Homing(playerTransform, moveSpeed_Walk);
                return;
            }
        }
        if (playerToDist <= walkDist && playerToDist > idleDist)
        {
            sm.ChangeState(PetActionStates.Idle);
            return;
        }
    }

    protected override void Idle_Update()
    {
        if (playerToDist < runDist)
        {
            if (nearestEnemy != null)
            {
                float dist = Vector3.Distance(transform.position, nearestEnemy.transform.position);
                if (dist <= attackRange)
                {
                    // 近くの敵が攻撃範囲内なら攻撃状態に遷移
                    sm.ChangeState(PetActionStates.Attack);
                    return;
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

        if (stateCnt <= stateZeroCnt)
        {
            // 待機状態のカウンターが0以下になったら待機モーションに遷移
            sm.ChangeState(PetActionStates.IdleMotion);
            return;
        }
    }

    protected override void Attack_Enter()
    {
        petAnimator.SetBool("isWalking", false);
        petAnimator.SetBool("isIdle", false);
        petAnimator.SetBool("isRunning", false);
        petAnimator.SetTrigger("isAttack");


        //現在のマップデータを取得
        MapSpecificAttack currentAttack = GetCurrentMapAttack();


        //該当する攻撃が見つからなかった場合は処理を終了
        if (currentAttack == null || currentAttack.objectPool == null)
        {
            return;
        }

        //速度を付ける
        SetAttackSpeed(currentAttack.attackSpeedMultiplier);

        petAnimator.speed = this.currentAttackSpeedMultiplier;

        lookingAtEnemies = true; // 敵を見ている状態にする
        stateCnt = attackCooldown;
    }

    protected override void Attack_Update()
    {
        // プレイヤーとの距離を測定
        if (enemyTransform)
        {
            if (lookingAtEnemies)
            {
                LookAt(enemyTransform);
            }
        }

        if (stateCnt <= stateZeroCnt)
        {
            if (playerToDist > walkDist)
            {
                // プレイヤーとの距離がwalkDist以上なら歩く
                sm.ChangeState(PetActionStates.Walk);
                return;
            }
            if (playerToDist <= idleDist)
            {
                // プレイヤーとの距離がidleDist以下なら待機状態に遷移
                sm.ChangeState(PetActionStates.Idle);
                return;
            }
            if (playerToDist > runDist)
            {
                // プレイヤーとの距離がrunDist以上なら走る
                sm.ChangeState(PetActionStates.Run);
                return;
            }

            lookingAtEnemies = true; // 敵を見ている状態にする
            stateCnt = attackCooldown; // 再度クールダウンタイマーをリセット
        }
    }

    protected override void IdleMotion_Update()
    {
        if (playerToDist < runDist)
        {
            if (nearestEnemy != null)
            {
                float dist = Vector3.Distance(transform.position, nearestEnemy.transform.position);
                if (dist <= attackRange)
                {
                     // 近くの敵が攻撃範囲内なら攻撃状態に遷移
                    sm.ChangeState(PetActionStates.Attack);
                    return;
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

    #endregion --------------------------------------------

    public override void PerformAttack()
    {
        PetAttackAction();
    }

    protected override void PetAttackAction()
    {
        //nullチェック
        if (enemyTransform == null) { return; }

        if (lookingAtEnemies)
        {
            Vector3 direction = (enemyTransform.position - petSkillPoint.transform.position).normalized;

            direction.y = 0;
            direction = direction.normalized;

            skillRot = direction; 
        }

        lookingAtEnemies = false;

        //現在のマップデータを取得
        MapSpecificAttack currentAttack = GetCurrentMapAttack();

        //該当する攻撃が見つからなかった場合は処理を終了
        if (currentAttack == null || currentAttack.objectPool == null)
        {
            return; 
        }

        GameObject skill = currentAttack.objectPool.GetObject(); //オブジェクトプールからスキルのオブジェクトを取得

        skill.transform.position = petSkillPoint.transform.position; //スキルの位置を設定

        //回転を一度取得して修正後に再設定
        Vector3 fixedEulerAngles = transform.rotation.eulerAngles;
        fixedEulerAngles.x = 0; // X軸の回転を固定
        fixedEulerAngles.z = 0; // Z軸の回転を固定
        transform.rotation = Quaternion.Euler(fixedEulerAngles);

        skill.transform.rotation = Quaternion.Euler(fixedEulerAngles); //スキルの回転を初期化

        PetSkillData proj = skill.GetComponent<PetSkillData>();
        if (proj != null)
        {
            proj.SetPool(currentAttack.objectPool); //オブジェクトプールを設定

            float FinalDamage = takeDamages * currentAttack.attackMultiplier; //攻撃力に倍率をかける

            proj.Initialize(FinalDamage,this.gameObject); //ペットからのダメージを初期化
            proj.SetDirection(skillRot);
            proj.speed = currentAttack.bulletSpeed;
        }

        PlayAttackSound();
    }

    protected override GameObject FindNearestEnemy()
    {
        // 索敵したいレイヤーをまとめたレイヤーマスクを作成
        int enemyMask = LayerMask.GetMask("EnemySpider", "EnemyMage", "EnemyDragon", "EnemyBossSpider", "EnemyMushroom");

        // ペットの位置からattackRangeの範囲内にある、指定したレイヤーのコライダーを全て取得
        Collider[] enemiesInAttackRange = Physics.OverlapSphere(transform.position, attackRange, enemyMask);

        // 範囲内に敵が一人もいなければ、何もせずに処理を終了
        if (enemiesInAttackRange.Length == 0)
        {
            enemyTransform = null;
            return null;
        }

        GameObject closestEnemy = null;
        float minSqrDistance = Mathf.Infinity;

        // 見つかったコライダーのリストをループして、どれが一番近いか調べる
        foreach (Collider enemyCollider in enemiesInAttackRange)
        {
            // コライダーから、その親であるゲームオブジェクトの位置までの距離を計算（高速な2乗距離で）
            float sqrDist = (transform.position - enemyCollider.transform.position).sqrMagnitude;

            float minimumAttackDistance = 3.0f;

            //平方フィートで比較するため、minimumAttackDistanceも2乗する
            if (sqrDist < minimumAttackDistance * minimumAttackDistance)
            {
                // 近すぎるので、この敵は無視して次の敵のチェックに移る
                continue;
            }

            // もし、今まで見つけたどの敵よりも距離が近ければ
            if (sqrDist < minSqrDistance)
            {
                // こちらを新しい「最も近い敵」として記憶する
                minSqrDistance = sqrDist;
                closestEnemy = enemyCollider.gameObject; // コライダーからゲームオブジェクト本体を取得
            }
        }



        // 最も近い敵をenemyTransformに設定
        enemyTransform = closestEnemy?.transform;

        // 最も近い敵を返す
        return closestEnemy;
    }

    protected override void PlayAttackSound()
    {
        attackCounter++;

        MapSpecificAttack currentAttack = GetCurrentMapAttack();

        if (currentAttack == null) return;
        
        if (attackCounter >= 3)
        {
            SoundEffect.Instance.Play(currentAttack.attackSE);
            attackCounter = 0; //カウンターをリセット
        }
    }

    private MapSpecificAttack GetCurrentMapAttack()
    {
        //現在のマップタイプを取得
        MapType currentMapType = StageManager.Instance.mapData.mapType;

        //現在のマップタイプに一致する攻撃設定を探す
        foreach (var attack in mapAttacks)
        {
            if (attack.mapType == currentMapType)
            {
                return attack;
            }
        }

        return null;
    }
}
