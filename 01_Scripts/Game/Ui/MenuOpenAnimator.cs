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

public class MenuOpenAnimator : MonoBehaviour
{
   
    Sequence current;

    [SerializeField] float showDuration = 0.45f;
    [SerializeField] float hideDuration = 0.25f;

    public float scaleStart = 1f;
    public float scaleEnd = 0.8f;

    public RectTransform root;
    public CanvasGroup canvasGroup;

    public bool deactiveOnHide = false;

    private void Start()
    {
        root = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void PlayBuySE()
    {
        SoundEffect.Instance.Play(SoundList.ShopBuySe); 
    }

    public void PlayUnlockAchSe()
    {
        SoundEffect.Instance.PlayUnlockAchSe();
    }

    [ContextMenu("Debug Show")]
    public void DebugShow()
    {
               PlayeMenuAni(true);
    }

    [ContextMenu("Debug Hide")]
        public void DebugHide()
        {
                PlayeMenuAni(false);
    }


    public void PlayeMenuAni(bool show)
    {

        if (current != null && current.IsActive()) current.Kill();

        float dur = show ? showDuration : hideDuration;
        float startScale = show ? scaleEnd : scaleStart;
        float endScale   = show ? scaleStart   : scaleEnd;
        float startAlpha = show ? 0f   : 1f;
        float endAlpha   = show ? 1f   : 0f;

        root.localScale  = Vector3.one * startScale;
        canvasGroup.alpha = startAlpha;
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = false;

        current = DOTween.Sequence()
            .SetUpdate(true)                          // © unscaled time
            .Append(canvasGroup.DOFade(endAlpha, dur))
            .Join(root.DOScale(endScale, dur)
                       .SetEase(show ? Ease.OutBack : Ease.InBack))
            .OnComplete(() =>
            {
                canvasGroup.interactable   = show;
                canvasGroup.blocksRaycasts = show;
                if (!show && deactiveOnHide) gameObject.SetActive(false);
            });

    }


    public void PlayeMenuAniNoRayCast(bool show)
    {

        if (current != null && current.IsActive()) current.Kill();

        float dur = show ? showDuration : hideDuration;
        float startScale = show ? scaleEnd : scaleStart;
        float endScale   = show ? scaleStart   : scaleEnd;
        float startAlpha = show ? 0f   : 1f;
        float endAlpha   = show ? 1f   : 0f;

        root.localScale  = Vector3.one * startScale;
        canvasGroup.alpha = startAlpha;
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = false;

        current = DOTween.Sequence()
            .SetUpdate(true)                          // © unscaled time
            .Append(canvasGroup.DOFade(endAlpha, dur))
            .Join(root.DOScale(endScale, dur)
                       .SetEase(show ? Ease.OutBack : Ease.InBack))
            .OnComplete(() =>
            {
                canvasGroup.interactable   = show;
                //canvasGroup.blocksRaycasts = show;
                //if (!show) gameObject.SetActive(false);
            });

    }

}

