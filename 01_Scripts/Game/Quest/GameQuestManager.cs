using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class GameQuestManager : Singleton<GameQuestManager>
{
   
    public List<StageQuestConfig> stageConfigs = new List<StageQuestConfig>();

    public bool isQuestSystemActive = true; //overall is quest exist in game 
    public float questTriggerTimer;
    public int currentStageId = -1;
    public StageQuestConfig currentStageConfig;
    public List<GameObject> activeQuests = new List<GameObject>();

    public bool isKillingQuestActive = false; //is killing quest active

    public Image questMessageImage;
    public AudioClip questAcitveSe;

    private int nextQuestIndex = 0;

    public TextMeshProUGUI goToQuestMessageObj;

    public GameObject enchantChestObj;

    public Vector3 questFoucuPos;

    public GameObject questFinishEffect;

    private void Start()
    {
        //currentStageId = (int)StageManager.Instance.mapData.mapType;
        
        LoadStageConfiguration();
        ResetQuestTimer();
    }

    private void Update()
    {
        if (!isQuestSystemActive) return;
        UpdateQuestTrigger();
    }

    public void SpawnEnchantChest(Vector3 spawnPos)
    {

        if (enchantChestObj == null) return;
        Quaternion spawnRotation = Quaternion.Euler(0, 180, 0);
        Instantiate(enchantChestObj, spawnPos, spawnRotation);

    }

    private void LoadStageConfiguration()
    {
        currentStageConfig = stageConfigs.Find(config => config.stageType == StageManager.Instance.mapData.mapType);
        
        if (currentStageConfig == null) //save check
        {
            //Debug.LogWarning($"No quest configuration found for stage {currentStageId}");
            isQuestSystemActive = false;
        }
        else nextQuestIndex = 0; 
    }

    private void UpdateQuestTrigger()
    {
        questTriggerTimer -= Time.deltaTime;
        
        if (questTriggerTimer <= 0 && CanActivateNewQuest() && nextQuestIndex < 2)
        {
            ActivateNewQuest();
            ResetQuestTimer();
        }
    }
    
    private bool CanActivateNewQuest()
    {
        return activeQuests.Count < currentStageConfig.maxConcurrentQuests && TimeManager.Instance.gameTimeLeft > 149f; 
    }

    public void ActivateNewQuest()
    {
        if (currentStageConfig.availableQuests.Count == 0) return;
        
        //QuestData selectedQuest = GetWeightedRandomQuest();
        //if (selectedQuest != null)
        //{
        //    SpawnQuest(selectedQuest);
        //}

        QuestData selectedQuest = currentStageConfig.availableQuests[nextQuestIndex];
        nextQuestIndex++;
        if (selectedQuest != null) SpawnQuest(selectedQuest);
   

        DisplayQuestMessage();
        SoundEffect.Instance.PlayOneSound(questAcitveSe, 0.14f);

    }

    public void DisplayQuestMessage()
    {
        //dowtween scale questMessageImage from scale.y 's 0 to 1.49 in 0.49f sec
        questMessageImage.transform.localScale = new Vector3(5.6f, 0f, 1f);
        Color originalColor = questMessageImage.color;
        questMessageImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.21f);

        //also fade in alpha of questMessageImage from 0 to 1 in 0.28f sec: 
        questMessageImage.DOFade(1f, 0.21f).SetEase(Ease.InOutSine).OnComplete(() => {
            //after 2.49f sec ,fade out alpha from 1 to 0 in 0.49f sec
            //DOVirtual.DelayedCall(2.21f, () => {
            //    questMessageImage.DOFade(0f, 0.49f).SetEase(Ease.InOutSine);
            //});
        });




        questMessageImage.transform.DOScaleY(1.49f, 0.49f).SetEase(Ease.OutBack).OnComplete(() => {
            //after 2.49f sec ,downtween scale.y from 1.49 to 0 in 0.49f sec
            DOVirtual.DelayedCall(2.1f, () => {
                questMessageImage.transform.DOScaleY(0f, 0.49f).SetEase(Ease.InBack);
                DisplayerGoToQuestMessage();
            });
        });
    }

    public void DisplayerGoToQuestMessage()
    {
        CanvasGroup can = goToQuestMessageObj.GetComponent<CanvasGroup>();

        goToQuestMessageObj.gameObject.SetActive(true);
        can.DOFade(1f, 0.21f).SetEase(Ease.InOutSine);

        

        //set active false after 4.49f sec
        DOVirtual.DelayedCall(35f, () => {
            can.DOFade(0f, 0.49f).SetEase(Ease.InOutSine).onComplete = () => {
                goToQuestMessageObj.gameObject.SetActive(false);
            };

        });

    }

    private QuestData GetWeightedRandomQuest()
    {
        if (currentStageConfig.questWeights.Count != currentStageConfig.availableQuests.Count)
        {
            // Fallback to simple random if weights are not properly configured
            int randomIndex = Random.Range(0, currentStageConfig.availableQuests.Count);
            return currentStageConfig.availableQuests[randomIndex];
        }
        
        float totalWeight = 0f;
        foreach (float weight in currentStageConfig.questWeights)
        {
            totalWeight += weight;
        }
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        for (int i = 0; i < currentStageConfig.availableQuests.Count; i++)
        {
            currentWeight += currentStageConfig.questWeights[i];
            if (randomValue <= currentWeight)
            {
                return currentStageConfig.availableQuests[i];
            }
        }
        
        return currentStageConfig.availableQuests[0]; // Fallback
    }

    private void SpawnQuest(QuestData questData)
    {
        Vector3 spawnPosition = questData.spawnPos;
        GameObject questInstance = Instantiate(questData.questPrefab, spawnPosition, Quaternion.identity);
        questFoucuPos = spawnPosition;

        GameQuestHandler questHandler = questInstance.GetComponent<GameQuestHandler>();
        if (questHandler != null)
        {
            questHandler.Initialize(questData);
            activeQuests.Add(questInstance);
        }

        Invoke(nameof(DelayPanToQuest), 0.7f);
    }

    private void DelayPanToQuest()
    {
        CameraViewManager.Instance.PanToQuestLocation(questFoucuPos);

        EffectManager.Instance.SpawnQuestTraceTrailEffectDelay(questFoucuPos);
    }

    private void ResetQuestTimer()
    {
        //questTriggerTimer = currentStageConfig?.questTriggerInterval ?? 180f;
        //questTriggerTimer += nextQuestIndex * 300f;

        if (questTriggerTimer == 0) questTriggerTimer = 35f; //1min
        else if (questTriggerTimer == 1) questTriggerTimer = 219f; //5min
        else questTriggerTimer = 219f;

    }

    public void OnQuestCompleted(GameObject questObject,Vector3 pos)
    {
        activeQuests.Remove(questObject);

        //cast a sphere cast with 5 radius to search for enemy with layer "EnemySpider","EnemyBomber"

        

        //Debug.Log("ClearEnemy and spawn Effect");

        


        


    }

    public void ClearAllEnemyQuest(Vector3 pos)
    {
        //dotween delay 0.49f to pan camera to player
        DOVirtual.DelayedCall(0.35f, () => {
            Instantiate(questFinishEffect, pos, Quaternion.identity);

            Collider[] hitColliders = Physics.OverlapSphere(pos, 11f, LayerMask.GetMask("EnemySpider", "EnemyBat","EnemyMushroom","EnemyDragon"), QueryTriggerInteraction.Ignore);
            foreach (Collider hitCollider in hitColliders)
            {
                EnemyStatusBase enemy = hitCollider.GetComponent<EnemyStatusBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(99);
                }
            }
        });
    }


    public void OnQuestFailed(GameObject questObject)
    {
        activeQuests.Remove(questObject);
    }




}

