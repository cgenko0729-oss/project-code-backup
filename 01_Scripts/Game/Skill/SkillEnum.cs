using UnityEngine;

public enum SkillIdType
{
    [JPName("スキルなし")]  None,
    [JPName("ファイヤーボール")]  Fireball, //not finished
    [JPName("ボール")]  CircleBall, //Meteors orbit around the player
    [JPName("竜巻")]  Tornado, //Summon a forward-moving tornado to damage enemies ahead
    [JPName("スラッシュ")]  Slash, //Attacks in front of the player
    [JPName("雷")]  Thunder, //Call lightning on nearby enemies
    [JPName("毒フィールド")]　PoisonField, //Create a poison field at your feet
    [JPName("炎隕石")] Meteor, ////not finished
    [JPName("騎士の盾")] circleShield, //orbit around the player and blocks enemy's projectile
    [JPName("ザメ")] bombShark, //Explode Uponing Contacting Enemies
    [JPName("ブーメラン")] boomerang, //Also deals damage on return
    [JPName("星魔法")] starMagic, //not finished
    [JPName("バウンサー")] Bounce, //Reflects off enemies or screen edges
    [JPName("矢")] arrow, //Shoot arrows at enemies ahead
    [JPName("ハンマー")] Hammer, //Slam the ground to create a shockwave
    [JPName("氷魔法")] Ice, //Summon ice magic
    [JPName("手裏剣")] Suriken, //プレーヤーの前方に手裏剣を発射する,スキル終了時に三つの小さい手裏剣に分裂する
    [JPName("花")] MonsterFlower, //Placed around the player, it automatically fires bullets and attacks enemies.
    [JPName("鳥")] Bird, //Tracks the player’s mouse and attacks enemies.
    [JPName("ブラックホール")] BlackHole, //not finished
    [JPName("DF")] DamageField, //spawn on player's feet and stay and damage enemy 
    [JPName("pet")] Pet,
}

public enum SkillStatusType
{
    [JPName("クールダウン")]Cooldown,
    [JPName("スピード")]Speed,
    [JPName("持続時間")]Duration,
    [JPName("ダメージ")]Damage,
    [JPName("サイズ")]Size,
    [JPName("弾数")]ProjectileNum,
    [JPName("なし")]None,
}

public enum SideEffectType
{
    [JPName("なし")]           None               = 0,
    [JPName("ダメージ")]    DamagePercent,         // 与ダメ％ ↓
    [JPName("クールダウン")]CooldownPercent,       // CT％ ↑
    [JPName("スピード")]SpeedPercent,           // 投射／移動速度％ ↓
    [JPName("持続時間")]       DurationPercent,        // 持続時間％ ↑
    [JPName("サイズ")]         SizePercent,             // サイズ％ ↓

    // …
}

public enum PassiveSkillStatusType
{
    None,
    PickupRangeBonus,
    ExpBonus,
    HpBonus,
    MoveSpeedBonus,
    SpeedBonus,
    DurationBonus,
    DamageBonus,
    SizeBonus,
    ProjectileNumBonus,
    CooldownBonus,



}

public enum OptionRarity
{
    [JPName("ノーマル")]Normal,
    [JPName("レア")]Rare,
    [JPName("エピック")]Epic,
    [JPName("レジェンド")]Legendary,
}

public enum SkillElementType
{
    None,
    Fire,
    Water,
    Thunder,
}
