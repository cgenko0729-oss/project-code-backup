using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine;
using UnityEngine.EventSystems; //StateMachine


public class LevelUpCard : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public CanvasGroup cg;
    public RectTransform rt;
    public TextMeshProUGUI rarityText;

    public RectTransform buttonFrame;
    private UIFXController buttonFrameFx;

    Sequence hoverSeq;
    Tween clickTween;

    public Vector3 cardScale = Vector3.one;

    [Header("Hover Settings")]
    [SerializeField] private float hoverScaleFactor = 1.4f;
    [SerializeField] private float hoverDuration    = 0.15f;
    [SerializeField] private Ease  hoverEase        = Ease.OutBack;
    private Vector3 _originalScale;
    private Tween    _hoverTween;

    private Vector3 _frameOriginalScale;
    private Tween _hoverTweenFrame;

    public int cardIndex;

    public float scrollSpeedX = 1f;

    public bool isResetScaleOnEventCalled = false;



    void Awake ()
    {
        _originalScale = rt.localScale;
        _frameOriginalScale = buttonFrame.localScale;

        buttonFrameFx= buttonFrame.GetComponent<UIFXController>();

        //rt.localScale = Vector3.zero;     
        //cg.alpha = 0;

        //hoverSeq = DOTween.Sequence().Append(rt.DOScale(1.05f, 0.15f)).Append(rt.DOScale(1.00f, 0.15f)).SetLoops(-1).Pause();
    }

    public void OnEnable()
    {
        EventManager.StartListening("OpenLevelUpWindow", ResetColorAndScale);

        EventManager.StartListening("UpdateLevelUpFrame", UpdateFrameColorAndTexture);
        
        //if (isResetScaleOnEventCalled)
        {
            EventManager.StartListening("ButtonResetScale", ResetScale);
        }

    }

    public void OnDisable()
    {
        EventManager.StopListening("OpenLevelUpWindow", ResetColorAndScale);

        EventManager.StopListening("UpdateLevelUpFrame", UpdateFrameColorAndTexture);
        
        //if (isResetScaleOnEventCalled)
        {
            EventManager.StopListening("ButtonResetScale", ResetScale);
        }
    
    }

    public void ResetScale()
    {
        //rt.localScale = _originalScale;

        _hoverTween?.Kill();
        _hoverTween = rt.DOScale(_originalScale, hoverDuration).SetEase(hoverEase).SetUpdate(true);

        _hoverTweenFrame?.Kill();
        _hoverTweenFrame = buttonFrame.DOScale(_frameOriginalScale, hoverDuration).SetEase(hoverEase).SetUpdate(true);
    }

    private void Start()
    {
        buttonFrame.gameObject.SetActive(true);
    }

    private void Update()
    {
        if(!SkillManager.Instance.waitingForPlayer) return;

        Color rarityColor = DecideRarityColor();
        buttonFrame.GetComponent<UIFXController>().SetOutlineColor(rarityColor);
    }

    public void Show(float delay)
    {

        float cardDelay = delay / 7f;

        cg.alpha = 0;
        cg.DOFade(1, 0.28f).SetDelay(cardDelay).SetUpdate(true);
        
        //set cg.GetComponent<RectTransform>().anchoredPosition.y to 225f

        RectTransform cgRect = cg.GetComponent<RectTransform>();
        cgRect.anchoredPosition = new Vector2(cgRect.anchoredPosition.x, 560f);
        cgRect.DOAnchorPosY(0, 0.28f).SetDelay(cardDelay).SetUpdate(true);
        //rt.DOScale(1, 0.45f).SetDelay(delay).SetEase(Ease.OutBack);
    }

    public void Hide()
    {
        //hoverSeq.Rewind();
        //cg.DOFade(0, 1f);
        //rt.DOScale(0, 0.2f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SkillManager.Instance.rerollWaitCnt >0) return;

        SoundEffect.Instance.Play(SoundList.UiHover);

        // kill any running hover tween
        _hoverTween?.Kill();
        // tween up to hoverScaleFactor * original
        _hoverTween = rt.DOScale(_originalScale * hoverScaleFactor, hoverDuration).SetEase(hoverEase).SetUpdate(true);

        _hoverTweenFrame?.Kill();
        _hoverTweenFrame = buttonFrame.DOScale(_frameOriginalScale * hoverScaleFactor, hoverDuration).SetEase(hoverEase).SetUpdate(true);

        //buttonFrameFx.SetOutline(true, Color.yellow);

        //Color rarityColor = DecideRarityColor();
        //buttonFrame.GetComponent<UIFXController>().SetOutlineColor(rarityColor);
        
    }

    public void UpdateFrameColorAndTexture()
    {
        Color rarityColor = DecideRarityColor();
        buttonFrame.GetComponent<UIFXController>().SetOutlineColor(rarityColor);
    }

    public Color DecideRarityColor()
    {
        Color rarityColor = Color.white;

        

       
            switch (SkillManager.Instance.rarityToLevelUp[cardIndex])
            {
                case OptionRarity.Normal:
                    //rarityColor = Color.white;
                    buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.normalColorTex);
                    buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
                    buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0));
                    break;
                case OptionRarity.Rare:
                    //rarityColor = new Color(0.2f, 0.8f, 0.2f); // Green
                    buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.rareColorTex);
                    buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
                    buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0)); //10, 0.5
                    break;
                case OptionRarity.Epic:
                    //rarityColor = new Color(0.8f, 0.2f, 0.8f); // Purple
                    buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.epicColorTex);
                    buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
                    buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0));
                    break;
                case OptionRarity.Legendary:
                    //rarityColor = new Color(1.0f, 0.84f, 0); // Gold
                    buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.LegendColorTex);
                    buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
                    buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0));
                    break;
       
                default:
                    rarityColor = Color.white; // Default to white if no match
                    break;
            }
        
        //else if(LocalizationManager.Instance.currentLanguage == GameLanguage.English)
        //{
        //    switch (rarityText.text)
        //    {
        //        case "Normal":
        //            //rarityColor = Color.white;
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.normalColorTex);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0));
        //            break;
        //        case "Rare":
        //            //rarityColor = new Color(0.2f, 0.8f, 0.2f); // Green
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.rareColorTex);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0)); //10, 0.5
        //            break;
        //        case "Epic":
        //            //rarityColor = new Color(0.8f, 0.2f, 0.8f); // Purple
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.epicColorTex);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0));
        //            break;
        //        case "Legendary":
        //            //rarityColor = new Color(1.0f, 0.84f, 0); // Gold
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.LegendColorTex);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0));
        //            break;
       
        //        default:
        //            rarityColor = Color.white; // Default to white if no match
        //            break;
        //    }
        //}
        //else if (LocalizationManager.Instance.currentLanguage == GameLanguage.ChineseSimple)
        //{
        //    switch (rarityText.text)
        //    {
        //        case "普通":
        //            //rarityColor = Color.white;
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.normalColorTex);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0));
        //            break;
        //        case "稀有":
        //            //rarityColor = new Color(0.2f, 0.8f, 0.2f); // Green
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.rareColorTex);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0)); //10, 0.5
        //            break;
        //        case "史诗":
        //            //rarityColor = new Color(0.8f, 0.2f, 0.8f); // Purple
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.epicColorTex);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0));
        //            break;
        //        case "传说":
        //            //rarityColor = new Color(1.0f, 0.84f, 0); // Gold
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.LegendColorTex);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0));
        //            break;
       
        //        default:
        //            rarityColor = Color.white; // Default to white if no match
        //            break;
        //    }
           
        //}
        //else if (LocalizationManager.Instance.currentLanguage == GameLanguage.ChineseTrad)
        //{
        //    switch (rarityText.text)
        //    {
        //        case "普通":
        //            //rarityColor = Color.white;
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.normalColorTex);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0));
        //            break;
        //        case "稀有":
        //            //rarityColor = new Color(0.2f, 0.8f, 0.2f); // Green
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.rareColorTex);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0)); //10, 0.5
        //            break;
        //        case "史詩":
        //            //rarityColor = new Color(0.8f, 0.2f, 0.8f); // Purple
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.epicColorTex);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0));
        //            break;
        //        case "傳說":
        //            //rarityColor = new Color(1.0f, 0.84f, 0); // Gold
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.LegendColorTex);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
        //            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0));
        //            break;
       
        //        default:
        //            rarityColor = Color.white; // Default to white if no match
        //            break;
        //    }
           
        //}

        

        if (SkillManager.Instance.isOptionEvolving[cardIndex])
        {
            rarityColor = Color.white;
            buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.EvolColorTex);
            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0));
        }

        if (SkillManager.Instance.isSelectingTrait)
        {
            //rarityColor = Color.red;
            buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.EnchantColorTex);
            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0));
        }

        if (SkillManager.Instance.isGetNewSkill)
        {
            buttonFrame.GetComponent<UIFXController>().SetOutlineTexture(SkillManager.Instance.NewSkillColorTex);
            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureEnable(true);
            buttonFrame.GetComponent<UIFXController>().SetOutlineTextureScroll(new Vector2(0, 0));
        }



        return rarityColor;

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // kill and tween back to original
        _hoverTween?.Kill();
        _hoverTween = rt.DOScale(_originalScale, hoverDuration).SetEase(hoverEase).SetUpdate(true);

        _hoverTweenFrame?.Kill();
        _hoverTweenFrame = buttonFrame.DOScale(_frameOriginalScale, hoverDuration).SetEase(hoverEase).SetUpdate(true);

        //buttonFrameFx.SetOutline(false, Color.yellow,1,1.5f,true);
        
        //Color rarityColor = DecideRarityColor();
        //buttonFrame.GetComponent<UIFXController>().SetOutlineColor(rarityColor);
        
        //buttonFrame.gameObject.SetActive(false);
    }

    public void ResetColorAndScale()
    {
        // Reset the scale to original
        rt.localScale = _originalScale;

        // Reset the color of the rarity text
        rarityText.color = Color.white;

        // Reset the button frame scale and outline color
        buttonFrame.localScale = _frameOriginalScale;
        Color rarityColor = DecideRarityColor();
        buttonFrame.GetComponent<UIFXController>().SetOutlineColor(rarityColor);
        //buttonFrame.gameObject.SetActive(false);

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SoundEffect.Instance.Play(SoundList.UiClick);

        // example click “pop”
        //_hoverTween?.Kill();
        //rt
        //  .DOPunchScale(_originalScale * 0.28f, 0.3f, vibrato: 7, elasticity: 1)
        //  .SetUpdate(true);

        ResetColorAndScale();
        
        
        
        //Button bt= rt.GetComponent<Button>();
        //bt?.onClick.Invoke();

        //SkillManager.Instance.OnOptionButtonPressed(0);
    }

    //public void OnPointerEnter(PointerEventData _) => hoverSeq.Play();
    //public void OnPointerExit (PointerEventData _) => hoverSeq.Rewind();
    //public void OnPointerClick(PointerEventData _)
    //{
    //    hoverSeq.Rewind();
    //    clickTween?.Kill();
    //    //clickTween = rt.DOPunchScale(Vector3.one * 0.2f, 0.3f, 8, 1);
        
    //    //UiFxPlayer.Instance.PlayClick();            // SFX/VFX
    //}

    public void SetRarityColor(Color c) => rarityText.color = c;




    public void Select()
{
    // Play sound only if this isn't already the selected card, to avoid spamming.
    // We will let SkillManager handle the sound.

    // kill any running hover tween
    _hoverTween?.Kill();
    // tween up to hoverScaleFactor * original
    _hoverTween = rt.DOScale(_originalScale * hoverScaleFactor, hoverDuration).SetEase(hoverEase).SetUpdate(true);

    _hoverTweenFrame?.Kill();
    _hoverTweenFrame = buttonFrame.DOScale(_frameOriginalScale * hoverScaleFactor, hoverDuration).SetEase(hoverEase).SetUpdate(true);
}

// 2. CREATE the new public Deselect() method by moving code from OnPointerExit
public void Deselect()
{
    // kill and tween back to original
    _hoverTween?.Kill();
    _hoverTween = rt.DOScale(_originalScale, hoverDuration).SetEase(hoverEase).SetUpdate(true);

    _hoverTweenFrame?.Kill();
    _hoverTweenFrame = buttonFrame.DOScale(_frameOriginalScale, hoverDuration).SetEase(hoverEase).SetUpdate(true);
}








}

