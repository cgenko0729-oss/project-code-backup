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

/*
 it may be the safest build in this game , enemy can't even get close to me !

it is the most chaotic scene i have ever seen , everything is on fire !



 */

public enum TraitType
{
    None = 0,
    DashReleaseSkill, //Enchanted skill activates immediately after dashing
    HpChangeReleaseSkill, //Automatically unleashes the enchanted skill to counterattack when damage is taken
    PickUpReleaseSkill,  //Picking up an item will activates the enchanted skill. (Triggers every 15 EXP gems collected)
    DashEnhancedSkill, //The Enchant skill Size Increase by 50% after dashing
    HpChangeEnhancedSkill, //not finished
    PickUpEnhancedSkill, //not finished
    FinishCastExplosion, //Causes an explosion at the end of the enchanted skill's effect
    FinishCastSplit, //after the skill hit the target , split into 3 direction projectile
    KillEnemyExplosion, //Enemies defeated by the enchanted skill will have 40% chance to explode
    KillEnemySkullSoul, //Enemies defeated by enchanted skill have 30% chance to release a skeletal soul that trace player and attack enemy collided
    KillEnemyBrightSoul, //30% chance to purify enemies defeated.Every 5 enemies purified will restore Player's HP
    SkillMovingDropFire, //name Trail of Flames :Creates a path of fire that incinerates enemies in the enchanted skill's path of movement
    SkillMovingDropSpike , //not finished
    EnchantFire, // Enchant skill with the fire element, inflicting continuous burn damage on enemies
    EnchantIce, //Enchant skill with the ice element, enveloping enemies in a cold aura and slows their movement
    EnchantPoison,  //Enchant skill with poison element, enemy affected will receive 25% more damage
    EnchantLightning, //not finished
    SkillPushBack, //Adds a shockwave effect to the enchanted skill, forcefully blowing away enemies it hits
    DoubleCast, //The enchant skill will cast one more time
    QuickButWeak, //cd -50 dmg -30
    BigButSlow,   //size +50 cd +30
    DashCastIce, //not finished
    DashCastFire, //not finished
    DoubleDash, //dash can stack twice ,but each dash only last half distance and time
    MoreBullet, //projectileNum +2, cd +20%
    Sniper, //projectilenum become 1, each lose projectile will add 50% size, 50%dmg,    projectileSpeed +50%, projectileDmg +30%, cd +30%
    DashEnhancedBullet, //after dash , next projectile num +2 ダッシュ後の次のスキルは、射撃数が一時的に増加する
    DashGetShield, //after dash , get a shield that can block one time damage
    TreasureHunter, //increase luck? get 100 coin? enemy drop coin? 強欲ノ嗅覚 星屑ノ恩恵 幸運ノ寵愛
    ProBomber, //fire and explosion damage +40% 爆破ノ達人,紅蓮ノ奥義 爆発と炎の専門家。関連する全てのダメージを大幅に強化する
    ProPoisoner, //poison and ghost damage +40% 劇毒ノ心得 毒と魂を操る専門家。関連する全てのダメージを大幅に強化する
    IceBreaker, //deal 50& more damage to frozen enemy 凍結状態の敵に対して、与えるダメージが劇的に増加する
    GetDamageIceExplosion, //when get damage, freeze nearby enemy
    FastAndStrong, //speed +35% dmg + 35%
    LastAndStrong, //duration +40% dmg +30%
    AllRounder, //dmg + 15%, cd -15%, size +15%,speed +15%, duration +15%
    CirticalHitter, //30% chance to deal 150% dmg
    HpGetHealCast, //when get heal, release a skill
    AfterDashEnhanceCrit, //after dash ,the next attack + 50% critChance
    GobalCritHitter, //increase gobal crit chance by 5% (all attack have 5% to crit)
    CritHealer, //if deal critical damage , 10% to heal player by 1 hp
    StandStillEnhance, //stand still for certain amount of time will increase enchanted skill dmg by 50% , size by 50%
    StandStillGetShield,// bullet shield , can block mage enemy's magic projectile
    StrongLuck, //luck + 35%
    LimitBreak, //Max Level + 3　限界突破
    QuickGrower, //level up faster + 30% 急成長
    GlassCannon, //Receive 50% more damage, but deal 50% more damage
    HpLowStronger, //Lower the hp ,higher the damage, Max 50% more damage
    GobalStrongFire, //all fire dmage +50% (firePath & fireEnchant Dot & Explosion chance)
    GobalStrongIce, //Ice can Freeze Enemy
    GobalStrongPoison, //acid explosion chance +50%
    FireSpeedUp, //MoveSpeed + 20% , move with fire path
    StandStillGetHeal, //stand still for certain amount of time will gradually heal player hp
    WalkReleaseSkill, //walk for a certain distance will release the enchanted skill 
    AfterCastSpeedAdd, //after cast skill , move speed + 5%, max 10 stack, each last 4 sec
    HealDouble, //double all heal amount
    EnhanceFire, //double fire related damage
    EnhanceIce, //enchance ice , now ice can freeze enemy compeletely
    EnhancePoison,//damage poisoned enemy receive + 50%, acid explosion chance will be double
    EnhanceCrit, //crit damage become 200%, all weapon + 5% crit chance
    Gambler,// cast skill have 50% to double damage , 25% for damage to be 0
    FairJudge, //all trait who have 50% to trigger will have 100% to trigger
    Berserker, //when hp lower than 30%, move speed +50%, attack speed +50%
    Defender, //player defence + 35 , moveSpeed -20%
    Reviver, //when hp drop to 0 , revive once with 50% hp
    SlotAddDamage, //each slot will increase damage by 25%
    GoldPicker, //increase gold gain by 40%, pickUp Range by 50% 
    KillEnemyDropHeal, //kill enemy will have 3% drop heal item to heal player hp by 3
    ItemEffectDouble, //all item effect will double,last time double, pickuprange + 50%, item box will drop 2 items
    GetChestItemAddDamage, //each chest or item you pick will permenantly increase your damage by 5%
    AddMaxHp, //MaxHp + 50
    LuckySeven, //luck + 7%, all chance to trigger +7%
    SoulEater, //each kill enemy will reduce skill cd by 0.5 sec
    Isolator, //when target enemy has no other enemy around 5 meter , skill damage +100%
    GetDamageSummonPuppet, //when get damage , 50% release a puppet ,puppet when get damage can trigger revenge.
    KillEnemyStronger, //each kill enemy will increase skill damage and cast speed by 5% , max 10 stack , last 5 sec
    DealMoreDamageFullHp, //deal more damage when enemy hp >90% 
    CastSummonWolf, // when cast enchanted skill, have 50% to summon a wolf to attack nearby enemy 
    DebuffDisable, //player will not be affected by debuff from enemy and map
    Equilibrium, //player 's all status +5% (attack , defence , crit , move speed , luck),player's damage,defence,moveSpeed,cirtChance,Luck,exp gain + 5%
    FireWalker, //walk on fire path will increase move speed by 30%
    AfterDashAddMoveSpeed, // after player dash , increase move speed by 30% for 2.5s
    HitEnemyAddSize, // each hit on enemy will increase skill size by 2% , max +50%
    PickCoinGetCrit, // each pick coin will increase crit chance by 3% ,last 3 sec , max 30% Lucky boy
    FirePathDealFireDot, // fire path will deal fire dot to enemy who walk on it
    OnFireAddCrit, // when enemy is on fire , crit chance + 30%
    DashPushBackEnemy, // dash will push back enemy and damage enemy, player dash cooldown -15% (Brave dash), or dash dist longer
    GetDamageAddAttack, // when get damage , attack + 25% , last 5 sec 仇討ちの スキル名 仕返し
    GetCoinAddAttack,
    PetGetStronger, // pet Damage * 2, SkillCooldown * 0.5
    LuckAddCrit, //each luck point will increase crit chance by 1%
    KillEnemyBigger, //each kill enemy will increase skill size by 10% , max 100% ， but speed will decrease by 5% each stack,
    LonelyMan, //carry no pet, player all status + 10% (luck ,damage , defence, move speed , crit chance)
    Elementalist, // each cast randomly enchant skill with fire , ice , poison.
    MoveSpeedAddAttack, // each 10% move speed will increase attack by 5% 暴走
    BloodPrice, //each cast cost 2 hp lost , but damage + 30%,crit + 30%　血まみれ
    AdrenalineRush,         // 15. Kill enemy = Attack Speed +2% (stacks, resets on hit)　
    ItemMaster, //each 60s spawn a item box nearby player, itemBox show icon for location
    HpLimitless, //player hp  can heal beyond max hp , up to 150% max hp
    PlayerGetBigger, //player size + 30% , skill size +30%, move speed -15%
    PlayerGetSmaller, //player size - 30% , skill size -20%, move speed +15%
    ElementalResonance, //deal 50% more damage to enemy who is affected by any element (burn , slow , poison) 
    GiftAngel, //periodically spawn a gift box that contain random item or heal or gold , angel effect
    DogSkill1_DefenceMind,      // PerCoinEffect
    DogSkill2_AttackMind,       // PerExpEffect
    DogSkill3_DogNail,          // PerAttackEffect
    RabbitSkill1_ArcherSpread,  // ProjNumScalingEffect
    RabbitSkill2_SurvivalSkill, // UpGetExp&ItemEffect
    RabbitSkill3_ArcherSpeed,   // SpeedScalingEffect
    LionSkill1_WarriorPride,    // HealScalingEffect
    LionSkill2_Berserker,       // PerDamageEffect
    LionSkill3_LonelyLion,      // PetScalingEffect
    OwlSkill1_EmergencyBarrier, // PinchBuffEffect
    OwlSkill2_WonderPocket,     // RandomEffect
    OwlSkill3_TenshionUp,       // AddAttackEffect

}

//Useless Count = 

public enum TraitJobType
{
    None = 0,
    Universal = 1,
    Knight = 2,
    Archer = 3,
    Mage = 4,
    Warrior = 5,
}


/*
  
 均衡の律 (Law of Equilibrium)　 調和の恩寵 全域強化 能力値を一括+5% 攻撃・防御・速度など一律+5%
 
 賭徒的幸運 (Gambler's Fortune)
效果: 技能有50%的機率造成雙倍傷害，但也有50%的機率只造成一半傷害。 
 
嗜血渴望 (Bloodlust)
效果: 技能每擊中一名敵人，回復自身1%的已損失生命值。

元素共鳴 (Elemental Resonance)
效果: 當此技能擊中一個已經附有任何元素狀態（燒傷、緩速、中毒）的敵人時，會引發一次元素爆炸，對周圍敵人造成範圍傷害。
分析: 這是元素流派的核心特性，極大地增強了爆炎刻印、冰霜刻印和猛毒刻印之間的協同作用。
連鎖閃電 (Chain Lightning)
效果: 技能擊中敵人時，會釋放一道閃電鏈，彈射至最多3個額外的敵人，造成電擊傷害並有機率造成短暫麻痺。

霜火交融 (Frostfire)
效果: 當一個被冰霜刻印緩速的敵人再受到爆炎刻印的燒傷傷害時，會立即解除緩速並造成一次巨大的額外傷害。

毒擴散 (Virulent Contagion)
效果: 附有猛毒刻印的敵人在被擊倒時，會將其剩餘的中毒效果傳播給周圍的所有敵人。
分析: 增強了毒素的群體傷害能力，特別是在面對大量敵人時，可以形成毀滅性的連鎖反應。

超載運轉 (Overdrive)
效果: 技能的冷卻時間增加100%，但傷害和範圍也增加100%。
分析: 創造了一個“一擊制勝”的玩法風格，將普通技能變為終極大招。與神速之力結合可以平衡其過長的冷卻時間，形成一種獨特的平衡。

靈魂吞噬 (Soul Eater) //cd + 100% , 
效果: 每擊倒一個敵人，此技能的冷卻時間減少0.5秒。
分析: 在面對大量敵人時極為有效，擊殺越多，技能施放越頻繁，形成一個高效的“割草”循環。

//Cd -100% ,size + 100% , dmg to 1, 

背水の戦い (Fight to the Death)

幸運の七　(Lucky Seven) ; luck + 7%, all chance to trigger +7%

孤立無援 (Isolator)
效果: 對周圍5米內沒有其他敵人的目標，此技能傷害增加100%。
分析: 鼓勵玩家優先處理落單的敵人，或利用疾風之力等擊退效果創造出孤立的目標，進行精準打擊。

連斬之魂 (Soul of Culling)
效果: 每當此技能擊倒一名敵人，會在5秒內獲得一層“連斬”增益，每層使此技能的施放速度和傷害增加5%。最多可疊加10層。
分析: 創造了一種“愈戰愈勇”的動態，鼓勵玩家快速連續地擊殺敵人以保持巔峰狀態，形成收割循環。

鏡像僕從 (Mirror Servant)
效果: 技能施放時，創造一個會模仿你施放此技能的鏡像。鏡像造成的傷害為原來的30%，存在5秒。
分析: 提供了額外的火力來源和戰術可能性，玩家可以利用鏡像從不同角度攻擊敵人，或同時打擊多個目標。

//when get damage , 50% release a puppet ,puppet when get damage can trigger revenge.

混亂低語 (Whispers of Chaos)
效果: 被技能擊中的非精英敵人有15%的機率陷入混亂狀態，持續5秒，期間會隨機攻擊其他敵人。
分析: 強大的群體控制特性，能有效瓦解敵人的陣型，讓它們自相殘殺，為玩家創造喘息和輸出的機會。

終詠の疾走 残響の疾歩 余韻疾走

行軍解呪 逢魔の歩 路紋発動

慈愛の恵　聖療増幅

紅蓮増幅

氷華封印

瘴気増幅　毒脈濃縮　蛇毒覚醒　腐蝕倍化
　
致命倍撃　破甲閃　断罪　凶星の牙

賭博師の掟　　天運断章

天秤の宣告　公正裁断　天秤の裁断　均衡の律　all positive buff that trigger chance less than 50% will have 50% to trigger

狂戦覚醒 修羅駆動 逆境猛進

盾紋堅守 鉄壁の誓約 守護刻印 防衛姿勢 甲冑の重み 

不死鳥の翼 復活の祝福 生命の灯火 蘇生印 黄泉 不死の誓い

枠力増幅 格納火力 スロット共鳴 枠力共鳴 連結増傷 収納火力域 

不動の祝祭  祈静の陣 静止強化 沈黙の増幅 佇立

双閃疾駆 影踏み

慈雨ノ欠片
敵を倒すと3%の確率で回復の欠片を落とす 恵みの滴

宝祈ノ祝福 全アイテム効果×2／持続時間×2 箱+1個排出 富者の加護

金運招来 金脈の嗅覚 財宝ハンター 強欲の手 ゴールド**+40%、拾い範囲+50%**。

戦利の刻印 収奪の誓約 貪欲の契約 戦果吸収  箱/アイテム取得ごとに与ダメ+5%（永続）。

会心覇道 弱点看破 クリティカルダメージ200%、全武器会心+5%

公正なる審判 均衡ノ断 発動率50%の効果が100%**で発動
 
命中時、三方向に分裂弾を生成

堅命の符 最大HP+50

for the naming , please use more idea of animal , like 


 */