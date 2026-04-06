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
using System.Collections;

public class SkillWolfMove : MonoBehaviour
{

    public Vector3 startPos = Vector3.zero;
    public Vector3 targetPos = Vector3.zero;

    public float duration = 3f;

    public float curveOffset = 10f;
    private Tweener moveTween;

    SkillModelBase model;

    void Start()
    {
        model = GetComponent<SkillModelBase>();
    }

    void Update()
    {
        
    }

    public void StartMove(Vector3 start, Vector3 target, float offset, float travelDuration)
    {
        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill();
        }

        transform.position = start;
        
        Vector3 forwardDirection = (target - start).normalized;
        Vector3 sideDirection = Vector3.Cross(forwardDirection, Vector3.up);
        Vector3 midpoint = start + (target - start) / 2;
        Vector3 controlPoint = midpoint + sideDirection * offset;
        
        float t = 0f;

        // Use the 'travelDuration' parameter here instead of 'this.duration'
        moveTween = DOTween.To(() => t, (x) => t = x, 1f, travelDuration)
            .OnUpdate(() =>
            {
                Vector3 positionOnCurve =
                    Mathf.Pow(1 - t, 2) * start +
                    2 * (1 - t) * t * controlPoint +
                    Mathf.Pow(t, 2) * target;
                
                transform.position = positionOnCurve;
            })
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                model.skillDuration = 0f;
            });
    }

    private IEnumerator MoveAlongCurve()
    {
        float elapsedTime = 0f;

        // 1. Calculate the direction from start to target.
        Vector3 forwardDirection = (targetPos - startPos).normalized;

        // 2. Find a perpendicular vector (sideways) using the Cross Product.
        // This gives us a "right" vector relative to the forward direction.
        Vector3 sideDirection = Vector3.Cross(forwardDirection, Vector3.up);

        // 3. Calculate the control point.
        // We find the midpoint and then push it to the side.
        Vector3 midpoint = startPos + (targetPos - startPos) / 2;
        Vector3 controlPoint = midpoint + sideDirection * curveOffset;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            // Calculate the position on the Quadratic Bezier curve using the new control point.
            Vector3 positionOnCurve =
                Mathf.Pow(1 - t, 2) * startPos +
                2 * (1 - t) * t * controlPoint +
                Mathf.Pow(t, 2) * targetPos;

            transform.position = positionOnCurve;

            yield return null;
        }

        transform.position = targetPos;
        model.skillDuration = 0f;

        Debug.Log("Sideways wolf move finished!");
    }


}

