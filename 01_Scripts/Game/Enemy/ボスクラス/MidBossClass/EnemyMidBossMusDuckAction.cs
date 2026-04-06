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

public class EnemyMidBossMusDuckAction : BossActionBase
{
     public enum BossState
     {
         Idle,
         Move,
         Dizzy,
         Death,
         CircleAttack,
         CrossCircleAttack,
         TriangleCircleAttack,
        RectangleCircleAttack,
        CircleCircleAttack,
        RotatePillarAttack,
        SurroundComboAttack,
        JumpAttack,
        LaserAttack,
    }

     public float stateCnt;                    // 各状態の継続時間などをカウントするタイマー
     public StateMachine<BossState> stateMachine;　　// 状態遷移を管理するステートマシン
     public string stateInfoText; // 状態番号を文字列に変換するためのテキスト

     public float attackCnt = 0f;
     public float turningCnt = 0f;

     public Vector3 centerPos = Vector3.zero;

    public int attackTimes = 0;

    public int attackStyle = 0;

    public GameObject aoeEffect;
    public AudioClip aoeEffectSe1;

    public GameObject aoeEffect2;

    public bool isFirstAttack = false;

    [SerializeField] AnimationCurve timeByDistance = AnimationCurve.Linear(0, 0.5f, 20, 1.5f);
    [SerializeField] AnimationCurve heightByDistance = AnimationCurve.Linear(0, 3f, 20, 8f);
    public GameObject landEffect;
    public AudioClip landSE;

    public Transform headTrans;
    public Vector3 headOriginRot = new Vector3(0f,0f,-14.78f);
    public Vector3 headStartRot = new Vector3(0f,0f,-0.7f); //-14.78
    public Vector3 headEndRot = new Vector3(0f,0f,42f);

    public ParticleSystem laserEffect;
    public ParticleSystem laserEffect2;

    public bool isCustomMoveTime = false;
    public float customMoveTime = 7.9f;
    public float rotateTime = 2.1f;
    public ParticleSystem LaserCharingEffect;

    public GameObject aoeRectObj;
    public AoeRectIndicator rectIndicator;

    public AudioClip laserSe;
    public AudioClip laserFillingSe;


    private void Start()
     {
         InitBoss();

         stateMachine = StateMachine<BossState>.Initialize(this);
         stateMachine.ChangeState(BossState.Move);

         currentAtttackPattern = 0;
     }

     private void Update()
     {
         UpdateStateInfo();
         UpdateGobalCounter();
        UpdatePlayerInfo();
     }


     void Move_Enter()
     {
        stateCnt = 14f;

        if(isCustomMoveTime)
        {
            stateCnt = 3.5f; //customMoveTime
        }

        moveSpeed = 2.1f;

        if (!isFirstAttack)
        {
            isFirstAttack = true;
            stateCnt = 7f;
        }

        animator.SetBool("isIdling", false);
        animator.SetBool("isMoving", true);

    }

     void Move_Update()
     {
         HomingPlayer();
         RotateTowardPlayer();

        if (attackStyle == 1)
        {
            PlanStateChange(BossState.CircleAttack);
        }
        else if (attackStyle == 2)
        {
            PlanStateChange(BossState.SurroundComboAttack);
        }
        else if(attackStyle == 3)
        {
            PlanStateChange(BossState.JumpAttack);
        }
        else if (attackStyle == 4)
        {
            PlanStateChange(BossState.LaserAttack);
        }
    }

    public bool isRectBeginFill = false;

    public void LaserAttack_Enter()
    {
        attackCnt = 3.5f;
        stateCnt = 7.7f;
        headTrans.localEulerAngles = headStartRot;

        rotateTime = 2.1f;

        animator.SetBool("isMoving", false);
        animator.SetBool("isIdling", true);

        LaserCharingEffect.Play();

        FacePlayerNow();

        isRectBeginFill = false;
        rectIndicator = Instantiate(aoeRectObj, transform.position, Quaternion.identity).GetComponent<AoeRectIndicator>();


    }

    public float placeOffset = 7f;

    public void LaserAttack_LateUpdate()
    {

        rotateTime -= Time.deltaTime;
        if(rotateTime >= 0f)
        {
            RotateTowardPlayer();

            Vector3 baseDirToPlayer = (playerTrans.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, playerTrans.position);

            Vector3 indicatorPosition = transform.position + baseDirToPlayer * placeOffset;
            Quaternion indicatorRotation = Quaternion.LookRotation(baseDirToPlayer, Vector3.up) * Quaternion.Euler(0f, -90f, 0f); //indicator rot with -90 rotY offset
            Vector2 indicatorSize = new Vector2(17f, 3.5f); 

            rectIndicator.UpdateTransform(indicatorPosition, indicatorRotation, indicatorSize);



        }
        else
        {
            if (!isRectBeginFill)
            {
                isRectBeginFill = true;
                rectIndicator.BeginFill(3.5f - 2.1f, 0.1f, 0, true);
            }
        }


        if (attackCnt <= 0f)
        {
            SoundEffect.Instance.PlayOneSound(laserSe, 0.77f);
            attackCnt = 7.7f;
            LaserCharingEffect.Stop();
            //disable animator component:
            animator.enabled = false;

            laserEffect.gameObject.SetActive(true);
            laserEffect2.gameObject.SetActive(true);

            laserEffect.Play();
            laserEffect2.Play();

            headTrans.localEulerAngles = headStartRot;

            var seq = DOTween.Sequence()
            .SetUpdate(UpdateType.Late); // <- important

            seq.Append(headTrans.DOLocalRotate(headEndRot, 3.5f).SetEase(Ease.InOutSine)).OnComplete(() => {
                //return head to origin rot instantly without dotween:
                headTrans.localEulerAngles = headOriginRot;
                laserEffect.Stop();
                laserEffect2.Stop();

                laserEffect.gameObject.SetActive(false);

                laserEffect2.gameObject.SetActive(false);
                animator.enabled = true;
            });



        }

        PlanStateChange(BossState.Move);

    }

    void LaserAttack_Exit()
    {
        animator.SetBool("isMoving", true);
        animator.SetBool("isIdling", false);

    }

     void Dizzy_Enter()
     {

     }

     void Dizzy_Update()
     {

     }

     void Death_Enter()
     {

     }

     void Death_Update()
     {
        
     }

    public Vector3 jumpTragetPos = Vector3.zero;
    public float baseJumpAnimDuration = 0.5f;
    void JumpAttack_Enter()
    {
        attackCnt = 2.8f;
        attackTimes = 0;
        stateCnt = 5f;
        animator.SetBool("isMoving", false);
        animator.SetBool("isIdling", true);
        SetInnerGlow();

        jumpTragetPos = playerTrans.position;
        EnemyEffectManager.Instance.SpawnAoeCircle(jumpTragetPos, 5.6f, 3.5f, 21f);

    }

    void JumpAttack_Update()
    {
        PlanStateChange(BossState.Move);

        if (attackCnt <= 0)
        {
            attackCnt = 7f;
            
           //float jumpHeight = 3.5f;
           //float jumpDuration = 0.5f;
            animator.SetTrigger("isAttacking");

            float dist = Vector3.Distance(transform.position, jumpTragetPos);
            float duration = Mathf.Clamp(timeByDistance.Evaluate(dist), 0.4f, 2.5f);
            float h = heightByDistance.Evaluate(dist);
            
            float speedMultiplier = baseJumpAnimDuration / duration;
            animator.SetFloat("attackSpeed", speedMultiplier);
            animator.SetTrigger("isAttacking");

            //dotween jump
            transform.DOJump(jumpTragetPos, h, 1, duration, snapping: false)
            .SetEase(Ease.Linear) 
            .OnComplete(() => {
                animator.SetFloat("attackSpeed", 1f); 
                if(landEffect != null) Instantiate(landEffect, transform.position, Quaternion.identity);
                ChangeToState(BossState.Move);
                SoundEffect.Instance.PlayOneSound(landSE, 0.77f);
            });

        }

    }



    void CircleAttack_Enter()
    {
        stateCnt = 8.4f;
        attackCnt = 0.5f;
        attackTimes = 0;

        moveSpeed = 2.1f;

        SetInnerGlow();

        animator.SetBool("isMoving", false);
        animator.SetBool("isIdling", true);

    }
    void CircleAttack_Update()
    {

        //HomingPlayer();
        RotateTowardPlayer();

        if(attackCnt <= 0f)
        {
            

            //if attackTimes can be divided by 5
            if (attackTimes % 5 == 0 && attackTimes != 0)
            {
                attackCnt = 1.49f;
                attackTimes++;
                animator.SetTrigger("isAttacking");

                PlayerController playerCon = playerTrans.GetComponent<PlayerController>();
                Vector3 playerMove = playerCon.MoveVec;
                float predictionDistance = 3.5f; 
                Vector3 centerPos = playerTrans.position + playerMove * predictionDistance;

                EnemyEffectManager.Instance.SpawnAoeCircle(centerPos, 7.7f, 2.1f, 25f,false,false,false);

                //dotween delay called SpawnAoeCircleBigEffect:
                DOVirtual.DelayedCall(1.4f, () => {
                    SpawnAoeCircleBigEffect(centerPos);
                });

            }
            else
            {
                attackTimes++;
                attackCnt = 0.7f;

                //EnemyAnimUtil.LookAround(transform);
                animator.SetTrigger("isAttacking");

                PlayerController playerCon = playerTrans.GetComponent<PlayerController>();
                Vector3 playerMove = playerCon.MoveVec;
                float predictionDistance = 0.77f; 
                Vector3 centerPos = playerTrans.position + playerMove * predictionDistance;
                EnemyEffectManager.Instance.SpawnAoeCircle(centerPos, 4.2f, 2.1f, 20f);

                //dotween delay called SpawnAoeCircleEffect:
                 DOVirtual.DelayedCall(1.77f, () => {
                     SpawnAoeCircleEffect(centerPos);
                 });

            }

            
        }

        PlanStateChange(BossState.Move);
    }

    public void SpawnAoeCircleEffect(Vector3 pos) 
    {
        Instantiate(aoeEffect, pos, Quaternion.identity);
        SoundEffect.Instance.PlayOneSound(aoeEffectSe1, 0.42f);
    }

    public void SpawnAoeCircleBigEffect(Vector3 pos) 
    {
        GameObject obj = Instantiate(aoeEffect, pos, Quaternion.identity);
        obj.transform.localScale = Vector3.one * 1.4f;
        SoundEffect.Instance.PlayOneSound(aoeEffectSe1, 0.42f);
    }

    void SpawnAoeCircleEffect2(Vector3 pos)
    {
        GameObject obj = Instantiate(aoeEffect2, pos, Quaternion.identity);
        obj.transform.localScale = Vector3.one * 1f;
        SoundEffect.Instance.PlayOneSound(aoeEffectSe1, 0.07f);
    }

    void SpawnAoeCircleBigEffect2(Vector3 pos)
    {
        GameObject obj = Instantiate(aoeEffect2, pos, Quaternion.identity);
        obj.transform.localScale = Vector3.one * 1.7f;
        SoundEffect.Instance.PlayOneSound(aoeEffectSe1, 0.07f);
    }

    void SurroundComboAttack_Enter()
    {
        attackTimes = 0;
        stateCnt = 7.7f;
        attackCnt = 2.1f;

        animator.SetBool("isMoving", false);
        animator.SetBool("isIdling", true);

    }

    void SurroundComboAttack_Update()
    {
       RotateTowardPlayer();


        if(attackCnt <= 0)
        {
            if(attackTimes == 0)
            {
                attackTimes++;
                attackCnt = 1.14f;
                SpawnCircleAoe();

            }
            else if(attackTimes == 1)
            {
                attackTimes++;
                attackCnt = 2.1f;
                SpawnCross();

            }
            else if (attackTimes == 2)
            {
                ChangeToState(BossState.Move);
                attackTimes++;
                attackCnt = 2.8f;

            }
            else
            {
                ChangeToState(BossState.Move);
            }
        }

        

        



    }
    

    void SpawnCircleAoe()
    {
        animator.SetTrigger("isAttacking");

        Vector3 centerPos = playerTrans.position;
            for (int i = 0; i < numberOfCircles; i++)
            {
                
                float angle = i * (360.0f / numberOfCircles);

                float angleInRadians = angle * Mathf.Deg2Rad;

                
                float x = Mathf.Cos(angleInRadians);
                float z = Mathf.Sin(angleInRadians);
                
                Vector3 direction = new Vector3(x, 0f, z);
                Vector3 spawnPos = centerPos + direction * ringRadius;

                EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos, 4.9f, 1.5f, 10f);
                DOVirtual.DelayedCall(1.4f, () => {
                     SpawnAoeCircleBigEffect2(spawnPos);
                 });
        }
    }

    void SpawnCross()
    {
        animator.SetTrigger("isAttacking");

        PlayerController playerCon = playerTrans.GetComponent<PlayerController>();
            Vector3 playerMove = playerCon.MoveVec;
            float predictionDistance = 4.9f; 
             Vector3 centerPos = playerTrans.position ;

            float aoeSpacing = 2.8f;

            Vector3 diagonalDir1 = new Vector3(1f, 0f, 1f).normalized;
            Vector3 diagonalDir2 = new Vector3(-1f, 0f, 1f).normalized;
            for (int i = -2; i <= 2; i++)
            {
                Vector3 spawnPos = centerPos + diagonalDir1 * i * aoeSpacing;
                EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos, 3.5f, 1.5f, 10f);
                DOVirtual.DelayedCall(1.4f, () => {
                              SpawnAoeCircleEffect2(spawnPos);
                          });
            }

            for (int i = -2; i <= 2; i++)
            {
                if (i != 0)
                {
                    Vector3 spawnPos = centerPos + diagonalDir2 * i * aoeSpacing;
                    EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos, 3.5f, 1.5f, 10f);

                     DOVirtual.DelayedCall(1.4f, () => {
                          SpawnAoeCircleEffect2(spawnPos);
                      });
                }
            }




    }



    void CrossCircleAttack_Enter()
    {
        stateCnt = 7.7f;
        attackCnt = 3.5f;

    }

    void CrossCircleAttack_Update()
    {
        if (attackCnt <= 0f)
        {
            attackCnt = 3.5f;

            PlayerController playerCon = playerTrans.GetComponent<PlayerController>();
            Vector3 playerMove = playerCon.MoveVec;
            float predictionDistance = 4.9f; 
             Vector3 centerPos = playerTrans.position + playerMove * predictionDistance;

            float aoeSpacing = 2.8f;

            for (int i = -2; i <= 2; i++)
            {
                Vector3 spawnPos = centerPos + Vector3.forward * i * aoeSpacing;
                EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos, 3.5f, 1.5f, 10f);
            }

            for (int i = -2; i <= 2; i++)
            {
                Vector3 spawnPos = centerPos + Vector3.right * i * aoeSpacing;
                if (i != 0)
                {
                    EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos, 3.5f, 1.5f, 10f);
                }
            }

            //Vector3 diagonalDir1 = new Vector3(1f, 0f, 1f).normalized;
            //Vector3 diagonalDir2 = new Vector3(-1f, 0f, 1f).normalized;
            //for (int i = -2; i <= 2; i++)
            //{
            //    Vector3 spawnPos = centerPos + diagonalDir1 * i * aoeSpacing;
            //    EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos, 3.5f, 1.5f, 10f);
            //}

            //for (int i = -2; i <= 2; i++)
            //{
            //    if (i != 0)
            //    {
            //        Vector3 spawnPos = centerPos + diagonalDir2 * i * aoeSpacing;
            //        EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos, 3.5f, 1.5f, 10f);
            //    }
            //}

           
        }

        PlanStateChange(BossState.CircleCircleAttack);
    }


    private float triangleRadius = 7f;

    void TriangleCircleAttack_Enter()
    {
        stateCnt = 7.7f;
        attackCnt = 2.0f;

    }

    void TriangleCircleAttack_Update()
    {
        PlanStateChange(BossState.CrossCircleAttack);

        if (attackCnt <= 0f)
        {
            attackCnt = 3.5f;

             Vector3 centerPos = playerTrans.position;
            float aoeSpacing = 2.8f;
            float circleSize = 4.2f;

            //Vector3 forwardDir = (playerTrans.position - transform.position);
            //forwardDir.y = 0; 
            //forwardDir.Normalize();
            //Vector3 rightDir = new Vector3(forwardDir.z, 0, -forwardDir.x);

            Vector3 forwardDir = Vector3.forward; // World's "up" direction (0, 0, 1)
            Vector3 rightDir = Vector3.right;

             

            float height = triangleRadius * 1.5f; // Full height of the triangle
            float sideOffset = triangleRadius * Mathf.Sqrt(3) / 2;

            Vector3 vertexA = centerPos + forwardDir * triangleRadius;
            Vector3 baseCenter = centerPos - forwardDir * (triangleRadius / 2);
            Vector3 vertexB = baseCenter + rightDir * sideOffset;
            Vector3 vertexC = baseCenter - rightDir * sideOffset;

            for (int i = 0; i < 4; i++)
             {
                 Vector3 spawnPos = Vector3.Lerp(vertexC, vertexB, (float)i / 3.0f);
                 EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos, circleSize, 1.5f, 10f);
             }

            for (int i = 0; i < 4; i++)
            {
                Vector3 spawnPos = Vector3.Lerp(vertexB, vertexA, (float)i / 3.0f);
                EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos, circleSize, 1.5f, 10f);
            }

            for (int i = 0; i < 4; i++)
            {
                Vector3 spawnPos = Vector3.Lerp(vertexA, vertexC, (float)i / 3.0f);
                EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos, circleSize, 1.5f, 10f);
            }



        }

    }

    public float rectangleWidth = 7.7f;   // The width of the rectangle
    public float rectangleHeight = 7.7f;  // The height of the rectangle
    public int circlesOnWidth = 4;         // Number of circles on the top and bottom sides
    public int circlesOnHeight = 4;        // Number of circles on the left and right sides

    void RectangleCircleAttack_Enter()
    {
        stateCnt = 7.7f;
        attackCnt = 3.5f;

    }

    void RectangleCircleAttack_Update()
    {
        PlanStateChange(BossState.Move);

        if (attackCnt <= 0f)
        {
            attackCnt = 3.5f;

            Vector3 centerPos = playerTrans.position;
            float circleSize = 4.2f;

            //Vector3 forwardDir = (playerTrans.position - transform.position);
            //forwardDir.y = 0; 
            //forwardDir.Normalize();
            //Vector3 rightDir = new Vector3(forwardDir.z, 0, -forwardDir.x);  // The "right" direction is perpendicular, aligning with the rectangle's width

            Vector3 forwardDir = Vector3.forward; // World's "up" direction (0, 0, 1)
            Vector3 rightDir = Vector3.right;

             // 2. CALCULATE THE FOUR CORNER POINTS
             Vector3 halfWidthVec = rightDir * (rectangleWidth / 2.0f);
             Vector3 halfHeightVec = forwardDir * (rectangleHeight / 2.0f);

             Vector3 cornerTopRight    = centerPos + halfWidthVec + halfHeightVec;
             Vector3 cornerTopLeft     = centerPos - halfWidthVec + halfHeightVec;
             Vector3 cornerBottomRight = centerPos + halfWidthVec - halfHeightVec;
             Vector3 cornerBottomLeft  = centerPos - halfWidthVec - halfHeightVec;

             // 3. SPAWN CIRCLES ALONG THE SIDES

             // Top Side (spawns 'circlesOnWidth' circles)
             for (int i = 0; i < circlesOnWidth; i++)
             {
                 float t = (float)i / (circlesOnWidth - 1); // t goes from 0.0 to 1.0
                 Vector3 spawnPos = Vector3.Lerp(cornerTopLeft, cornerTopRight, t);
                 EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos, circleSize, 1.5f, 10f);
             }

             // Bottom Side (spawns 'circlesOnWidth' circles)
             for (int i = 0; i < circlesOnWidth; i++)
             {
                 float t = (float)i / (circlesOnWidth - 1);
                 Vector3 spawnPos = Vector3.Lerp(cornerBottomLeft, cornerBottomRight, t);
                 EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos, circleSize, 1.5f, 10f);
             }

             // Left Side (skips corners to avoid overlap, spawning 'circlesOnHeight - 2' circles)
             for (int i = 1; i < circlesOnHeight - 1; i++)
             {
                 float t = (float)i / (circlesOnHeight - 1);
                 Vector3 spawnPos = Vector3.Lerp(cornerBottomLeft, cornerTopLeft, t);
                 EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos, circleSize, 1.5f, 10f);
             }

             // Right Side (skips corners, spawning 'circlesOnHeight - 2' circles)
             for (int i = 1; i < circlesOnHeight - 1; i++)
             {
                 float t = (float)i / (circlesOnHeight - 1);
                 Vector3 spawnPos = Vector3.Lerp(cornerBottomRight, cornerTopRight, t);
                 EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos, circleSize, 1.5f, 10f);
             }


        }

    }

    public int numberOfCircles = 12;   // How many AOE circles will form the ring
    public float ringRadius = 10.0f; 

    void CircleCircleAttack_Enter()
    {
        stateCnt = 7.7f;
        attackCnt = 1.0f;

    }

    void CircleCircleAttack_Update()
    {
        PlanStateChange(BossState.Move);

        if (attackCnt <= 0f)
        {
            attackCnt = 1.0f;

            Vector3 centerPos = playerTrans.position;
            for (int i = 0; i < numberOfCircles; i++)
            {
                
                float angle = i * (360.0f / numberOfCircles);

                float angleInRadians = angle * Mathf.Deg2Rad;

                
                float x = Mathf.Cos(angleInRadians);
                float z = Mathf.Sin(angleInRadians);
                
                Vector3 direction = new Vector3(x, 0f, z);
                Vector3 spawnPos = centerPos + direction * ringRadius;

                EnemyEffectManager.Instance.SpawnAoeCircle(spawnPos, 4.2f, 1.5f, 10f);
        }
         

        }

    }


    void RotatePillarAttack_Enter()
    {
        stateCnt = 7.7f;
        attackCnt = 0.5f;

    }

    void RotatePillarAttack_Update()
    {
        //self-rotate around centerPos
        transform.RotateAround(transform.position, Vector3.up, 21f * Time.deltaTime);

        PlanStateChange(BossState.Move);

    }



    //==========================Utility&Helper===================================//
    public void ChangeToState(BossState newState)
     {
         stateMachine.ChangeState(newState);
         stateInfoText = stateMachine.State.ToString(); // 状態名を文字列に変換
     }
     public void ChangeToMoveState() => stateMachine.ChangeState(BossState.Move);

     public void ChangeToDeath()
     {
         ChangeToState(BossState.Death);
     }

     void PlanStateChange(BossState targetState)
     {
        if(stateCnt <= 0)
         {
             ChangeToState(targetState);
         }
     }
    
     public void UpdateStateInfo()
     {
         stateInfoText = stateMachine.State.ToString();
         stateCnt -= Time.deltaTime;
     }

     public void UpdateGobalCounter()
     {
         attackCnt -= Time.deltaTime;
         turningCnt -= Time.deltaTime;
     }

}

