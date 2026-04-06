using System.Collections.Generic;
using TigerForge;
using UnityEngine;

public class ActivePetManager : SingletonA<ActivePetManager>
{
    //現在スポーンしているペットのリスト
    public List<GameObject> activePets { get; private set; } = new List<GameObject>();

    [Header("ペットの鳴き声関連設定")]
    public int PetSoundHitCount = 3;

    [Header("ペットのスキル持続時間増加量(%)")]
    [Tooltip("ここでは変更しない")]
    public float PetSkillDuration = 1f;

    [Header("ペットのスキル最大分身数")]
    [Tooltip("ここでは変更しない")]
    public int PetCloneCount = 0;

    //金を落とす確率(%)、例えば20なら20%の確率で金を落とす
    [HideInInspector]public int PetDropMoneyChance = 10;

    //金ミイラがいるかどうか
    [HideInInspector]public bool isGoldMummyInGame = false;

    //マッスルアヒルがいるかどうか
    [HideInInspector]public bool isMuscleDuckInGame = false;

    //カブキングがいるかどうか
    [HideInInspector]public bool isTurnipaKingInGame = false;

    //呪文起爆
    [HideInInspector]public bool spellBomb = false;

    //マッスルアヒルの全インスタンスリスト
    [HideInInspector]public List<PetMuscleDuckAction> allDucks = new List<PetMuscleDuckAction>();

    //ペットのスキル待機時間
    [HideInInspector]public float petSkillWaitTime = 1f;

    //スポーンしたペットをリストに登録する
    public void RegisterActivePetList(GameObject petInstance)
    {
        if (!activePets.Contains(petInstance))
        {
            activePets.Add(petInstance);

            EventManager.EmitEvent("ChangePetList");
        }
    }

    //ペットが消えたときにリストから削除する
    public void RemoveActivePet(GameObject petInstance)
    {
        if (activePets.Contains(petInstance))
        {
            activePets.Remove(petInstance);

            EventManager.EmitEvent("ChangePetList");
        }
    }

    //ゲーム終了時やシーン遷移時、リストから削除する
    public void ClearPets()
    {
        activePets.Clear();

        EventManager.EmitEvent("ChangePetList");
    }

    //種族を調べるメゾット
    public int CountPetsByRace(PetRace targetRace)
    {
        int count = 0;
        foreach (var petObj in activePets)
        {
            if (petObj == null) continue;

            // ペットの共通スクリプトを取得
            var action = petObj.GetComponent<ActivePetActionBase>();

            // PetDataを持っていて、かつ種族が一致したらカウント
            if (action != null && action.GetPetData() != null)
            {
                if (action.GetPetData().race == targetRace)
                {
                    count++;
                }
            }
        }
        return count;
    }

    public PetMuscleDuckAction GetMuscleDuckScript()
    {
        foreach (var pet in activePets)
        {
            var script = pet.GetComponent<PetMuscleDuckAction>();
            if (script != null) return script;
        }
        return null;
    }
}

