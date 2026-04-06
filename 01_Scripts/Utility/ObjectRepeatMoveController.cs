using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine

public enum RepeatAxis { Horizontal, Vertical }

/// <summary>
/// Repeatedly moves a Transform or RectTransform left↔right or up↔down
/// around its start position.
/// Attach to the cursor (or any object) and tweak in the Inspector.
/// </summary>
[DisallowMultipleComponent]
public class ObjectRepeatMoveController : MonoBehaviour
{
    [Header("Movement Settings")]
    public RepeatAxis axis = RepeatAxis.Horizontal;

    [Tooltip("How far from the centre to travel (units for 3D, pixels for UI).")]
    public float amplitude = 20f;

    [Tooltip("Seconds for a full there-and-back cycle (i.e. period).")]
    [Min(0.01f)]
    public float period = 1f;

    [Tooltip("Uncheck if you *want* slow-motion / pause to affect the motion.")]
    public bool ignoreTimeScale = true;

    [Tooltip("Set true for RectTransform (anchoredPosition).")]
    public bool isUIElement = true;

    /* ─────────── private ─────────── */
    Vector3 _origin;          // cached start position
    RectTransform _rect;      // non-null if it’s UI

    void Awake()
    {
        if (isUIElement)
            _rect = transform as RectTransform;

        _origin = isUIElement ? (Vector3)_rect.anchoredPosition
                              : transform.localPosition;
    }

    void Update()
    {
        float t = (ignoreTimeScale ? Time.unscaledTime : Time.time);
        // Map time → smooth oscillation in range −amplitude … +amplitude
        float offset = Mathf.Sin(t * Mathf.PI * 2f / period) * amplitude;

        if (axis == RepeatAxis.Horizontal)
            ApplyOffset(offset, 0);
        else
            ApplyOffset(0, offset);
    }

    void ApplyOffset(float x, float y)
    {
        if (isUIElement && _rect != null)
            _rect.anchoredPosition = _origin + new Vector3(x, y, 0);
        else
            transform.localPosition = _origin + new Vector3(x, y, 0);
    }

    public void SetNewOrigin(Vector3 newOriginPosition)
    {
        _origin = newOriginPosition;
    }

}