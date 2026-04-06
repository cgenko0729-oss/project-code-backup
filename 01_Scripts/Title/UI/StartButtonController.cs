using EasyTransition;
using TigerForge;
using UnityEngine;

public class StartButtonController : MonoBehaviour
{
    [Header("フェードアウトの演出")]
    public TransitionSettings transSettings;
    [Header("開始までの時間(秒)")]
    public float fadeStartTime = 0;
    private bool onClickFlg = false;

    public void OnClick()
    {
        if(onClickFlg == true) { return; }
        onClickFlg = true;

        EventManager.EmitEvent(GameEvent.pushTitlebtn);

        // ゲームシーンに遷移する
        TransitionManager.Instance().
            Transition("GameScene", transSettings, fadeStartTime);
    }
}
