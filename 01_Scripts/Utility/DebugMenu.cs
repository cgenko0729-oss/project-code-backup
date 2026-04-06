using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;

using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool



public class DebugMenu : Singleton<DebugMenu>
{
    public RectTransform      container;     
    public TextMeshProUGUI    linePrefab;    
    Dictionary<string, TextMeshProUGUI> lines = new();
    public GameObject debugMenu;

    PlayerState playerState;

    public ObjectPool expPool;

    private void Show(string id, string text)
    {
        if (!lines.TryGetValue(id, out var label))
        {
            label      = Instantiate(linePrefab, container);
            lines[id]  = label;
        }
        label.text = text;
    }

    public void ShowFloat(string id, float value, int precision = 1)
    {
        string format = "F" + precision;
        Show(id, $"{id}: {value.ToString(format)}");
    }
    public void ShowInt(string id, int value)=> Show(id, $"{id}: {value}"); 
    public void ShowBool(string id, bool value) => Show(id, $"{id}: {value}");
    public void ShowVector3(string id, Vector3 value) =>  Show(id, $"{id}: {value.x:F1}, {value.y:F1}, {value.z:F1}");
    public void ShowVector2(string id, Vector2 value) => Show(id, $"{id}: {value.x:F1}, {value.y:F1}");
    public void ShowString(string id, string text) => Show(id, text);
    
    public void ShowList<T>(string id, List<T> list)
    {
        string text = $"{id}: ";
        for (int i = 0; i < list.Count; i++) {
            text += $"{list[i]}";
            if (i < list.Count - 1) text += ", ";
        }
        Show(id, text);
    }
    
    public void ShowQuaternion(string id, Quaternion value) => Show(id, $"{id}: {value.x:F1}, {value.y:F1}, {value.z:F1}, {value.w:F1}");      
    public void ShowTransform(string id, Transform value) => Show(id, $"{id}: {value.position.x:F1}, {value.position.y:F1}, {value.position.z:F1} , {value.rotation.x:F1}, {value.rotation.y:F1}, {value.rotation.z:F1}, {value.rotation.w:F1}");   
    public void ShowRotation(string id, Transform tran)
    {
        Vector3 euler = tran.rotation.eulerAngles;
        Show(id, $"{id}: {euler.x:F1}, {euler.y:F1}, {euler.z:F1}");
    }

    public void Start()
    {
        playerState = GameObject.FindWithTag("Player").GetComponent<PlayerState>();

    }

    void Update()
    {
        if (GameManager.Instance.isDebugMode && (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.F2)) ) debugMenu.SetActive(!debugMenu.activeSelf);

        ShowFloat("FPS", GameManager.Instance.gameFps);
        ShowInt("段階", TimeManager.Instance.gamePhrase);
        ShowFloat("Hp",playerState.NowHp);
        ShowFloat("レベル",playerState.NowLv);
        ShowFloat("経験値", playerState.NowExp);
        ShowFloat("必要な経験値", playerState.NextLvExp);
        ShowString("===", "==============");
        ShowFloat("クモ敵数", EnemyManager.Instance.moverNum);
        ShowInt("敵数", EnemyManager.Instance.allEnemyNum);
        ShowInt("敵撃破数", EnemyManager.Instance.allEnemyKillNum);
        ShowString("===", "==============");
        ShowFloat("総合Dps", DpsManager.Instance.TotalMeasuredDps);
        ShowFloat("Dps", DpsManager.Instance.boomerangDps);
        ShowFloat("バウンドナイフDps", DpsManager.Instance.bounceDps);
        ShowFloat("スラッシュDps", DpsManager.Instance.slashDps);
        ShowFloat("ボールDps", DpsManager.Instance.circleDps);    
        ShowFloat("雷Dps", DpsManager.Instance.thunderDps);


        


    }

    public void DebugSpawn30()
    {

        Vector3 playerPos = playerState.transform.position;
        for (int i = 0; i < 35; i++)
        {
            Vector3 spawnPos = playerPos + Random.onUnitSphere * 7f;
            spawnPos.y = 0.1f; 
            GameObject expObj = expPool.GetObject(spawnPos);
            
            expObj.SetActive(true);
        }



    }


    public void DebugSpawnExpAroundCertainDistToPlayer()
    {
        //spawn 300 exp object from pool that keep 15 distance from player 's position and each exp object is 0.3f distance apart

        Vector3 playerPos = playerState.transform.position;
        for (int i = 0; i < 300; i++)
        {
            Vector3 spawnPos = playerPos + Random.onUnitSphere * 15f;
            spawnPos.y = 0.1f; // set y position to 0.1f
            GameObject expObj = expPool.GetObject();
            expObj.transform.position = spawnPos;
            expObj.transform.rotation = Quaternion.identity;
            expObj.SetActive(true);
        }



    }

}

