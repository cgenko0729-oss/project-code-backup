using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PetDataBase", menuName = "GameScene/Pet/PetDataBase")]

public class PetDataBase : ScriptableObject
{
    [Header("ペットデータが登録されたデータベース")]
    [Tooltip("この中にPetDataを格納していく\n(例)PetData_Kappa、PetData_Naga etc...")]
    [PreviewSprite]public List<PetData> allPetData;
}
