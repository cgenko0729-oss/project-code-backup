using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class SkillBoomerangMove : MonoBehaviour
{
    // 共通で使用するメンバ
    public Vector3 moveVec;     // 移動方向
    public float speedFinal;    // 移動スピード

    public Vector3 controlVec;  // 引き延ばす曲線の方向
    public Vector3 endValue;    // 終了地点
    public Vector3 startValue;  // 開始地点

    private float bazieTime = 10;       // 曲線を描くのにかかる時間
    private float elapsedTime = 0;      // 経過時間
    private float bazieSpeed = 5.0f;    // 曲線を描くスピード
    private float hitDuration = 0.5f;   // 当たり判定の発生間隔
    public float hitTime = 0;           // 次の当たり判定発生までの残り時間

    // 進化後に使用されるメンバ
    public bool isFinalSkill = false;   // スキルが進化後かどうか
    public GameObject slashWaveEffect;  // 発生するエフェクト
    public GameObject waveCollisionObj; // エフェクトの当たり判定
    public bool isWait = false;        // 待機フラグ
    private float bazieHalfTime = 0;    // 丁度折り返す地点での時間
    private float waitTime = 1.5f;      // 折り返し地点での待機時間

    public void Init()
    {
        elapsedTime = 0;
        bazieTime = 10;
        bazieHalfTime = bazieTime / 2;
        startValue = transform.position;
        endValue = transform.position;
        controlVec = (moveVec).normalized * speedFinal;
        controlVec = startValue + (controlVec * speedFinal);

        waitTime = 2;
        hitTime = 0;
        slashWaveEffect.SetActive(false);

        //Debug.Log("boomerangMove Init");
    }

    public float deg = 0;
    void Update()
    {
        deg += 720.0f * Time.deltaTime;
        if (deg >= 360.0f)
        {
            deg -= 360.0f;
        }
        transform.rotation = Quaternion.Euler(0, deg, 0);

        hitTime -= Time.deltaTime;
        var normalCol = GetComponent<SphereCollider>();
        normalCol.enabled = true;
        var finalCol = waveCollisionObj.GetComponent<SphereCollider>();
        finalCol.enabled = true;
        if (hitTime <= 0)
        {
            normalCol.enabled = false;
            finalCol.enabled = false;

            hitTime = hitDuration;
        }

        if (elapsedTime <= bazieTime)
        {
            if (isFinalSkill == true &&
                elapsedTime >= bazieHalfTime)
            {
                isWait = true;
                isFinalSkill = false;

                slashWaveEffect.SetActive(true);

                SoundEffect.Instance.Play(SoundList.BoomerangEvolveSe);
            }

            if (isWait == true)
            {
                waitTime -= Time.deltaTime;
                if(waitTime <= 0)
                {
                    isWait = false;
                    slashWaveEffect.SetActive(false);
                }
                

            }
            else
            { 
                elapsedTime += bazieSpeed * Time.deltaTime;
                float progress = elapsedTime / bazieTime;
                Vector3 position = CalcBeziePoint(progress, startValue, controlVec, endValue);
                transform.position = position;
            }
        }
        else
        {
            transform.position += -moveVec * (speedFinal* bazieSpeed) * Time.deltaTime;
        }
    }

    Vector3 CalcBeziePoint(float progress,
        Vector3 startValue,Vector3 controllValue,Vector3 endValue)
    {
        Vector3 point =
            Mathf.Pow(1 - progress, 2) * startValue +
            2 * (1 - progress) * progress * controllValue +
            Mathf.Pow(progress, 2) * endValue;

        return point;
    }
}

