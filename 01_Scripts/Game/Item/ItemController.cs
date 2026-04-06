using System.IO.IsolatedStorage;
using TigerForge;
using UnityEngine;

//アイテムの種類
public enum ItemType
{
    HealingPotion,  //回復ポーション
    PowerUpPotion,  //攻撃ポーション
    Magnet,         //マグネット
    SpdUpWing,      //スピードアップウィング
    StarCoin,       //スターコイン
    SwapOrb,        //スワップオーブ
    ForgeOrb,       //フォージオーブ
    RerollOrb,      //リロールオーブ
    Invincible,     //無敵アイテム
}

public class ItemController : MonoBehaviour, IHittable
{
    [Header(@"アイテム側のコライダーにトリガーを付ける!")]

    [Header("アイテムの種類")]               public ItemType       itemLabel;         
    [Header("アイテムのエフェクトのデータ")] public ItemEffectData effectData;

    [Header("アイテムエフェクトのスケール")] public Vector3    itemEffectScale = new(1.0f, 1.0f, 1.0f);
    [Header("アイテムエフェクトの回転")]     public Vector3    itemEffectRotation = new(0.0f, 0.0f, 0.0f);
    [Header("アイテムエフェクトの座標")]     public Vector3    itemEffectPos   = new(0.0f, 0.0f, 0.0f); 

    private GameObject itemEffectPrefab;                        //アイテムエフェクトプレハブのデータ
    private GameObject playerObj;                               //プレイヤーのデータ

    void Start()
    {    
        playerObj = GameObject.FindWithTag("Player");        
    }

    //当たった時
    public void OnHit()
    {

        ResultMenuController.Instance.totalGameItemGet++;

        // プレイヤーに対応するエフェクトを再生
        PlayItemEffect(itemLabel);

        EventManager.EmitEvent(GameEvent.PlayerGetItem);
        //ActiveBuffManager.Instance.AddStack(TraitType.PickUpReleaseSkill);

        switch (itemLabel)
        {
            //アイテムの種類が回復ポーションだったら
            case ItemType.HealingPotion:
                //プレイヤーのHPを回復する
                ItemManager.Instance.Healing();
                break;

            //攻撃ポーションだったら
            case ItemType.PowerUpPotion:
                //プレイヤーの攻撃力が一定時間上昇する
                ItemManager.Instance.PowerUp();      
                break;

            //マグネットだったら
            case ItemType.Magnet:
                //EXPを吸収する
                ItemManager.Instance.ExpAbsorption();
                break;

            //スピードアップウィングだったら
            case ItemType.SpdUpWing:
                //速度を一定時間上昇させる
                ItemManager.Instance.SpeedUp(); 
                break;

            //スワップオーブだったら
            case ItemType.SwapOrb:
                //スワップ回数を増やす
                ItemManager.Instance.GetSwapCount(); 
                break;

            //フォージオーブだったら
            case ItemType.ForgeOrb:
                //ブースト回数を増やす
                ItemManager.Instance.GetBoostCount(); 
                break;

            //リロールオーブだったら
            case ItemType.RerollOrb:
                //リロール回数を増やす
                ItemManager.Instance.GetRerollCount(); 
                break;

            //無敵アイテムだったら
            case ItemType.Invincible:
                //無敵状態にする
                ItemManager.Instance.Invincible();
                break;
        }

        //排除を共通処理にする
        Destroy(gameObject);
    }


    //プレイヤーのアイテム取得時のエフェクトを再生
    public void PlayItemEffect(ItemType type)
    {
        GameObject itemEffectObj; //アイテムエフェクトのオブジェクト

        itemEffectPrefab = null;

        switch (type)
        {
            //アイテムの種類が回復ポーションの場合
            case ItemType.HealingPotion:

                SetParamItemEffect(effectData.healingEffect);

                break;

            //アイテムの種類が攻撃力上昇ポーションの場合
            case ItemType.PowerUpPotion:

                SetParamItemEffect(effectData.powUpEffect);
                break;

            //アイテムの種類がマグネットの場合
            case ItemType.Magnet:

                SetParamItemEffect(effectData.expAbsorptionEffect);

                break;

            //アイテムの種類がスピードアップウィングの場合
            case ItemType.SpdUpWing:

                SetParamItemEffect(effectData.spdUpEffect);
                break;

            //アイテムの種類がスピードアップウィングの場合
            case ItemType.StarCoin:

                SetParamItemEffect(effectData.getCoinEffect);
                break;

            //アイテムの種類がスワップオーブの場合
            case ItemType.SwapOrb:

                SetParamItemEffect(effectData.getSwapOrbEffect);

                break;
            //アイテムの種類がフォージオーブの場合
            case ItemType.ForgeOrb:

                SetParamItemEffect(effectData.getForgeOrbEffect);

                break;

            //アイテムの種類がリロールオーブの場合
            case ItemType.RerollOrb:

                SetParamItemEffect(effectData.getRerollOrbEffect);

                break;

            //アイテムの種類が無敵アイテムの場合
            case ItemType.Invincible:

                SetParamItemEffect(effectData.getInvincbleEffect);

                break;
        }

        if (itemEffectPrefab != null && playerObj != null)
        {
            //エフェクトをプレイヤーの位置に追従させる
            itemEffectObj = Instantiate(itemEffectPrefab, playerObj.
                                        transform.position + itemEffectPos,
                                         Quaternion.Euler(itemEffectRotation),
                                        playerObj.transform);

            itemEffectObj.transform.localScale = itemEffectScale;
        }

        SoundEffect.Instance.Play(SoundList.GetItem); //アイテム取得のSEを再生
    }

    //アイテムエフェクトのパラメーターを設定する関数
    //_itemEffectData ...再生したいアイテムエフェクトを入れる引数
    public void SetParamItemEffect(GameObject _itemEffectData)
    {
        //エフェクトを再生する
        itemEffectPrefab = _itemEffectData;
    }
}
