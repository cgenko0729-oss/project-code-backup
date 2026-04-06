using UnityEngine;
using DG.Tweening;

public class RandomItemSpawner : MonoBehaviour
{
    [Header("複数のオブジェクトを設定できる配列")]
    public GameObject[] prefabObjects;

    [Header("ランダムに選ばれるPrefabのインデックス")]
    private int prefabObjNum;

    [Header("Sprite表示用の共通マテリアル（Sprite用Shader）")]
    public Material spriteMaterial;

    [Header("アイテムがスポーンしたとき\nPrefab通りの大きさに戻るまでの長さ")]
    public float itemSpawnPrefabScaleDuration = 0.5f;

    public void RandomItemSpawn(Vector3 spawnPos)
    {
        // 配列からランダムにPrefabを選ぶ
        prefabObjNum = Random.Range(0, prefabObjects.Length);

        //アイテムをランダムな種類に生成
        GameObject clone = Instantiate(prefabObjects[prefabObjNum],                    //複製するオブジェクト(オブジェクトもランダム生成)
                                       spawnPos,                                       //生成時のオブジェクトの位置
                                       prefabObjects[prefabObjNum].transform.rotation  //回転
                                      );

        // Dotweenを使ってアイテム生成に躍動感を出す
        clone.transform.localScale = Vector3.zero;
        clone.transform.DOScale(prefabObjects[prefabObjNum].transform.localScale, itemSpawnPrefabScaleDuration)
                       .SetEase(Ease.OutBack);

        // SpriteRendererがあるなら、マテリアルを明示的に設定
        var spriteRenderer = clone.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteMaterial != null)
        {
            spriteRenderer.material = spriteMaterial;
        }
    }
}
