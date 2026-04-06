using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class ShopManager : Singleton<ShopManager>
{
   
    //public CanvasGroup shopWindowParentCg;
    public PurchaseMenuAnimator purchaseMenu;

    [Header("タイトルUIの表示・非表示用")]
    public GameObject titleGroup;
    public GameObject languageFlagBtnList;
    
    public float buyCooltime = 777f;

    public float resetSkillTreeUiCooltime = 7f;

    public float buffManagerResetCnt = 7f;

    public void OpenShopWindow()
    {
        //shopWindowParentCg.gameObject.SetActive(true);
        //shopWindowParentCg.alpha = 1f;
        //shopWindowParentCg.blocksRaycasts = true;
        //shopWindowParentCg.interactable = true;

        // タイトルUIを非表示にする
        buyCooltime = 1.4f;
        titleGroup.SetActive(false);
        languageFlagBtnList.SetActive(false);
        purchaseMenu.gameObject.SetActive(true);
        purchaseMenu.Show();

        CurrencyManager.Instance.OpenCoinTextDisplay();
        SoundEffect.Instance.Play(SoundList.ShopOpenSe);

        

    }

    public void CloseShopWindow()
    {
        purchaseMenu.Hide();
        SoundEffect.Instance.Play(SoundList.ShopOpenSe);

        // タイトルUIを表示にする
        titleGroup.SetActive(true);
        languageFlagBtnList.SetActive(true);
    }

    public void InitShop()
    {
        //purchaseMenu.InitData
    }


    public void Update()
    {

        //buffManagerResetCnt -= Time.deltaTime;
        //if(buffManagerResetCnt <= 0f)
        //{
        //    buffManagerResetCnt = 77777f;
        //    BuffManager.Instance.ResetAllStatus();
        //}

        resetSkillTreeUiCooltime -= Time.deltaTime;
        if(resetSkillTreeUiCooltime <= 0f)
        {
            resetSkillTreeUiCooltime = 77777f;

            if (!AchievementManager.Instance.progressData.isSkillTreeUiInited)
            {
                if(CharaUpgradeShopManager.Instance != null)
                {
                    CharaUpgradeShopManager.Instance.ResetAllUpgradeProgress();
                }
                AchievementManager.Instance.progressData.isSkillTreeUiInited = true;
                AchievementManager.Instance.SaveProgress();
            }

            
        }

        //Debug
        //if(Input.GetKeyDown(KeyCode.P))
        //{
        //    OpenShopWindow();
        //}

        buyCooltime -= Time.deltaTime;

        Time.timeScale = 1f;

    }

    public void Start()
    {
        InitShop();

      

    }

}

