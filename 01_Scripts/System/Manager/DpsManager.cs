using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class DpsManager : Singleton<DpsManager>
{
    [SerializeField] private float windowSeconds = 6f;

    public  readonly Dictionary<SkillIdType, float> skillDamageList = new(); // スキルIDごとのダメージ合計を保持する辞書
    public  readonly Queue<(float time, SkillIdType type, float dmg)> samples = new();

    public event Action OnDpsUpdate; 

    public float TotalMeasuredDps; // 全体のDPS合計
    public float slashDps;  // スラッシュスキルのDPS
    public float circleDps; // 円形ボールスキルのDPS
    public float thunderDps;// 雷スキルのDPS
    public float poisonFieldDps; // 毒フィールドスキルのDPS
    public float tornadoDps; // 竜巻スキルのDPS
    public float shieldDps; // シールドスキルDPS
    public float boomerangDps; // ブーメランスキルのDPS
    public float arrowDps; // 矢スキルのDPS
    public float starMagicDps; // 星の魔法スキルのDPS
    public float bounceDps; // バウンススキルのDPS

    private float turnStartTime;
    public float lifeTimeDps;
    public float allSkillLifeTimeTotalDamage;
    public float slashLifeTimeTotalDamage;
    public float circleLifeTimeTotalDamage;
    public float thunderLifeTimeTotalDamage;
    public float poisonFieldLifeTimeTotalDamage;
    public float tornadoLifeTimeTotalDamage;
    public float shieldLifeTimeTotalDamage;

    public float starMagicLifeTimeTotalDamage;
    public float boomerangLifeTimeTotalDamage;
    public float arrowLifeTimeTotalDamage;
    public float bounceLifeTimeTotalDamage;

    public readonly Dictionary<SkillIdType, float> skillLifeTimeTotalDamageList = new();

    public float GetMeasuredDPS(SkillIdType type) => skillDamageList.TryGetValue(type, out var dmg) ? dmg / windowSeconds : 0f;

    public void Awake()
    {
        turnStartTime = Time.time;  // ターン開始時刻を記録
    }

    public float GetDamageBySkillType(SkillIdType type)
    {
        return skillDamageList.TryGetValue(type, out var dmg) ? dmg : 0f; // スキルIDごとのダメージを取得 (存在しない場合は0を返す)

    }

    public float GetLifeTimeTotalDamageBySkillType(SkillIdType type)
    {
        return skillLifeTimeTotalDamageList.TryGetValue(type, out var dmg) ? dmg : 0f;
    }

    public void ReportDamage(SkillIdType type, float dmg)
    {
        float now = Time.time;
        samples.Enqueue((now, type, dmg));  // サンプルをキューに追加

        if (!skillDamageList.ContainsKey(type)) skillDamageList[type] = 0; // 初回登録時は0で初期化
        skillDamageList[type] += dmg;   // スキルIDごとのダメージ合計に加算

        while (samples.Count > 0 && now - samples.Peek().time > windowSeconds) // ウィンドウ期間を超えたサンプルは削除
        {
            var s = samples.Dequeue();          // キューからサンプルを取り出す
            skillDamageList[s.type] -= s.dmg;  // そのスキルIDのダメージ合計から減算
        }

        allSkillLifeTimeTotalDamage += dmg;

        if (!skillLifeTimeTotalDamageList.ContainsKey(type))
        {
            skillLifeTimeTotalDamageList[type] = 0;
        }
        skillLifeTimeTotalDamageList[type] += dmg;

        switch (type)
        {
            case SkillIdType.Slash:
                slashLifeTimeTotalDamage += dmg;
                break;
            case SkillIdType.CircleBall:
                circleLifeTimeTotalDamage += dmg;
                break;
            case SkillIdType.Thunder:
                thunderLifeTimeTotalDamage += dmg;
                break;
            case SkillIdType.PoisonField:
                poisonFieldLifeTimeTotalDamage += dmg;
                break;
            case SkillIdType.Tornado:
                tornadoLifeTimeTotalDamage += dmg;
                break;
            case SkillIdType.circleShield:
                shieldLifeTimeTotalDamage += dmg; // シールドスキルのDPSを計算
                break;
            case SkillIdType.boomerang:
                boomerangLifeTimeTotalDamage += dmg; // ブーメランスキルのDPSを計算
                break;
            case SkillIdType.arrow:
                arrowLifeTimeTotalDamage += dmg; // 矢スキルのDPSを計算
                break;
            case SkillIdType.starMagic:
                starMagicLifeTimeTotalDamage += dmg; // 星の魔法スキルのDPSを計算
                break;
            case SkillIdType.Bounce:
                bounceLifeTimeTotalDamage += dmg; // バウンススキルのDPSを計算
                break;

        }


        OnDpsUpdate?.Invoke();  // DPS更新イベントを発動
    }


    public void Update()
    {
        TotalMeasuredDps = skillDamageList.Values.Sum() / windowSeconds;　// 全体のDPS合計を計算

        float elapsed = Time.time - turnStartTime;
        lifeTimeDps = elapsed > 0f ? allSkillLifeTimeTotalDamage / elapsed : 0f;

        slashDps = GetMeasuredDPS(SkillIdType.Slash);
        circleDps = GetMeasuredDPS(SkillIdType.CircleBall);
        thunderDps = GetMeasuredDPS(SkillIdType.Thunder);
        poisonFieldDps = GetMeasuredDPS(SkillIdType.PoisonField);
        tornadoDps = GetMeasuredDPS(SkillIdType.Tornado);
        shieldDps = GetMeasuredDPS(SkillIdType.circleShield);
        boomerangDps = GetMeasuredDPS(SkillIdType.boomerang);
        arrowDps = GetMeasuredDPS(SkillIdType.arrow);
        starMagicDps = GetMeasuredDPS(SkillIdType.starMagic);
        bounceDps = GetMeasuredDPS(SkillIdType.Bounce);
    }

}

