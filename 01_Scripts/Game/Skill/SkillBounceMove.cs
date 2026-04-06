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

public class SkillBounceMove : MonoBehaviour
{
    public Vector3 moveVec;
    public float margin = 0.05f;
    public float lookAtTime = 5.0f;

    //進化しているか
    public bool isFinalSkill = false;

    private bool reflect = true;
    private Camera mainCamera;

    public Transform playerTrans;
    private Vector2 bounceAreaSize = new Vector2(14f, 14f);

    public void OnEnable()
    {
        EventManager.StartListening(GameEvent.CutSceneStart, NotReflect);
        EventManager.StartListening(GameEvent.StartBossFight, DoReflect);
    }

    public void OnDisable()
    {
        EventManager.StopListening(GameEvent.CutSceneStart, NotReflect);
        EventManager.StopListening(GameEvent.StartBossFight, DoReflect);
    }

    public void NotReflect()
    {
        reflect = false;
    }

    public void DoReflect()
    {
        reflect = true;
    }

    void Start()
    {
        mainCamera = Camera.main;
         playerTrans = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        if(CameraViewManager.Instance.currentMode == CameraViewManager.CameraMode.TacticView)
        {
            CameraReflection();
        }
        else
        {
            FixBoxReflection();
        }
        


        

    }

    void CameraReflection()
    {
        if (moveVec != Vector3.zero)
        {
            transform.position += moveVec * Time.deltaTime;
        }

        if (!reflect || mainCamera == null) return;

        if (reflect)
        {

            // カメラの forward 方向に投影して距離を求める（X軸に60度回転でもOK）
            float zDist = Vector3.Dot(
                transform.position - mainCamera.transform.position,
                mainCamera.transform.forward
            );

            // ビューポート座標を使って画面端をワールド座標に変換（マージンあり）
            Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0.0f + margin, 0.0f + margin, zDist));
            Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1.0f - margin, 1.0f - margin, zDist));

            if (!IsFinite(min) || !IsFinite(max)) return;

            Vector3 pos = transform.position;

            // X方向の反射
            if (pos.x < min.x || pos.x > max.x)
            {
                moveVec.x *= -1;
                transform.position = new Vector3(Mathf.Clamp(pos.x, min.x, max.x), pos.y, pos.z);

                transform.DOKill();
                //transform.rotation = Quaternion.LookRotation(moveVec.normalized, Vector3.up);
                transform.DORotateQuaternion(Quaternion.LookRotation(moveVec.normalized, Vector3.up), lookAtTime).SetEase(Ease.OutQuad);
            }

            // Z方向の反射（見下ろし型ゲーム）
            if (pos.z < min.z || pos.z > max.z)
            {
                moveVec.z *= -1;
                transform.position = new Vector3(pos.x, pos.y, Mathf.Clamp(pos.z, min.z, max.z));

                transform.DOKill();
                //transform.rotation = Quaternion.LookRotation(moveVec.normalized, Vector3.up);
                transform.DORotateQuaternion(Quaternion.LookRotation(moveVec.normalized, Vector3.up), lookAtTime).SetEase(Ease.OutQuad);
            }
        }
    }

    void FixBoxReflection()
    {
        // --- 1. Basic Movement ---
        if (moveVec != Vector3.zero)
        {
            transform.position += moveVec * Time.deltaTime;
        }

        // --- 2. Conditions to Skip Bouncing ---
        // Stop if reflect is false or if the player transform hasn't been assigned.
        if (!reflect || playerTrans == null) return;


        // --- 3. Bounding Box Logic (The Core Change) ---
        // Calculate the boundaries of the box based on the player's current position.
        Vector3 playerPos = playerTrans.position;
        float minX = playerPos.x - bounceAreaSize.x / 2.0f;
        float maxX = playerPos.x + bounceAreaSize.x / 2.0f;
        float minZ = playerPos.z - bounceAreaSize.y / 2.0f;
        float maxZ = playerPos.z + bounceAreaSize.y / 2.0f;

        Vector3 pos = transform.position;

        // Check and reflect on the X-axis (width of the box)
        if (pos.x < minX || pos.x > maxX)
        {
            moveVec.x *= -1; // Reverse X direction
            // Clamp the position to be inside the box, preventing it from getting stuck outside.
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            transform.position = pos;

            // Smoothly rotate to face the new direction
            transform.DOKill();
            transform.DORotateQuaternion(Quaternion.LookRotation(moveVec.normalized, Vector3.up), lookAtTime).SetEase(Ease.OutQuad);
        }

        // Check and reflect on the Z-axis (depth of the box)
        if (pos.z < minZ || pos.z > maxZ)
        {
            moveVec.z *= -1; // Reverse Z direction
             // Clamp the position to be inside the box.
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
            transform.position = pos;

            // Smoothly rotate to face the new direction
            transform.DOKill();
            transform.DORotateQuaternion(Quaternion.LookRotation(moveVec.normalized, Vector3.up), lookAtTime).SetEase(Ease.OutQuad);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        //進化スキルならこの処理はしない
        if (isFinalSkill) { return; }

        //敵・破壊可能オブジェクトのタグをチェック
        if (col.CompareTag("Enemy"))
        {
            Reflect(col);
        }
        if (col.CompareTag("DestroyObj"))
        {
            Reflect(col);
        }
    }

    //敵や破壊可能オブジェクトに触れても反射
    public void Reflect(Collider col)
    {
        Vector3 normal = (transform.position - col.ClosestPoint(transform.position)).normalized;
        normal.y = 0f;

        Vector3 dir = Vector3.Reflect(moveVec, normal);
        dir.y = 0f;

        // スピード維持してセット
        moveVec = dir.normalized * moveVec.magnitude;

        transform.DOKill();
        //向きを進行方向に合わせる
        //transform.rotation = Quaternion.LookRotation(moveVec.normalized, Vector3.up);
        transform.DORotateQuaternion(Quaternion.LookRotation(moveVec.normalized, Vector3.up), lookAtTime).SetEase(Ease.OutQuad);
    }

    static bool IsFinite(Vector3 v)
    {
        return float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);
    }

}

