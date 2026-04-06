using UnityEngine;
using TigerForge;
using System.Collections.Generic;

[System.Serializable]
// 1キャラ分の保存するデータ
public class OneCharaSaveData
{
    public Dictionary<string, bool> isUnlockedFlgs = new Dictionary<string, bool>();
    public int useTotalCoin;
}

public static class CharaUpgradeSaveSystem
{
    const string FilePath = "charaUpgrade_progress";

    // 指定したJobの強化進捗を保存する
    public static void Save(Dictionary<string, OneCharaSaveData> allCharaSaveDatas)
    {
        // 保存を行う
        var file = new EasyFileSave(FilePath);
        if (file.Load() == false)
        {
            if (CreateFile() == false)
            {
                Debug.Log("保存処理を行えませんでした");
                return;
            }
        }

        // 保存する1キャラ分のデータを移す
        foreach(var saveData in allCharaSaveDatas)
        {
            string jobId = saveData.Key;
            OneCharaSaveData oneCharaData = saveData.Value;
            file.Add(jobId, oneCharaData);
        }

        // 最後にファイルの変更を保存する
        file.Save();
    }

    // 指定したJobの強化進捗を読み込む
    public static bool Load(string jobId,out Dictionary<string,bool> isUnlockedFlgs, out int useTotalCoin)
    {
        isUnlockedFlgs = new Dictionary<string, bool>();
        useTotalCoin = 0;
        var file = new EasyFileSave(FilePath);
        // 読み込むことができなければFalseを返す
        if(file.Load() == false) { return false; }

        OneCharaSaveData saveData = new OneCharaSaveData();
        // 値を読み込む(読み込みの成功を返す)
        saveData = (OneCharaSaveData)file.GetData(jobId);
        if(saveData == null) { return false; }
        isUnlockedFlgs = saveData.isUnlockedFlgs;
        useTotalCoin = saveData.useTotalCoin;

        // メモリ解放
        file.Dispose();

        return true;
    }

    public static bool Exists() => new EasyFileSave(FilePath).FileExists();
    public static void Delete() => new EasyFileSave(FilePath).Delete();
    public static bool CreateFile()
    {
        var file = new EasyFileSave(FilePath);
        if(Exists() == true)
        {
            Debug.Log("すでに作成済み");
            return false;
        }

        file.Save();
        return true;
    }
}

