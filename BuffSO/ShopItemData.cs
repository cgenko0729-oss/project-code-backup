using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Shop Item")]
public class ShopItemData : ScriptableObject
{
    public ShopItemType itemType;
    public string displayName;                  // “Move Speed”
    [TextArea] public string descriptionFormat; //   e.g. "Increase move speed by {0}% (Total +{1}%)"
    
    public Sprite icon;

    [System.Serializable]
    public struct Level
    {
        public int needMoney;    // そのレベルの必要コイン
        public float addAmount;  // そのレベルで増える数値
    }

    // レベル 0 → 1、1 → 2 … すべてここに並べる
    public List<Level> levels = new List<Level>() { new Level{needMoney = 50, addAmount = 0.5f} };
}
