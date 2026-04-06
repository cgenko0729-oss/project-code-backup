using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine

//coroutine
using System.Collections;

public class CutSceneManager : Singleton<CutSceneManager>
{

    public GameObject cutSceneObjBossSpider;
    public GameObject cutSceneObjBossTurnipa;
    public GameObject cutSceneObjBossDragon;
    public GameObject cutSceneObjBossAnubis;

     public GameObject playerObj;

    public GameObject spiderBossObj;
    public GameObject spiderBossHpBar;

    public GameObject turnipaBossObj;
    public GameObject turnipaBossHpBar;

    public GameObject dragonBossObj;

    public GameObject anubisBossObj;


    public GameObject bossFieldSpider;
    public GameObject bossFieldTurnipa;
    public GameObject bossFieldDragon;
    public GameObject bossFieldAnubis;

    public GameObject bossCutSceneSkipButton; //ボス戦カットシーンのスキップボタン

    public Transform mainCamCurrentTrans;
    public Vector3 camPos;
    public Quaternion camRot;

    public float cutSceneCnt;
    public float cutSceneCntMax = 7f;
    public bool isCutSceneStarted = false;

    public RectTransform cutSceneFrame;
    public Vector2 frameScaleStart = new Vector2(19.3f, 14f);
    public Vector2 frameScaleEnd = new Vector2(19.3f, 12.21f);

    public bool isBossFightStarted = false;

    public GameObject cutSceneSkipButtonkObj;


    void Start()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");

    }

    void Update()
    {
    
        if ( (TimeManager.Instance.gameTimeLeft <=0) && !CutSceneManager.Instance.isBossFightStarted && !StageManager.Instance.isEndlessMode && !QuickRunModeManager.Instance.isQuickRunMode && !GameManager.Instance.isGameOver)
        {
            CutSceneManager.Instance.ClearAllGameObjectForCutScene();

        }


    }

    public void ClearAllGameObjectForCutScene()
    {
        isBossFightStarted = true;
        GameManager.Instance.isBossFight = true;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            EnemyStatusBase es = enemy.GetComponent<EnemyStatusBase>();
            es.DeadNoExp();
        }

        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            Destroy(item);
        }

        GameObject[] quests = GameObject.FindGameObjectsWithTag("Quest");
        foreach (GameObject quest in quests)
        {
            quest.SetActive(false);
        }

        GameObject[] skill = GameObject.FindGameObjectsWithTag("Skill");
        foreach (GameObject sk in skill)
        {

            if (sk.TryGetComponent<SkillModelBase>(out SkillModelBase skillModel))
            {
                 skillModel.ClearOutSkill();
            }
   

            //debug log 
            //Debug.Log($"Clear Skill: {sk.name}");
        }

        EventManager.EmitEvent(GameEvent.CutSceneStart);
        TransitionController.Instance.DoFadeTransition();

        HIdeManager.Instance.HideSpawners();
        HIdeManager.Instance.HideCutsceneUiObjects();

        Invoke(nameof(StartBossFightCutScene),2f);
    }

    public void RestoreAllUiElement() 
    {

        HIdeManager.Instance.RestoreCutsceneUiObjects();
    }


    public void ActivateBoss()
    {

        if(StageManager.Instance.mapData.mapType == MapType.SpiderForest)
        {
            bossFieldSpider.SetActive(true);
           spiderBossObj.SetActive(true);
            spiderBossHpBar.SetActive(true);
        }
        else if (StageManager.Instance.mapData.mapType == MapType.AncientForest)
        {
            bossFieldTurnipa.SetActive(true);
            turnipaBossObj.SetActive(true);
            turnipaBossHpBar.SetActive(true);
        }
        else if (StageManager.Instance.mapData.mapType == MapType.Desert)
        {
            bossFieldDragon.SetActive(true);
            dragonBossObj.SetActive(true);
            //dragonBossHpBar.SetActive(true);
            turnipaBossHpBar.SetActive(true);
        }
        else if (StageManager.Instance.mapData.mapType == MapType.Temple)
        {
            bossFieldAnubis.SetActive(true);         
            anubisBossObj.SetActive(true);
            turnipaBossHpBar.SetActive(true);
        }




    }

    public void StartBossFightCutScene()
    {
 
        bossCutSceneSkipButton.gameObject.SetActive(true); //ボス戦カットシーンのスキップボタンを表示

        playerObj.transform.position = new Vector3(1000, 1000, 0); 

        isCutSceneStarted = true;
        cutSceneSkipButtonkObj.SetActive(true);

        camPos = Camera.main.transform.position;   
        camRot = Camera.main.transform.rotation;
        
        if(StageManager.Instance.mapData.mapType == MapType.SpiderForest)
        {
           Invoke(nameof(StopTime), 1f); 
           cutSceneObjBossSpider.SetActive(true);
        }
        else if (StageManager.Instance.mapData.mapType == MapType.AncientForest)
        {
            //cutSceneObjBossTurnipa.SetActive(true);
            //EndBossFightCutScene();
            //StartCoroutine(DelayEndBossFight());
            
            cutSceneObjBossTurnipa.SetActive(true);
            Invoke(nameof(StopTime), 1f);

        }
        else if (StageManager.Instance.mapData.mapType == MapType.Desert)
        {
            cutSceneObjBossDragon.SetActive(true);
            Invoke(nameof(StopTime), 1f);
        }
        else if (StageManager.Instance.mapData.mapType == MapType.Temple)
        {
            cutSceneObjBossAnubis.SetActive(true);
            Invoke(nameof(StopTime), 1f);
        }

        cutSceneFrame.gameObject.SetActive(true);
        cutSceneFrame.localScale = frameScaleStart;
        cutSceneFrame.DOScale(frameScaleEnd, 1f).SetEase(Ease.OutBack).OnComplete(() => {
            
        });

    }

    public void StopTime()
    {
        Time.timeScale = 0;
    }

    IEnumerator DelayEndBossFight()
    {
        yield return new WaitForSecondsRealtime(0.21f);
        EndBossFightCutScene();
    }

    public void EndBossFightCutScene()
    {
      
        if(cutSceneObjBossSpider)cutSceneObjBossSpider.SetActive(false);
        if(cutSceneObjBossTurnipa)cutSceneObjBossTurnipa.SetActive(false);
        if(cutSceneObjBossDragon)cutSceneObjBossDragon.SetActive(false);
        if (cutSceneObjBossAnubis) cutSceneObjBossAnubis.SetActive(false);

        playerObj.transform.position = new Vector3(0, 0, 0); 

        //if(StageManager.Instance.mapData.mapType == MapType.SpiderForest)
        //{
        //    cutSceneObjBossSpider.SetActive(false);
        //    playerObj.transform.position = new Vector3(0, 0, 0); 
        //}
        //if (StageManager.Instance.mapData.mapType == MapType.AncientForest)
        //{
        //    cutSceneObjBossTurnipa.SetActive(false);
        //    //playerObj.transform.position = new Vector3(32, 0, -44);
        //    playerObj.transform.position = new Vector3(0, 0, 0); 
        //}

        bossCutSceneSkipButton.gameObject.SetActive(false);
        isCutSceneStarted = false;       

        Camera.main.transform.position = Vector3.zero;
        Camera.main.transform.rotation = Quaternion.identity;

        EventManager.EmitEvent(GameEvent.CutSceneEnd);    
        EventManager.EmitEvent(GameEvent.StartBossFight);

        CameraShake.Instance.StartPhrase2GroundShake();

        Invoke(nameof(ActivateBoss), 1.5f); 
        Invoke(nameof(RestoreAllUiElement), 3f);
   
        cutSceneFrame.DOScale(frameScaleStart, 1f).SetEase(Ease.InBack).OnComplete(() => {
            cutSceneFrame.gameObject.SetActive(false);
        });

        Time.timeScale = 1;
        GameManager.Instance.isBossFight = true;

        cutSceneSkipButtonkObj.SetActive(false);

    }

}

