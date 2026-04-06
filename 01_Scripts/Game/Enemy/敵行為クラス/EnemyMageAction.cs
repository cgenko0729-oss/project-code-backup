using MonsterLove.StateMachine; //StateMachine
using QFSW.MOP2;
using UnityEngine;


public class EnemyMageAction : EnemyStateActionBase
{

    public float distNeedToMagicAttack = 14f;

    public GameObject atkMagicObj;

    //EnemyStatusBase enemyStatus;

    public bool hasAttacked = false;
    public bool hasArrived = false;

    public int attackTime = 3;
    public int attackMinTime = 1;
    public int attackMaxTime = 3;
    public float attackInterval = 3.5f;
    public float attackIntervalMax = 3.5f;

    private Vector3 wanderTarget;

    public ObjectPool bulletPool;

    protected override void Awake()
    {
        enemyStatus = GetComponent<EnemyStatusBase>();
        stateMachine = StateMachine<EnemyState>.Initialize(this);
        stateMachine.ChangeState(EnemyState.Move);
        GetPlayerInfo();
        GetAnimator();
        InitSeperationInfo();

        

    }

    private void OnEnable()
    {
        stateMachine.ChangeState(EnemyState.Move);  //no double init, only change state
        GetPlayerInfo();
        GetAnimator();
        InitSeperationInfo();
    }

    protected override void Update()
    {
        GetStateInfo();
        UpdateStateCnt();
        CalDistToPlayer();




    }

    public void Move_Enter()
    {
        if (animator)
        {
            //animator.SetTrigger("isMove");
            animator.SetBool("isMoving", true);
        }
        stateCnt = 4f;
        hasArrived = false;
    }

    public void Move_Update()
    {
        //EnemySimpleHoming();

        if(distToPlayer > distNeedToMagicAttack) transform.position = Vector3.MoveTowards(transform.position, playerTrans.position, enemyStatus.enemyMoveSpd * Time.deltaTime);
        else if(distToPlayer <= distNeedToMagicAttack && !hasArrived)
        {
            hasArrived = true;
            stateCnt = 1.5f;
            animator.SetTrigger("isAttack");
        }
        EnemyRotation();

        if (hasArrived && stateCnt <= 0)
        {
            stateMachine.ChangeState(EnemyState.SkillAttack);
        }
    }

    void Move_Exit()
    {
        animator.SetBool("isMoving", false);
    }

    public void SkillAttack_Enter()
    {
        stateCnt = 3f;
        hasAttacked = false;
        animator.SetTrigger("isAttack"); 

        attackTime = Random.Range(attackMinTime, attackMaxTime);
        attackInterval = attackIntervalMax;

    }

    public void SkillAttack_Update()
    {

        EnemyRotation();
        attackInterval -= Time.deltaTime * enemyStatus.iceDebuffFactor * enemyStatus.poisonSpeedDownFactor;

        if(attackInterval <= 0 && attackTime > 0)
        {
            attackTime--;
            attackInterval = attackIntervalMax;
            MagicAttack();

        }

        if(attackInterval <= 0 && attackTime <= 0)
        {
            if(distToPlayer > distNeedToMagicAttack)stateMachine.ChangeState(EnemyState.Move);
            else stateMachine.ChangeState(EnemyState.Wander);
        }

        

    }

    public void Wander_Enter()
    {
        hasArrived = false;
        stateCnt = 5f;
        PickWanderTarget();
        if (animator)
        {
            //animator.SetTrigger("isMove");
            animator.SetBool("isMoving", true);
        }
    }

    public void Wander_Update()
    {
        //with player as a center of circle , distNeedToMagicAttack as radius, find a random point on the margin of the circle ,set it as the moving target , this point must be on same side of player to mage , for example , if mage is on the left side of player, then the random point must be on the left side of player too

        transform.position = Vector3.MoveTowards(transform.position, wanderTarget, enemyStatus.enemyMoveSpd * Time.deltaTime * enemyStatus.iceDebuffFactor * enemyStatus.poisonSpeedDownFactor);

        if (!hasArrived) EnemyRotationFaceTarget(wanderTarget);
        else EnemyRotation();

        if (!hasArrived && Vector3.Distance(transform.position, wanderTarget) < 0.2f)
        {
            hasArrived = true;
            animator.SetTrigger("isAttack");
            stateCnt = 1f;
        }

        if (hasArrived && stateCnt <= 0f)
        {
            stateMachine.ChangeState(EnemyState.SkillAttack);
        }
    }

    void Wander_Exit()
    {
        animator.SetBool("isMoving", false);
    }

    private void PickWanderTarget()
    {
        Vector3 playerPos = playerTrans.position;
        playerPos.y = transform.position.y;

        bool mageOnLeft = (transform.position.x < playerPos.x);
        //bool mageOnLeft = Vector3.Dot(playerTrans.right, transform.position - playerPos) < 0f;

        float radius = distNeedToMagicAttack;

        float angle;
        if (mageOnLeft)
        {
            angle = Random.Range(Mathf.PI * 0.5f, Mathf.PI * 1.5f);
        }
        else
        {
            angle = Random.Range(-Mathf.PI * 0.5f, Mathf.PI * 0.5f);
        }

        float x = playerPos.x + radius * Mathf.Cos(angle);
        float z = playerPos.z + radius * Mathf.Sin(angle);
        Vector3 candidate = new Vector3(x, transform.position.y, z);

        wanderTarget = candidate;
    }


    

    public void SkillAttack_Exit()
    {

    }

    public void MagicAttack()
    {
        if (distToPlayer <= 5.6)
        {
            attackInterval = 0.07f;
            return;
        }

        animator.SetTrigger("isCast");

        //GameObject atkObj = Instantiate(atkMagicObj, transform.position, transform.rotation);
        
        EffectLifeController atkObj = bulletPool.GetObjectComponent<EffectLifeController>(transform.position, transform.rotation);
        atkObj.lifeCnt = atkObj.lifeCntMax;
        EffectMoveController effect = atkObj.GetComponent<EffectMoveController>();

        Vector3 DirToPlayer = (playerTrans.position - transform.position).normalized;

        effect.moveVec = DirToPlayer;

        SoundEffect.Instance.Play(SoundList.EnemyMageShotSe);

        if (attackTime>0) Invoke("SetAttackReadyAni", 0.5f);

    }

    public void SetAttackReadyAni()
    {
        animator.SetTrigger("isAttack");
    }


}

