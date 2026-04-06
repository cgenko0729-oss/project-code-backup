using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TigerForge;
using TMPro;
using UnityEngine.InputSystem;

public class UIManager : Singleton<UIManager>
{

 public   RectTransform rect1, rect2,rect3;
 public   CanvasGroup CG;
 public   GameObject PauseMenu;

    public TextMeshProUGUI playerDamageText;
    public TextMeshProUGUI playerDefenceText;
    public TextMeshProUGUI playerLuckText;
    public TextMeshProUGUI playerCritChanceText;
    public TextMeshProUGUI playerMoveSpeedText;

    public TextMeshProUGUI turnCoinText;

    public bool isStatusMenuOpen = false;

    public GameObject scrollViewWindow;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rect1.DOAnchorPos(new Vector2(1000, 350), 1f);
        rect2.DOAnchorPos(new Vector2(1000, 200), 2f);
        rect3.DOAnchorPos(new Vector2(1000, 50), 3f).OnComplete((() =>
        {
            CG.DOFade(0.0f, 3f);
        }));

        if (PauseMenu != null)
        {
            PauseMenu.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Tabキーでポーズメニューを開く
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            ChangeActivePauseMenu();
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.startButton.wasPressedThisFrame)
            {
                ChangeActivePauseMenu();
            }
        }

        //if(SkillManager.Instance.waitingForPlayer == true)
        //{
        //    PauseMenu.gameObject.SetActive(false);
        //}
        
        //playerMoveSpeedText.text = "Move Speed: " + (int)BuffManager.Instance.gobalMoveSpeed;
        //playerDamageText.text = "Damage: " + (int)BuffManager.Instance.gobalDamageAdd;
        //playerDefenceText.text = "Defence: " + (int)BuffManager.Instance.gobalPlayerDefenceAdd;
        //playerLuckText.text = "Luck: " + (int)SkillManager.Instance.luck;
        //playerCritChanceText.text = "Crit Chance: " + (int)BuffManager.Instance.gobalCritChanceAdd + "%";

        //playerMoveSpeedText.text = L.UI("PlayerMoveSpeed") + ": " + ((int)BuffManager.Instance.gobalMoveSpeed + 100)+ "%";
        //playerDamageText.text = L.UI("PlayerDamage") + ": " + ((int)BuffManager.Instance.gobalDamageAdd + 100)+ "%";
        //playerDefenceText.text = L.UI("PlayerDefence") + ": " + ((int)BuffManager.Instance.gobalPlayerDefenceAdd + 100)+ "%";
        //playerLuckText.text = L.UI("PlayerLuck") + ": " + (int)SkillManager.Instance.luck+ "%";
        //playerCritChanceText.text = L.UI("PlayerCrit") + ": " + (int)BuffManager.Instance.gobalCritChanceAdd + "%";

        //playerMoveSpeedText.text =": " + ((int)BuffManager.Instance.gobalMoveSpeed + 0)+ "%";
        //playerDamageText.text = ": " + ((int)BuffManager.Instance.gobalDamageAdd + 100)+ "%";
        //playerDefenceText.text = ": " + ((int)BuffManager.Instance.gobalPlayerDefenceAdd + 0)+ "%";
        //playerLuckText.text =": " + (int)SkillManager.Instance.luck+ "%";
        //playerCritChanceText.text = ": " + (int)BuffManager.Instance.gobalCritChanceAdd + "%";

        playerMoveSpeedText.text  = $": {ColoredSignedNumber((int)BuffManager.Instance.gobalMoveSpeed + 0)}%";
        playerDamageText.text     = $": {ColoredSignedNumber((int)BuffManager.Instance.gobalDamageAdd + 0)}%";
        playerDefenceText.text    = $": {ColoredSignedNumber((int)BuffManager.Instance.gobalPlayerDefenceAdd + 0)}%";
        playerLuckText.text       = $": {ColoredSignedNumber((int)SkillManager.Instance.luck+ (int)BuffManager.Instance.gobalLuckAdd)}%";
        playerCritChanceText.text = $": {ColoredSignedNumber((int)BuffManager.Instance.gobalCritChanceAdd)}%";

    }

    private string ColoredSignedNumber(int value,
    string positiveHex = "#35D07F",
    string negativeHex = "#FF4B4B")
{
    // build the number text with sign
    string numStr = value > 0 ? $"+{value}" : value.ToString();

    // color ONLY the number text
    if (value > 0) return $"<color={positiveHex}>{numStr}</color>";
    if (value < 0) return $"<color={negativeHex}>{numStr}</color>";
    return numStr; // 0 = no color
}

    //public void OnOffScrollViewSkillWindow()
    //{
    //    scrollViewWindow.SetActive(false ); 
    //    scrollViewWindow.SetActive(true);


    //}

    // ポーズメニューの表示・非表示の切り替え処理
    public void ChangeActivePauseMenu()
    {      
        // ゲームクリアとゲームオーバーの状態、スキル強化画面が出ている状態
        // であればポーズUIは開けないようにする
        if (GameManager.Instance.stateMachine.State == GameState.GameClear ||
            GameManager.Instance.stateMachine.State == GameState.GameOver 
            )
        {
            return;
        }

        //Update UI text
        turnCoinText.text = ResultMenuController.Instance.turnCoinGet.ToString();




        // レベルアップメニューが表示されている場合は入力を無視する
        //if (SkillManager.Instance.waitingForPlayer == false)
        {
            // 現在のポーズメニューのアクティブフラグを反転して取得する
            bool activeFlg = !PauseMenu.activeSelf;

            if (activeFlg)
            {
                OptionMenuManager.Instance.CloseOptionMenu();

                CameraViewManager.Instance.ShowAndUnlockCursor();

                // trueの場合はゲームを一時停止
                if(!SkillManager.Instance.waitingForPlayer && !SkillManager.Instance.isGetNewTrait)GameManager.Instance.PauseGame();
                EventManager.EmitEvent(GameEvent.isPauseMenu);

                isStatusMenuOpen = true;
                EventManager.EmitEvent("OpenStatusMenu");

                

            }
            else
            {

                if(CameraViewManager.Instance.currentMode == CameraViewManager.CameraMode.CloseView && !SkillManager.Instance.isLevelUpWindowOpen)
                {
                    CameraViewManager.Instance.HideAndLockCursor();
                }

                // falseの場合はゲームを再開
                if(!SkillManager.Instance.waitingForPlayer && !SkillManager.Instance.isGetNewTrait)GameManager.Instance.UnPauseGame();
                EventManager.EmitEvent(GameEvent.isPauseMenu);

                isStatusMenuOpen = false;

            }

            if(activeFlg)PauseMenu.SetActive(activeFlg);
            MenuOpenAnimator menuAni = PauseMenu.GetComponent<MenuOpenAnimator>();
            menuAni.PlayeMenuAni(activeFlg);
        }
    }
}
