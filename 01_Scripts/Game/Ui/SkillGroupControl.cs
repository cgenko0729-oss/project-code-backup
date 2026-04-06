using DG.Tweening;
using System.Collections.Generic;
using TigerForge;
using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillGroupControl : MonoBehaviour,
    IPointerEnterHandler,IPointerExitHandler,ISelectHandler,IDeselectHandler
{
    [Header("スキル表示に必要な情報")]
    public Image skillImage;
    public TextMeshProUGUI skillName;
    public List<GameObject> lvStarIcons;
    public int skillNowLv;
    public int skillMaxLv;
    public int casterId;
    private SkillManager sm;

    [Header("スキルウィンドウに表示中の情報")]
    [SerializeField] public GameObject IconGroup;


    [SerializeField] public float scaleDuration;
    [SerializeField] public float addScale;
    private Vector3 defaultScale;
    private Tween animeTween;
    private RectTransform iconGroupTrans;

    public int CasterId
    {
        set => casterId = value;
    }

    private void OnDisable()
    {
        animeTween?.Kill();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sm = SkillManager.Instance;
        foreach(var star in lvStarIcons)
        {
            star.SetActive(false);
        }

        if (IconGroup == null)
        {
            Debug.Log("IcouGroupがNULLです");
        }
        else
        {
            defaultScale =
                IconGroup.gameObject.transform.localScale;
            iconGroupTrans =
                IconGroup.gameObject.GetComponent<RectTransform>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        SkillCasterBase skill = sm.activeSkillCasterCollections[casterId];
        skillName.text = L.SkillName(skill.casterIdType);
        Sprite Image = skill.casterSpriteImage;
        skillImage.sprite = Image;

        skillMaxLv = skill.casterLevelMax;
        if (skillNowLv != skill.casterLevel)
        {
            skillNowLv = skill.casterLevel;
            ApplyLvStarIcons();
        }

        gameObject.SetActive(skill.isActiveAndEnabled);
    }

    void ApplyLvStarIcons()
    {
        for (int n = 0; n < skillNowLv - 1; n++) 
        {

            if (n >= lvStarIcons.Count) break;

            if (lvStarIcons[n] != null && lvStarIcons[n].activeSelf == false)
            {
                lvStarIcons[n].SetActive(true);
            }
        }
    }
    private void ScaleAnimation(Vector3 scale,float animeDuration)
    {
        animeTween?.Kill();
        animeTween = iconGroupTrans.DOScale(scale, animeDuration).SetUpdate(0, true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventManager.EmitEventData(GameEvent.OnActiveSkillDataWindow, casterId);
        ScaleAnimation(defaultScale * addScale, scaleDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EventManager.EmitEvent(GameEvent.OnInactiveSkillDataWindow);
        ScaleAnimation(defaultScale, scaleDuration);
    }

    public void OnSelect(BaseEventData eventData)
    {
        EventManager.EmitEventData(GameEvent.OnActiveSkillDataWindow, casterId);
        ScaleAnimation(defaultScale * addScale, scaleDuration);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        EventManager.EmitEvent(GameEvent.OnInactiveSkillDataWindow);
        ScaleAnimation(defaultScale, scaleDuration);
    }

}
