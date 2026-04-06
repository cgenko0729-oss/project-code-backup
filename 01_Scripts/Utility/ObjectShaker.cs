using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class ObjectShaker : MonoBehaviour
{

    [Tooltip("Seconds the shake/bounce lasts (position & rotation share this).")]
    public float shakeDuration = 0.45f;

    [Header("✦ Position Shake")]
    public bool enablePosShake = true;
    [Tooltip("Per-axis strength (0 disables that axis).")]
    public Vector3 posStrength = new Vector3(0.1f, 0.0f, 0.1f);
    public int posVibrato = 7;
    public float posRandomness = 70f;

    [Header("✦ Rotation Shake")]
    public bool enableRotShake = false;
    [Tooltip("Per-axis strength in *degrees* (0 disables that axis).")]
    public Vector3 rotStrength = new Vector3(0, 14, 0);
    public int rotVibrato = 7;
    public float rotRandomness = 45f;

    [Header("✦ Scale Bounce (Punch)")]
    public bool enableScaleBounce = true;
    [Tooltip("How far to ‘punch’ the scale on each axis (0 disables that axis).")]
    public Vector3 scalePunch = new Vector3(0.21f, 0.0f, 0.21f);
    public float scaleDuration = 0.4f;
    public int scaleVibrato = 7;
    [Range(0, 1)]
    public float scaleElasticity = 0.7f;

    
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public bool isAutoShake = false; // 自動シェイクフラグ
    public float autoShakeInterval = 2f; // 自動シェイクの間隔
    private float autoShakeTimer = 0f; // 自動シェイクタイマー



    void Start()
    {
        initialPosition = transform.position; // 初期位置を保存
        initialRotation = transform.rotation; // 初期回転を保存


    }

    void Update()
    {
        if (isAutoShake)
        {
            autoShakeTimer -= Time.deltaTime; // タイマーを減少
            if (autoShakeTimer <= 0f)
            {
                DoShake(); // シェイクを実行
                autoShakeTimer = autoShakeInterval; // タイマーをリセット
            }

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

}

