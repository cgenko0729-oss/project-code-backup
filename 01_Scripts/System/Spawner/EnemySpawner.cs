using UnityEngine;
using System.Collections.Generic;    
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;          
using QFSW.MOP2;           



public class EnemySpawner : Singleton<EnemySpawner>
{

    
    private Transform playerTrans;                               // プレイヤーのTransform参照
    [SerializeField] private ObjectPool enemyPool;          // 敵オブジェクトのプール参照
    [SerializeField] private EnemyType enemyType;           // スポーンする敵の種類
    public SpawnMethod spawnMethod;                         // スポーン方法の選択

    [SerializeField] private float spawnCooldownMax = 1f;    // スポーン間隔の最大値（秒）
    public float spawnCooldownScaler = 100f;                // スポーン間隔のスケール（難易度に応じて変化）
    public float spawnCooldownActual ; // 実際のスポーン間隔

    public float spawnCoolDown;                             // 現在のスポーンタイマー

    [SerializeField] private int minEnemies = 2;             // スポーン時の最小敵数
    [SerializeField] private int maxEnemies = 6;             // スポーン時の最大敵数
    public float spawnNumberScaler = 1f;          // スポーン数のスケール（難易度に応じて変化）
    public float spawnNumberActual;

    [SerializeField] private float minSpawnDist = 18f;     // プレイヤーから最小スポーン距離
    [SerializeField] private float maxSpawnDist = 26f;     // プレイヤーから最大スポーン距離

    [SerializeField] public int enemyNumMax = 200;        // 同時に出現可能な最大敵数
    public int nowEnemyNum;                               // 現在出現中の敵数

   
    [Header("円形スポーン")]
    [SerializeField] private int  circleMinEnemies  = 8;     // 円形スポーンの最小敵数
    [SerializeField] private int  circleMaxEnemies  = 16;    // 円形スポーンの最大敵数
    [SerializeField] private float circleRadius    = 30f;    // 円形スポーンの半径
    [SerializeField] private float circleJitter    = 1.5f;   // 半径に対するランダムばらつき
    [SerializeField] private bool  circleRandomYaw = true;  // 開始角度をランダムにするか

    [Header("斜めスポーン")]   
    public float slashDistToPlayer = 25f;                         // スラッシュスポーンのプレイヤーからの距離
    public float slashSpacing = 0f;                               // スラッシュスポーンの間隔倍率（0.1f など）
    [SerializeField] private float horizontalDistToPlayer = 25f;
    [SerializeField] private float horizontalSpacing      = 2.5f;

    [Header("TwoSideSpawn")]
    public int sideSpawnCount = 21; //each side have 21 enemy
    public float sideSpawnDist = 21f; // dist from player to side spawn point
    public float sideSpawnSpacing = 2.0f; 

    private PlayerState playerStatus;
    private EnemyManager enemyManager;

    public float enemySpawnHp =100;
    public bool isDebugEnemyNum = false; // デバッグ用の敵数表示フラグ

    private void Start()
    {
        if (playerTrans == null) playerTrans = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerStatus = playerTrans.GetComponent<PlayerState>();
        enemyManager = EnemyManager.Instance;
    }

    public void HandleMoverNum()
    {
         if (isDebugEnemyNum) return;

        if(enemyType == EnemyType.Mover)
        {
            if (TimeManager.Instance.gamePhrase == 6)
            {
                minEnemies = 14;
                maxEnemies = 28;
                enemyManager.moverMaxNum = 490; //560 - 700 
                enemySpawnHp = 350;
                spawnCooldownMax = 0.77f;
                return;
            }
            if (TimeManager.Instance.gamePhrase == 5)
            {
                minEnemies = 14;
                maxEnemies = 21;
                enemyManager.moverMaxNum = 350; //560 - 700 
                enemySpawnHp = 280;
                spawnCooldownMax = 0.77f;
                return;
            }
            else if (TimeManager.Instance.gamePhrase == 4)
            {
                minEnemies = 7;
                maxEnemies = 14;
                enemyManager.moverMaxNum = 140;
                enemySpawnHp = 210;
                return;
            }
            else if (TimeManager.Instance.gamePhrase == 3)
            {
                minEnemies = 4;
                maxEnemies = 7;
                enemyManager.moverMaxNum = 77;
                enemySpawnHp = 140;
                return;
            }          
            else if (TimeManager.Instance.gamePhrase == 2)
            {
                minEnemies = 3;
                maxEnemies = 5;
                enemyManager.moverMaxNum = 49;
                enemySpawnHp = 100;
                return;
            }
            else if(TimeManager.Instance.gamePhrase == 2)
            {
                minEnemies = 2;
                maxEnemies = 4;
                enemyManager.moverMaxNum = 35;
                enemySpawnHp = 100;
                return;
            }


        }

        if(enemyType == EnemyType.EliteMover)
        {

            if (TimeManager.Instance.gamePhrase == 5)
            {
                minEnemies = 2;
                maxEnemies = 3;
                enemyManager.EliteMoverMaxNum = 11;
                spawnCooldownMax = 7.7f;
                return;
            }
            else if (TimeManager.Instance.gamePhrase == 4)
            {
                minEnemies = 2;
                maxEnemies = 2;
                enemyManager.EliteMoverMaxNum = 11;
                spawnCooldownMax = 14f;
                return;
            }

        }


        if(enemyType == EnemyType.Bomber)
        {


            if (TimeManager.Instance.gamePhrase == 5)
            {
                minEnemies = 2;
                maxEnemies = 3;
                enemyManager.bomberMaxNum = 7;
                spawnCooldownMax = 14f;
                return;
            }
            else if (TimeManager.Instance.gamePhrase == 4)
            {
                minEnemies = 2;
                maxEnemies = 2;
                enemyManager.EliteMoverMaxNum = 5;
                spawnCooldownMax = 21f;
                return;
            }

        }

    }

    public void DifficultyHandler()
    {
        HandleMoverNum();
       

        if(enemyType == EnemyType.Surrounder)
        {
            if (TimeManager.Instance.gamePhrase == 5)
            {
                spawnCooldownMax = 21f;
                return;
            }
        }


    }

    private void Update()
    {
        if (playerTrans == null) return;

        spawnCoolDown -= Time.deltaTime;

        DifficultyHandler();


        if (spawnCoolDown <= 0f)  // クールダウン終了かつ敵数が上限以下ならスポーン
        {
            
            if(enemyType == EnemyType.Mover && TimeManager.Instance.gameTimeLeft < 350 &&  !GameManager.Instance.isBossFight)
            {
                float targetNumnber = enemyManager.moverMaxNum; // スポーンする敵の最大数を取得
                float currentNumber = enemyManager.moverNum; // 現在のスポーン中の敵数を取得
                float ratio = currentNumber / targetNumnber; // 現在の敵数の割合を計算
                spawnCooldownScaler =  ratio; // 割合に応じてスポーンクールダウンを調整
                spawnNumberScaler = 1f - ratio; // 割合に応じてスポーン数を調整
            }else
            {
                spawnCooldownScaler = 1f;
                spawnNumberScaler = 0f;
            }


            if (enemyType == EnemyType.Mover)
            {
                spawnCoolDown = spawnCooldownMax * spawnCooldownScaler;
                spawnCooldownActual = spawnCoolDown;
            }
            else spawnCoolDown = spawnCooldownMax;
           
            if (!EnemyManager.Instance.CheckEnemyNumLimit(enemyType)) return;

            // 選択中のスポーン方法に応じて呼び出す
            if (spawnMethod == SpawnMethod.RandomRange) SpawnBurst();
            else if (spawnMethod == SpawnMethod.Circular) SpawnCircle();
            else if (spawnMethod == SpawnMethod.Slash) SpawnSlash();
            else if (spawnMethod == SpawnMethod.TargetPoint) SpawnTargetPoint(transform.position);
            else if (spawnMethod == SpawnMethod.TwoSide) SpawnTwoSide();
           
        }

        Debug();

    }

    private void SpawnBurst()
    {     
        
         float countf = Random.Range(minEnemies, maxEnemies + 1);
        int count = (int)(countf * (1+ spawnNumberScaler) );
        spawnNumberActual = count;

  
    float angleStep  = 360f / count;
    float startAngle = Random.Range(0f, angleStep);

    float jitter = angleStep * 0.2f;

        for (int i = 0; i < count; ++i)
        {
            float angleDeg = startAngle + i * angleStep + Random.Range(-jitter, jitter);
            float angleRad = angleDeg * Mathf.Deg2Rad;

            // 4. Radius anywhere between the 2 sliders.
            float radius = Random.Range(minSpawnDist, maxSpawnDist);

            Vector3 dir = new(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad));
            Vector3 pos = playerTrans.position + dir * radius;

            GameObject enemyObj = enemyPool.GetObject(pos);
            //EnemyActionBase enemyMove = enemyObj.GetComponent<EnemyActionBase>();
            //enemyMove.targetPoint = player.position;

            if (enemyType == EnemyType.Mover) {
                EnemyStatusBase enemyStatus = enemyObj.GetComponent<EnemyStatusBase>();
                enemyStatus.enemyMaxHp = enemySpawnHp;
                    }

            // 6. Counters
            nowEnemyNum++;
            EnemyManager.Instance.AddEnemyCount(enemyType);
        }





    }

    private void SpawnCircle()   // プレイヤーを中心に円状に敵をスポーンする,各敵は中心（プレイヤー）を目指して移動する
    {
        
        int count = Random.Range(circleMinEnemies, circleMaxEnemies + 1);

        
        float angleStep = 360f / count;
        float startYaw  = circleRandomYaw ? Random.Range(0f, 360f) : 0f;

        for (int i = 0; i < count; ++i)
        {
            float angleDeg = startYaw + i * angleStep;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            
            float radius   = circleRadius + Random.Range(-circleJitter, circleJitter);

            Vector3 dir    = new(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad));
            Vector3 pos    = playerTrans.position + dir * radius;

            //enemyPool.GetObject(pos + new Vector3(0,-1,0));

            Vector3 targetPoint = playerTrans.position;
            EnemyActionBase enemyMove = enemyPool.GetObject(pos).GetComponent<EnemyActionBase>();
            enemyMove.targetPoint = targetPoint;
            //enemyMove.targetPoint.y = 0.7f; // Ensure y is 0
           
            nowEnemyNum++;
            EnemyManager.Instance.AddEnemyCount(enemyType);
        }
    }

    public void SpawnTwoSide()
    {

       // Ensure we have a valid player reference
    if (playerTrans == null) return;

    // We only need the player's position. Their rotation will be ignored.
    Vector3 playerPos = playerTrans.position;

    // --- THE FIX: Use World-Space Vectors ---
    // Instead of player.right, we use Vector3.right. This is always (1, 0, 0).
    // This ensures the spawn lines are always to the absolute left and right of the player.
    Vector3 worldRight = Vector3.right;
    
    // Instead of player.forward, we use Vector3.forward. This is always (0, 0, 1).
    // This ensures the "wall" of enemies is always aligned vertically on the screen.
    Vector3 worldForward = Vector3.forward;
    // ----------------------------------------

    // Calculate the center points for the left and right lines using WORLD vectors.
    Vector3 leftLineCenter = playerPos - worldRight * sideSpawnDist;
    Vector3 rightLineCenter = playerPos + worldRight * sideSpawnDist;

    // Loop 'sideSpawnCount' times to spawn enemies on each side.
    for (int i = 0; i < sideSpawnCount; i++)
    {
        if (!EnemyManager.Instance.CheckEnemyNumLimit(enemyType)) return;

        // Calculate the offset from the center of the line.
        float offsetMagnitude = (i - (sideSpawnCount - 1) / 2.0f) * sideSpawnSpacing;

        // Create the line offset using the WORLD forward vector.
        // This builds the line of enemies along the world's Z-axis.
        Vector3 spawnOffset = worldForward * offsetMagnitude;

        // Define Spawn and Target Positions
        Vector3 leftSpawnPos = leftLineCenter + spawnOffset;
        Vector3 rightSpawnPos = rightLineCenter + spawnOffset;

        // The target for the left enemy is the corresponding position on the right line.
        Vector3 leftEnemyTarget = rightSpawnPos;
        // The target for the right enemy is the corresponding position on the left line.
        Vector3 rightEnemyTarget = leftSpawnPos;

        // --- Spawn Left Side Enemy ---
        GameObject leftEnemyObj = enemyPool.GetObject(leftSpawnPos);
        if (leftEnemyObj != null)
        {
            EnemyActionBase leftEnemyMove = leftEnemyObj.GetComponent<EnemyActionBase>();
            if (leftEnemyMove != null)
            {
                leftEnemyMove.targetPoint = leftEnemyTarget;
            }
            nowEnemyNum++;
            EnemyManager.Instance.AddEnemyCount(enemyType);
        }

        // --- Spawn Right Side Enemy ---
        GameObject rightEnemyObj = enemyPool.GetObject(rightSpawnPos);
        if (rightEnemyObj != null)
        {
            EnemyActionBase rightEnemyMove = rightEnemyObj.GetComponent<EnemyActionBase>();
            if (rightEnemyMove != null)
            {
                rightEnemyMove.targetPoint = rightEnemyTarget;
            }
            nowEnemyNum++;
            EnemyManager.Instance.AddEnemyCount(enemyType);
        }
    }

    }


    public void SpawnSlash() /// プレイヤーの前後に帯状に敵をスポーンし、反対側へ移動させる
    {
       
        Vector3 playerPos = playerTrans.position;
        
        Vector3 topLeftDir = (-playerTrans.right + playerTrans.forward).normalized;       // 左上方向を計算し、開始位置を決定
        Vector3 spawnOrigin = playerPos + topLeftDir * slashDistToPlayer;
        
        Vector3 bottomRightDir = (playerTrans.right - playerTrans.forward).normalized;    // 右下方向の目標位置
        Vector3 moveTarget = playerPos + bottomRightDir * slashDistToPlayer;

        int count = Random.Range(minEnemies, maxEnemies + 1);

        Vector3 rightwardOffset = Vector3.Cross(Vector3.up, (moveTarget - spawnOrigin).normalized);  // スラッシュ軸に垂直な右方向ベクトル
        //float spacing = 0.1f; //2.5 // 間隔倍率

        for (int i = 0; i < count; ++i)
        {
            Vector3 offset = rightwardOffset * (i - (count - 1) / 2f) * slashSpacing;
            Vector3 spawnPos = spawnOrigin + offset;

            GameObject enemy = enemyPool.GetObject(spawnPos);
            EnemyActionBase enemyMove = enemy.GetComponent<EnemyActionBase>();
            enemyMove.targetPoint = moveTarget;

            EnemyManager.Instance.AddEnemyCount(enemyType);
            nowEnemyNum++;

         }

    }

    public void SpawnTargetPoint(Vector3 targetPoint)
    {
        //if (nowEnemyNum >= enemyNumMax) return; // 最大数に達している場合はスポーンしない

        GameObject enemy = enemyPool.GetObject(targetPoint);
 
        nowEnemyNum++;
        EnemyManager.Instance.AddEnemyCount(enemyType);


    }

    
 

    public void Debug()
    {
        //DebugMenu.Instance.ShowInt("EnemyTotalNum",nowEnemyNum);

       

    }

}


