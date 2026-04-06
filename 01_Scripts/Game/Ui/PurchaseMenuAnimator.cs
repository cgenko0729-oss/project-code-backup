using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class PurchaseMenuAnimator : MonoBehaviour
{
     [Header("Tweaks")]
    [SerializeField] float showDuration = 0.45f;
    [SerializeField] float hideDuration = 0.25f;

    [Space, SerializeField] CanvasGroup canvasGroup;        // Å© assign in Inspector
    [SerializeField] RectTransform root;                    // Å© assign in Inspector

    Sequence current;

    public ShopWindow shopWindow;

    void Awake()
    {
        // Fallback assignments
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!root)        root        = (RectTransform)transform;

        // Put the menu in a hidden, ready-to-animate state.
        //canvasGroup.alpha          = 0f;
        //canvasGroup.interactable   = false;
        //canvasGroup.blocksRaycasts = false;
        //root.localScale            = Vector3.one * 0.8f;
        //gameObject.SetActive(false);
    }

    // ---------- PUBLIC API ----------
    public void Show()
    {
        gameObject.SetActive(true);
        shopWindow.RefreshShopPanelLanguage();
        Play(show: true);
    }

    public void Hide()
    {
        Play(show: false);
    }

    public void InitData()
    {
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0;
    }

    // ---------- INTERNAL ----------
    void Play(bool show)
    {
        // Clean up previous run, if any
        if (current != null && current.IsActive()) current.Kill();

        float dur = show ? showDuration : hideDuration;
        float startScale = show ? 0.8f : 1f;
        float endScale   = show ? 1f   : 0.8f;
        float startAlpha = show ? 0f   : 1f;
        float endAlpha   = show ? 1f   : 0f;

        // Prep starting values
        root.localScale  = Vector3.one * startScale;
        canvasGroup.alpha = startAlpha;
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = false;

        current = DOTween.Sequence()
            .SetUpdate(true)                          // Å© unscaled time
            .Append(canvasGroup.DOFade(endAlpha, dur))
            .Join(root.DOScale(endScale, dur)
                       .SetEase(show ? Ease.OutBack : Ease.InBack))
            .OnComplete(() =>
            {
                canvasGroup.interactable   = show;
                canvasGroup.blocksRaycasts = show;
                if (!show) gameObject.SetActive(false);
            });
    }
}

