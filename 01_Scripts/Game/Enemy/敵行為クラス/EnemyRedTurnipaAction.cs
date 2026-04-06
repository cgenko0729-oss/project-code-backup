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

public class EnemyRedTurnipaAction : EnemyStateActionBase
{
    public float distNeedToExplode = 3.5f;
    public GameObject aoeCircleObj;
    EnemyStatusBase es;

    public float moveSpdNormal = 1.49f;
    public float moveSpdExplode = 0.77f;

    public float punchScaleMax = 0.8f;
    public float punchScaleMinTime = 0.28f;
    public int punchScaleLoopTimes = 14;
    public Color explodeColor = Color.yellow;

    public bool isExplodeFollow = false;
    public bool isExplodeFillSet = false;

    public AoeCircle _currentAoe;

    public float bombLifeCnt = 3f;

    protected override void Awake()
    {
        enemyStatus = GetComponent<EnemyStatusBase>();
        stateMachine = StateMachine<EnemyState>.Initialize(this);
        stateMachine.ChangeState(EnemyState.Move);
        GetPlayerInfo();
        GetAnimator();
        InitSeperationInfo();

        
    }

    void OnEnable()
    {
        stateMachine.ChangeState(EnemyState.Move);  
        GetPlayerInfo();
        GetAnimator();
        InitSeperationInfo();
    }

    protected override void Update()
    {
       GetStateInfo();
        UpdateStateCnt();
        CalDistToPlayer();
        //UpdateSpeedDebuff();

        //fix pos.y to 0.56f
        Vector3 pos = transform.position;
        pos.y = 0.56f;
        transform.position = pos;


    }

    public void Move_Enter()
    {
        stateCnt = 0f;
        animator = GetComponent<Animator>();
        animator.SetBool("IsExplode",false);

        enemyStatus.enemyMoveSpd = moveSpdNormal; 
    }

    public void Move_Update()
    {
        EnemySimpleHoming();
        EnemyRotation();

        if (distToPlayer < distNeedToExplode) {
            stateMachine.ChangeState(EnemyState.Attack);
        }
    }

    public void Attack_Enter()
    {
        stateCnt = 3f;
        bombLifeCnt = 3f;
        //enemyStatus.enemyMoveSpd = moveSpdExplode;
            
        Renderer rend = GetComponentInChildren<Renderer>();
        rend.material.DOColor(explodeColor, 0.5f).SetLoops(-1, LoopType.Yoyo);
        _currentAoe = Instantiate(aoeCircleObj, new Vector3(transform.position.x,0.028f,transform.position.z), Quaternion.identity).GetComponent<AoeCircle>();

        transform.DOScale(punchScaleMax, punchScaleMinTime).SetLoops(punchScaleLoopTimes, LoopType.Yoyo).SetEase(Ease.InOutSine); //doTween doScale 0.7 to 1.4 in 0.7 second loop 4 time 
        animator.SetBool("IsExplode",true);

        es = GetComponent<EnemyStatusBase>();
        es.ExplodeFlash();

        

    }

    public void Attack_Update()
    {
        bombLifeCnt -= Time.deltaTime;

        es.enemyHp = 1400f;
       
        if(bombLifeCnt < 0f) {

            EnemyStatusBase enemyStatusBase = GetComponent<EnemyStatusBase>();
            enemyStatusBase.DeadNoExp();
            stateMachine.ChangeState(EnemyState.Move);

        }

        if (isExplodeFollow)
        {
            EnemyExplodeHoming();
            EnemyRotation();

            Vector3 aoePos = new Vector3(transform.position.x, 0.028f, transform.position.z);
            if(_currentAoe)_currentAoe.UpdateTransform(aoePos);
            if (!isExplodeFillSet)
            {
                //_currentAoe.BeginFill(3f, 10);
                //isExplodeFillSet = true;
            }

        }


    }


}

