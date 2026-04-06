using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public enum SceneType
{
    Title,
    Loading,
    Game,
}

public class StageManager : SingletonA<StageManager>
{

    public MapData mapData;
    public int stageId = 1;
    public bool isEndlessMode = false;

    public bool isShownWelcomePage = false;

    public bool isGameScene = false;

    public SceneType currentScene = SceneType.Title;

    public float gameHighestScore = 0f;

    [PreviewSprite][Searchable]public List<TraitData> allTraitInitList;

    private void Start()
    {
        if(mapData != null && mapData.stageDifficulty == DifficultyType.None)
        {
            mapData.stageDifficulty = DifficultyType.Normal;
        }


        foreach (TraitData trait in allTraitInitList)
        {
            trait.CheckUnlockStatus();
        }

    }

    private void OnEnable()
    {
        EventManager.StartListening(GameEvent.pushTitlebtn, ChangeToGameScene);
    }

    private void OnDisable()
    {
        EventManager.StopListening(GameEvent.pushTitlebtn, ChangeToGameScene);
    }

    public void ChangeToGameScene()
    {
        currentScene = SceneType.Game;
        EventManager.EmitEvent(GameEvent.SceneChanged);
    }

    public void ChangeToTitleScene()
    {
        currentScene = SceneType.Title;
        EventManager.EmitEvent(GameEvent.SceneChanged);
    }


}

