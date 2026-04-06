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

public class ModeCancelKeyIconContorller : MonoBehaviour
{
    [SerializeField] private Sprite isBoostModeSprite;
    [SerializeField] private Sprite isExchangeModeSprite;
    [SerializeField] private Image iconImage;
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        bool isMode = SkillManager.Instance.isBoostMode || SkillManager.Instance.isExchangeMode;
        if (isMode == true)
        {
            if (canvasGroup != null) { canvasGroup.alpha = 1; }

            // 「強化」と「交換」モードで別の画像を表示する
            if (SkillManager.Instance.isBoostMode == true)
            {
                iconImage.sprite = isBoostModeSprite;

            }
            else if (SkillManager.Instance.isExchangeMode == true)
            {
                iconImage.sprite = isExchangeModeSprite;
            }
        }
        else
        {
           if (canvasGroup != null) { canvasGroup.alpha = 0; }
        }
        
    }
}

