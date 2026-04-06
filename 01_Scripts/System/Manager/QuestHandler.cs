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

public class QuestHandler : MonoBehaviour
{

    public float questTimeLimit = 120f;
    public float questProgress = 0;
    public bool isQuestFinished = false;

    public bool isPlayerWithinQuestRange = false; //is player within quest range
    public float questRange = 14f; //range of the quest, if player is within this range, quest will be activated
    public float questDistToPlayer;

    public int questEnemyKillCount = 0; //how many enemies player has killed for the quest
    public int questEnemyKillRequired = 100;

    public QuestType questType = QuestType.None;

    public Transform playerTrans;
    
    public Material mat;

    public bool isQuestInit = false;

    public float progressAddAmount = 7f;

    public Vector3 dataPos = Vector3.zero;


    public DamageNumber damageNumPrefab;
    DamageNumber questPopup;
    public string questText = "Quest:";

    public TextMeshProUGUI questTextMesh;
    public DestinationIndicator2D indicatorIcon;

    public GameObject treasureChest;

    public void OnEnable()
    {
        EventManager.StartListening("QuestEnemyKill", UpdaetQuestEnemyKill);
    }


    public void OnDisable()
    {
        EventManager.StopListening("QuestEnemyKill", UpdaetQuestEnemyKill);

        Destroy(gameObject, 0.5f); 
            if(questPopup)questPopup.DestroyDNP();
    }

    public void UpdaetQuestEnemyKill()
    {
        if(questType != QuestType.KillGroupEnemyWithinRange) return;
        if (!isPlayerWithinQuestRange) return;

        questEnemyKillCount++;

        //dataPos = (Vector3)EventManager.GetData("QuestEnemyKill");
    }

    void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;

        mat = GetComponentInChildren<MeshRenderer>().material;
        mat.SetFloat("_Progress", 0);

        Vector3 spawnPos =transform.position + Vector3.up * 1.5f;
        questPopup = damageNumPrefab.Spawn(spawnPos,questText,transform);   
        questPopup.enableRightText = true;
        questPopup.enableBottomText = true;

        indicatorIcon = GetComponentInChildren<DestinationIndicator2D>();
        questTextMesh = indicatorIcon.questText;

    }

    void Update()
    {

        questTimeLimit -= Time.deltaTime;

        if(questTimeLimit <= 0 && !isQuestFinished)
        {
            Destroy(gameObject, 0.5f); 
            questPopup.DestroyDNP();
        }

        if (questProgress >= 100 && !isQuestFinished)
        {
            isQuestFinished = true;
            
            indicatorIcon.isActivated = false;
            EventManager.EmitEventData("FinishQuest",transform.position);
            Destroy(gameObject, 0.5f);
            questPopup.DestroyDNP();

            //CoinSpawner.Instance.SpawnWithDelay(transform.position + new Vector3(0,0.5f,0));

            Quaternion spawnRotation = Quaternion.Euler(0, 180, 0);

            Instantiate(treasureChest, transform.position + new Vector3(0, 0f, 0), spawnRotation);

            SoundEffect.Instance.Play(SoundList.QuestFinishSe);
        }

        UpdateKillEliteEnemyQuest();
        UpdateTraceRareEnemyQuest();
        UpdatePickMushRoomQuest();
        UpdateKillGroupEnemyQuest();

        CalIfPlayerWithQuestRange();
        mat.SetFloat("_Progress", questProgress/100);

        if(!questPopup) return;
        questPopup.rightText = questProgress.ToString("F0") + "%";
        questPopup.bottomText = questTimeLimit.ToString("F0") + "s";
        questPopup.UpdateText();

        if(questTextMesh)questTextMesh.text = questTimeLimit.ToString("F0") + "s";
        else
        {
            Debug.LogWarning("Quest Text Mesh is null, please assign it in the inspector.");
        }
    }

    public void UpdatePickMushRoomQuest()
    {
        if (questType != QuestType.PickUpMushroom) return;

        if (isPlayerWithinQuestRange)
        {
            questProgress += progressAddAmount * Time.deltaTime;
        }
        

    }

    public void UpdateKillGroupEnemyQuest()
    {
        if(questType != QuestType.KillGroupEnemyWithinRange) return;

        if (!isQuestInit)
        {
            isQuestInit = true;
            questEnemyKillCount = 0;
            questEnemyKillRequired = 50;
            questProgress = 0f;
        }

        QuestManager.Instance.isQuestActivated = isPlayerWithinQuestRange;

        questProgress = (float)questEnemyKillCount / questEnemyKillRequired * 100f;


    }



    public void UpdateKillEliteEnemyQuest()
    {
        if (questType != QuestType.KillEliteEnemy) return;


    }

    public void UpdateTraceRareEnemyQuest()
    {
        if (questType != QuestType.TraceRareEnemy) return;

    }

    public void CalIfPlayerWithQuestRange()
    {
        if (playerTrans == null) return;

        questDistToPlayer = Vector3.Distance(playerTrans.position, transform.position);
        isPlayerWithinQuestRange = questDistToPlayer <= questRange;
        


    }

        //draw a debug sphere to visualize the quest range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, questRange);
    }

    private void OnDrawGizmos()
    {
        //OnDrawGizmosSelected();
    }





}

