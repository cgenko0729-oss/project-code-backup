using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class EnemySpiderDen : MonoBehaviour
{

    public ObjectPool spiderPool;
    public float spawnCnt = 0f; // スポーンのカウントタイマー
    public float spawnCntMax = 5f; // スポーンの最大カウント時間
    public int spawnNum = 5; // スポーンするスパイダーの数


     [Tooltip("Seconds the shake/bounce lasts (position & rotation share this).")]
    public float shakeDuration = 0.45f;

    [Header("✦ Position Shake")]
    public bool enablePosShake = true;
    [Tooltip("Per-axis strength (0 disables that axis).")]
    public Vector3 posStrength = new Vector3(0.25f, 0.25f, 0.25f);
    public int posVibrato = 15;
    public float posRandomness = 90f;

    [Header("✦ Rotation Shake")]
    public bool enableRotShake = false;
    [Tooltip("Per-axis strength in *degrees* (0 disables that axis).")]
    public Vector3 rotStrength = new Vector3(0, 0, 15);
    public int rotVibrato = 10;
    public float rotRandomness = 45f;

    [Header("✦ Scale Bounce (Punch)")]
    public bool enableScaleBounce = true;
    [Tooltip("How far to ‘punch’ the scale on each axis (0 disables that axis).")]
    public Vector3 scalePunch = new Vector3(0.15f, 0.15f, 0.15f);
    public float scaleDuration = 0.30f;
    public int scaleVibrato = 6;
    [Range(0, 1)]
    public float scaleElasticity = 0.5f;

    
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public float targetPosY = 2.3f; 

    private void Awake()
    {
        initialPosition = transform.position; // 初期位置を保存
        initialRotation = transform.rotation; // 初期回転を保存
        spawnCnt = spawnCntMax; // スポーンカウントタイマーを初期化


    }

    void Start()
    {
        
    }

    void Update()
    {
        spawnCnt -= Time.deltaTime; // カウントタイマーを更新
        SpawnSpiderFromDen();


        if(transform.position.y < targetPosY) // 位置が目標の高さより低い場合
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(initialPosition.x, targetPosY, initialPosition.z), Time.deltaTime * 2f);
        }

    }
    public void DoShake()
    {
        DOTween.Kill(transform);

        Sequence seq = DOTween.Sequence().SetLink(gameObject);

        if (enablePosShake && posStrength != Vector3.zero)
        {
            seq.Join(transform.DOShakePosition(
                shakeDuration,
                posStrength,     
                posVibrato,
                posRandomness,
                snapping: false,    
                fadeOut: true
            ));
        }

        if (enableRotShake && rotStrength != Vector3.zero)
        {
            seq.Join(transform.DOShakeRotation(
                shakeDuration,
                rotStrength,
                rotVibrato,
                rotRandomness,
                fadeOut: true
            ));
        }

        if (enableScaleBounce && scalePunch != Vector3.zero)
        {
            seq.Join(transform.DOPunchScale(
                scalePunch,
                scaleDuration,
                scaleVibrato,
                scaleElasticity
            ).SetEase(Ease.OutQuad));
        }

        seq.SetEase(Ease.OutQuad);
    }

    public void SpawnSpiderFromDen()
    {
        if (spawnCnt <= 0f)
        {
            //for (int i = 0; i < spawnNum; i++)
            //{
            //    spiderPool.GetObject(transform.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f)));             
            //}

            DoShake();

            spawnCnt = spawnCntMax; // カウントタイマーをリセット
        }
    }

    }

