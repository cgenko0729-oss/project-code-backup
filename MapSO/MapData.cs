using UnityEngine;

[CreateAssetMenu(menuName = "Game/Map Data", fileName = "Map_")]
public class MapData : ScriptableObject
{
    public MapType    mapType = MapType.None; 
    public DifficultyType stageDifficulty = DifficultyType.Normal;
}