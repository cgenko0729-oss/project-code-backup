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

public class MidBossGolemStone : MonoBehaviour
{


    public Vector3 startPos;
    public Vector3 targetPos;

    //public float arcHeight = 7f;

    [SerializeField] AnimationCurve timeByDistance   = AnimationCurve.Linear(0, 0.4f, 20, 1.4f);
    [SerializeField] AnimationCurve heightByDistance = AnimationCurve.Linear(0, 3f, 20, 8f);
    [SerializeField] float minFlightTime  = 0.25f;     
    [SerializeField] float maxFlightTime  = 2.0f;
    [SerializeField] float maxArcHeight   = 10f;

    public GameObject dustEffectObj;

    [SerializeField] float minAngleStep = 45f;   // per tween, per axis
    [SerializeField] float maxAngleStep = 180f;  // per tween, per axis
    [SerializeField] float minRotateTime = 0.25f;
    [SerializeField] float maxRotateTime = 0.75f;
    [SerializeField] bool rotateWhilePaused = false;
    Tween rotateTween;

    public AudioClip stoneHitSe;

    private void Start()
    {
        Vector3 playerPos = GameObject.FindWithTag("Player").transform.position;
        targetPos = playerPos;

        CurveProjectileMove();
        KeepRandomlyRotate();
    }

    private void Update()
    {
        
    }

    void KeepRandomlyRotate()
    {
        if (rotateTween != null && rotateTween.IsActive()) rotateTween.Kill();

        // Recursive local function that schedules the next random rotate
        void NextStep()
        {
            // Random signed step on each axis
            float sx = Random.Range(minAngleStep, maxAngleStep) * (Random.value < 0.5f ? -1f : 1f);
            float sy = Random.Range(minAngleStep, maxAngleStep) * (Random.value < 0.5f ? -1f : 1f);
            float sz = Random.Range(minAngleStep, maxAngleStep) * (Random.value < 0.5f ? -1f : 1f);

            float dur = Random.Range(minRotateTime, maxRotateTime);

            // Tween to current Euler + random delta, then schedule another
            rotateTween = transform
                .DORotate(transform.eulerAngles + new Vector3(sx, sy, sz), dur, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetUpdate(rotateWhilePaused) // true => ignores Time.timeScale
                .OnComplete(NextStep);
        }

        NextStep();

    }

    public void CurveProjectileMove()
    {
        Vector3 targetPoint = targetPos;

        float dist = Vector3.Distance(transform.position, targetPoint);
        float t = Mathf.Clamp(timeByDistance.Evaluate(dist), minFlightTime, maxFlightTime);
        float h = Mathf.Min(heightByDistance.Evaluate(dist), maxArcHeight);

        transform.DOJump(targetPoint, h, 1, t, snapping: false).SetEase(Ease.Linear) //where it lands, how high above its start it goes, number of gjumpsh (use 1 for a single arc), total flight time
        .OnComplete(() => {
            
            Instantiate(dustEffectObj,transform.position, Quaternion.identity);
            Destroy(gameObject);
            SoundEffect.Instance.PlayOneSound(stoneHitSe, 0.7f);
        });



    }

}

