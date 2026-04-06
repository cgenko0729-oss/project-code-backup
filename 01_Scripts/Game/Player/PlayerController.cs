using TigerForge;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ─────────── 調整可能なパラメータ ──────────────────────
    [Header("衝突判定対象のレイヤー(複数選択可能)")]
    [SerializeField] LayerMask obstacleMask;        // 衝突判定対象のレイヤー
    [Header("障害物から離れるための余裕距離")]
    [SerializeField] float skinWidth = 0.02f;       // 障害物から離れるための余裕距離
    // ────────────────────────────────────────────

    // ─────────── キャッシュ用コンポーネント ───────────────────
    CapsuleCollider caps;       // プレイヤーのカプセルコライダー
    PlayerState ps;             // 移動速度やダッシュ距離などを保持するステート
    InputAction moveAct;        // Input System の "Move" アクション
    // ────────────────────────────────────────────

    // ─────────── ランタイムステート ───────────────────────
    Vector3 moveVec;                        // 入力による移動方向ベクトル (XZ 平面)
    [Header("マウスの方向を正面にする")]
    public bool useMouseRotation = true;    // マウス回転モードのフラグ
    public bool isDashing;                  // ダッシュ中フラグ
    float oneDashDistance;                  // ダッシュ時の移動速度 (距離/時間)
    // ────────────────────────────────────────────

    // ─────────── 外部参照用プロパティ ──────────────────────
    public Vector3 MoveVec => moveVec;      // 外部から移動入力ベクトルを参照可能
    public bool EnableDash => !isDashing && currentDashCharges > 0;   // 外部からダッシュ可否を参照可能
    public int CurrentDashCharges => currentDashCharges; 
    // ────────────────────────────────────────────

    [Header("デバックプロパティ")]
    public bool debugMoveFlg = false;
    public float debugMoveSpeed=10.0f;

    bool isBeingPushedBack;

    private Transform mainCameraTransform;

    [Header("Dash Stack Settings")]
    public int currentDashCharges;
    public float dashRechargeTimer;
    [SerializeField] public int maxDashCharges = 1; 
    [SerializeField] public float dashRechargeTime = 2.0f;

    void Start()
    {
        mainCameraTransform = Camera.main.transform;

        // コンポーネントの取得
        caps = GetComponent<CapsuleCollider>();
        ps = GetComponent<PlayerState>();
        moveAct = InputSystem.actions.FindAction("Move");

        // ダッシュ距離 (ps.DashSpeed) とダッシュ時間 (ps.DashDuration) から速度を算出
        oneDashDistance = ps.DashSpeed / ps.DashDuration;

        // このプレイヤーがアクティブになったことを知らせる
        UpgradeEffectManager.Instance.Active(BuffType.ApplyBuffHealthScaling);
    }

    void Update()
    {
        // プレイヤーがゲームオーバーしている場合は処理を行わない
        if(GetComponent<PlayerState>().IsAliveFlg == false) { return; }
        if (isBeingPushedBack) return;

        // ゲームクリアしている場合は処理を行わない
        if(GameManager.Instance.stateMachine.State == GameState.GameClear){ return; }
       
        // 入力を読み取る
        ReadMoveInput();

        // デバックキー：回転モードの切り替え (O キー)
        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            useMouseRotation = !useMouseRotation;
            EventManager.EmitEvent("RotationModeChanged", useMouseRotation);
        }

        HandleDashRecharge();
            

        // デバックキー：回転モードの切り替え (I キー)
        if (Keyboard.current.iKey.wasPressedThisFrame && GameManager.Instance.isDebugMode)
            debugMoveFlg = !debugMoveFlg;

        // ダッシュ開始 (Space キー)
        if ((Keyboard.current.spaceKey.wasPressedThisFrame ||  (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame))
            && currentDashCharges > 0
            && !isDashing
            && moveVec.sqrMagnitude > 0.0001f
            && !SkillManager.Instance.waitingForPlayer
            //&& ps.DashCoolDownCnt <= 0
            )
        {
            if (EnemyManager.Instance.playerCannotDash || EnemyManager.Instance.playerCannotMove)
            {
                SoundEffect.Instance.Play(SoundList.ButtonCancelSe);
                return;
            }

            if (currentDashCharges == maxDashCharges)
            {
                dashRechargeTimer = dashRechargeTime;
            }
            
            currentDashCharges--;
            StartCoroutine(DashRoutine());
        }

        // 通常の回転と歩行 (ダッシュ中はスキップ)
        if (!isDashing)
        {
            if (EnemyManager.Instance.playerCannotMove) // 移動禁止デバフ
            {
                return;
            }

            if (!SkillEffectManager.Instance.isPlayerFixRot)
            {
                if (useMouseRotation)
                RotateByInputDevice();       // 入力デバイスからの入力方向に向く
                else
                RotateByMovement();    // 移動方向に向く
            }

           

            float moveSpeed = 0;
            if (!debugMoveFlg)
            {
                moveSpeed = ps.MoveSpeed * ItemManager.Instance.spdUpAmount;
            }
            else
            {
                moveSpeed = debugMoveSpeed;
            }
            Walk(moveVec, moveSpeed * (1 + BuffManager.Instance.gobalMoveSpeed/100)); // 歩行移動
            
            // 移動スピードに基づいてバフ獲得
            //if()
        }
    }

    /// ダッシュ用コルーチン。
    /// FixedUpdate 相当で自前の衝突処理を共有して移動。
    System.Collections.IEnumerator DashRoutine()
    {
        isDashing = true;
        EventManager.EmitEvent(GameEvent.PlayerDash); // ダッシュアニメなどのトリガー
        //transform.localScale = new Vector3(0.1f, 0.1f, 0.1f); // ダッシュ中はスケールを小さくする

        // 水平面上のダッシュ方向ベクトルを作成
        Vector3 dir = new Vector3(moveVec.x, 0f, moveVec.z).normalized;
        float remaining = ps.DashSpeed;  // 残り移動距離 (ps.DashSpeed を利用)

        // ダッシュ方向に即座に振り向く
        transform.rotation = Quaternion.LookRotation(dir);

        // 残距離がなくなるまで FixedUpdate ごとに移動
        while (remaining > 0f)
        {
            float step = oneDashDistance * Time.deltaTime; // 1 フレーム分の移動距離
            if (step > remaining) step = remaining;

            MoveWithCollision(dir * step); // 衝突付き移動
            remaining -= step;

            // 壁に完全に阻まれた場合は脱出
            if (step < 0.0001f) break;

            yield return null;
        }

        EventManager.EmitEvent(GameEvent.PlayerDashEnd); 
        //transform.localScale = Vector3.one; // ダッシュ終了時に元のスケールに戻す

        isDashing = false;
    }

    private void HandleDashRecharge()
    {
        // If we have less than the maximum number of charges
        if (currentDashCharges < maxDashCharges)
        {
            // If the timer is running, count it down.
            if (dashRechargeTimer > 0)
            {
                if(!isDashing)dashRechargeTimer -= Time.deltaTime;
            }
            // If the timer has finished
            else
            {
                currentDashCharges++; // Add a charge back.
                
                // If we still have more charges to replenish, reset the timer for the next one.
                if (currentDashCharges < maxDashCharges)
                {
                    dashRechargeTimer = dashRechargeTime;
                }
            }
        }
    }

    /// 通常歩行処理。
    // MoveWithCollision と同じ衝突ロジックを使用
    void Walk(Vector3 dir, float speed)
    {
        if (dir.sqrMagnitude < 0.0001f) return;
        MoveWithCollision(dir.normalized * speed * Time.deltaTime);
    }

    void MoveWithCollision(Vector3 delta)
    {
        // 現在の移動量を remainingDelta として初期化
        Vector3 remainingDelta = delta;
        // 衝突解決のループ回数を制限し、無限ループを防ぐ
        int iterations = 3; 

        for (int i = 0; i < iterations && remainingDelta.magnitude > Mathf.Epsilon; i++)
        {
            float dist = remainingDelta.magnitude;
            Vector3 dir = remainingDelta / dist;
            GetCapsuleWorld(out Vector3 p1, out Vector3 p2, out float radius);

            // カプセルキャストで障害物検知
            if (Physics.CapsuleCast(p1, p2, radius, dir, out RaycastHit hit, dist + skinWidth, obstacleMask))
            {
                // 1. 当たり直前まで移動
                float moveDist = Mathf.Max(hit.distance - skinWidth, 0f);
                transform.position += dir * moveDist;

                // 2. 残りの移動量を計算し、法線に沿ってスライドさせるベクトルを次の移動量とする
                float remainingDist = dist - moveDist;
                Vector3 slide = Vector3.ProjectOnPlane(dir, hit.normal) * remainingDist;
                
                // Y軸の移動を制限（必要に応じて）
                slide.y = 0;

                // 次のループでこのスライドベクトルを使って移動と衝突判定を行う
                remainingDelta = slide;
            }
            else
            {
                // 衝突がなければ、残りの距離をすべて移動してループを終了
                transform.position += remainingDelta;
                break; // ループを抜ける
            }
        }
    }

    public void ApplyPushback(Vector3 direction, float distance, float duration)
    {
        // Ensure we don't start a new pushback while another is in progress.
        if (!isBeingPushedBack)
        {
            StartCoroutine(PushbackRoutine(direction, distance, duration));
        }
    }

    private System.Collections.IEnumerator PushbackRoutine(Vector3 direction, float distance, float duration)
    {
        isBeingPushedBack = true;
    
        float elapsedTime = 0f;
        float pushSpeed = distance / duration;
    
        while (elapsedTime < duration)
        {
            // Calculate the movement for this frame
            Vector3 movementStep = direction * pushSpeed * Time.deltaTime;
    
            // Use the existing collision-aware movement function
            MoveWithCollision(movementStep);
    
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
    
        isBeingPushedBack = false;
    }

    /// カプセルのワールド空間での上端・下端位置と半径を計算。
    void GetCapsuleWorld(out Vector3 p1, out Vector3 p2, out float radius)
    {
        radius = caps.radius * 1;            // 半径をスケール考慮で計算
        float hh = Mathf.Max(caps.height * 0.5f - radius, 0f);  // 中心から頂点までの距離
        Vector3 up = Vector3.up * hh;
        Vector3 centerWorld = transform.position + transform.rotation * caps.center;
        p1 = centerWorld + up;   // 上端
        p2 = centerWorld - up;   // 下端
    }

    /// 入力方向に向けて回転。
    void RotateByInputDevice()
    {
        if(!InputDeviceManager.Instance) return;

        // ゲームパッドでの方向入力
        // Check for controller input first
        var inputDevice = InputDeviceManager.Instance.GetLastUsedDevice();
        if (inputDevice is Gamepad)
        {
            if(Gamepad.current == null) return;
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();

            // Check if the right stick is being used (and has a significant magnitude)
            if (rightStick.sqrMagnitude > 0.1f)
            {
                // Convert the 2D stick input to a 3D direction relative to the camera
                Vector3 camForward = Camera.main.transform.forward;
                Vector3 camRight = Camera.main.transform.right;

                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                // Create the look direction from the stick input and camera orientation
                Vector3 lookDirection = (camRight * rightStick.x + camForward * rightStick.y).normalized;

                if (lookDirection.sqrMagnitude > 0.0001f)
                {
                    float rotSpd = ps.RotationSpeed;
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(lookDirection),
                        rotSpd * Time.deltaTime
                    );
                }
                return; // Exit the method after handling controller rotation
            }

            return;
        }

        // マウスで方向を決める
        // ---- TPViewでのマウス方向 ----
        if (inputDevice is Keyboard)
        {
            if (CameraViewManager.Instance.currentMode != CameraViewManager.CameraMode.CloseView)
            {
                var plane = new Plane(Vector3.up, transform.position);
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

                if (plane.Raycast(ray, out float enter))
                {
                    Vector3 hit = ray.GetPoint(enter);
                    Vector3 look = hit - transform.position; look.y = 0f;
                    if (look.sqrMagnitude > 0.0001f)
                    {
                        float rotSpd = ps.RotationSpeed;
                        transform.rotation = Quaternion.Slerp(
                            transform.rotation,
                            Quaternion.LookRotation(look),
                            rotSpd * Time.deltaTime
                        );
                    }
                }
            }
            else
            // ---- TopViewでのマウス方向 ----
            {
                // Get the camera’s forward vector
                Vector3 camForward = Camera.main.transform.forward;

                // Ignore vertical tilt so player doesn’t look up/down
                camForward.y = 0f;
                camForward.Normalize();

                // Only rotate if camera forward is valid
                if (camForward.sqrMagnitude > 0.0001f)
                {
                    float rotSpd = ps.RotationSpeed;
                    Quaternion targetRot = Quaternion.LookRotation(camForward);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSpd * Time.deltaTime);
                }
            }
        }
    }

    /// 移動方向ベクトルに合わせて回転。
    void RotateByMovement()
    {
        if (moveVec.sqrMagnitude < 0.0001f) return;
        float rotSpd = ps.RotationSpeed;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(moveVec),
            rotSpd * Time.deltaTime
        );
    }

    /// 入力システムから入力ベクトルを取得し正規化する。
    void ReadMoveInput()
    {
        Vector2 inputVector = moveAct.ReadValue<Vector2>();
        Vector3 cameraForward = mainCameraTransform.forward;
        Vector3 cameraRight = mainCameraTransform.right;

        // 3. Project the camera vectors onto the horizontal plane (XZ) by zeroing out the Y component
        cameraForward.y = 0;
        cameraRight.y = 0;

        // 4. Normalize the vectors to ensure consistent speed regardless of camera angle
        cameraForward.Normalize();
        cameraRight.Normalize();

        //if (EnemyManager.Instance.playerCannotMove)
        //{
        //    moveVec = Vector3.zero;
        //    return;
        //}

        // 5. Combine the input and camera directions to get the final world-space movement vector
          moveVec = (cameraForward * inputVector.y + cameraRight * inputVector.x).normalized;
    }


    public void OnTriggerEnter(Collider col)
    {
        //インターフェースのコンポーネントを取得
        IHittable hit = col.GetComponent<IHittable>();

        //当たった時の関数を呼び出す
        if (hit != null)
        {
            hit.OnHit();
        }
    }
}
