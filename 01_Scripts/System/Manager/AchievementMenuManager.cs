using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

public class AchievementMenuManager : Singleton<AchievementMenuManager>
{
    public GameObject trophyPageObj;
    public GameObject traitCardListPageObj;
    public GameObject skillListPageObj;

    public  GameObject achievementItemPrefab;

    public  Transform contentContainer;

    public TextMeshProUGUI finishStatusText;

    public GameObject previousBtn;
    public GameObject nextBtn;


    void OnEnable()
    {
        //PopulateMenu();
    }

    void Start()
    {
        
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.M))
        //{
        //    PopulateMenu();
        //}

        if (InputDeviceManager.Instance.GetLastUsedDevice() is Gamepad)
        {
            previousBtn.GetComponent<Button>().interactable = false;
            nextBtn.GetComponent<Button>().interactable = false;
        }
        else
        {
            previousBtn.GetComponent<Button>().interactable = true;
            nextBtn.GetComponent<Button>().interactable = true;
        }
    }

    

    public void OpenTrophyPage()
    {
        trophyPageObj.SetActive(true);
        traitCardListPageObj.SetActive(false);

        GamePadManager.Instance.ResetScrollToTop();
    }



    public void AchNextPage()
    {
        if (trophyPageObj.activeSelf)
        {
            trophyPageObj.SetActive(false);
            skillListPageObj.SetActive(false);
            traitCardListPageObj.SetActive(true);
            GamePadManager.Instance.ResetScrollToTop();
            return;
        }

        if (traitCardListPageObj.activeSelf)
        {
            traitCardListPageObj.SetActive(false);
            trophyPageObj.SetActive(false);
            skillListPageObj.SetActive(true);
            GamePadManager.Instance.ResetScrollToTop();
            return;
        }

        if (skillListPageObj.activeSelf)
        {
            skillListPageObj.SetActive(false);
            traitCardListPageObj.SetActive(false);
            trophyPageObj.SetActive(true);
            GamePadManager.Instance.ResetScrollToTop();
            return;
        }

    }

    public void AchPreviousPage()
    {
        if (trophyPageObj.activeSelf)
        {
            trophyPageObj.SetActive(false);
            skillListPageObj.SetActive(true);
            traitCardListPageObj.SetActive(false);
            GamePadManager.Instance.ResetScrollToTop();
            return;
        }

        if (traitCardListPageObj.activeSelf)
        {
            traitCardListPageObj.SetActive(false);
            trophyPageObj.SetActive(true);
            skillListPageObj.SetActive(false);
            GamePadManager.Instance.ResetScrollToTop();
            return;
        }

        if (skillListPageObj.activeSelf)
        {
            skillListPageObj.SetActive(false);
            traitCardListPageObj.SetActive(true);
            trophyPageObj.SetActive(false);
            GamePadManager.Instance.ResetScrollToTop();
            return;
        }
    }

    public void PopulateMenu()
    {
        int achCount = AchievementManager.Instance.achTotalNum;
        int achFinishedCount = AchievementManager.Instance.achUnlockedNum;
        float achCompletionRate = AchievementManager.Instance.achFinishRate;

        //äÆê¨ìx:0%(0/50)
        finishStatusText.text = $"{L.UI("title.AchievementFinishPercentage")}:{achCompletionRate:F0}%({achFinishedCount}/{achCount})";


        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        List<Achievement> allAchievements = AchievementManager.Instance.allAchievements;
        AchievementSaveData saveData = AchievementManager.Instance.progressData;


        foreach (Achievement ach in allAchievements)
        {
            // 4. Create an instance of the prefab
            GameObject itemGO = Instantiate(achievementItemPrefab, contentContainer);

            // 5. Get the UI item script and set it up
            AchievementUiItem uiItem = itemGO.GetComponent<AchievementUiItem>();
            if (uiItem != null)
            {
                uiItem.Setup(ach, saveData);
            }
        }


        skillListPageObj.SetActive(false);
        traitCardListPageObj.SetActive(false);
        trophyPageObj.SetActive(true);


    }

}

