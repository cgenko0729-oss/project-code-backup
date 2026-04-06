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
using DamageNumbersPro;

public class GameQuestHandler : MonoBehaviour
{
    public QuestName questName = QuestName.None;
    public QuestData questData;
    public float currentTimeLimit;
    public float currentProgress = 0f;
    public bool isQuestFinished = false;
    public bool isGroupKillQuest = false;

    public bool isPlayerWithinRange = false;
    public float currentQuestRange = 14f; 
    public int currentKillCount = 0;
    public int currentCollectedItems = 0;

    public Transform playerTransform;
    public Material questMaterial;
    public DestinationIndicator2D indicatorIcon;

    public string questText = "Quest:";  //inside quest 's description, show within questrange
    public DamageNumber damageNumPrefab;  //template used for spawning questpopup
    public DamageNumber questPopup;       //the autal string holder
    public TextMeshProUGUI questTextMesh; //use for icon 's time counter
    
    private void OnEnable()
    {
        EventManager.StartListening("QuestEnemyKill", OnEnemyKilled);
        //EventManager.StartListening("QuestItemCollected", OnItemCollected);
        EventManager.StartListening("QuestEliteKilled", OnEliteEnemyKilled);
        EventManager.StartListening("QuestBossKilled", OnBossDefeated);

        EventManager.StartListening(GameEvent.PushObjectArriveTarget,ObjectEnterTarget);

        EventManager.StartListening("QuestGhostGetPurify", OnGhostGetPurify);

        EventManager.StartListening("AddQuestProgress", OnAddQuestProgress);

    }
    
    private void OnDisable()
    {
        EventManager.StopListening("QuestEnemyKill", OnEnemyKilled);
        //EventManager.StopListening("QuestItemCollected", OnItemCollected);
        EventManager.StopListening("QuestEliteKilled", OnEliteEnemyKilled);
        EventManager.StopListening("QuestBossKilled", OnBossDefeated);

        EventManager.StopListening(GameEvent.PushObjectArriveTarget, ObjectEnterTarget);

        EventManager.StopListening("QuestGhostGetPurify", OnGhostGetPurify);

        if (questPopup != null) questPopup.DestroyDNP();

        ClearQuestState();
       
    }

    private void Awake()
    {
        questText = L.QuestName(questName);
        
    }

    public void Initialize(QuestData data)
    {
        questData = data;
        currentTimeLimit = data.timeLimit;
        currentQuestRange = data.questRange;

        SetupReferences();
        SetupUI();

        if(questData.questType == QuestType.KillGroupEnemyWithinRange || questData.questType == QuestType.KillBossEnemy || questData.questType == QuestType.KillEliteEnemy)
        {
            isGroupKillQuest = true;
            GameQuestManager.Instance.isKillingQuestActive = true;
        }

        EventManager.EmitEventData("QuestStarted", this.gameObject); // Play start effects

        //Debug.Log($"Quest Initialized: {questName}, Type: {questData.questType}, Time Limit: {currentTimeLimit}s");

    }

    private void SetupReferences()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        questMaterial = GetComponentInChildren<MeshRenderer>()?.material;
        
        
    }

    private void SetupUI()
    {
        questMaterial.SetFloat("_Progress", 0);  
        
        Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
        questPopup = damageNumPrefab.Spawn(spawnPos,questText,transform);   

         //questPopup.rightText = "-:0%";

        questPopup.enableRightText = true;
        questPopup.enableBottomText = true;

        indicatorIcon = GetComponentInChildren<DestinationIndicator2D>();
        questTextMesh = indicatorIcon.questText;

       

    }

    private void Update()
    {
        if (isQuestFinished) return;
        
        UpdateTimer();
        UpdateQuestProgress();
        UpdatePlayerRange();
        UpdateUI();
        
        CheckQuestCompletion();
        CheckQuestFailure();
    }
    
    private void UpdateTimer()
    {
        currentTimeLimit -= Time.deltaTime;
    }

    private void UpdateQuestProgress()
    {
        switch (questData.questType)
        {
            case QuestType.PickUpMushroom:
                UpdateMushroomQuest();
                break;
            case QuestType.KillGroupEnemyWithinRange:
                UpdateKillQuest();
                break;
            case QuestType.PushObjectToTarget:
                UpdatePushObjectToTarget();
                break;
                //case QuestType.CollectCoins:
                //    UpdateCollectionQuest();
                //    break;
        }
    }

    void UpdatePushObjectToTarget()
    {

    }

    void ObjectEnterTarget()
    {
        currentProgress += 51f;
    }

    private void UpdateMushroomQuest()
    {
        if (isPlayerWithinRange)
        {
            currentProgress += questData.progressRate * Time.deltaTime;
            currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
        }
        else
        {
            currentProgress -= questData.progressRate*0.49f * Time.deltaTime;
            currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
        }
    }

    private void UpdateKillQuest()
    {
        currentProgress = (float)currentKillCount / questData.targetAmount * 100f;
        currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
    }

    private void UpdatePlayerRange()
    {
        if (playerTransform == null) return;
        
        float distance = Vector3.Distance(playerTransform.position, transform.position);
        isPlayerWithinRange = distance <= questData.questRange;
    }

    private void UpdateUI()
    {
        questMaterial.SetFloat("_Progress", currentProgress / 100f);
        questPopup.rightText = currentProgress.ToString("F0") + "%";
        questPopup.bottomText = currentTimeLimit.ToString("F0") + "s";
        questPopup.UpdateText();
        questTextMesh.text = currentTimeLimit.ToString("F0") + "s";
        
        
    }

    private void CheckQuestCompletion()
    {
        if (currentProgress >= 100f)
        {
            CompleteQuest();
            ClearQuestState();
        }
    }

    private void CheckQuestFailure()
    {
        if (currentTimeLimit <= 0f)
        {
            FailQuest();
            ClearQuestState();
        }
    }

    public void ClearQuestState()
    {
        isGroupKillQuest = false;
        if(GameQuestManager.Instance)GameQuestManager.Instance.isKillingQuestActive = false;
    }

    private void CompleteQuest()
    {
        isQuestFinished = true;       
        indicatorIcon.isActivated = false;
               
        Quaternion spawnRotation = Quaternion.Euler(0, 180, 0);
        Instantiate(questData.treasureChestPrefab, transform.position, spawnRotation); // Spawn reward
  
        EventManager.EmitEventData("FinishQuest", transform.position); // Play completion effects
        SoundEffect.Instance.Play(SoundList.QuestFinishSe);
        
        GameQuestManager.Instance.OnQuestCompleted(gameObject,transform.position);
        Destroy(gameObject, 0.5f);

        


    }

    private void FailQuest()
    {
        //QuestManager.Instance.OnQuestFailed(gameObject);
        Destroy(gameObject, 0.5f);
    }

    private void OnEnemyKilled()
    {
        if (questData.questType == QuestType.KillGroupEnemyWithinRange && isPlayerWithinRange)
        {
            currentKillCount++;
        }
    }

    private void OnEliteEnemyKilled()
    {
        if (questData.questType == QuestType.KillEliteEnemy && isPlayerWithinRange)
        {
            currentKillCount++;
        }
    }
    private void OnBossDefeated()
    {
        if (questData.questType == QuestType.KillBossEnemy)
        {
            currentProgress = 100f;
        }
    }

    void OnGhostGetPurify()
    {
        currentProgress += 7f;
    }

    public void OnAddQuestProgress()
    {
        
        float addMount  = EventManager.GetFloat("AddQuestProgress");
        currentProgress += addMount;

    }


}

