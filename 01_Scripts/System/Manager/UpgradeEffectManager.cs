using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using MonsterLove.StateMachine; //StateMachine
using QFSW.MOP2;                //Object Pool
using System.Collections.Generic;
using TigerForge;               //EventManager
using TMPro;    
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;     

public class UpgradeEffectManager : Singleton<UpgradeEffectManager>
{
    [Header("特殊な強化を行うためのスクリプトリスト")]
    public List<UpgradeEffectBase> upgradeEffectList;

    private bool initFlg = false;

    private void Start()
    {
        initFlg = false;
    }

    private void Update()
    {
        if(initFlg == false)
        {
            // 適用されるかどうかをそれぞれが自分で調べる
            foreach (var effect in upgradeEffectList)
            {
                effect.CanEnableBuff();
            }

            initFlg = true;
        }
    }

    private void FixedUpdate()
    {
        if(GameManager.Instance.isGameClear || 
            GameManager.Instance.isGameOver) { return; }

        foreach(var effect in upgradeEffectList)
        {
            if(effect.isEnable == false) { continue; }

            // 継続時間やリキャストなど、更新の必要なものの更新処理を行う
            effect.EffectUpdate();

            // バフの合計値を適用する
            effect.ApplyBufftotalAmount();
        }
    }

    // 発動させたいバフのタイプを渡すと、そのバフの発動処理を実行する
    public bool Active(BuffType type)
    {
        foreach(var effect in upgradeEffectList)
        {
            if(effect.BuffType == type && effect.isEnable == true)
            {
                return effect.ActiveBuff();
            }
        }

        return false;
    }

    public bool Active(BuffType type, Vector3 effectPos, float damageAmount = 0)
    {
        foreach (var effect in upgradeEffectList)
        {
            if (effect.BuffType == type && effect.isEnable == true)
            {
                return effect.ActiveBuff(effectPos,damageAmount);
            }
        }

        return false;
    }

#if UNITY_EDITOR
    [ContextMenu("LoadPrefab")]
    public void LoadPrefab()
    {
        upgradeEffectList.Clear();

        // 検索するフォルダのパス（Assetsからの相対パス）
        string[] searchFolders = { "Assets/02_Prefab/PlayerPrefabs/UpgradePrefabs" };

        // 1. Prefabの読み込み (t:Prefab でフィルタリング)
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", searchFolders);
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var effect = prefab.GetComponent<UpgradeEffectBase>();
            if (effect != null)
            {
                upgradeEffectList.Add(effect);
            }
        }
    }
#endif
}

