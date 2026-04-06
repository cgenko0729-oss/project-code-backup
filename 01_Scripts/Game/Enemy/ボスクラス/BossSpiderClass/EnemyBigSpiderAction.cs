//using UnityEngine;
//using System.Collections.Generic;
//using UnityEngine.UI;     
//using TMPro;    
//using DG.Tweening;
//using TigerForge;               //EventManager
//using QFSW.MOP2;                //Object Pool
//using MonsterLove.StateMachine; //StateMachine

//using HighlightPlus;

//public class EnemyBigSpiderAction : MonoBehaviour
//{

//    public enum BossState
//    {
//        Spawn,
//        Idle,
//        Move,
//        WebAttack,
//        DashAttack,
//        JumpAttack,
//        SpinAttack,
//        MagicBallAttack,
//        Dizzy,
//        Attack,
//        Dead
//    }

//    public GameObject bossAoeAtkMagicCircle;　　　//魔法aoe 
//    public GameObject bossAoeAtkWebCircle;       //糸ネットaoe
//    public GameObject bossAoeAtkDashRectangle;      //ダッシュaoe 

//    public GameObject bossSpiderDenObject;    // 巣穴オブジェクト
//    public GameObject bossMagicProjectileObj; // 魔法弾
//    public GameObject bossWebProjectileObj;   // 糸弾

//    HighlightEffect highlightEffect;  //ボスのシェーダーとアウトラインエフェクト 

//    public float stateCnt;                    // 各状態の継続時間などをカウントするタイマー
//    public StateMachine<BossState> stateMachine;　　// 状態遷移を管理するステートマシン
//    public int stateInfo;
//    public int preStateInfo; // 前の状態番号（enum を int に変換したもの）
//    public int nextStateInfo; // 次の状態番号（enum を int に変換したもの）

//    private Animator animator;

//    Transform playerTrans; // プレイヤーのTransform
//    public float distToPlayer; // プレイヤーとの距離
//    public Vector3 playerDir;

//    public int randAttackPattern; //ランダムで選択される攻撃パターン
                               
//    public float distToAttack = 14f; // 攻撃可能な距離
//    public bool hasAttacked = false; // 既に攻撃したかどうかのフラグ

//    public float moveSpeed = 4f; // 移動速度
//    public float moveSpeedNormal = 4f;

//    private float bossFieldPosXMax = 14.5f; // ボスフィールドのX軸の最大値
//    private float bossFieldPosXMin = -14f; // ボスフィールドのX軸の最小値
//    private float bossFieldPosZMax = 14f; // ボスフィールドのZ軸の最大値
//    private float bossFieldPosZMin = -14.5f; // ボスフィールドのZ軸の最小値

//    public float facePlayerCnt; // プレイヤーの方向を向くためのカウントダウン

//    public Vector3 punchRotationAmount = new Vector3(45f, 0f, 0f); //30 about X
//    public float punchDuration        = 0.4f;
//    public int   punchVibrato         = 4;   
//    public float punchElasticity      = 0.2f; 

//    public void Awake()
//    {
        
//        var player = GameObject.FindWithTag("Player");
//        if (player) playerTrans = player.transform;

//        animator = GetComponent<Animator>();

//        stateMachine = StateMachine<BossState>.Initialize(this);
//        stateMachine.ChangeState(BossState.Move);

//        //DoTween to fluctuate float vlue highlightProfile.innerGlowPower from 0 to 1.5 in 2 seconds, then back to 0 in 2 seconds, repeat forever     
//        //highlightEffect = GetComponent<HighlightEffect>();
//        //highlightEffect.innerGlow = 1f; // 初期値を設定
//        //DOTween.To(() => highlightEffect.innerGlow, x => highlightEffect.innerGlow = x, 7.7f, 1f)
//        //    .SetLoops(-1, LoopType.Yoyo) // 無限ループで往復
//        //    .SetEase(Ease.InOutSine); // イージングを設定

//    }

//    public void Update()
//    {
//        stateInfo = (int)stateMachine.State;
//        stateCnt -= Time.deltaTime;
//        GetPlayerInfo();
//    }

//    public void GetPlayerInfo()
//    {
//        if (playerTrans == null) return;
//        playerDir = (playerTrans.position - transform.position).normalized; // プレイヤーの方向を計算
//        distToPlayer = Vector3.Distance(transform.position, playerTrans.position); // プレイヤーとの距離を計算

//    }

//    public void Move_Enter()
//    {
//        stateCnt = 2f;
//        animator.SetTrigger("isWalking");
//    }

//    public void Move_Update()
//    {
//        HomingPlayer();
//        RotateTowardPlayer();
//        FixPosYRotX(0.5f,0f);

//        if (distToPlayer < distToAttack && stateCnt <= 0)
//        {
//            ChangeToDashAttack();
//            //ChangeToDizzy();
//        }
//    }

//    public void HomingPlayer()
//    {
//        if (playerTrans == null) return;
//        Vector3 direction = (playerTrans.position - transform.position).normalized;
//        transform.position += direction * Time.deltaTime * moveSpeed; // 移動速度は調整可能
//        //transform.LookAt(playerTrans); // プレイヤーの方向を向く
       

//    }

//    public void RotateTowardPlayer()
//    {
//        if (playerTrans == null) return;
//        Vector3 direction = (playerTrans.position - transform.position).normalized;
//        Quaternion lookRotation = Quaternion.LookRotation(direction);
//        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f); // 回転速度は調整可能

//    }

//    public void MagicBallAttack_Enter()
//    {
//        stateCnt = 3f;
//        animator.SetTrigger("isIdle");
//        //randAttackPattern = Random.Range(0, 4); // 0から3のランダムな整数を生成
//        hasAttacked = false;
    
//    }

//    public void MagicBallAttack_Update()
//    {

//        RotateTowardPlayer();

//        if (stateCnt > 0) return;

//        SpawnMagicAoeAttck();
//        Invoke("ChangeToMoveState", 1.5f);

//    }

//    public void SpawnMagicAoeAttck()
//    {
//        if(hasAttacked) return;
//        hasAttacked = true;

//        animator.SetTrigger("isShotAttack");

//        float spawnRadius = 11f;
//        int spawnNum = 7;

//        List<Vector3> spawnPositions = new List<Vector3>();
//        for (int i = 0; i < spawnNum; i++)
//        {
//            Vector3 spawnPos;
//            bool validPosition;

//            do
//            {
//                validPosition = true;
//                float angle = Random.Range(0f, 360f);
//                float radius = Random.Range(0f, spawnRadius);
//                spawnPos = playerTrans.position + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

//                foreach (Vector3 pos in spawnPositions)
//                {
//                    if (Vector3.Distance(spawnPos, pos) < 3.5f)
//                    {
//                        validPosition = false;
//                        break;
//                    }
//                }

//            } while (!validPosition);

//            spawnPositions.Add(spawnPos);
//        }

//        foreach (Vector3 pos in spawnPositions)
//        {
//            float circleScale = Random.Range(2.8f, 5.6f); // ランダムなスケールを生成
//            float circleDuration = Random.Range(1.4f, 2.8f); // ランダムな持続時間を生成
//            Vector3 aoeSpawnPos = new Vector3(pos.x, 0.5f, pos.z); // Y座標を0.5に固定
//            GameObject aoeBall = Instantiate(bossAoeAtkMagicCircle, aoeSpawnPos, Quaternion.identity);
//            aoeBall.SetActive(true);
//            AoeCircle aoeCircle = aoeBall.GetComponent<AoeCircle>();
//            aoeCircle.InitCircle(circleScale, circleDuration, 10);

//        }
//    }

//    public void ChangeToMoveState() => stateMachine.ChangeState(BossState.Move);   
//    public void ChangeToJumpAttack() => stateMachine.ChangeState(BossState.JumpAttack);
//    public void ChangeToWebAttack() => stateMachine.ChangeState(BossState.WebAttack);
//    public void ChangeToDashAttack() => stateMachine.ChangeState(BossState.DashAttack);
//    public void ChangeToSpinAttack() => stateMachine.ChangeState(BossState.SpinAttack);
//    public void ChangeToDizzy() => stateMachine.ChangeState(BossState.Dizzy);
//    public void ChangeToIdle() => stateMachine.ChangeState(BossState.Idle);
//    public void ChangeToState(BossState nextState)
//    {
//        preStateInfo = (int)stateMachine.State; // 現在の状態を保存
//        stateMachine.ChangeState(nextState); // 次の状態に変更
//        nextStateInfo = (int)stateMachine.State; // 次の状態を保存
//        //Debug.Log("ChangeToState: " + nextState + ", PreState: " + preStateInfo + ", NextState: " + nextStateInfo);
//    }


//    public void FixPosYRotX(float posY, float rotX)
//    {
//        Vector3 pos = transform.position;
//        pos.y = posY;
//        transform.position = pos;

//        Vector3 rotation = transform.rotation.eulerAngles;
//        rotation.x = rotX;
//        transform.rotation = Quaternion.Euler(rotation);

//        //Debug.Log("FixPosYRotX: " + posY + ", RotX: " + rotX);
//    }

//    public void JumpAttack_Enter()
//    {
//        stateCnt = 4f;
//        TurnAround360Degree();
//        //TurnFaceUp();
//    }

//    public void JumpAttack_Update()
//    {
        

//        if (stateCnt <= 0)
//        {
//            //ChangeToMoveState();
//        }


//    }

//    public void SpawnMagicProjectile()
//    {
//        int spawnNum = 20;
//        Vector3 spawnPos = transform.position;
//        spawnPos.y = 0.5f; // Y座標を0.5に固定
//        for (int i = 0; i < spawnNum; i++)
//        {
//            GameObject magicProjectile = Instantiate(bossMagicProjectileObj, spawnPos, Quaternion.identity);
//            magicProjectile.SetActive(true);
//            EffectMoveController projectile = magicProjectile.GetComponent<EffectMoveController>();

//            //each projectile to move toward a circle with this.transform.position as center, with a radius of 10f, and each projectile will be evenly distributed around the circle , they all spawned from the center of circle
//            float angle = i * (360f / spawnNum) * Mathf.Deg2Rad; // ラジアンに変換
//            Vector3 targetPos = new Vector3(
//                this.transform.position.x + Mathf.Cos(angle) * 10f,
//                this.transform.position.y,
//                this.transform.position.z + Mathf.Sin(angle) * 10f
//            );
//            projectile.moveVec = (targetPos - this.transform.position).normalized * 5f; // プレイヤーの位置をターゲットに設定
//        }

//        Debug.Log("Spawned " + spawnNum + " magic projectiles.");

//    }

//    public void ShotWebProjectile()
//    {       
//        int projectileCount = 3;
//        float sectorAngle = 45f;
//        Vector3 spawnOffset = Vector3.up * 0.5f; 
//        //float speed = 8f;                

//        float angleStep = (projectileCount > 1)? sectorAngle / (projectileCount - 1) : 0f;
//        float halfSector = sectorAngle * 0.5f; //-halfSector = left → right
//        Vector3 origin = transform.position + spawnOffset;

//        for (int i = 0; i < projectileCount; i++)
//        {
//           float currentAngle = -halfSector + angleStep * i;
//           Vector3 dir = Quaternion.Euler(0f, currentAngle, 0f) * transform.forward;
//           Quaternion look = Quaternion.LookRotation(dir);
//           Quaternion offset = Quaternion.Euler(-90f, 0f, 0f);

//           GameObject web = Instantiate(bossWebProjectileObj,origin,look * offset);
//           var mov = web.GetComponent<EffectMoveController>();
//           if (mov != null) mov.moveVec = dir;
                                   
//        }


//    }

//    public void ClimbUpField()
//    {
//        float jumpHeight = 7f; // ジャンプの高さ
//        float jumpDuration = 1.5f; // ジャンプの持続時間

//        transform.DOMoveY(jumpHeight, 1).OnComplete(() => {
//            transform.DORotate(new Vector3(0, transform.rotation.eulerAngles.y, 0), 1f, RotateMode.Fast)
//                .SetEase(Ease.Linear)
//                .OnComplete(() => {
//                    ChangeToMoveState(); // ジャンプが完了したら、移動状態に戻る
//                });

//            transform.DOMoveY(0.5f, 1); // ジャンプが完了したら、元の位置に戻る
//        });
//    }

 

//    public void TurnFaceUp()
//    {
//        // move rotation x to -90f gradually in 1 second with DOTween
//        transform.DORotate(new Vector3(-90f, transform.rotation.eulerAngles.y, 0), 1f, RotateMode.Fast)
//            .SetEase(Ease.Linear)
//            .OnComplete(() => {
//                ClimbUpField();
//            });

//    }

//    public void TurnAround360Degree()
//    {
//        //current 's rotationY += 360  gradually in 1 second with DOTween
//        transform.DORotate(new Vector3(0, transform.rotation.eulerAngles.y + 360f, 0), 1f,RotateMode.LocalAxisAdd)
//            .SetEase(Ease.Linear)
//            .OnComplete(() => {
//                // 回転が完了したら、元の状態に戻る
//                //FixPosYRotX();
//                SpawnMagicProjectile();
//                ChangeToMoveState();
//            });

        


//    }

//    public void ShakeCamera()
//    {
//        Camera.main.transform.DOShakePosition(1f, new Vector3(0.5f, 0.5f, 0.5f), 10, 90, false, true).SetEase(Ease.Linear);
//    }

//    public void DashAttack_Enter()
//    {
//        stateCnt = 3f;
//        hasAttacked = false;

//        //GameObject aoeRect = Instantiate(bossAoeAtkDashRectangle, transform.position, Quaternion.identity);
//        //aoeRect.SetActive(true);
//        //AoeRectIndicator rect = aoeRect.GetComponent<AoeRectIndicator>();
//        //rect.InitRect(distToPlayer, 4, 1.5f, 10, true);
//        ////set aoeRect to face player direction
//        //aoeRect.transform.rotation = Quaternion.LookRotation(playerDir, Vector3.up);

//        // direction & distance to player (XZ plane only)
//    Vector3 dir   = (playerTrans.position - transform.position);
//    dir.y = 0f;
//    float  dist  = dir.magnitude;
//    Vector3 fwd  = dir.normalized;

//    // bar centre = half-way between spider and player
//    Vector3 centre = transform.position + fwd * (dist * 0.5f);

//    // instantiate and configure
//    GameObject aoeRect = Instantiate(bossAoeAtkDashRectangle, centre, Quaternion.identity);
//    AoeRectIndicator rect = aoeRect.GetComponent<AoeRectIndicator>();

//    // pass width & length      ▼width    ▼length
//    rect.InitRect(new Vector2(dist, dist), 4f, 1.5f, 10, true);

//    // ③  rotate: local X = bar length, so add 90° after LookRotation
//    aoeRect.transform.rotation =
//        Quaternion.LookRotation(fwd, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);

//    }

//    public void DashAttack_Update()
//    {
//        if(!hasAttacked)RotateTowardPlayer();

//        float dashDist = distToPlayer; // ダッシュの距離

//        Vector3 dashTargetPoint = transform.position + playerDir * dashDist;

//        if (stateCnt <= 0 && !hasAttacked)
//        {
//            hasAttacked = true;
//            animator.SetTrigger("isCloseAttack");
//            transform.DOMove(dashTargetPoint, 1f).SetEase(Ease.Linear).OnComplete(() => 
//            {
//               Invoke("ChangeToMoveState", 1.5f);
//            });
            
//        }
//    }

//    public void Dizzy_Enter()
//    {
//        //transform.rotation = Quaternion.Euler(0, 0, 180f);
//        stateCnt = 1f;
//        //animator.SetTrigger("isMove");

//        //ShotWebProjectile();

//        DoAttackAniTween();

//    }

//    public void Dizzy_Update()
//    {
//        if (stateCnt <= 0) ChangeToMoveState();
        
//        //FixPosYRotX(1.7f,0f);
//    }

//    void DoAttackAniTween()
//    {
//       transform.DOPunchRotation(punchRotationAmount,punchDuration,vibrato: punchVibrato,elasticity: punchElasticity).SetEase(Ease.OutQuad);
//    }


//}

