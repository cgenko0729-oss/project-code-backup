using System.Linq;
using UnityEngine;
using System.Collections;

public enum DestroyObjType
{
    ItemDrops, //アイテムドロップ...プレイヤーに関するアイテムが出る方
    Remnant    //遺物...色んなアイテムが出る方
}

public class DestroyObject : MonoBehaviour
{

    [Header("破壊可能オブジェクトの種類")] public DestroyObjType DestroyObjLabel;

    [Header("アイテムのランダム生成情報を取得")]
    public RandomItemSpawner itemSpawner;

    [Header("アイテムの生成情報を取得")]
    public Vector3 ItemPos = new(0, 0, 0);

    [Header("フェード開始時間")]
    public float fadeStartTime;

    [Header("フェード時間")]
    public float fadeTime;

    private int spawnCount = 1;

    private Vector3 rndItemPos = new(0, 0,0);

    //アイテムのランダムな座標の緩急(レムナントのみ)
    private float rndItemPosMin = -1.0f;
    private float rndItemPosMax = 1.0f;

    //オブジェクトを破壊する関数
    public void ObjectDestroy()
    {
        switch (DestroyObjLabel)
        {
            case DestroyObjType.ItemDrops:
                spawnCount = 1;
                break;

            case DestroyObjType.Remnant:
                spawnCount = 3;
                break;
        }

        var random = new System.Random();
        var min = -3;
        var max = 3;

        //自分自身の当たり判定を消す
        var ownCollider = GetComponent<Collider>();
        if (ownCollider != null)
        {
            ownCollider.enabled = false;
        }

        gameObject.GetComponentsInChildren<Rigidbody>().ToList().ForEach(r =>
        {
            //子オブジェクトのキネティックを消す
            r.isKinematic = false;           

            //破壊したときに破片がランダムで吹き飛ぶ
            var vect = new Vector3
            (random.Next(min, max), random.Next(0, max), random.Next(min, max));
            r.AddForce(vect, ForceMode.Impulse);
            r.AddTorque(vect, ForceMode.Impulse);
        });
        //アイテムをランダムな種類に生成

        for (int i = 0; i < spawnCount; ++i)
        {
            Vector3 rndPos = rndItemPos;

            switch (DestroyObjLabel)
            {
                case DestroyObjType.ItemDrops:
                    itemSpawner.RandomItemSpawn(transform.position + ItemPos);
                    break;

                case DestroyObjType.Remnant:
                    rndPos += new Vector3( Random.Range(-1.0f, 1.0f), 0,Random.Range(-1.0f, 1.0f) );

                    itemSpawner.RandomItemSpawn(transform.position + ItemPos + rndPos);
                    break;
            }
        }

        //コルーチン開始
        StartCoroutine(FadeAndDestroy(gameObject, fadeTime));
    }

    IEnumerator FadeAndDestroy(GameObject obj, float duration)
    {

        //fadeStartTimeだけ待機
        yield return new WaitForSeconds(fadeStartTime);


        gameObject.GetComponentsInChildren<Rigidbody>().ToList().ForEach(r =>
        {
            //子オブジェクトのキネティックを付与する
            //(物理挙動による処理を軽減するため)
            r.isKinematic = true;
        });

        //fadeTimeだけ待機
        yield return new WaitForSeconds(fadeTime);

        Destroy(obj);
    }
}
