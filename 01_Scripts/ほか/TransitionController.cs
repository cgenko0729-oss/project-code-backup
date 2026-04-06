using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;

using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool
using EasyTransition;       //シーン遷移


public class TransitionController : Singleton<TransitionController>
{

    public TransitionSettings tSetting;

    public TransitionSettings newSetting;


    void Start()
    {
        
    }

    void Update()
    {
        //使う時はusing EasyTransition; を追加
        //シーン追加方法: File > Build Profiles > Drag and Drop Scene from Folder

        //if (Input.GetKeyDown(KeyCode.Space)) {
        //    TransitionManager.Instance().Transition("testScene",tSetting,0f);//シーン遷移実行
        //}

        //if(Input.GetKeyDown(KeyCode.N)) {
        //    TransitionManager.Instance().Transition(tSetting, 0f); //シーン遷移エフェクトを流すだけ
        //}

    }

    public void DoFadeTransition()
    {
         TransitionManager.Instance().Transition(tSetting, 0f);
    }

    public void DoSceneTranstion()
    {

    }

    public void TransitionToTitleScene()
    {
        //Time.timeScale = 1f; //時間を戻す
        //AudioManager.Instance.FadeOutNowBGM();

        //StageManager.Instance.isGameScene = false;
        //StageManager.Instance.ChangeToTitleScene();

        //TransitionManager.Instance().Transition("TitleScene",tSetting,0f);

    }

    public void ChangeToTitleScene()
    {
        Time.timeScale = 1f; //時間を戻す
        AudioManager.Instance.FadeOutNowBGM();

        StageManager.Instance.isGameScene = false;
        StageManager.Instance.ChangeToTitleScene();

        TransitionManager.Instance().Transition("TitleScene",tSetting,0f);
    }


}

