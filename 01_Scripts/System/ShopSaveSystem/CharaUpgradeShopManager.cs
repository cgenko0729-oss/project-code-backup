using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;
using System.Linq;

public class CharaUpgradeShopManager : Singleton<CharaUpgradeShopManager>
{
    [Header("全キャラの強化データベースリスト")]
    public List<CharaUpgradeDataBase> charaList;
    [Header("表示の切り替え用オブジェクト")]
    public GameObject charaUpgradeGroup;
    public GameObject charaSelectGroup;
    [Header("遷移後に表示する強化画面のキャラを決めるJobIdを取得する")]
    public PlayerData playerData;

    // 選択中の強化項目の内容表示用メンバ
    public string nowSelectedName = "";
    public string nowSelectedDesc = "";
    public int nowSelectedUnlockCoin = 0;

    void Awake()
    {
        // ファイルがあるかを確認してなければファイルを作成する
        CharaUpgradeSaveSystem.CreateFile();

        foreach(var chara in charaList)
        {
            foreach(var upgradeData in chara.Upgrades)
            {
                upgradeData.dataId = upgradeData.name;
            }
        }

        // ファイルから強化進捗を読み込む
        LoadUpgradeProgress();

        charaUpgradeGroup.SetActive(true);
    }

    void Update()
    {

    }

    public Dictionary<string, OneCharaSaveData> allCharaSaveDatas = new Dictionary<string, OneCharaSaveData>();
    [ContextMenu("SaveUpgradeProgress")]
    public void SaveUpgradeProgress()
    {
        //Dictionary<string, SaveData> saveDatas = new Dictionary<string, SaveData>();

        for (int i = 0; i < charaList.Count; i++)
        {
            var upgradeData = charaList[i];
            if (upgradeData == null) { continue; }

            OneCharaSaveData datas = new OneCharaSaveData();
            datas.isUnlockedFlgs = new Dictionary<string, bool>();
            // Dictionaryデータに置き換える
            foreach (var upgrade in upgradeData.Upgrades)
            {
                if(upgrade.dataId == "")
                {
                    Debug.Log("dataIdが設定されていない");
                    continue;
                }

                if (datas.isUnlockedFlgs.TryAdd(upgrade.dataId, upgrade.isUnlocked) == false)
                {
                    datas.isUnlockedFlgs[upgrade.dataId] = upgrade.isUnlocked;
                }
            }
            datas.useTotalCoin = upgradeData.useTotalCoin;

            if(allCharaSaveDatas.TryAdd(upgradeData.jobId.ToString(), datas) == false)
            {
                allCharaSaveDatas[upgradeData.jobId.ToString()] = datas;
            }
        }

        // 全てのキャラの情報をまとめて送って保存を行う
        CharaUpgradeSaveSystem.Save(allCharaSaveDatas);
    }

    [ContextMenu("LoadUpgradeProgress")]
    public void LoadUpgradeProgress()
    {
        Debug.Log("キャラの強化進捗を読み込みます");

        for (int i = 0; i < charaList.Count; i++)
        {
            var upgradeData = charaList[i];

            Dictionary<string, bool> progressDatas = new Dictionary<string, bool>();
            if (CharaUpgradeSaveSystem.Load(
                upgradeData.jobId.ToString(), out progressDatas,out upgradeData.useTotalCoin) == false)
            {
                Debug.Log(upgradeData.jobId.ToString() + "の強化進捗を読み込めませんでした");
                continue;
            }

            // 強化項目ごとに読み込んだ値をセットする
            foreach (var data in progressDatas)
            {
                int index = upgradeData.Upgrades.FindIndex(
                    d => d.dataId == data.Key);
                if(index == -1) { continue; }

                var upgrade = upgradeData.Upgrades[index];
                upgrade.isUnlocked = data.Value;
                upgradeData.Upgrades[index] = upgrade;
            }

            charaList[i] = upgradeData;
        }
    }

    // 全キャラの強化をリセット
    [ContextMenu("ResetAllUpgraeProgress")]
    public void ResetAllUpgradeProgress()
    {
        for (int i = 0; i < charaList.Count; i++)
        {
            var upgradeData = charaList[i];

            if(upgradeData.Upgrades.Count <= 0) { continue; }

            List<CharaUpgradeDataSO> resetUpgrades = new List<CharaUpgradeDataSO>();
            foreach(var data in upgradeData.Upgrades)
            {
                var resetData = data;
                resetData.isUnlocked = false;
                resetData.enableUnlock = false;
                resetUpgrades.Add(resetData);
            }

            resetUpgrades.First().enableUnlock = true;
            CurrencyManager.Instance.Coins += upgradeData.useTotalCoin;
            CurrencyManager.Instance.SaveCoinToFile();
            upgradeData.useTotalCoin = 0;
            upgradeData.Upgrades = resetUpgrades;
        }

        EventManager.EmitEvent("UpdateAllShopItems");
    }

    // キャラ1体分(画面表示中キャラ)の強化をリセット
    public void ResetUpgradeProgress()
    {
        int nowJobId = (int)charaUpgradeGroup?.GetComponent<CharaUpgradeShopController>().nowJobId;

        var upgradeData = charaList[nowJobId];
        if (upgradeData.Upgrades.Count <= 0) { return; }

        List<CharaUpgradeDataSO> resetUpgrades = new List<CharaUpgradeDataSO>();
        foreach (var data in upgradeData.Upgrades)
        {
            var resetData = data;
            resetData.isUnlocked = false;
            resetData.enableUnlock = false;
            resetUpgrades.Add(resetData);
        }

        resetUpgrades.First().enableUnlock = true;
        CurrencyManager.Instance.Coins += upgradeData.useTotalCoin;
        CurrencyManager.Instance.SaveCoinToFile();
        upgradeData.useTotalCoin = 0;
        upgradeData.Upgrades = resetUpgrades;

        EventManager.EmitEvent("UpdateAllShopItems");
    }

    public void SubmitUpgrade(JobId jobId,string dataId)
    {
        var upgrades = charaList[(int)jobId].Upgrades;

        int index = upgrades.FindIndex(d => d.dataId == dataId);
        var data = upgrades[index];
        data.isUnlocked = true;
        upgrades[index] = data;
    }

    public void ApplyUpgradeValue()
    {
        if(playerData == null) { return; }

        int jobId = (int)playerData.jobId;
        var upgrades = charaList[(int)jobId].Upgrades;

        // 指定されたキャラ強化値をBuffManagerに渡す
        foreach (var upgrade in upgrades)
        {
            if(upgrade.isUnlocked == false) { continue; }

            // 強化カテゴリごとの値をBuffManagerに渡す
            foreach(var param in upgrade.buffParam)
            {
                Debug.Log("ApplyCharaBuff: " + param.buffType + " " + param.amount);
                BuffManager.Instance.ApplyCharaBuff(
                    param.buffType, param.amount);
            }
        }
    }

    public void AddUseTotalCoin(JobId jobId, int amount)
    {
        charaList[(int)jobId].useTotalCoin += amount;
    }

    public void OpenCharaUpgradeWindow()
    {
        charaUpgradeGroup.SetActive(true);
        charaSelectGroup.SetActive(false);
    }

    public void CloseCharaUpgradeWindow()
    {
        charaUpgradeGroup.SetActive(false);
        charaSelectGroup.SetActive(true);

        SaveUpgradeProgress();
    }
}

