using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using UnityEngine.Localization.Settings;

public static class L
{
    public static string DiffDesc(DifficultyType diff) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "TextTable", $"difficulty.{diff}.desc");


    public static string AchName(string key) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "Achievement", $"ach.{key}.name");

    public static string AchDesc(string key) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "Achievement", $"ach.{key}.desc");

    public static string AchReward(string key) =>
         LocalizationSettings.StringDatabase.GetLocalizedString(
            "Achievement", $"achReward") +
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "Achievement", $"ach.{key}.reward");

    public static string AchCoin() =>   
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "Achievement", $"achReward") +
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "Achievement", $"ach.coinReward");

    public static string CharacterName(JobId jobType) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "TextTable", $"character.{jobType}.name");

    public static string CharacterJobType(JobId jobType) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "TextTable", $"character.{jobType}.jobType");

    public static string MapName(MapType nowMapType) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "TextTable", $"map.{nowMapType}.name");

    public static string SkillName(SkillIdType id, bool final = false) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "Skills", final ? $"skill.{id}.finalName" : $"skill.{id}.name");

    public static string SkillDesc(SkillIdType id, bool final = false) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "Skills", final ? $"skill.{id}.finalDesc" : $"skill.{id}.desc");

    public static string TraitName(TraitType t) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "Traits", $"trait.{t}.name");

    public static string TraitDesc(TraitType t) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "Traits", $"trait.{t}.desc");

    public static string PetName(PetType t) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "Pets", $"pet.{t}.name");

    public static string PetDesc(PetType t) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "Pets", $"pet.{t}.desc");

    public static string PetSkillName(PetType t) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "Pets", $"pet.{t}.skillName");

    public static string PetSkillDesc(PetType t) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "Pets", $"pet.{t}.skillDesc");

    public static string TraitTable(string key) => 
        LocalizationSettings.StringDatabase.GetLocalizedString("Traits", key);


    public static string UI(string key) =>
        LocalizationSettings.StringDatabase.GetLocalizedString("UI", key);

    public static string EnumKey(string key) =>
        LocalizationSettings.StringDatabase.GetLocalizedString("Enums", key);

    public static string ItemName(ShopItemType t) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "Shop", $"shop.item.{t}.name");

     //public static string ItemDesc(ShopItemType t, float next, float total) =>
     //   LocalizationSettings.StringDatabase.GetLocalizedString(
     //       "Shop", $"shop.item.{t}.desc", new object[] { next, total });

    public static string ItemDesc(ShopItemType t, float next, float total)
    {
        //if(next == 0)
        //{
        //    return  LocalizationSettings.StringDatabase.GetLocalizedString(
        //    "Shop", $"shop.item.{t}.desc", new object[] { "-", total });
        //}
        //else
        //{
        //    return  LocalizationSettings.StringDatabase.GetLocalizedString(
        //    "Shop", $"shop.item.{t}.desc", new object[] { next, total });
        //}

        // 1. Define your green color. 
    // You can use standard names like "green" or specific Hex codes like "#00FF00".
    string colorTag = "#00FF00"; 

    // 2. Format the "Next" value
    // If next is 0, we use "-", otherwise we wrap the number in color tags
    string nextValStr;
    if (next == 0)
    {
        // If you want the "-" to be white/default, leave it as is. 
        // If you want the "-" to be green too, wrap it like the else block.
        nextValStr = "-"; 
    }
    else
    {
        // Result looks like: <color=#00FF00>5.5</color>
        nextValStr = $"<color={colorTag}>{next}</color>";
    }

    // 3. Format the "Total" value
    string totalValStr = $"<color={colorTag}>{total}</color>";

    // 4. Pass the modified STRINGS into the localization system
    // The Localization system will replace {0} with nextValStr and {1} with totalValStr
    return LocalizationSettings.StringDatabase.GetLocalizedString(
        "Shop", 
        $"shop.item.{t}.desc", 
        new object[] { nextValStr, totalValStr }
    );
         
    }
       

    public static string QuestName(QuestName t) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "Quests", $"quest.{t}.name");

    public static string UpgradeName(JobId jobId, string dataId) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "UpgradeTree", $"{jobId}.{dataId}.name");

    public static string UpgradeDesc(JobId jobId, string dataId) =>
        LocalizationSettings.StringDatabase.GetLocalizedString(
            "UpgradeTree", $"{jobId}.{dataId}.desc");
}

public static class EnumLocalizationExtensions
{
    public static string ToLocalized(this OptionRarity r)
        => L.EnumKey($"OptionRarity.{r}");
    public static string ToLocalized(this SkillStatusType t)
        => L.EnumKey($"SkillStatusType.{t}");
    public static string ToLocalized(this SideEffectType t)
        => L.EnumKey($"SideEffectType.{t}");
}




/*
Achievement list : 

ach.Stage_SpiderForest_Normal_Cleared.name 森の征服者
ach.Stage_SpiderForest_Normal_Cleared.desc ステージ1のノーマル難易度を初めてクリアする
ach.Stage_SpiderForest_Normal_Cleared.reward 新しいステージ、キャラクターアンロック

ach.Stage_AncientForest_Normal_Cleared.name 遺跡の征服者
ach.Stage_AncientForest_Normal_Cleared.desc　ステージ2のノーマル難易度を初めてクリアする
ach.Stage_AncientForest_Normal_Cleared.reward　新しいステージアンロック

ach.Stage_Desert_Normal_Cleared.name 砂漠の征服者
ach.Stage_Desert_Normal_Cleared.desc ステージ3のノーマル難易度を初めてクリアする
ach.Stage_Desert_Normal_Cleared.reward 新しいステージアンロック

ach.Stage_Temple_Normal_Cleared.name 神殿の征服者
ach.Stage_Temple_Normal_Cleared.desc ステージ4のノーマル難易度を初めてクリアする
ach.Stage_Temple_Normal_Cleared.reward 新しいステージアンロック

//killed_10000_enemies
ach.killed_10000_enemies.name モンスタースレイヤー1
ach.killed_10000_enemies.desc 敵を10000体倒す
ach.killed_10000_enemies.reward コイン300枚

ach.killed_50000_enemies.name モンスタースレイヤー2
ach.killed_50000_enemies.desc 敵を50000体倒す
ach.killed_50000_enemies.reward コイン500枚

ach.killed_100000_enemies.name モンスタースレイヤー3
ach.killed_100000_enemies.desc 敵を100000体倒す
ach.killed_100000_enemies.reward コイン1000枚

ach.Quest_FInish_5.name クエストマスター1
ach.Quest_FInish_5.desc クエストを5回クリアする
ach.Quest_FInish_5.reward コイン200枚

ach.Quest_FInish_10.name クエストマスター2
ach.Quest_FInish_10.desc クエストを10回クリアする
ach.Quest_FInish_10.reward コイン500枚

ach.Quest_FInish_20.name クエストマスター3
ach.Quest_FInish_20.desc クエストを20回クリアする
ach.Quest_FInish_20.reward コイン700枚

ach.GameClear_1.name 初クリア
ach.GameClear_1.desc 初めてゲームをクリアする
ach.GameClear_1.reward コイン200枚

ach.GameClear_5.name ゲームマスター1
ach.GameClear_5.desc ゲームを5回クリアする
ach.GameClear_5.reward コイン500枚

ach.GameClear_10.name ゲームマスター2
ach.GameClear_10.desc ゲームを10回クリアする
ach.GameClear_10.reward コイン1000枚

ach.GameDeath_1.name 初ゲームオーバー
ach.GameDeath_1.desc 初めてゲームオーバーになる
ach.GameDeath_1.reward コイン200枚

ach.GameDeath_10.name 不屈の挑戦者1
ach.GameDeath_10.desc ゲームオーバーを10回迎える
ach.GameDeath_10.reward コイン500枚

ach.GameDeath_50.name 不屈の挑戦者2
ach.GameDeath_50.desc ゲームオーバーを50回迎える
ach.GameDeath_50.reward コイン1000枚

ach.ItemCollect_20.name アイテムコレクター1
ach.ItemCollect_20.desc アイテムを20個集める
ach.ItemCollect_20.reward コイン200枚

ach.ItemCollect_.50.name アイテムコレクター2
ach.ItemCollect_50.desc アイテムを50個集める
ach.ItemCollect_50.reward コイン500枚

ach.ItemCollect_100.name アイテムコレクター3
ach.ItemCollect_100.desc アイテムを100個集める
ach.ItemCollect_100.reward コイン1000枚

ach.Quest_FInish_5.name クエストハンター1
ach.Quest_FInish_5.desc クエストを5回クリアする
ach.Quest_FInish_5.reward コイン200枚

ach.Quest_FInish_10.name クエストマスター2
ach.Quest_FInish_10.desc クエストを10回クリアする
ach.Quest_FInish_10.reward コイン500枚

ach.Quest_FInish_20.name クエストマスター3
ach.Quest_FInish_20.desc クエストを20回クリアする
ach.Quest_FInish_20.reward コイン700枚

ach.DamageDeal_500000.name ダメージディーラー1
ach.DamageDeal_500000.desc 総ダメージ量が500,000を超える
ach.DamageDeal_500000.reward コイン300枚

ach.DamageDeal_1000000.name ダメージディーラー2
ach.DamageDeal_1000000.desc 総ダメージ量が1,000,000を超える
ach.DamageDeal_1000000.reward コイン500枚

ach.DamageDeal_5000000.name ダメージディーラー3
ach.DamageDeal_5000000.desc 総ダメージ量が5,000,000を超える
ach.DamageDeal_5000000.reward コイン1000枚


ach.CoinCollect_10000.name 金持ち
ach.CoinCollect_10000.desc ゲーム中で総コイン獲得数が2000を超える
ach.CoinCollect_10000.reward コイン300枚

ach.CoinCollect_50000.name 富豪
ach.CoinCollect_50000.desc ゲーム中で総コイン獲得数が5000を超える
ach.CoinCollect_50000.reward コイン500枚

ach.CoinCollect_100000.name 大富豪
ach.CoinCollect_100000.desc ゲーム中で総コイン獲得数が10000を超える
ach.CoinCollect_100000.reward コイン1000枚

ach.Survive_Endless_30min.name 不屈の闘志
ach.Survive_Endless_20min.desc エンドレスモードで30分間生存する
ach.Survive_Endless_20min.reward コイン700枚

ach.PlayTime_60min.name 耐久者1
ach.PlayTime_60min.desc 総プレイ時間が60分を超える
ach.PlayTime_60min.reward コイン300枚

ach.PlayTime_120min.name 耐久者2
ach.PlayTime_120min.desc 総プレイ時間が120分を超える
ach.PlayTime_120min.reward コイン500枚

ach.PlayTime_300min.name 耐久者3
ach.PlayTime_300min.desc 総プレイ時間が300分を超える
ach.PlayTime_300min.reward コイン1500枚





 */


