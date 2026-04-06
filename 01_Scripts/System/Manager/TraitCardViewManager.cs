using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class TraitCardViewManager : Singleton<TraitCardViewManager>
{

    [Searchable][PreviewSprite]public List<TraitData> allTraitDatas;
    
    public  Transform contentContainer;

    public GameObject traitCardPrefabObj;

    public GameObject detailedcard;
    public GameObject detailCardBackground;

    public float viewCooldown = 0f;

    public TextMeshProUGUI finishTraitCollectStatusText;
    public TextMeshProUGUI skillCollectionStatusText;

    public int totalTraitNum = 0;
    public int unlockedTraitNum = 0;
    public float traitCollectRate = 0f;

    private void OnEnable()
    {
        EventManager.StartListening(GameEvent.LanguageChanged, PopulateMenu);

    }

    private void OnDisable()
    {
        EventManager.StopListening(GameEvent.LanguageChanged, PopulateMenu);

    }

    void Start()
    {
        PopulateMenu();
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.M)){
        //    PopulateMenu();
        //}

        viewCooldown -= Time.deltaTime;

    }

    public void UpdateTraitCollectStatus()
    {
        CheckCardsUnlockStatus();

        totalTraitNum = allTraitDatas.Count;
        unlockedTraitNum = 0;

        foreach (TraitData data in allTraitDatas)
        {
            if (AchievementManager.Instance.IsTraitUnlocked(data.traitType))
            {
                unlockedTraitNum++;
            }
        }

        traitCollectRate = ((float)unlockedTraitNum / (float)totalTraitNum) * 100f;

        finishTraitCollectStatusText.text = $"{L.UI("title.EnchantmentCollectionPercentage")}:{traitCollectRate:F0}%({unlockedTraitNum}/{totalTraitNum})";

        int skillUnlockedNum = 18;
        int totalSkillNum = 18;
        float skillCollectRate = ((float)skillUnlockedNum / (float)totalSkillNum) * 100f;
        skillCollectionStatusText.text = $"{L.UI("title.EnchantmentCollectionPercentage")}:{skillCollectRate:F0}%({skillUnlockedNum}/{totalSkillNum})";

    }

    public void CheckCardsUnlockStatus()
    {

        //loop all allTraitDatas
        foreach (TraitData data in allTraitDatas)
        {
            data.CheckUnlockStatus();
        }

    }

    public void ShowDetailCard(TraitData data)
    {
        detailedcard.SetActive(true);
        MenuOpenAnimator ani = detailedcard.GetComponentInParent<MenuOpenAnimator>();
        ani.PlayeMenuAni(true);
        TraitUiDisplay uiDisplay = detailedcard.GetComponent<TraitUiDisplay>();
        uiDisplay.SetUpCard(data);

        detailCardBackground.SetActive(true);

    }

    public void PopulateMenu()
    {
        UpdateTraitCollectStatus();

        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        foreach(TraitData data in allTraitDatas)
        {
            GameObject traitCard = Instantiate(traitCardPrefabObj, contentContainer);
            TraitUiDisplay uiDisplay = traitCard.GetComponent<TraitUiDisplay>();
            uiDisplay.SetUpCard(data);
        }

    }


}

