using UnityEngine;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool

public class DestroyObjectController : MonoBehaviour, IHittable
{
    [Header("破壊可能オブジェクトの種類")] public DestroyObjType DestroyObjLabel;

    [SerializeField] private GameObject destroyObjPrefab;   //破壊オブジェクトプレハブのデータ

    [Header("破壊時エフェクト")]
    public GameObject destroyEffectObj;

    [Header("破壊時エフェクトのオブジェクトプール")]
    public ObjectPool destroyEffectObjPool;

    public void OnHit()
    {
        EventManager.EmitEvent(GameEvent.ItemBoxDestroyed);

        GameObject destroyObj; //破壊オブジェクトのオブジェクト

        //自分自身の当たり判定を消す
        var ownCollider = GetComponent<Collider>();
        if (ownCollider != null)
        {
            ownCollider.enabled = false;
        }

        //自分を消す
        Destroy(gameObject);

        switch (DestroyObjLabel)
        {
            case DestroyObjType.ItemDrops:
                SoundEffect.Instance.Play(SoundList.HitItemBox);
                break;

            case DestroyObjType.Remnant:
                SoundEffect.Instance.Play(SoundList.HitRemnant);
                break;
        }

        //プレハブ生成
        destroyObj = Instantiate(destroyObjPrefab, transform.position, Quaternion.identity);

        if (destroyEffectObj != null)
        {
            //エフェクト生成
            GameObject hitObj = destroyEffectObjPool.GetObject();

            hitObj.transform.position = transform.position;

            //スクリプトを取得
            PetSkillHitEffectData hitEffectData = hitObj.GetComponent<PetSkillHitEffectData>();

            //保持していたダメージ値を渡す
            if (hitEffectData != null)
            {
                hitEffectData.SetPool(destroyEffectObjPool);
            }
        }

        //生成したオブジェクトのObjectDestroyを呼び出す
        DestroyObject destroyScr = destroyObj.GetComponent<DestroyObject>();
        if(destroyScr != null)
        {
            destroyScr.ObjectDestroy();
        }
    }
}

