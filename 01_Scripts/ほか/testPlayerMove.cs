using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool

public class testPlayerMove : MonoBehaviour
{
    public float playerSpeed = 3.0f;

    [Header("エフェクトのデータ")] public ItemEffectData effectData;

    private GameObject effectObj;                   //エフェクトオブジェクト
    private Vector3    effectScale = new(0, 0, 0);  //エフェクトのスケール
  
    [Header("回復エフェクトのスケール")]
    public Vector3 healingEffectScale = new(0.0f, 0.0f, 0.0f);

    [Header("攻撃上昇エフェクトのスケール")]
    public Vector3 powUpEffectScale = new(0.0f, 0.0f, 0.0f);

    void Update()
    {
       
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        transform.position += movement * playerSpeed * Time.deltaTime;
        //rotate with move Direction
        if (movement != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }  
    }

    public void OnTriggerEnter(Collider col)
    {
        //インターフェースのコンポーネントを取得
        IHittable hit = col.GetComponent<IHittable>();

        //当たった時の関数を呼び出す
        if (hit != null)
        {
            hit.OnHit();
        }
    }

    //プレイヤーのアイテム取得時のエフェクトを再生
    public void PlayItemEffect(ItemType type)
    {
        GameObject effect = null;
        switch (type)
        {
            //アイテムの種類が回復ポーションの場合
            case ItemType.HealingPotion:

                //回復エフェクトのスケールにする
                effectScale = healingEffectScale;

                //エフェクトを回復エフェクトにする
                effect = effectData.healingEffect;

                break;
            //アイテムの種類が攻撃力上昇ポーションの場合
            case ItemType.PowerUpPotion:

                //攻撃上昇エフェクトのスケールにする
                effectScale = powUpEffectScale;

                //エフェクトを攻撃力上昇エフェクトにする
                effect = effectData.powUpEffect;

                break;
        }

        if (effect != null)
        {
            //エフェクトをプレイヤーの位置に追従させる
            this.effectObj = Instantiate(effect, transform.position, Quaternion.identity,this.transform);

            //エフェクトのスケールを変更
            this.effectObj.transform.localScale = effectScale;
        }
    }
}

