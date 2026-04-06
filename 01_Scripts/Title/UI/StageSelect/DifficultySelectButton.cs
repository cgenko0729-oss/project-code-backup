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

public class DifficultySelectButton : MonoBehaviour
{
    [Header("難易度の変更に必要なデータ")]
    [SerializeField] public MapData mapData;
    [SerializeField] public DifficultyType difficultyType;

    [Header("選択中かの判別に必要なデータ")]
    public Image buttonImage;
    [SerializeField] private Color selectedColor;

    private void Update()
    {
        if(mapData.stageDifficulty == difficultyType)
        {
            buttonImage.color = selectedColor;
        }
        else
        {
            buttonImage.color = Color.white;
        }
    }

    public void OnClickButton()
    {
        mapData.stageDifficulty = difficultyType;
    }
}

