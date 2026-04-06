using MonsterLove.StateMachine; //StateMachine
using System.Collections;
using System.Collections.Generic;
using TigerForge;               //EventManager
using TMPro;
using UnityEngine;

public abstract class ActivePetActionBase : MonoBehaviour
{
    public enum PetActionStates
    {
        Walk,             //�������
        Idle,             //�ҋ@���
        HomingEnemy,      //�G��ǂ���������
        Attack,           //�U�����
        Run,              //������
        Lose,             //�s�k���
        Victory,          //�������
        IdleMotion,       //�ҋ@���[�V����
        ActiveSkillMotion,//�A�N�e�B�u�X�L�����[�V����
        Warp              //���[�v���(�����p)
    }

    [Header("���ʃX�e�[�^�X")]

    [Space]

    [Header("���̃y�b�g�̑��_���[�W��")]
    [SerializeField] protected float takeDamagesTotal = 0f;

    [Space]

    [Header("�y�b�g�̃f�[�^")]
    [SerializeField] protected PetData petData;

    [Header("���s���x")]
    [SerializeField] protected float moveSpeed_Walk = 0.0f;

    [Header("���s���x")]
    [SerializeField] protected float moveSpeed_Run = 12.0f;

    [Header("�U���͈�")]
    [SerializeField] protected float attackRange = 5.5f;

    [Header("�U���ړ��͈�")]
    [SerializeField] protected float attackMoveRange = 1.5f;

    [Header("�U���N�[���^�C��")]
    [SerializeField] protected float attackCooldown = 2.0f;


    [Header("�U������p�R���C�_�[")]
    [SerializeField] protected Collider petAttackCol;

    [Header("�U�����̃T�E���h")]
    [SerializeField] protected SoundList attackSound;

    [Header("�A�N�e�B�u�X�L�����[�V�����̃N�[���^�C��")]
    [SerializeField] protected float ASMCoolTime = 0.0f;

    [Header("�X�L����������SE")]
    [SerializeField] protected SoundList skillEffectSound;

    [Header("���݂̍U�����x")]
    [SerializeField] protected float currentAttackSpeedMultiplier = 1.0f;

    [Space]

    [Header("���ʂ̋����ݒ�")]
    [Header("�����A�N�V�����Ɉڍs���鋗��")]
    [SerializeField] protected float walkDist = 3.0f;

    [Header("�ҋ@�A�N�V�����Ɉڍs���鋗��")]
    [SerializeField] protected float idleDist = 1.0f;

    [Header("����A�N�V�����Ɉڍs���鋗��")]
    [SerializeField] protected float runDist = 7.0f;

    [Header("�u�Ԉړ��A�N�V�����Ɉڍs���鋗��")]
    [SerializeField] protected float warpDist = 20.0f;

    protected StateMachine<PetActionStates> sm;
    protected Animator petAnimator;
    protected Transform playerTransform;
    protected PlayerState playerStatus;
    protected Transform enemyTransform;
    protected GameObject nearestEnemy;

    //�t�H�[���[�V�����p�̃I�t�Z�b�g�ʒu
    protected Vector3 formationOffset = Vector3.zero;

    protected float playerToDist;
    public float stateCnt = 3f;

    protected bool lookingAtEnemies = true; // �G�����Ă��邩�ǂ����̃t���O

    protected float stateZeroCnt = 0f; // �X�e�[�g�̃J�E���^�[�i�������p�j

    protected float searchTimer = 1f; // �G�̍��G�^�C�}�[

    //�U�����̃N�[���^�C���Ǘ��p
    protected float attackSoundCoolTime = 0f;

    //attackCooldown�̒l�������̂ŁA���̒l��ۑ����Ă���
    public float attackCooldownOriginal = 0f;

    //�U�����̃N�[���^�C��(���񖈂Ɉ�x�o����)
    protected float attackSoundCoolMax = 3f;

    //���ɃX�L�����g�p�\�ɂȂ鎞��
    protected float ResetCoolTime;

    //�A�N�e�B�u�X�L���̎�������
    protected float activeSkillDurationTime = 0;

    //�X�L�����A�N�e�B�u���ǂ���
    protected bool skillActive = false;

    //�^����_���[�W��(�����Ȃ��Ă���)
    [HideInInspector]
    public float takeDamages = 0f;

    //�U���񐔃J�E���^�[
    protected int attackCounter = 3;

    //���Ȕj��p�R���[�`��
    protected Coroutine selfDestructCoroutine = null;

    //�U���\�ȍő卂�x��
    protected float maxAttackHeightDifference = 2.5f;

    //�������I���W�i�����N���[����
    [SerializeField] protected PetCloneType whoIam = PetCloneType.Original;

    protected float pendingAttackMultiplier = 1.0f;

    protected virtual void OnEnable()
    {
        EventManager.StartListening("isGameClear", OnVictory);
        EventManager.StartListening("isGameOver", OnLose);
        EventManager.StartListening(GameEvent.AllAttackStart, CloneActiveSkill);
    }

    protected virtual void OnDisable()
    {
        EventManager.StopListening("isGameClear", OnVictory);
        EventManager.StopListening("isGameOver", OnLose);
        EventManager.StopListening(GameEvent.AllAttackStart, CloneActiveSkill);
    }

    protected virtual void Awake()
    {  
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            //�v���C���[��Transform���擾
            playerTransform = playerObj.transform;

            //�v���C���[�X�e�[�^�X�R���|�[�l���g���擾
            playerStatus = playerObj.GetComponent<PlayerState>();

            //�R���|�[�l���g���擾�ł��Ȃ���΁A�������I��
            if (playerStatus == null)
            {
                this.enabled = false;
                return;
            }
        }
        else
        {
            this.enabled = false;
            return;
        }

        //�X�e�[�g�}�V���̏�����
        sm = StateMachine<PetActionStates>.Initialize(this);

        //������
        petAnimator = GetComponent<Animator>();

        //attackCooldown�̌��̒l��ۑ����Ă���
        attackCooldownOriginal = attackCooldown;
    }

    protected virtual void Start()
    {
        if (petData != null)
        {
            this.takeDamages = petData.attackPower * pendingAttackMultiplier;
        }
        else
        {
            Debug.LogWarning("PetData���ݒ肳��Ă��܂���I", this.gameObject);
        }

        sm.ChangeState(PetActionStates.Idle);       
    }

    protected virtual void Update()
    {
        if (sm.State == PetActionStates.Lose || sm.State == PetActionStates.Victory) return;

        if (playerTransform != null)
        {
            Vector3 targetDestination = playerTransform.position + formationOffset;
            playerToDist = Vector3.Distance(transform.position, targetDestination);
        }

        //�G�̍��G�^�C�}�[���X�V
        searchTimer -= Time.deltaTime;

        //�^�C�}�[��0�ȉ��ɂȂ�����G���ēx���G
        if (searchTimer <= 0f)
        {
            nearestEnemy = FindNearestEnemy();
            searchTimer  = 1f; // �^�C�}�[�����Z�b�g
        }

        //���זh�~�@
        if (attackSoundCoolTime > 0f)
        {
            attackSoundCoolTime -= Time.deltaTime;
        }

        if(stateCnt > stateZeroCnt)
        {
            stateCnt -= Time.deltaTime;
        }
    }

    #region --�e�X�e�[�g--
    //Walk-----------------------------------------
    protected virtual void Walk_Enter() //���̃X�e�[�g�ɓ���������񂾂����s
    {
        petAnimator.SetBool("isWalking", true);
        petAnimator.SetBool("isIdle", false);
        petAnimator.SetBool("isRunning", false);
    }

    protected virtual void Walk_Update() //�X�e�[�g�������Ǝ��s
    {
        // �v���C���[�Ƃ̋����𑪒�
        if (playerToDist > runDist)
        {
            // �v���C���[�Ƃ̋�����runDist�ȏ�Ȃ瑖��
            sm.ChangeState(PetActionStates.Run);
            return;
        }
        if (playerToDist < runDist)
        {
            if (nearestEnemy != null)
            {
                // �߂��ɓG�����邩�`�F�b�N
                if (petData.petRoles == null || !petData.petRoles.Contains(PetRole.NoAttack))
                {

                    float dist = Vector3.Distance(transform.position, nearestEnemy.transform.position);
                    if (dist <= attackRange)
                    {
                        // �߂��̓G���U���͈͓��Ȃ�G�Ǐ]��ԂɑJ��
                        sm.ChangeState(PetActionStates.HomingEnemy);
                        return;
                    }
                }
            }
        }

        //�v���C���[�Ƃ̋�����walkDist�ȏ�Ȃ�z�[�~���O
        if (playerToDist > walkDist)
        {
            if (playerTransform)
            {
                Homing(playerTransform, moveSpeed_Walk);
                return;
            }
        }
        // �v���C���[�Ƃ̋�����walkDist�ȉ�����idleDist���傫���Ȃ�ҋ@��ԂɑJ��
        if (playerToDist <= walkDist && playerToDist > idleDist)
        {
            sm.ChangeState(PetActionStates.Idle);
            return;
        }
    }

    protected virtual void Walk_Exit() //�X�e�[�g���痣�ꂽ����񂾂����s
    {

    }
    //----------------------------------------------

    //Idle------------------------------------------
    protected virtual void Idle_Enter()
    {
        petAnimator.SetBool("isWalking", false);
        petAnimator.SetBool("isIdle", true);
        petAnimator.SetBool("isRunning", false);

        stateCnt = 3f; // �ҋ@��Ԃ̃J�E���^�[�����Z�b�g
    }

    protected virtual void Idle_Update()
    {
        // �v���C���[�Ƃ̋����𑪒�
        if (nearestEnemy != null)
        {
            if (petData.petRoles == null || !petData.petRoles.Contains(PetRole.NoAttack))
            {
                // ���G�͈͓��ɓG������΁A�����ɒǐՂ��J�n
                if (Vector3.Distance(transform.position, nearestEnemy.transform.position) <= attackRange)
                {
                    sm.ChangeState(PetActionStates.HomingEnemy);
                    return;
                }
            }
        }
        if (playerToDist > runDist)
        {
            // �v���C���[�Ƃ̋�����runDist�ȏ�Ȃ瑖��
            sm.ChangeState(PetActionStates.Run);
            return;
        }
        if (playerToDist > walkDist)
        {
            // �v���C���[�Ƃ̋�����walkDist�ȏ�Ȃ����
            sm.ChangeState(PetActionStates.Walk);
            return;
        }

        if (stateCnt <= stateZeroCnt)
        {
            // �ҋ@��Ԃ̃J�E���^�[��0�ȉ��ɂȂ�����ҋ@���[�V�����ɑJ��
            sm.ChangeState(PetActionStates.IdleMotion);
            return;
        }
    }
    protected virtual void Idle_Exit()
    {

    }
    //-----------------------------------------------

    //HomingEnemy------------------------------------------
    protected virtual void HomingEnemy_Enter()
    {
        petAnimator.SetBool("isWalking", false);
        petAnimator.SetBool("isIdle", false);
        petAnimator.SetBool("isRunning", true);
    }

    protected virtual void HomingEnemy_Update()
    {
        // �ǐՑΏۂ̓G�����Ȃ��inull�ɂȂ����j�ꍇ�A�v���C���[��ǂ���Ԃɖ߂�
        if (nearestEnemy == null || enemyTransform == null)
        {
            // �v���C���[�Ƃ̋����ɉ�����Walk��Run��ԂɑJ�ڂ���
            if (playerToDist > runDist)
            {
                sm.ChangeState(PetActionStates.Run);
            }
            else
            {
                sm.ChangeState(PetActionStates.Walk);
            }
            return; // �G�����Ȃ��̂ŁA�ȍ~�̏����͍s��Ȃ�
        }

        //�G��Transform��null�łȂ����Ƃ��m�F
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
            sm.ChangeState(PetActionStates.Attack);
            return;
        }

        // �ǐՒ��ɓG�����G�͈́iattackRange�j�̊O�ɏo�Ă��܂����ꍇ���A�v���C���[��ǂ���Ԃɖ߂�
        if (dist > attackRange)
        {
            // �v���C���[�Ƃ̋����ɉ�����Walk��Run��ԂɑJ�ڂ���
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
    protected virtual void HomingEnemy_Exit()
    {

    }
    //-----------------------------------------------


    //Run------------------------------------------
    protected virtual void Run_Enter()
    {
        petAnimator.SetBool("isWalking", false);
        petAnimator.SetBool("isIdle", false);
        petAnimator.SetBool("isRunning", true);
    }

    protected virtual void Run_Update()
    {
        // �v���C���[�Ƃ̋����𑪒�
        if (playerTransform)
        {
            Homing(playerTransform, moveSpeed_Run);
        }

        if (playerToDist <= walkDist)
        {
            // �v���C���[�Ƃ̋�����walkDist�ȉ��Ȃ������ԂɑJ��
            sm.ChangeState(PetActionStates.Walk);
            return;
        }
    }
    protected virtual void Run_Exit()
    {

    }
    //-----------------------------------------------


    //Attack------------------------------------------
    protected virtual void Attack_Enter()
    {
        petAnimator.SetBool("isWalking", false);
        petAnimator.SetBool("isIdle", false);
        petAnimator.SetBool("isRunning", false);
        petAnimator.SetTrigger("isAttack");

        petAnimator.speed = this.currentAttackSpeedMultiplier;

        lookingAtEnemies = true; // �G�����Ă����Ԃɂ���
        stateCnt = attackCooldown;
    }

    protected virtual void Attack_Update()
    {
        // �v���C���[�Ƃ̋����𑪒�
        if (enemyTransform)
        {
            if (lookingAtEnemies)
            {
                LookAt(enemyTransform);
            }
        }

        if (stateCnt <= stateZeroCnt)
        {

            //���g�̃R���C�_�[������
           DisableAttackCollider();

            if (playerToDist > runDist)
            {
                // �v���C���[�Ƃ̋�����runDist�ȏ�Ȃ瑖��
                sm.ChangeState(PetActionStates.Run);
                return;
            }
            if (playerToDist > walkDist)
            {
                // �v���C���[�Ƃ̋�����walkDist�ȏ�Ȃ����
                sm.ChangeState(PetActionStates.Walk);
                return;
            }
            if (playerToDist <= idleDist)
            {
                // �v���C���[�Ƃ̋�����idleDist�ȉ��Ȃ�ҋ@��ԂɑJ��
                sm.ChangeState(PetActionStates.Idle);
                return;
            }

            if (nearestEnemy != null)
            {
                float dist = Vector3.Distance(transform.position, nearestEnemy.transform.position);
                if (dist >= attackMoveRange)
                {
                    //�߂��̓G���U���͈͓��Ȃ�G�Ǐ]��ԂɑJ��
                    sm.ChangeState(PetActionStates.HomingEnemy);
                    return;
                }
                else
                {
                    sm.ChangeState(PetActionStates.Attack, StateTransition.Overwrite);
                }
            }

            lookingAtEnemies = true; // �G�����Ă����Ԃɂ���
            stateCnt = attackCooldown; // �ēx�N�[���_�E���^�C�}�[�����Z�b�g
        }
    }
    protected virtual void Attack_Exit()
    {
        petAnimator.speed = 1.0f;
    }
    //-----------------------------------------------

    //Lose------------------------------------------
    protected virtual void Lose_Enter()
    {
        petAnimator.SetBool("isWalking", false);
        petAnimator.SetBool("isIdle", false);
        petAnimator.SetBool("isRunning", false);
        petAnimator.SetBool("isLoser", true);
    }

    protected virtual void Lose_Update()
    {

    }
    protected virtual void Lose_Exit()
    {

    }
    //-----------------------------------------------

    //Victory------------------------------------------
    protected virtual void Victory_Enter()
    {
        petAnimator.SetBool("isWalking", false);
        petAnimator.SetBool("isIdle", false);
        petAnimator.SetBool("isRunning", false);
        petAnimator.SetBool("isVictory", true);
    }

    protected virtual void Victory_Update()
    {
        LookAt(playerTransform);
    }
    protected virtual void Victory_Exit()
    {

    }
    //-----------------------------------------------

    //IdleMotion------------------------------------------
    protected virtual void IdleMotion_Enter()
    {
        petAnimator.SetBool("isIdle", false);
        petAnimator.SetTrigger("IdleMotion");
    }

    protected virtual void IdleMotion_Update()
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
                        // �߂��̓G���U���͈͓��Ȃ�G�Ǐ]��ԂɑJ��
                        sm.ChangeState(PetActionStates.HomingEnemy);
                        return;
                    }
                }
            }
        }
        if (playerToDist > runDist)
        {
            // �v���C���[�Ƃ̋�����runDist�ȏ�Ȃ瑖��
            sm.ChangeState(PetActionStates.Run);
            return;
        }
        if (playerToDist > walkDist)
        {
            // �v���C���[�Ƃ̋�����walkDist�ȏ�Ȃ����
            sm.ChangeState(PetActionStates.Walk);
            return;
        }
    }
    protected virtual void IdleMotion_Exit()
    {

    }
    //-----------------------------------------------

    //ActiveSkillMotion------------------------------------------
    protected virtual void ActiveSkillMotion_Enter()
    {
        petAnimator.SetBool("isWalking", false);
        petAnimator.SetBool("isIdle", false);
        petAnimator.SetBool("isRunning", false);
        petAnimator.SetTrigger("ActiveSkillMotion");
    }

    protected virtual void ActiveSkillMotion_Update()
    {
     
    }
    protected virtual void ActiveSkillMotion_Exit()
    {

    }
    //-----------------------------------------------

    //Warp------------------------------------------
    protected virtual void Warp_Enter()
    {
        Vector3 WarpPos = playerTransform.position + new Vector3(3f, 0f, 0f);
        transform.position = WarpPos;

        petAnimator.SetBool("isWalking", false);
        petAnimator.SetBool("isIdle", false);
        petAnimator.SetBool("isRunning", false);
        petAnimator.SetTrigger("Warp");
    }

    protected virtual void Warp_Update()
    {
        
    }
    protected virtual void Warp_Exit()
    {

    }
    //-----------------------------------------------

    #endregion --�e�X�e�[�g�I��--

    protected virtual void Homing(Transform target, float moveSpeed)
    {
        // �v���C���[��Transform��null�łȂ����Ƃ��m�F
        if (target == null) return;

        Vector3 targetPosition = Vector3.zero;

        //�G�̏ꍇ�ƃv���C���[�̏ꍇ�ŖڕW�ʒu�𕪂���
        if (target == enemyTransform)
        {
            // �t�H�[���[�V�����I�t�Z�b�g���l�������ڕW�ʒu���v�Z
            targetPosition = target.position;
        }
        if(target==playerTransform)
        {
            // �t�H�[���[�V�����I�t�Z�b�g���l�������ڕW�ʒu���v�Z
            targetPosition = target.position + formationOffset;
        }

        // �^�[�Q�b�g�Ƃ́u�n�ʂ����̋����v���v�Z����
        Vector3 targetPositionOnGround = targetPosition;
        Vector3 myPositionOnGround = transform.position;
        targetPositionOnGround.y = 0;
        myPositionOnGround.y = 0;
        float groundDistance = Vector3.Distance(myPositionOnGround, targetPositionOnGround);

        if (groundDistance <= attackMoveRange)
        {
            //�ړ����X�g�b�v����
            LookAt(target);
            return;
        }

        //�v���C���[�̕������v�Z
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0;

        direction = direction.normalized;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

        //��]����x�擾���ďC����ɍĐݒ�
        Vector3 fixedEulerAngles = transform.rotation.eulerAngles;
        fixedEulerAngles.x = 0; // X���̉�]���Œ�
        fixedEulerAngles.z = 0; // Z���̉�]���Œ�
        transform.rotation = Quaternion.Euler(fixedEulerAngles);

        // �v���C���[�̕����Ɍ������Ĉړ�
        transform.position += direction * Time.deltaTime * moveSpeed; // �ړ����x�͒����\
    }

    protected virtual void LookAt(Transform target)
    {
        // �^�[�Q�b�g��null�łȂ����Ƃ��m�F
        if (target == null) return;

        // �^�[�Q�b�g�̕������v�Z
        Vector3 direction = (target.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // �^�[�Q�b�g�̕����Ɍ����ĉ�]
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);

        //��]����x�擾���ďC����ɍĐݒ�
        Vector3 fixedEulerAngles = transform.rotation.eulerAngles;
        fixedEulerAngles.x = 0; // X���̉�]���Œ�
        fixedEulerAngles.z = 0; // Z���̉�]���Œ�
        transform.rotation = Quaternion.Euler(fixedEulerAngles);
    }

    protected virtual GameObject FindNearestEnemy()
    {
        // ���G���������C���[���܂Ƃ߂����C���[�}�X�N���쐬
        int enemyMask = LayerMask.GetMask("EnemySpider", "EnemyMage", "EnemyDragon", "EnemyBossSpider", "EnemyMushroom");

        // �y�b�g�̈ʒu����attackRange�͈͓̔��ɂ���A�w�肵�����C���[�̃R���C�_�[��S�Ď擾
        Collider[] enemiesInAttackRange = Physics.OverlapSphere(transform.position, attackRange, enemyMask);

        // �͈͓��ɓG����l�����Ȃ���΁A���������ɏ������I��
        if (enemiesInAttackRange.Length == 0)
        {
            enemyTransform = null;
            return null;
        }

        GameObject closestEnemy = null;
        float minSqrDistance = Mathf.Infinity;

        // ���������R���C�_�[�̃��X�g�����[�v���āA�ǂꂪ��ԋ߂������ׂ�
        foreach (Collider enemyCollider in enemiesInAttackRange)
        {
            // �R���C�_�[����A���̐e�ł���Q�[���I�u�W�F�N�g�̈ʒu�܂ł̋������v�Z�i������2�拗���Łj
            float sqrDist = (transform.position - enemyCollider.transform.position).sqrMagnitude;

            // �����A���܂Ō������ǂ̓G�����������߂����
            if (sqrDist < minSqrDistance)
            {
                // �������V�����u�ł��߂��G�v�Ƃ��ċL������
                minSqrDistance = sqrDist;
                closestEnemy = enemyCollider.gameObject; // �R���C�_�[����Q�[���I�u�W�F�N�g�{�̂��擾
            }
        }

        // ���[�v���I��������_�ŁAclosestEnemy�ɍł��߂��G�������Ă���
        // �G��Transform���X�V
        enemyTransform = (closestEnemy != null) ? closestEnemy.transform : null;

        // �ł��߂��G�̃Q�[���I�u�W�F�N�g��Ԃ�
        return closestEnemy;
    }

    protected virtual void OnLose()
    {
        // �s�k�����Ƃ��̏�����ǉ�
        sm.ChangeState(PetActionStates.Lose);
    }

    protected virtual void OnVictory()
    {
        // ���������Ƃ��̏�����ǉ�
        sm.ChangeState(PetActionStates.Victory);
    }

    protected virtual void OnTriggerEnter(Collider col)
    {
        if (!col.CompareTag("Enemy")) return;

        //�G�̃X�e�[�^�X�R���|�[�l���g���擾����
        EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();

        //�R���|�[�l���g���擾�ł��Ȃ���΁A�������I��
        if (enemyStat == null) return;

        //�G�Ƀ_���[�W��^���鏈�������s
        OnTakeDamegesEnemy(enemyStat);

        //�U���C�x���g���Z�b�g
        SetPetAttackEvent();

        //�G�ɓ��������Ƃ��̏��������s
        OnHitEnemy(enemyStat);

        //�U���C�x���g���Z�b�g
        SetPetAttackEvent();

        //3�񖈂�1�x�A�U������炷
        PlayAttackSound();

        //�U������p�̃R���C�_�[�𖳌�������
        DisableAttackCollider();
    }

    protected virtual void OnTakeDamegesEnemy(EnemyStatusBase enemyStat)
    {
        //�G�Ƀ_���[�W��^����

        //float nowEnemyHP = enemyStat.enemyHp;
        //float petAttackPower = takeDamages;
        //bool isEnemydead = false;
        //if (nowEnemyHP - petAttackPower <= 0f)
        //{
        //    isEnemydead = true;
        //}

        FinalDanages(enemyStat, takeDamages);

        takeDamagesTotal += takeDamages;
    }

    //�ŏI�_���[�W
    public void FinalDanages(EnemyStatusBase enemyStat,float finalDamages)
    {
        bool isPetDamageDouble = SkillEffectManager.Instance.universalTrait.isPetGetStronger;

        if (isPetDamageDouble)
        {
            enemyStat.TakeDamage(finalDamages * 2f,isShowDamageNumber: true, skillType: SkillIdType.Pet);
        }
        else enemyStat.TakeDamage(finalDamages,isShowDamageNumber:true,skillType:SkillIdType.Pet);

        takeDamagesTotal += finalDamages;
    }

    //�ŏI�_���[�W�i���X�g�A�^�b�N�^�C�v�t���j
    public void FinalDanages(EnemyStatusBase enemyStat,float finalDamages,LastAttackType lastAttack)
    {
        bool isPetDamageDouble = SkillEffectManager.Instance.universalTrait.isPetGetStronger;

        if (isPetDamageDouble)
        {
            enemyStat.TakeDamage(finalDamages * 2f, isShowDamageNumber: true, skillType: SkillIdType.Pet, LastAttack: lastAttack);
        }
        else enemyStat.TakeDamage(finalDamages, isShowDamageNumber: true, skillType: SkillIdType.Pet, LastAttack: lastAttack);

        takeDamagesTotal += finalDamages;
    }

    protected virtual void OnHitEnemy(EnemyStatusBase enemyStat)
    {
        //�e�N���X�ł͉������Ȃ��B�q�N���X�����R�ɏ㏑�����邽�߂̋�̃��\�b�h�B
    }

  
    //�U������p�̃R���C�_�[�𖳌�������w���p�[�֐�
    protected virtual void DisableAttackCollider()
    {
        if (petAttackCol != null)
        {
            petAttackCol.isTrigger = false;
        }
    }

    //�U�����]�b�g
    protected virtual void PetAttackAction()
    {
        //null�`�F�b�N
        if (enemyTransform == null) { return; }

        //�G�����Ă��Ȃ���Ԃɂ���
        lookingAtEnemies = false;

        //���g�̃R���C�_�[������
        if (petAttackCol != null)
        {
            petAttackCol.isTrigger = true; // �R���C�_�[��t����
        }
    }

    //�N�[���^�C�������Z�b�g
    public void ResetCoolDown()
    {
        if(petData == null) { return; }

        bool isPetDamageDouble = SkillEffectManager.Instance.universalTrait.isPetGetStronger;

        if (isPetDamageDouble)
        {
            //�X�L���g�p��A�N�[���_�E�����J�n����
            petData.activeSkillRemainingCooldown = petData.activeSkillTotalCooldown / 2f;
        }
        else petData.activeSkillRemainingCooldown = petData.activeSkillTotalCooldown;
    }
    //�N�[���^�C�����Z�b�g
    public void SetCoolDown()
    {
        if (petData == null) { return; }

        petData.activeSkillRemainingCooldown = ResetCoolTime;
    }

    //�N�[���^�C����ϓ�������
    public void ChangeCoolDown()
    {
        if (petData == null) { return; }

        //������1,2,3�Ԗڂ̃y�b�g���m�F
        int petIndex = ActivePetManager.Instance.activePets.IndexOf(this.gameObject);

        //�Ⴄ�Ȃ珈�����I��
        if (petIndex == -1|| petIndex > 2) { return; }

        //����petdata�̃A�N�e�B�u�X�L���̃N�[���^�C�����ύX����Ă�����A���ɖ߂�
        if (petData.activeSkillRemainingCooldown != ResetCoolTime)
        {
            //�N�[���^�C��������������
            if (petData.activeSkillRemainingCooldown >= ResetCoolTime)
            {
                petData.activeSkillRemainingCooldown -= Time.deltaTime;
            }
            //�N�[���^�C����0�ȉ��ɂȂ�����A���ɖ߂�
            else
            {
                //���ɖ߂�
                petData.activeSkillRemainingCooldown = ResetCoolTime;
            }
        }
    }

    public void ChangeActiveSkill()
    {
        if (skillActive)
        {
            activeSkillDurationTime -= Time.deltaTime;

            if (activeSkillDurationTime <= 0f)
            {
                ResetActiveSkillAction();
                skillActive = false;
            }
        }
    }

    //�U�����x��ݒ肷�郁�\�b�h
    public void SetAttackSpeed(float speedMultiplier)
    {
        if (speedMultiplier <= 0) return;

        //�A�j���[�V�����̍Đ����x���X�V    
        this.currentAttackSpeedMultiplier = speedMultiplier;

        //�U���N�[���_�E���̎��Ԃ��X�V
        attackCooldown = attackCooldownOriginal / speedMultiplier;
    }

    public void AnimationEndChange(string stateName = "", PetActionStates states = PetActionStates.Idle)
    {
        //�A�j���[�V���������������A�w�肵���X�e�[�g�ɖ߂�
        AnimatorStateInfo stateInfo = petAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(stateName) && stateInfo.normalizedTime >= 0.95f)
        {
            sm.ChangeState(states);
        }
    }

    //�t�H�[���[�V�����p�̃I�t�Z�b�g�ʒu��ݒ肷�郁�\�b�h
    public void SetFormationOffset(Vector3 offset)
    {
        this.formationOffset = offset;
    }

    //�U�������A�j���[�V�����C�x���g����Đ����郁�\�b�h
    //���x�ω��Ή���
    protected virtual void PlayAttackSound()
    {
        attackCounter++;

        int ResetCount = 3;

        //3�񖈂Ɉ�x�A�U������炷
        if (attackCounter >= ResetCount)
        {
            //���ʉ��������Đ�����Ă�����A�X���[
            if (!SoundEffect.Instance.IsPlaying(attackSound))
            {
                SoundEffect.Instance.Play(attackSound);

                //�J�E���^�[�����Z�b�g
                attackCounter = 0; 
            }         
        }
    }

    protected virtual void ActiveSkillAction()
    {

    }

    protected virtual void ResetActiveSkillAction()
    {

    }

    public void SetPetAttackEvent()
    {
        var eventData = new Dictionary<string, object>();

        eventData["attacker"] = this.gameObject;
        eventData["target"] = this.enemyTransform;

        //�U���̍��߂𔭐M����(���y�b�g���U�������Ƃ��̏���������)
        EventManager.SetData(GameEvent.PetAttack, eventData);
        EventManager.EmitEvent(GameEvent.PetAttack);
    }

    //�y�b�g�̃f�[�^���擾���郁�\�b�h
    public PetData GetPetData() 
    { 
        if(petData == null) 
        { 
            return null;
        }
        return petData; 
    }

    public void ForceChangeState(PetActionStates newState)
    {
        if (sm != null)sm.ChangeState(newState);
    }

    public void SetSelfTimer(float lifetime)
    {
        if (selfDestructCoroutine != null)
        {
            StopCoroutine(selfDestructCoroutine);
        }
        selfDestructCoroutine = StartCoroutine(SelfDestruct_Coroutine(lifetime));
    }

    public void Doppelganger()
    {
        //�����́u�N���[���v��
        whoIam = PetCloneType.Clone;
    }

    protected virtual void CloneActiveSkill()
    {
        //�������I���W�i�����m�F
        if (whoIam == PetCloneType.Original) return;

        //�A�N�e�B�u�X�L���������Ă��邩�m�F
        if (!petData.hasActiveSkill) return;
        
        //�Ⴄ�Ȃ�A�N�e�B�u�X�L���̌��ʔ���
        this.ActiveSkillAction();
    }

    public void SetAttackMaltiplier(float multiplier)
    {
        //�U���͂̔{�����X�V
        pendingAttackMultiplier = multiplier;
    }


    private IEnumerator SelfDestruct_Coroutine(float lifetime)
    {
        // �w�肳�ꂽ���ԁA�ҋ@
        yield return new WaitForSeconds(lifetime);
     
        ActivePetManager.Instance.RemoveActivePet(this.gameObject); 

        Destroy(this.gameObject);
    }

    // �p����ŋ�̓I�ȍU���������������邽�߂̃��\�b�h
    public abstract void PerformAttack();
}

