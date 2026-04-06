using UnityEngine;
using UnityEngine.UI;   // UI関係のクラスを使用するために必要
using DG.Tweening;      // DOTween関係のクラスを使用するために必要

//DOTween 公式サイト: https://dotween.demigiant.com/documentation.php

public class AssetPluginNote : MonoBehaviour
{

    public RectTransform uiTransform;
    public Image image;
    public Sprite sprite;

    Tween action1;    //アニメーションを実行するためのTween
    Sequence action2; //複数のアニメーションをまとめて実行するためのシーケンス

    void Start()
    {
        //uiTransform.DOMove(new Vector3(0, 0, 0), 3f); //オブジェクトを指定した座標に移動させる
        //uiTransform.DORotate(new Vector3(0, 0, 0), 3f); //オブジェクトを指定した角度に回転させる
        //uiTransform.DOScale(new Vector3(1, 1, 1), 3f); //オブジェクトのスケールを指定した値に変更する
        //uiTransform.DOAnchorPos(new Vector3(0, 0, 0), 3f); //オブジェクトを指定した座標に移動させる


        //image.DOColor(new Color(1, 1, 1, 1), 3f); //オブジェクトの色を指定した値に変更する
        //image.DOFade(1f, 3f); //オブジェクトの透明度を指定した値に変更する

        //action1 = uiTransform.DOMove(new Vector3(0, 0, 0), 3f); //オブジェクトを指定した座標に移動させる

        //追加オプション 
        //OnComplete, OnStart, SetEase,  
    }

    void Update()
    {
        action1.Kill(); //アニメーションを停止する

        if(Input.GetKeyDown(KeyCode.T)) //スペースキーが押されたら
        {
            uiTransform.DOAnchorPos(new Vector3(0, 0, 0), 3f); 
            uiTransform.DOScale(new Vector3(7, 7, 7), 3f);
            
            //image.DOColor(new Color(1, 0, 0, 1), 3f); //オブジェクトの色を指定した値に変更する
           action1 = image.DOFade(0.1f, 3f).
                OnComplete(() => { Action1();} ).
                SetEase(Ease.OutBounce); 

            ;

            action1.Play();

            
            
        }

        //if(Input.GetKeyDown(KeyCode.K)) action1.Kill();

    }

    void Action1()
    {
        image.DOFade(1f, 3f); 
        uiTransform.DOMove(new Vector3(-200, 0, 0), 3f); //オブジェクトを指定した座標に移動させる

    }


}


//===========================================オブジェクトプール=======================================================//
//Object Pool: MOP2(Master Object Pooler)公式ドキュメント: https://www.qfsw.co.uk/docs/MOP2/api/QFSW.MOP2.AutoPool.html

//新しいプールを作成): Create -> Master Object Pooler 2 -> Object Pool
//指定のプール内オブジェクトを取得する:
/*
 
 ObjectPool bulletPool; //スクリプトのheaderでプールを定義し、inspectorであのプールをfolderからドラグ&ドロップでスクリプトに配る
 bulletPool.GetObject(new Vector3(10,0,0));　//rotationも指定できる, Instantiate(bulletObj,new Vector3(10,0,0)); と同じ意味
 bulletPool.Release(this.gameObject); //プールに戻す , Destroy(this.GameObject)と同じ意味

 */

//===========================================イベントマネージャー=======================================================//
//EventManager: TigerForge Easy Event Manager 公式ドキュメント: https://tigerforge.altervista.org/easyeventmanager/

//Observerパターンの使い方：
//イベントを発動する主人公クラス
//EventManager.EmitEvent("イベント名前");
//EventManager.EmitEventData("イベント名前", data); //データを渡す場合

//イベントを参加したいクラス
//EventManager.StartListening("イベント名前",発動したい関数); //(Enable / Start / Awake 内で)
//EventManager.StopListening("イベント名前", 発動したい関数); //(Disable / OnDestroy 内で)

//public void 発動したい関数()
//{
//受け取りたい引数とデータ
//float amount = EventManager.GetFloat("ChangePlayerHp");

//}


//===========================================シーン遷移マネージャー=======================================================//
//Easy Transition Manager: 

/*
TransionSettings tSetting;
float transitionDelayTime = 0.5f;
TransitionManager.Instance.Transition("シーン名", tSetting, transitionDelayTime); //シーン遷移
 TransitionManager.Instance.Transition(tSetting, transitionDelayTime);　　　//シーン遷移なしでトランジションエフェクトのみ流す
 */


//===========================================ステートパターン =======================================================//
//MonsterLove StateMachine (thefuntastic/Unity3d-Finite-State-Machine): https://github.com/thefuntastic/Unity3d-Finite-State-Machine
/*StateMachine使用*/
/*
1.初期化: 
State enum を定義 :
  public enum DragonStates 
  {
      Idle,
      Chase,
      Attack,
      Dead
  }
2. StateMachineの定義と初期化:
  StateMachine<DragonStates> fsm;
  stateMachine = StateMachine<BossState>.Initialize(this);
  stateMachine.ChangeState(BossState.Move);

3.Staet名前 + _ + State状態(Enter/Exit/Update)で StateMachine使用する　,例えば　void Attacl_Enter() , Attack_Exit() , Attack_Update()

    void Idle_Enter()
    {
        Debug.Log("Entering Idle State");
    }

    void Idle_Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            fsm.ChangeState(DragonStates.Chase);　　//Stateの状態遷移
            Debug.Log("Transitioning from Idle State to Chase State");
        }

        stateCnt -= Time.deltaTime;
    }

    void Idle_Exit()
    {
        Debug.Log("Exiting Idle State");
    }
 
 */

//4.Stateの状態遷移 fsm.ChangeState(DragonStates.Chase);　　

//===========================================サウンドマネージャー(HellMade SoundManager)=======================================================//
//1.サンドエフェクト追加するときは まず  SoundListクラスのEnumを追加し、サウンドのIDを定義する
//2.次にSoundSoフォルダ内で右クリック->Create->SoundDataのScriptableObjectを作成し、サウンドのデータを設定する(Sound Clipを中にドラッグ&ドロップし、Enum(サウンドのIDを)を設定する)
//3.作成したSoundDataをHeirarchy内のSoundEffectLIstのSoundLibraryにドラッグ&ドロップで追加する
//4.ほかのクラスでサウンドエフェクトを流したいときは, SoundEffect.Instance.Play(SoundList.SpiderBossDashSe);　で流すことができる


//===========================================Mk Toon Shader=======================================================//
//3Dのオブジェクトに簡単にトゥーンシェーダーを応用できる シェーダーのアセットです
//Mk Toon Shader 公式サイト: https://assetstore.unity.com/packages/vfx/shaders/mk-toon-stylized-shader-178415

//===========================================HighLight Plus Outline=======================================================//
//Outline,Glow,See-Through,Selection Effectsなどのエフェクトを簡単に追加できる アセットです
//HighLight Plus Outline: https://assetstore.unity.com/packages/vfx/shaders/highlight-plus-2-all-in-one-outline-selection-effects-321005

//===========================================AllInOneSpriteShader=======================================================//
//スプライトと UI エフェクトをプロジェクトに最も簡単かつ素早く追加できる シェーダーのアセットです
//AllInOneSpriteShader: https://assetstore.unity.com/packages/vfx/shaders/all-in-1-sprite-shader-156513



