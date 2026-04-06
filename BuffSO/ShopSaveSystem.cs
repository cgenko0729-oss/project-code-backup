using System.Collections.Generic;
using UnityEngine;
using TigerForge;

public staticÅ@class ShopSaveSystem 
{

    const string FILE = "shop_progress";     // disk = ÅgÅc/shop_progress.datÅh

    public static void Save(int coins,
                            Dictionary<int,int> levels)
    {
        EasyFileSave file = new EasyFileSave(FILE);
        file.Add("coins",  coins);
        file.Add("levels", levels);          // auto-serialised
        file.Save();
    }

    public static bool Load(out int coins,
                            out Dictionary<int,int> levels)
    {
        coins = 0; levels = new();
        EasyFileSave file = new EasyFileSave(FILE);
        if (!file.Load()) return false;

        coins  = file.GetInt("coins", 0);
        levels = file.GetDictionary<int,int>("levels");
        file.Dispose();
        return true;
    }

    public static bool Exists() => new EasyFileSave(FILE).FileExists();
    public static void Delete() => new EasyFileSave(FILE).Delete();

}
