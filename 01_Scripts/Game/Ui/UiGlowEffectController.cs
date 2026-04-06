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

public class UiGlowEffectController : MonoBehaviour
{

    private UIFXController uifx;

    [SerializeField] private Color glowColor = Color.yellow;
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 70f;
    [SerializeField] private float oneWayDuration = 0.77f; // 0->70 takes this long
    [SerializeField] private Ease ease = Ease.InOutSine;
    [SerializeField] private bool useUnscaledTime = true;

        private Tween _tween;

    void Start()
    {
        uifx = GetComponent<UIFXController>();
    }

    private void OnEnable()
    {
        DOVirtual.DelayedCall(0.3f, Glow);
    }

    void Glow()
    {
        uifx.EnableGlow(true);
        uifx.SetGlowColor(glowColor);
        uifx.SetGlowGlobal(1f);

        StartLoop();
    }

     void OnDisable()
    {
        _tween?.Kill();
        _tween = null;
    }

    private void StartLoop()
    {
        _tween?.Kill();

        // DOTween drives this float forever
        float v = minIntensity;
        uifx.SetGlowIntensity(v);

        _tween = DOTween.To(() => v, x =>
                {
                    v = x;
                    uifx.SetGlowIntensity(v);
                },
                maxIntensity,
                oneWayDuration)
            .SetEase(ease)
            .SetLoops(-1, LoopType.Yoyo) // 0->70->0->70...
            .SetUpdate(useUnscaledTime)  // keeps animating even when Time.timeScale = 0
            .SetLink(gameObject);        // auto-kill when object is destroyed
    }

    void Update()
    {
        
    }
}

