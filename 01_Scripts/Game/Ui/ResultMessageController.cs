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

public class ResultMessageController : MonoBehaviour
{
    public enum DetailPageType
    {
        None,
        SkillPage,
        PlayerPage,
        EnemyPage

    }

    public DetailPageType currentDetailPage = DetailPageType.None;

    public CanvasGroup cg;

    private void OnEnable()
    {
        EventManager.StartListening("DetailPageChanged", SwitchMessage);
    }

    private void OnDisable()
    {
        EventManager.StopListening("DetailPageChanged", SwitchMessage);
    }

    void SwitchMessage()
    {
        if(ResultSkillDisplayDataHolder.Instance.detailedPageNum == 1)
        {
            if(currentDetailPage != DetailPageType.SkillPage)
            {
                cg.alpha = 0;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
            else
            {
                cg.alpha = 1;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
        else if (ResultSkillDisplayDataHolder.Instance.detailedPageNum == 2)
        {
            if (currentDetailPage != DetailPageType.PlayerPage)
            {
                cg.alpha = 0;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
            else
            {
                cg.alpha = 1;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
        else if (ResultSkillDisplayDataHolder.Instance.detailedPageNum == 3)
        {
            if (currentDetailPage != DetailPageType.EnemyPage)
            {
                cg.alpha = 0;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
            else
            {
                cg.alpha = 1;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }

    }

    void Start()
    {
        cg = GetComponent<CanvasGroup>();

    }

    void Update()
    {
        
    }

    


}

