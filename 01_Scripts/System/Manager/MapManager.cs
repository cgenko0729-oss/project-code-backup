using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;


public class MapManager : Singleton<MapManager>
{
    public float xBoundMax;
    public float zBoundMax;
    public float xBoundMin;
    public float zBoundMin;

    public GameObject mapPrefab = null;

    public GameObject spawnerPrefabSpiderForest ;
    public GameObject spawnerPrefabAncientForest;
    public GameObject spawnerPrefabCastle;
    public GameObject spawnerPrefabDesert;
    public GameObject spawnerPrefabTemple;

    public GameObject bossFieldPrefab = null;

    public GameObject LightManager;

    private Vector3 loadPos = Vector3.zero;

    public MapType nowMap = MapType.None;



    public int mapExpCount = 0;
    public int tooManyExpObj = 1000;

    public float magnetSpawnCntMax = 30f;
    public float magnetSpawnCnt = 30f;

    public GameObject magnetPrefab;
    public Transform playerTrans;

    private AsyncOperationHandle<GameObject> currentMapHandle;

    public int expTraitCnt = 0;

    public GameObject loadingImageAnimationObj;

    public Image loadingBlackScreenImage;

    public void Start()
    {

        loadingBlackScreenImage.gameObject.SetActive(true);

        nowMap = StageManager.Instance.mapData.mapType;

        LoadMapById();

        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;

    }

    public void LoadMapById()
    {

        //string key = null;

        //switch (nowMap)
        //{
        //    case MapType.SpiderForest:
        //        loadPos = new Vector3(0, -0.21f, 0);
        //        mapPrefab = Resources.Load<GameObject>("Maps/Fantasy Forest GameVersion");
                
        //        key = "Maps/Fantasy Forest GameVersion";
        //        spawnerPrefabSpiderForest.gameObject.SetActive(true);
        //        bossFieldPrefab.gameObject.SetActive(true);
        //        break;

        //    case MapType.AncientForest:
        //        mapPrefab = Resources.Load<GameObject>("Maps/AncientForest");
        //        key = "Maps/AncientForest";
        //        spawnerPrefabAncientForest.gameObject.SetActive(true);
        //        LightManager.gameObject.SetActive(true);
        //        break;

        //    case MapType.Castle:
        //        mapPrefab = Resources.Load<GameObject>("Maps/AncientForest");
        //        key = "Maps/AncientCastle";
        //        spawnerPrefabCastle.gameObject.SetActive(true);
        //        break;
        //    case MapType.Desert:
        //        mapPrefab = Resources.Load<GameObject>("Maps/AncientDesert");
        //        key = "Maps/AncientDesert";
        //        spawnerPrefabDesert.gameObject.SetActive(true);
        //        break;


        //    default:
        //        break;
        //}

        //if (mapPrefab) Instantiate(mapPrefab, loadPos, Quaternion.identity);
        //else Debug.Log("MapLoad Fail,No Map Prefabs in Resource Folder");

        ////currentMapHandle = Addressables.InstantiateAsync(key, loadPos, Quaternion.identity);


        StartCoroutine(LoadMapByIdAsync());


    }



    private IEnumerator LoadMapByIdAsync()
    {

        StageManager.Instance.isGameScene = true;

        // show loading
        if (loadingImageAnimationObj)
        {
           

            loadingImageAnimationObj.SetActive(true);
        }

        string key = null;

        // turn off all spawners/extra objects first (optional safety)
        //if (spawnerPrefabSpiderForest) spawnerPrefabSpiderForest.SetActive(false);
        //if (spawnerPrefabAncientForest) spawnerPrefabAncientForest.SetActive(false);
        //if (spawnerPrefabCastle)       spawnerPrefabCastle.SetActive(false);
        //if (spawnerPrefabDesert)       spawnerPrefabDesert.SetActive(false);
        //if (bossFieldPrefab)           bossFieldPrefab.SetActive(false);
        //if (LightManager)              LightManager.SetActive(false);

        switch (nowMap)
        {
            case MapType.SpiderForest:
                loadPos = new Vector3(0, -0.21f, 0);
                // mapPrefab = Resources.Load<GameObject>("Maps/Fantasy Forest GameVersion"); // OLD (sync)
                key = "Maps/Fantasy Forest GameVersion";
                if (spawnerPrefabSpiderForest) spawnerPrefabSpiderForest.SetActive(true);
                if (bossFieldPrefab)           bossFieldPrefab.SetActive(true);
                break;

            case MapType.AncientForest:
                // mapPrefab = Resources.Load<GameObject>("Maps/AncientForest"); // OLD (sync)
                key = "Maps/AncientForest";
                if (spawnerPrefabAncientForest) spawnerPrefabAncientForest.SetActive(true);
                if (LightManager)               LightManager.SetActive(true);
                break;

            case MapType.Castle:
                // mapPrefab = Resources.Load<GameObject>("Maps/AncientForest"); // OLD (sync?)
                key = "Maps/AncientCastle";
                if (spawnerPrefabCastle) spawnerPrefabCastle.SetActive(true);
                break;

            case MapType.Desert:
                // mapPrefab = Resources.Load<GameObject>("Maps/AncientDesert"); // OLD (sync)
                key = "Maps/AncientDesert";
                if (spawnerPrefabDesert) spawnerPrefabDesert.SetActive(true);
                break;
            case MapType.Temple:
                key = "Maps/AncientTemple";
                if (spawnerPrefabTemple) spawnerPrefabTemple.SetActive(true);
                break;

            case MapType.None:
                key = "Maps/AncientForest";
                if (spawnerPrefabAncientForest) spawnerPrefabAncientForest.SetActive(true);
                if (LightManager)               LightManager.SetActive(true);
                break;
        }

        // let one frame render so the loading UI appears before heavy work
        yield return null;

        // --- NEW: async load from Resources ---
        var req = Resources.LoadAsync<GameObject>(key);
        // If you want a progress bar, poll req.progress here.
        while (!req.isDone) { yield return null; }

        var prefab = req.asset as GameObject;
        if (prefab)
        {
            mapPrefab = prefab; // keep your field updated if you use it later
            Instantiate(mapPrefab, loadPos, Quaternion.identity);

            yield return null;
            FadeOutBlackScreenImageAfterLoad();

        }
        else
        {
            Debug.LogError($"MapLoad Fail, no prefab in Resources at '{key}'");
        }

        // hide loading
        if (loadingImageAnimationObj) loadingImageAnimationObj.SetActive(false);
    }




    public void AddMapExpCount() { 
        
        mapExpCount++;
        //ActiveBuffManager.Instance.AddStack(TraitType.PickUpReleaseSkill);
    }
    public void ReduceMapExpCount() { 
        
        mapExpCount--;

        if (SkillEffectManager.Instance.isGetItemCastEnabled)
        {
            expTraitCnt++;
            ActiveBuffManager.Instance.AddStack(TraitType.PickUpReleaseSkill);

            if (expTraitCnt >= 15)
            {
                expTraitCnt = 0;
                EventManager.EmitEvent(GameEvent.PlayerGetItem);
                ActiveBuffManager.Instance.SetStacks(TraitType.PickUpReleaseSkill, 1);
            }
        }
        
    }

    public void AdddExpTraitCount()
    {
        if (SkillEffectManager.Instance.isGetItemCastEnabled)
        {
            expTraitCnt++;
            ActiveBuffManager.Instance.AddStack(TraitType.PickUpReleaseSkill);

            if(expTraitCnt >= 15)
            {
                expTraitCnt = 0;
                EventManager.EmitEvent(GameEvent.PlayerGetItem);
                ActiveBuffManager.Instance.SetStacks(TraitType.PickUpReleaseSkill, 1);
            }
        }
    }

    public void MapCleaner()
    {
        magnetSpawnCnt -= Time.deltaTime;

        if(mapExpCount >= tooManyExpObj)
        {
            //instantiate magnet prefab nearby player position
            if (magnetSpawnCnt <= 0)
            {
                magnetSpawnCnt = magnetSpawnCntMax;
                Vector3 spawnPos = playerTrans.position + new Vector3(Random.Range(-14f, 14f), 0, Random.Range(-14f, 14f));
                Instantiate(magnetPrefab, spawnPos, Quaternion.identity);
            }
           
        }

    }

    public void LoadSpawnerPreset()
    {

    }

    public void LoadBossField()
    {

    }

    private void Update()
    {
        MapCleaner();
    }

    public void FadeOutBlackScreenImageAfterLoad()
    {

        if (loadingBlackScreenImage)
        {
            loadingBlackScreenImage.DOFade(0, 1f).SetEase(Ease.InOutSine).OnComplete(() => {
                loadingBlackScreenImage.gameObject.SetActive(false);
            });
        }
    }

}

