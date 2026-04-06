using System.Collections.Generic;
using UnityEngine;

//使いまわし
public class LoadingPetSpawner : MonoBehaviour
{
    void Start()
    {
        //選択されたペットをスポーンさせる処理を呼び出す
        SpawnSelectedPets();
    }

    //選択されたペットをプレイヤーの周りにスポーンさせる
    private void SpawnSelectedPets()
    {
        //PetSelectDataManagerから選択されたペットのリストを取得(シングルトン)
        List<PetData> visiblePetList = PetSelectDataManager.Instance.SelectedPets;

        //選択されたペットがいない、もしくはNullの場合何もしない
        if (visiblePetList == null || visiblePetList.Count == 0)
        {
            Debug.LogWarning("SelectPetSpawner: 選択されたペットのリストが空、またはNullです。");
            return;
        }

        //選択されたペットの数に応じて、プレイヤーの周りに均等な角度で配置する
        for (int i = 0; i < visiblePetList.Count; i++)
        {
            PetData petData = visiblePetList[i];

            //PetDataにプレハブが設定されているかチェック
            if (petData.petPrefab == null)
            {
                continue;
            }

            Vector3 offset = new Vector3(i * -2f, 0, 0);
            Vector3 spawnPosition = transform.position+offset;

            //ペットのプレハブをインスタンス化
            GameObject petInstance = Instantiate(petData.petPrefab, spawnPosition, Quaternion.Euler(0, 90, 0));
            SetLayerRecursively(petInstance, LayerMask.NameToLayer("RenderTexture"));

        }
    }
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
