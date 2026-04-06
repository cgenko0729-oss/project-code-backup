using UnityEngine;

//enum for enemyType
public enum EnemyType
{
    [JPName("敵なし")] NoEnemy,
    [JPName("クモ")]　Mover,
    [JPName("コウモリ")]　Flyer,
    [JPName("スライム")]　Slime,
    [JPName("ドラゴン")]　Bomber,
    [JPName("キノコ")]　Surrounder,
    [JPName("魔法使い")]　Caster,
    [JPName("クモボス")]Boss,
    [JPName("エリートクモ")]EliteMover,
    [JPName("中ボス")] MidBoss,
}

public enum SpawnMethod
{
    RandomRange,
    Circular,   
    Slash,
    Horizontal,
    Vertical,
    Single,     
    Spiral,
    TargetPoint,
    TwoSide,
}
