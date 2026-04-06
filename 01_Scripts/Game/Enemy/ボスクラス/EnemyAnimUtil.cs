using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using HighlightPlus;
using System;

public static class EnemyAnimUtil
{
   
        /// <summary>
    /// Creates a continuous squish-and-stretch bouncing animation.
    /// </summary>
    /// <param name="target">The transform to animate.</param>
    /// <param name="stretchAmount">How much to stretch/squash on the axes.</param>
    /// <param name="durationPerCycle">The time it takes to complete one full bounce cycle.</param>
    /// <returns>A DOTween Sequence for the animation. The caller is responsible for playing and killing it.</returns>
    public static Sequence BounceUpDown(Transform target, float stretchAmount = 0.2f, float durationPerCycle = 1.0f)
    {
        Vector3 baseScale = target.localScale;
    
        // Helper functions to calculate the scaled vectors
        Vector3 StretchX(float delta) => new Vector3(baseScale.x + delta, baseScale.y - delta, baseScale.z);
        Vector3 SquashX(float delta) => new Vector3(baseScale.x - delta, baseScale.y + delta, baseScale.z);
    
        Sequence seq = DOTween.Sequence();
        
        // We use stretchAmount directly for clarity
        // Phase 1 – inward squash
        seq.Append(target.DOScale(SquashX(stretchAmount), durationPerCycle * 0.25f).SetEase(Ease.OutSine));
        // Phase 2 – stretch
        seq.Append(target.DOScale(StretchX(stretchAmount), durationPerCycle * 0.25f).SetEase(Ease.InSine));
        
        seq.SetLoops(-1, LoopType.Yoyo); // Yoyo makes it go back and forth smoothly.
    
        // OnKill ensures that if the tween is stopped, the object returns to its original scale.
        seq.OnKill(() => target.localScale = baseScale);
        seq.SetTarget(target); // Good practice to associate the tween with the target object
    
        return seq;
    }

    /// <summary>
    /// Creates a "looking around" animation by rotating left and right several times.
    /// </summary>
    /// <param name="target">The transform to rotate.</param>
    /// <param name="lookAngle">The maximum angle to turn to each side.</param>
    /// <param name="timeToEdge">How long it takes to turn from the center to the edge.</param>
    /// <param name="rotations">How many times to look left and right.</param>
    /// <returns>A DOTween Sequence for the animation.</returns>
    public static Sequence LookAround(Transform target, float lookAngle = 50f, float timeToEdge = 0.25f, int rotations = 2)
    {
        Quaternion baseRot = target.rotation;
        Quaternion leftRot = baseRot * Quaternion.Euler(0f, -lookAngle, 0f);
        Quaternion rightRot = baseRot * Quaternion.Euler(0f, lookAngle, 0f);
    
        float degPerSec = lookAngle / timeToEdge;
    
        Sequence seq = DOTween.Sequence();
        
        float prevAngle = 0f;
        for (int i = 0; i < rotations * 2; i++) // *2 for left and right
        {
            bool goLeft = (i % 2 == 0);
            float nextAngle = goLeft ? -lookAngle : lookAngle;
            float delta = Mathf.Abs(nextAngle - prevAngle);
            float duration = delta / degPerSec;
            
            Quaternion targetRot = goLeft ? leftRot : rightRot;
            seq.Append(target.DORotateQuaternion(targetRot, duration).SetEase(Ease.InOutSine));
            prevAngle = nextAngle;
        }
    
        // Return to center
        float backDuration = lookAngle / degPerSec;
        seq.Append(target.DORotateQuaternion(baseRot, backDuration).SetEase(Ease.InOutSine));
        
        seq.OnKill(() => target.rotation = baseRot);
        seq.SetTarget(target);
    
        return seq;
    }


    /// <summary>
    /// Creates a "punch" or "hit" feedback animation by quickly rotating and snapping back.
    /// </summary>
    /// <param name="target">The transform to animate.</param>
    /// <param name="punchRotation">The direction and amount of rotation.</param>
    /// <param name="duration">Total duration of the punch animation.</param>
    /// <param name="vibrato">How many times it vibrates.</param>
    /// <param name="elasticity">How much it snaps back. 1 is full snap.</param>
    /// <returns>A DOTween Tween for the animation.</returns>
    public static Tween PunchRotation(Transform target, Vector3 punchRotation, float duration = 0.4f, int vibrato = 4, float elasticity = 0.2f)
    {
        return target.DOPunchRotation(punchRotation, duration, vibrato, elasticity)
                     .SetEase(Ease.OutQuad)
                     .SetTarget(target);
    }

    /// <summary>
    /// Creates a pulsing inner glow effect on a HighlightEffect component.
    /// </summary>
    /// <param name="highlightEffect">The component to animate.</param>
    /// <param name="targetGlow">The peak glow intensity.</param>
    /// <param name="duration">Duration of one pulse (from 0 to peak).</param>
    /// <param name="loops">How many times to pulse. Use -1 for infinite.</param>
    /// <returns>A DOTween Tween for the animation.</returns>
    public static Tween PulseInnerGlow(HighlightEffect highlightEffect, float targetGlow = 1.4f, float duration = 0.28f, int loops = 3)
    {
        // The loop count needs to be doubled for Yoyo
        int totalLoops = loops == -1 ? -1 : loops * 2;
        
        // Ensure we start from a known state
        highlightEffect.innerGlow = 0;
    
        return DOTween.To(() => highlightEffect.innerGlow, x => highlightEffect.innerGlow = x, targetGlow, duration)
            .SetLoops(totalLoops, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                // Reset on final completion
                highlightEffect.innerGlow = 0;
            })
            .SetTarget(highlightEffect.gameObject);
    }


    public static Tween TurnAround360Degree(Transform target,float duration = 1f,Ease ease = Ease.Linear,Action onFinish = null,RotateMode mode = RotateMode.LocalAxisAdd)
    {
        var tween = target.DORotate(new Vector3(0f, 360f, 0f), duration, mode).SetEase(ease)
            .SetRelative()               // ensure it's relative to current
            .SetTarget(target)
            .OnComplete(() => onFinish?.Invoke());
     
        return tween;
    }

    public static Tween Backstep(Transform target, float distance = 2.5f, float duration = 0.18f, Action onFinish = null)
    {
        Vector3 start = target.position;
        Vector3 back = -target.forward * distance;
        return target.DOMove(start + back, duration).SetEase(Ease.OutCubic)
                     .OnComplete(() => onFinish?.Invoke())
                     .SetTarget(target);
    }

    // 3) Strafe around a pivot by N degrees at fixed radius (keeps facing the center)
    public static Tween StrafeArcAround(Transform target, Transform center, float radius, float degrees, float duration, bool clockwise = true)
    {
        Vector3 c = center.position;
        Vector3 v = target.position - c;
        v.y = 0f;
        if (v.sqrMagnitude < 0.001f) v = (clockwise ? Vector3.right : Vector3.left) * radius;
        v = v.normalized * radius;

        float startAng = Mathf.Atan2(v.z, v.x);
        float endAng = startAng + Mathf.Deg2Rad * (clockwise ? -Mathf.Abs(degrees) : Mathf.Abs(degrees));

        return DOTween.To(() => 0f, t =>
        {
            float a = Mathf.Lerp(startAng, endAng, t);
            Vector3 p = new Vector3(Mathf.Cos(a) * radius, target.position.y, Mathf.Sin(a) * radius);
            target.position = c + p;
            target.LookAt(new Vector3(c.x, target.position.y, c.z), Vector3.up);
        }, 1f, duration).SetEase(Ease.Linear).SetTarget(target);
    }

    // 4) Jump along an arc to a point (uses DOJump for nice parabolic motion)
    public static Tween JumpTo(Transform target, Vector3 worldPos, float apexHeight = 2.8f, float duration = 0.5f, int bounces = 1, Action onFinish = null)
    {
        return target.DOJump(worldPos, apexHeight, Mathf.Max(1, bounces), duration)
                     .SetEase(Ease.OutQuad)
                     .OnComplete(() => onFinish?.Invoke())
                     .SetTarget(target);
    }

    // 5) Hover / bob loop (cheap idle for flyers/bosses)
    public static Tween HoverBob(Transform target, float amplitude = 0.25f, float period = 1.2f)
    {
        Vector3 basePos = target.localPosition;
        Tween t = target.DOLocalMoveY(basePos.y + amplitude, period * 0.5f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetTarget(target);
        t.OnKill(() => { if (target) target.localPosition = basePos; });
        return t;
    }

    // 6) Teleport blink (scale out -> move -> scale in); works without custom shaders
    public static Sequence TeleportBlink(Transform target, Vector3 worldPos, float hideTime = 0.1f, float showTime = 0.12f, Action onFinish = null)
    {
        Vector3 origScale = target.localScale;
        var seq = DOTween.Sequence().SetTarget(target);
        seq.Append(target.DOScale(Vector3.zero, hideTime).SetEase(Ease.InBack));
        seq.AppendCallback(() => target.position = worldPos);
        seq.Append(target.DOScale(origScale, showTime).SetEase(Ease.OutBack));
        seq.OnKill(() => { if (target) target.localScale = origScale; });
        seq.OnComplete(() => onFinish?.Invoke());
        return seq;
    }

    //Slam: wind-up (sink) -> lift -> slam down (great for boss AoE tells) ,small jump on ground
    public static Sequence SlamWindupAndImpact(Transform target, float sink = 0.15f, float lift = 1.2f, float windup = 0.25f, float airtime = 0.25f, float slam = 0.18f, Action onImpact = null)
    {
        float groundY = target.position.y;
        var seq = DOTween.Sequence().SetTarget(target);
        seq.Append(target.DOMoveY(groundY - sink, windup).SetEase(Ease.InSine));
        seq.Append(target.DOMoveY(groundY + lift, airtime).SetEase(Ease.OutQuad));
        seq.Append(target.DOMoveY(groundY, slam).SetEase(Ease.InQuad).OnComplete(() => onImpact?.Invoke()));
        return seq;
    }

    // 1) Quick forward lunge (dash) toward a point/target
    public static Tween LungeTo(Transform target, Vector3 worldPos, float duration = 0.25f, float overshoot = 0.5f, Action onFinish = null)
    {
        Vector3 start = target.position;
        Vector3 dir = (worldPos - start);
        float dist = dir.magnitude;
        if (dist < 0.01f) return DOVirtual.DelayedCall(0f, () => onFinish?.Invoke());
        dir /= dist;

        var tween = target.DOMove(start + dir * (dist * (1f + overshoot)), duration * 0.7f)
                          .SetEase(Ease.OutQuad)
                          .OnComplete(() =>
                          {
                              // tiny settle back
                              target.DOMove(worldPos, duration * 0.3f).SetEase(Ease.InQuad).OnComplete(() => onFinish?.Invoke());
                          })
                          .SetTarget(target);
        return tween;
    }

    // 7) Spiral-in toward a pivot (path generated on the fly)
    public static Tween SpiralIn(Transform target, Transform center, float startRadius, float endRadius, float turns, float duration, bool clockwise = true)
    {
        int steps = Mathf.Max(16, Mathf.CeilToInt(turns * 24f));
        Vector3[] pts = new Vector3[steps];
        Vector3 c = center.position;
        float startAng = Mathf.Atan2(target.position.z - c.z, target.position.x - c.x);
        float dir = clockwise ? -1f : 1f;
        for (int i = 0; i < steps; i++)
        {
            float t = (float)i / (steps - 1);
            float radius = Mathf.Lerp(startRadius, endRadius, t);
            float ang = startAng + dir * t * (turns * Mathf.PI * 2f);
            pts[i] = new Vector3(c.x + Mathf.Cos(ang) * radius, target.position.y, c.z + Mathf.Sin(ang) * radius);
        }
        return target.DOPath(pts, duration, PathType.CatmullRom, PathMode.Full3D)
                     .SetEase(Ease.Linear)
                     .SetLookAt(center, Vector3.up)
                     .SetTarget(target);
    }

    //Zigzag advance: move forward while weaving side-to-side (projectile-like serpentine)
    public static Tween ZigZagAdvance(Transform target, Vector3 forwardDir, float distance = 6f, float duration = 0.8f, float lateralAmp = 1.2f, float waves = 3f)
    {
        Vector3 start = target.position;
        forwardDir.y = 0f; forwardDir.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, forwardDir).normalized;

        return DOTween.To(() => 0f, t =>
        {
            float prog = t; // 0..1
            float lateral = Mathf.Sin(prog * waves * Mathf.PI * 2f) * lateralAmp;
            target.position = start + forwardDir * (distance * prog) + right * lateral;
        }, 1f, duration).SetEase(Ease.Linear).SetTarget(target);
    }

}

