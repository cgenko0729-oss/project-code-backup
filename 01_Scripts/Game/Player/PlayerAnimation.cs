using DG.Tweening;
using TigerForge;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour
{
    public enum AnimeState
    {
        Idle,
        Run,
        Dash
    }

    private Animator animator;          // アニメーター
    private AnimeState nowAnime = 0;    // 現在のアニメーション
    private AnimeState prevAnime = 0;   // 1つ前のアニメーション
    private InputAction moveAction;     // 移動の入力

    private void OnEnable()
    {
        EventManager.StartListening("PlayerAttack", SetAttackTrigger);
        EventManager.StartListening("PlayerDash", SetDashTrigger);
        EventManager.StartListening("isGameOver", SetDieTrigger);
        EventManager.StartListening("isGameClear", SetGameClearTrigger);
    }

    private void OnDisable()
    {
        EventManager.StopListening("PlayerAttack", SetAttackTrigger);
        EventManager.StopListening("PlayerDash", SetDashTrigger);
        EventManager.StopListening("isGameOver", SetDieTrigger);
        EventManager.StopListening("isGameClear", SetGameClearTrigger);
    }

    private void SetAttackTrigger()
    {
        if(GetComponent<PlayerState>().IsAliveFlg == false) { return; }

        animator = this.GetComponent<Animator>();
        animator.SetTrigger("Attack");
    }

    private void SetDashTrigger()
    {
        animator = this.GetComponent<Animator>();
        animator.SetTrigger("Dash");
    }

    private void SetDieTrigger()
    {
        animator = this.GetComponent<Animator>();
        animator.SetTrigger("Die");
        animator.SetLayerWeight(1, 0);
    }

    private void SetGameClearTrigger()
    {
        gameObject.transform.DORotateQuaternion(new Quaternion(0, 180, 0, 0), 1.0f);

        animator = this.GetComponent<Animator>();
        animator.SetTrigger("GameClear");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // アニメーションの初期化
        animator = GetComponent<Animator>();
        animator.Play("Idle_Battle");

        moveAction = InputSystem.actions.FindAction("Move");
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance.stateMachine.State == GameState.GameClear) { return; }

        Vector2 rValue = moveAction.ReadValue<Vector2>();

        // 移動入力があるかどうかを調べる
        if(rValue.magnitude > 0 )
        {
            nowAnime = AnimeState.Run;
        }
        else
        {
            nowAnime = AnimeState.Idle;
        }

        // アニメーションの変更がある場合は更新を行う
        if(nowAnime != prevAnime)
        {
            // 再生アニメーションの更新
            animator.SetInteger("State", (int)nowAnime);

            // １つ前にアニメーションの更新
            prevAnime = nowAnime;

            int runSpeedMultiplierHash = Animator.StringToHash("walkAniSpeedUp");
            animator.SetFloat(runSpeedMultiplierHash, 1 + (BuffManager.Instance.gobalMoveSpeed/100) );
        }
 
    }
}
