using DG.Tweening;
using MonsterLove.StateMachine; //StateMachine
using QFSW.MOP2;                //Object Pool
using System.Collections;
using System.Collections.Generic;
using TigerForge;               //EventManager
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;


public class ItemManager : Singleton<ItemManager>
{
    [Header("効果時間を維持しているときのパーティクルデータ")] 
    public ParticlesData itemKeepTimePs;


    [Header("回復量")]                              public float recoveryAmount         = 1.0f;
    [Header("攻撃力上昇量")]                        public float powUpAmount            = 1.0f;
    [Header("攻撃上昇倍率")]                        public float powUpAmountMultiplier  = 1.3f;
    [Header("攻撃力上昇有効時間")]                  public float powUpTime              = 0.0f;
    [Header("攻撃力上昇有効時間の最大量")]          public float powUpTimeMax           = 5.0f;
    [Header("攻撃ポーションの取得フラグ")]          public bool  pickUpPowUpPotion      = false;
    [Header("吸収の有効時間")]                      public float magnetTime             = 0.0f;
    [Header("吸収の有効時間の最大量")]              public float magnetTimeMax          = 1.0f;
    [Header("マグネットの取得フラグ")]              public bool  pickUpMagnet           = false;
    [Header("速度上昇量")]                          public float spdUpAmount            = 1.0f;
    [Header("速度上昇の有効時間")]                  public float spdUpTime              = 0.0f;
    [Header("速度上昇倍率")]                        public float spdUpAmountMultiplier  = 2.0f;
    [Header("速度上昇の有効時間の最大量")]          public float spdUpTimeMax           = 3.0f;
    [Header("スピードアップウィングの取得フラグ")]  public bool  pickUpSpdUpWing        = false;
    [Header("獲得したアイテムの増加量")]  　　　　　public int   getItemAmount          = 1;
    [Header("無敵アイテムの取得フラグ")]            public bool  pickUpInvincble        = false;
    [Header("無敵アイテムの取得した時のフラグ")]    public bool  nowPickUpInvincble     = false;
    [Header("無敵アイテムの有効時間の最大量")]      public float invincbleTimeMax       = 0.0f;

    [Header("プレイヤーの現在のHP(参照用)")]
    public float getPlayerNowHP = 0;

    //アイテムのスターコイン獲得
    private int getStarCoinAmount = 10;

    public Vector3 defaultEffectPos = new(0.0f, 0.0f, 0.0f);

    //受けたダメージの合計(デバッグ用)
    public float takedmgTotal=0f;

    // [再抽選][強化][交換]のアイテムを取得時の増加数の取得
    private int GetItemAmount()
    {
        // キャラ強化によるアイテム効果増加
        int amount = getItemAmount;
        if(BuffManager.Instance.gobalItemEffectAdd > 0)
        {
            amount++;
        }

        return amount;
    }

    private void Update()
    {
        //マグネット取得
        PickUpItem(ref pickUpMagnet, ref magnetTime);

        //攻撃ポーション取得
        PickUpItem(ref pickUpPowUpPotion, ref powUpTime, ref powUpAmount);

        //スピードアップウィング取得
        PickUpItem(ref pickUpSpdUpWing, ref spdUpTime, ref spdUpAmount);
    }

    public void Healing()
    {
        //回復
        float healAmount = recoveryAmount;
        if (SkillEffectManager.Instance.universalTrait.isItemEffectDoubled)
        {
            healAmount *= 2;
            ActiveBuffManager.Instance.AddStack(TraitType.ItemEffectDouble);
        }

        // キャラ強化によるアイテムの効果量増加
        if (BuffManager.Instance.gobalItemEffectAdd > 0)
        {
            healAmount *= 1.0f + BuffManager.Instance.gobalItemEffectAdd;
        }

        EventManager.EmitEventData("ChangePlayerHp", healAmount);
        
    }

    public void PowerUp()
    {
        //武器の攻撃力を上昇(掛け算)
        pickUpPowUpPotion = true;

        powUpAmount = powUpAmountMultiplier;
        powUpTime = powUpTimeMax;
        if (SkillEffectManager.Instance.universalTrait.isItemEffectDoubled)
        {
            powUpAmount *= 2;
            powUpTime = powUpTimeMax * 2;
            ActiveBuffManager.Instance.AddStack(TraitType.ItemEffectDouble);
        }

        PlayTimeParticleInPlayer(itemKeepTimePs.itemPowUpPs, powUpTime, defaultEffectPos);
    }

    public void ExpAbsorption()
    {
        //EXPを吸収
        pickUpMagnet = true;

        magnetTime = magnetTimeMax;

        // キャラ強化によるアイテム効果量増加
        magnetTime *= 1.0f + BuffManager.Instance.gobalItemEffectAdd;
    }

    public void SpeedUp()
    {
        //速度を上昇
        pickUpSpdUpWing = true;

        spdUpAmount = spdUpAmountMultiplier;
        spdUpTime   = spdUpTimeMax;
        if (SkillEffectManager.Instance.universalTrait.isItemEffectDoubled)
        {
            spdUpAmount *= 2;
            spdUpTime   = spdUpTimeMax * 2; 
            ActiveBuffManager.Instance.AddStack(TraitType.ItemEffectDouble);
        }

        // キャラ強化によるアイテム効果量増加
        spdUpAmount *= 1.0f + BuffManager.Instance.gobalItemEffectAdd;

        PlayTimeParticleInPlayer(itemKeepTimePs.itemSpdUpPs, spdUpTime, defaultEffectPos);
    }

    //無敵状態になる
    public void Invincible()
    {
        pickUpInvincble = true;

        // キャラ強化によるアイテム効果増加
        float itemEffectBuff = 1.0f + BuffManager.Instance.gobalItemEffectAdd;

        float powUpPlusAmount = 1.5f + itemEffectBuff;
        float spdUpPlusAmount = 1.25f + itemEffectBuff;

        //攻撃力と速度を上昇させる
        powUpAmount = powUpPlusAmount;
        spdUpAmount = spdUpPlusAmount;
        if (SkillEffectManager.Instance.universalTrait.isItemEffectDoubled)
        {
            powUpAmount = powUpPlusAmount * 2;
            spdUpAmount = spdUpPlusAmount * 2;
            invincbleTimeMax *= 2;
            ActiveBuffManager.Instance.AddStack(TraitType.ItemEffectDouble);
        }
    }

    public void InvinsbleOnTouchEnemy()
    {
        //無敵アイテムを取得している場合もreturnを返す
        if (pickUpInvincble)
        {
            //サウンドに再生間隔を付ける
            if (SoundEffect.Instance.getITESoundFrequency <= 0)
            {               
                SoundEffect.Instance.getITESoundFrequency = SoundEffect.Instance.getITESoundFrequencyMax;
                SoundEffect.Instance.Play(SoundList.ParrySe);
            }
            return;
        }
    }

    public void GetStarCoin()
    {
        CurrencyManager.Instance.Add(getStarCoinAmount);
    }

    public void GetSwapCount()
    {
        SkillManager.Instance.exchangeChance += GetItemAmount();
    }

    public void GetBoostCount()
    {
        SkillManager.Instance.boostChance += GetItemAmount();
    }

    public void GetRerollCount()
    {
        SkillManager.Instance.reRollChance += GetItemAmount();
    }

    //アイテムを取得したときの関数(倍率ありのアイテムを呼ぶ場合)
    //_pickUpItemFlag       ...各アイテム取得したときのフラグ
    //_itemTime             ...各アイテムの有効時間
    //_currentItemMultiplier...倍率があるアイテムの現在の倍率(デフォルト 0.0f)
    public void PickUpItem(ref bool _pickUpItemFlag, ref float _itemTime, ref float _currentItemAomunt)
    {
        //そのアイテムの取得フラグが有効になったら
        if (_pickUpItemFlag)
        {
            //有効時間を減らす
            _itemTime -= Time.deltaTime;

            //有効時間が0になったら
            if (_itemTime <= 0)
            {
                //有効時間を0にする
                _itemTime = 0;

                //有効フラグは無効になる
                _pickUpItemFlag = false;

                //上昇量を元に戻す
                _currentItemAomunt = 1.0f;
            }
        }
    }

    //倍率なしのアイテムを呼ぶ場合
    public void PickUpItem(ref bool _pickUpItemFlag, ref float _itemTime)
    {
        //そのアイテムの取得フラグが有効になったら
        if (_pickUpItemFlag)
        {
            //有効時間を減らす
            _itemTime -= Time.deltaTime;

            //有効時間が0になったら
            if (_itemTime <= 0)
            {
                //有効時間を0にする
                _itemTime = 0;

                //有効フラグは無効になる
                _pickUpItemFlag = false;
            }
        }
    }

    public void PlayTimeParticleInPlayer(ParticleSystem prefab, float duration,Vector3 ParticlePos)
    {
        if (prefab == null) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        ParticleSystem ps = Instantiate(prefab, player.transform);
        ps.transform.localPosition = Vector3.zero + ParticlePos;
        ps.Play();

        StartCoroutine(StopParticleAfterTime(ps, duration));
    }

    //プレイヤーの位置でパーティクルを再生（時間制限なし）
    //ループしないパーティクル用
    public void PlayTimeParticleInPlayer(ParticleSystem prefab, Vector3 ParticlePos)
    {
        if (prefab == null) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        ParticleSystem ps = Instantiate(prefab, player.transform);
        ps.transform.localPosition = Vector3.zero + ParticlePos;
        ps.Play();
    }

    //指定したオブジェクトの位置でパーティクルを再生（時間制限あり）
    public void PlayTimeParticleInObject(GameObject obj,ParticleSystem prefab, float duration, Vector3 ParticlePos)
    {
        if (prefab == null) return;

        if (obj == null) return;

        ParticleSystem ps = Instantiate(prefab,obj.transform);
        ps.transform.localPosition = Vector3.zero + ParticlePos;
        ps.Play();

        StartCoroutine(StopParticleAfterTime(ps, duration));
    }

    //指定したオブジェクトの位置でパーティクルを再生（時間制限なし）
    public void PlayTimeParticleInObject(GameObject obj,ParticleSystem prefab, Vector3 ParticlePos)
    {
        if (prefab == null) return;

        if (obj == null) return;

        ParticleSystem ps = Instantiate(prefab, obj.transform);
        ps.transform.localPosition = Vector3.zero + ParticlePos;
        ps.transform.localScale = new Vector3(1,1,1);
        ps.Play();
    }

    //指定したオブジェクトの位置でパーティクルを再生（時間制限なし、スケール指定あり）
    public void PlayTimeParticleInObject(GameObject obj, ParticleSystem prefab, Vector3 ParticlePos, Vector3 scale)
    {
        if (prefab == null) return;

        if (obj == null) return;

        ParticleSystem ps = Instantiate(prefab, obj.transform);
        ps.transform.localPosition = Vector3.zero + ParticlePos;
        ps.transform.localScale = scale;
        ps.Play();
    }

    //パーティクルを即座に停止する関数
    public void StopParticle(ParticleSystem ps)
    {
        if(ps == null) return;

        if (ps != null)
        {
            ps.Stop();
            Destroy(ps.gameObject); // 停止後に削除
        }
    }

    private IEnumerator StopParticleAfterTime(ParticleSystem ps, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (ps != null)
        {
            ps.Stop();
            Destroy(ps.gameObject); // 停止後に削除
        }
    }
}

