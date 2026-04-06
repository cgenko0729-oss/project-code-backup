using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           
using QFSW.MOP2;            
using MonsterLove.StateMachine; 

public class EnemyDragonAction : EnemyStateActionBase
{
    public float distNeedToExplode = 3.5f;
    public GameObject aoeCircleObj;
    public float moveSpdExplode = 0.56f;
    EnemyStatusBase es;

    public float punchScaleMax = 0.8f;
    public float punchScaleMin = 0.28f;
    public Color explodeColor = Color.yellow;

    public bool isExplodeFollow = false;
    public bool isExplodeFillSet = false;

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
        bombLifeCnt = 3f;
        //moveSpd = moveSpdExplode;

        Renderer rend = GetComponentInChildren<Renderer>();
        rend.material.DOColor(explodeColor, 0.5f).SetLoops(-1, LoopType.Yoyo);
        Instantiate(aoeCircleObj, new Vector3(transform.position.x,0.028f,transform.position.z), Quaternion.identity);

        transform.DOScale(punchScaleMax, punchScaleMin).SetLoops(8, LoopType.Yoyo).SetEase(Ease.InOutSine); //doTween doScale 0.7 to 1.4 in 0.7 second loop 4 time 
        animator.SetBool("IsExplode",true);

        es = GetComponent<EnemyStatusBase>();
        es.ExplodeFlash();

        

    }

    public void Attack_Update()
    {

        es.enemyHp = 1400f;

        bombLifeCnt -= Time.deltaTime;

        if (bombLifeCnt < 0f) {

            EnemyStatusBase enemyStatusBase = GetComponent<EnemyStatusBase>();
            enemyStatusBase.DeadNoExp();
            stateMachine.ChangeState(EnemyState.Move);

        }

        if (isExplodeFollow)
        {
            EnemyExplodeHoming();
            EnemyRotation();

             AoeCircleDynamic aoeCircle = aoeCircleObj.GetComponent<AoeCircleDynamic>();
            aoeCircle.UpdateTransform(transform.position);
            if (!isExplodeFillSet)
            {
                aoeCircle.BeginFill(3f, 10);
                isExplodeFillSet = true;
            }

        }


    }



}
