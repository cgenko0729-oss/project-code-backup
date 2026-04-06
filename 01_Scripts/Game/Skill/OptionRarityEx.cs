using UnityEngine;
using System.Collections.Generic;


public static class OptionRarityEx 
{
   public static OptionRarity? Next(this OptionRarity rar)
    {
        return rar switch
        {
            OptionRarity.Normal    => OptionRarity.Rare,
            OptionRarity.Rare      => OptionRarity.Epic,
            OptionRarity.Epic      => OptionRarity.Legendary,
            _                      => null          // Legendary has no next
        };
    }
}

