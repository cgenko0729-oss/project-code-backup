using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization;

public class UpgradeEffectBase : MonoBehaviour
{
    [SerializeField,Tooltip("バフタイプ")]
    public BuffType BuffType;
    [SerializeField,Tooltip("適用するかのフラグ")]
    public bool isEnable = false;
    [SerializeField,Tooltip("合計の効果量")] 
    public float totalAmount = 0;
    [SerializeField,Tooltip("継続時間")] 
    public float activeDuration = 0;
    [SerializeField, Tooltip("発動中かのフラグ")]
    public bool isActived = true;
    [SerializeField, Tooltip("BuffManagerのインスタンス")]
    private protected BuffManager buffManagerInst;

    void Start(){ isEnable = false; }
    void Update() {}

    // 更新が必要なものがあれば継承を行う
    public virtual void EffectUpdate() {}
    // 継承先で効果が適用されるかどうかを調べる
    public virtual void CanEnableBuff() { isEnable = false; }
    // 継承先で発動するかどうかの処理を行う
    public virtual bool ActiveBuff() { return false; }
    public virtual bool ActiveBuff(Vector3 effectPos, float damageAmout) { return false; }
    // バフの合計値を適用する
    public virtual void ApplyBufftotalAmount() {}
}

