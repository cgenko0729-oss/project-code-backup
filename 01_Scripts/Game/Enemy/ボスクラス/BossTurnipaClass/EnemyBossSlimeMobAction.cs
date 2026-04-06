using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager
using Cysharp.Threading.Tasks;

public class EnemyBossSlimeMobAction : MonoBehaviour
{

    public AoeRectIndicator aoeRectIndicator;
    public GameObject AoeDashAttackRectObj;
     public float aoeDashRectFillDuration = 1f;
    private float indicatorLingerTime = 0.2f;

    public float followCnt = 3f;

    public Transform playerTrans;
    public float distToPlayer; // プレイヤーとの距離
    public Vector3 dirToPlayer;

    public Vector3 dashTargetPos;

    public Vector3 dashOffset;

    public bool isFillRect = false;
    public bool isDashing = false;

    public float moveSpd = 2.1f;

    public Vector3 spawnPosTarget;
    public bool isReachSpawnPoint = false;
    public float distToSpawnPos = 10;

    EnemySpiderAction spiderAction;
    public bool isHomingEnable = false;


    void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player) playerTrans = player.transform;

        followCnt = Random.Range(4.5f, 7.9f);

        spiderAction = GetComponent<EnemySpiderAction>();
        
    }

    void Update()
    {

        if(GameManager.Instance.isGameClear || GameManager.Instance.isGameOver)
        {
            EnemyStatusBase enemyStatus = GetComponent<EnemyStatusBase>();
            enemyStatus.DeadNoExp();
        }

        dirToPlayer = (playerTrans.position - transform.position).normalized; // プレイヤーの方向を計算
        distToPlayer = Vector3.Distance(transform.position, playerTrans.position); // プレイヤーとの距離を計算

        if (!isReachSpawnPoint)
        {
            HomingSpawnPos();
            RotateTowardPlayer(false);
            if(distToSpawnPos <= 0.4f) isReachSpawnPoint = true; // スポーン位置に到達したらフラグを立てる
        }

        if (!isReachSpawnPoint) return;


        followCnt-= Time.deltaTime;

        if(followCnt <= 0)
        {
            //followCnt = rand 5 to 8 second 
            followCnt = Random.Range(4.5f, 7.9f);

            if (isHomingEnable)
            {
                isHomingEnable = false ;
                spiderAction.isEnableHoming = false;
                ChangeLayerTo("EnemyBat");
            }

            FacePlayerNow();
            GameObject aoeRect = Instantiate(AoeDashAttackRectObj);
            aoeRectIndicator = aoeRect.GetComponent<AoeRectIndicator>();


            //rand dashOffset around a circle with player as center
            //float randAngle = Random.Range(0f, 360f);
            //dashOffset = new Vector3(Mathf.Cos(randAngle * Mathf.Deg2Rad), 0f, Mathf.Sin(randAngle * Mathf.Deg2Rad)) * 1.5f; // 1.5f is the radius of the circle

            Vector3 targetDir = dirToPlayer;

            //rand targetDir around a circle with player as center
            float randAngle2 = Random.Range(-14f, 14f);
            targetDir = Quaternion.Euler(0f, randAngle2, 0f) * targetDir; // rotate around Y axis

            dirToPlayer = targetDir;

            dashTargetPos = transform.position + dirToPlayer * distToPlayer * 1.4f;


            Vector3 indicatorPosition = transform.position + dirToPlayer * (distToPlayer * 0.77f); //0.5f
            Quaternion indicatorRotation = Quaternion.LookRotation(dirToPlayer, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);
            Vector2 indicatorSize = new Vector2(distToPlayer*1.4f, 3f); 
            aoeRectIndicator.UpdateTransform(indicatorPosition + new Vector3(0,0.7f,0),indicatorRotation,indicatorSize);

            isFillRect = true;
            isDashing = true;
        }else
        {
            if(isDashing) return;

            if (!isHomingEnable)
            {
                isHomingEnable = true;
                spiderAction.isEnableHoming = true;
            }

            //HomingPlayer();
            //RotateTowardPlayer();
        }

        if (isFillRect)
        {
            isFillRect = false;
            aoeRectIndicator.BeginFill(aoeDashRectFillDuration, indicatorLingerTime, 25, true);

            Invoke(nameof(DashOnce), 1.1f);
        }

        
    }


    public void DashOnce()
    {
        transform.DOMove(dashTargetPos, 0.5f).SetEase(Ease.Linear).OnComplete(() => {
            isDashing = false;
            //ChangeLayerTo("EnemySpider");
        });

    }

     public void FacePlayerNow()
    {
        if (playerTrans == null) return;
        Vector3 direction = (playerTrans.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation; // プレイヤーの方向を向く

    }

    public void RotateTowardPlayer(bool isPlayer = true)
    {
        if (isDashing) return;
        if (playerTrans == null) return;
        Vector3 direction = Vector3.zero;
        if(isPlayer)direction = playerTrans.position - transform.position;
        else direction = spawnPosTarget - transform.position;
        direction.y = 0;
        if (direction == Vector3.zero) return;
        Quaternion targetLookRotation = Quaternion.LookRotation(-direction);
        float targetYAngle = targetLookRotation.eulerAngles.y;
        Quaternion finalTargetRotation = Quaternion.Euler(-180f, targetYAngle, 180f);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalTargetRotation, Time.deltaTime * 5f);

    }

    public void HomingPlayer()
    {
        Vector3 direction = (playerTrans.position - transform.position).normalized;
        transform.position += direction * Time.deltaTime * moveSpd; // 移動速度は調整可能
        
    }

    public void HomingSpawnPos()
    {
        Vector3 direction = (spawnPosTarget - transform.position).normalized;
        transform.position += direction * Time.deltaTime * moveSpd; // 移動速度は調整可能

        distToSpawnPos = Vector3.Distance(transform.position, spawnPosTarget); // スポーン位置との距離を計算
    }

    public void FixY(float posY)
    {
        Vector3 pos = transform.position;
        pos.y = posY; // Y座標を指定の値に設定
        transform.position = pos; // 位置を更新
    }

    public void ChangeLayerTo(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);

        gameObject.layer = layer;


    }



 }

