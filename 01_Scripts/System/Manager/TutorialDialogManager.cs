using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using DialogueEditor;

public class TutorialDialogManager : Singleton<TutorialDialogManager>
{
     public NPCConversation con;

    public bool isEndDialog = false;
    public bool isStartDialog = false;

    public float startTutorCnt = 2.1f;

    void Start()
    {
        
        //isStartDialog = true;
        //    Time.timeScale = 0f;
        //    ConversationManager.Instance.StartConversation(con);
    }

    public void EndDialog()
    {
        isEndDialog = true;

    }
    

    void Update()
    {
        
        //if (Input.GetKeyDown(KeyCode.Y))
        //{
        //    isStartDialog = true;
        //    Time.timeScale = 0f;
        //    ConversationManager.Instance.StartConversation(con);
        //}

        if (isStartDialog && isEndDialog)
        {
            isStartDialog = false;
            isEndDialog = false;
            Time.timeScale = 1f;
        }

        startTutorCnt -= Time.deltaTime;

        if (startTutorCnt <= 0)
        {
            startTutorCnt = 9999f;
            isStartDialog = true;
            Time.timeScale = 0f;
            ConversationManager.Instance.StartConversation(con);

        }


    }
}

