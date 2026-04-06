using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using UnityEngine.EventSystems;

public class LevelUpButton : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    RectTransform rt;

    Sequence hoverSeq;
    Tween clickTween;

    [SerializeField] private float hoverScaleFactor = 1.4f;
    [SerializeField] private float hoverDuration    = 0.15f;
    [SerializeField] private Ease  hoverEase        = Ease.OutBack;
    private Vector3 _originalScale;
    private Tween    _hoverTween;

    public bool hasHoverSound = true;

    public bool isInteractableCheck = false;

    public bool isResetScaleOnEventCalled = false;

    private void OnEnable()
    {
        if (isResetScaleOnEventCalled)
        {
            EventManager.StartListening("ButtonResetScale", ResetScale);
        }
    }

    private void OnDisable()
    {
        if (isResetScaleOnEventCalled)
        {
            EventManager.StopListening("ButtonResetScale", ResetScale);
        }
    }

    void Start()
    {
        rt = GetComponent<RectTransform>();
        _originalScale = rt.localScale;       

    }

    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isInteractableCheck)
        {
            if (!GetComponent<Button>().interactable) return;
        }

        _hoverTween?.Kill();
        _hoverTween = rt.DOScale(_originalScale * hoverScaleFactor, hoverDuration).SetEase(hoverEase).SetUpdate(true);
   
       if(hasHoverSound) SoundEffect.Instance.Play(SoundList.UiHover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hoverTween?.Kill();
        _hoverTween = rt.DOScale(_originalScale, hoverDuration).SetEase(hoverEase).SetUpdate(true);

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isInteractableCheck)
        {
            if (!GetComponent<Button>().interactable) return;
        }

        clickTween?.Kill();
        clickTween = rt.DOScale(_originalScale * 0.9f, hoverDuration / 2).SetEase(Ease.OutBack).SetUpdate(true)
            .OnComplete(() => rt.DOScale(_originalScale, hoverDuration / 2).SetEase(Ease.OutBack).SetUpdate(true));
    }

    public void ResetScale()
    {
        rt.localScale = _originalScale;
    }


}

