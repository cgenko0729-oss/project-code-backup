using UnityEngine;
using System.Collections.Generic;

public class SelectPetSpawner : MonoBehaviour
{
    public bool DebugMode = false;

    [Header("プレイヤーオブジェクトの座標")]
    [SerializeField] private Transform playerTransform;

    [Header("プレイヤー検索用のタグ")]
    [SerializeField] private string playerTag = "Player";

    [Header("スポーン設定")]
    [Tooltip("プレイヤーからどれくらい離れた位置にスポーンさせるか")]
    [SerializeField] private float spawnRadius = 2.0f;

    void Start()
    {
        //ゲーム中で手に入れたペットリストをリセット
        PetGetChanceManager.Instance.ClearStagePetList();

        //プレイヤーのTransformが設定されていなければタグで検索する
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag(playerTag);
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                return; //プレイヤーが見つからなければ処理を中断
            }
        }

        //選択されたペットをスポーンさせる処理を呼び出す
        SpawnSelectedPets();
    }


    //選択されたペットをプレイヤーの周りにスポーンさせる
    private void SpawnSelectedPets()
    {
        //PetSelectDataManagerから選択されたペットのリストを取得(シングルトン)
        List<PetData> selectedPets = PetSelectDataManager.Instance.SelectedPets;
        
        //選択されたペットがいない、もしくはNullの場合何もしない
        if (selectedPets == null || selectedPets.Count == 0)
        {
            Debug.LogWarning("SelectPetSpawner: 選択されたペットのリストが空、またはNullです。");
            return;
        }

        //前のペットリストを削除する
        ActivePetManager.Instance.ClearPets();

        //選択されたペットの数に応じて、プレイヤーの周りに均等な角度で配置する
        for (int i = 0; i < selectedPets.Count; i++)
        {
            PetData petData = selectedPets[i];

            //PetDataにプレハブが設定されているかチェック
            if (petData.petPrefab == null)
            {
                continue;
            }

            //スポーン位置を計算（プレイヤーを中心とした円周上）
            float angle = i * (360f / selectedPets.Count);

            Vector3 offset = Quaternion.Euler(0, angle, 0) * (Vector3.forward * spawnRadius);
            Vector3 spawnPosition = playerTransform.position + offset;

            //ペットのプレハブをインスタンス化
            GameObject petInstance = Instantiate(petData.petPrefab, spawnPosition, Quaternion.identity);

            ActivePetActionBase petAction = petInstance.GetComponent<ActivePetActionBase>();
            if (petAction != null)
            {
                // プレイヤー中心からのズレ（offset）を渡す
                petAction.SetFormationOffset(offset);
            }

            //スポーンしたペットをActivePetManagerに登録する
            ActivePetManager.Instance.RegisterActivePetList(petInstance);
        }
    }
}

