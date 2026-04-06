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

public class CoinMove : MonoBehaviour
{
    
    Transform playerTrans;
    public float distNeedHomePlayer= 1.4f; //プレイヤーに帰る距離
    public float distToPlayer = 10;

    public bool isHomePlayer = false;
    public bool isCoinActivated = false;

    public int coinAddAmount = 3;

    public Vector3 initPos = Vector3.zero;
    public Vector3 targetPos = Vector3.zero;

    [SerializeField] AnimationCurve timeByDistance   = AnimationCurve.Linear(0, 0.4f, 20, 1.4f);
    [SerializeField] AnimationCurve heightByDistance = AnimationCurve.Linear(0, 3f, 20, 8f);
    [SerializeField] float minFlightTime  = 0.25f;     
    [SerializeField] float maxFlightTime  = 2.0f;
    [SerializeField] float maxArcHeight   = 10f;

    public ObjectPool coinPool;

    private UpgradeEffectBase upgradeEffect;

    private void Start()
    {
        playerTrans = GameObject.FindWithTag("Player").transform;

        upgradeEffect =
            UpgradeEffectManager.Instance.upgradeEffectList.Find(t => t.BuffType == BuffType.ApplyBuffPerCoin);

        //TransitionFromInitPosToSpawnPosWithCurveMovement();
    }

    public void OnEnable()
    {
        //TransitionFromInitPosToSpawnPosWithCurveMovement();
        isCoinActivated = false;
        isHomePlayer = false;
    }

    private void Update()
    {
        CalDistToPlayer();
        HomingPlayer();
        FixPositionY();
    }

    public void TransitionFromInitPosToSpawnPosWithCurveMovement()
    {
        float dist = Vector3.Distance(transform.position, targetPos);
        float t    = Mathf.Clamp(timeByDistance.Evaluate(dist), minFlightTime, maxFlightTime);
        float h    = Mathf.Min(heightByDistance.Evaluate(dist), maxArcHeight);
    
        transform.DOJump(targetPos, h, 1, t,snapping: false).SetEase(Ease.Linear)
        .OnComplete(() => 
        {
            isCoinActivated = true;
           
        });

    }

    public void CalDistToPlayer()
    {
        if (!isCoinActivated) return;

        distToPlayer = Vector3.Distance(playerTrans.position, transform.position);
        if (distToPlayer < distNeedHomePlayer && !isHomePlayer)
        {
            //プレイヤーに帰る
            isHomePlayer = true;
        }

    }

    public void HomingPlayer()
    {
        if (!isCoinActivated) return;

        if (isHomePlayer)
        {
            //プレイヤーに向かって移動
            transform.position = Vector3.MoveTowards(transform.position, playerTrans.position, 10.0f * Time.deltaTime);
            if (Vector3.Distance(transform.position, playerTrans.position) < 0.77f)
            {
                //プレイヤーに到達したら
                //Destroy(gameObject);
                coinPool.Release(gameObject);
                MapManager.Instance.AdddExpTraitCount();

                if(SkillEffectManager.Instance.universalTrait.isGetCoinAddCrit)EffectManager.Instance.HandleGetCoinAddCrit();

                // コイン取得でのバフを発動する
                if(upgradeEffect != null)
                {
                    if(upgradeEffect.isEnable == true)
                    {
                        upgradeEffect.ActiveBuff();
                    }
                }

                //CurrencyManager.Instance.Add(coinAddAmount);
                ResultMenuController.Instance.turnCoinGet += coinAddAmount;
                SoundEffect.Instance.Play(SoundList.PickCoin);
            }
        }
    }

    public void FixPositionY(float yPos = 0.42f)
    {
        if (!isCoinActivated) return;
        Vector3 pos = transform.position;
        pos.y = yPos;
        transform.position = pos;
    }


    }

