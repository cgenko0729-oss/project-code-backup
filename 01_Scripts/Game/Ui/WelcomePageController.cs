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

public class WelcomePageController : MonoBehaviour
{
    private Image pageImage;
    private RectTransform pageRect;

    [Header("Animation Settings")]
    public float openDuration = 0.5f;
    public float closeDuration = 0.5f;
    public Vector3 startScale = Vector3.zero;
    public Vector3 endScale = Vector3.one; // 1 = normal size
    public Ease openEase = Ease.OutBack;
    public Ease closeEase = Ease.InBack;

    public TitleButtonController welcomePageOpenButton;






    private void Awake()
    {
        
    }

    private void Start()
    {
        pageRect = GetComponent<RectTransform>();
        pageImage = GetComponent<Image>();
        if (!StageManager.Instance.isShownWelcomePage)
        {
            StageManager.Instance.isShownWelcomePage = true;
            DotweenAnimToOpenThePage();
        }
    }

    public void DotweenAnimToOpenThePage()
    {
        if(this.gameObject.activeSelf) return;
        gameObject.SetActive(true);

        // Start invisible & shrunk
        pageRect.localScale = startScale;
        pageImage.color = new Color(1, 1, 1, 0);

        // Parallel animation: Fade + Scale
        Sequence seq = DOTween.Sequence();
        seq.Join(pageRect.DOScale(endScale, openDuration).SetEase(openEase));
        seq.Join(pageImage.DOFade(1f, openDuration).SetEase(Ease.InOutSine));
    
        welcomePageOpenButton.enabled = false;
        Button wButton = welcomePageOpenButton.GetComponent<Button>();
        wButton.interactable = false;

    }

    public void CloseWelcomePage()
    {
        Sequence seq = DOTween.Sequence();

        // Shrink + Fade Out
        seq.Join(pageRect.DOScale(startScale, closeDuration).SetEase(closeEase));
        seq.Join(pageImage.DOFade(0f, closeDuration).SetEase(Ease.InOutSine));

        // Disable after animation
        seq.OnComplete(() => gameObject.SetActive(false));

        pageImage.raycastTarget = false;

        welcomePageOpenButton.enabled = true;
        Button wButton = welcomePageOpenButton.GetComponent<Button>();
        wButton.interactable = true;

    }

    }

