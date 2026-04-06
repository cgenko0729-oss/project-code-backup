using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class QuestManager : Singleton<QuestManager>
{
    public bool isQuestActivated = false; //is quest activated or not
    public bool isPublishedQuest = false;

    public GameObject questAPickMushroom;
    public GameObject questBKillGroupEnemy;
    public GameObject questC;

    public float questTriggerTimer = 180f;
    public float questTimeLimit = 120f; //quest A time limit (activate and finish it within 120 seconds)

    public int currentStageId = -1;


    public void Start()
    {
        currentStageId = StageManager.Instance.stageId;
    }

    private void Update()
    {
        ActivateQuest();

    }

    public void ActivateQuest()
    {
        questTriggerTimer -= Time.deltaTime;

        if (isPublishedQuest) return;

        if (TimeManager.Instance.gameTimeLeft < 420 && !isPublishedQuest)
        {
            isPublishedQuest = true;
            questTriggerTimer = 210f;
            

            //rand 1 to 2 quest a or quest b
            int randomQuest = Random.Range(0, 2); // 1 or 2
            if (randomQuest == 0)
            {             
                questAPickMushroom.gameObject.SetActive(true);
                questBKillGroupEnemy.gameObject.SetActive(true);
            }
            else if (randomQuest == 1)
            {
                questBKillGroupEnemy.gameObject.SetActive(true);
                questAPickMushroom.gameObject.SetActive(true);
            }
        }
    }

  



}

