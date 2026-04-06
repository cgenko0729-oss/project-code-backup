using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool






public class CameraDebugController : MonoBehaviour
{
   //[Header("Toggle Debug Mode")]
   //[Tooltip("Press this key to turn camera debug on/off at runtime")]
    [SerializeField] private KeyCode toggleKey = KeyCode.C;

    [Header("Movement Speeds")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float verticalSpeed = 10f;
    [SerializeField] private float scrollSpeed = 100f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 0.2f;

    [Header(@"
    使用方法：
    Cキー：デバッグモードのオン／オフ切替
    上下左右矢キー ：前後左右移動
    Q/E：上下移動
    マウスホイール：ズームイン・アウト
    マウス中ボタン＋ドラッグ：カメラ回転
    ")]    public bool isCameraDebugMode = false;
    private Vector3 lastMousePosition;

    void Update()
    {
        // 1) toggle debug mode
        //if (Input.GetKeyDown(toggleKey))
        //{
        //    isCameraDebugMode = true;         
        //    Cursor.lockState = isCameraDebugMode
        //        ? CursorLockMode.Locked
        //        : CursorLockMode.None;
        //}

        if (!isCameraDebugMode) return;

        // 2) WASD + Q/E movement
        float x = Input.GetAxis("Horizontal");    // A/D
        float z = Input.GetAxis("Vertical");      // W/S
        
        //Vector3 forward = transform.forward;
        //Vector3 right   = transform.right;
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right   = Vector3.ProjectOnPlane(transform.right,   Vector3.up).normalized;
        Vector3 planarMove = (forward * z + right * x) * moveSpeed * Time.unscaledDeltaTime;

        
        Vector3 upDown  = Vector3.zero;

        if (Input.GetKey(KeyCode.Q)) upDown += Vector3.up;
        if (Input.GetKey(KeyCode.E)) upDown += Vector3.down;
         upDown *= verticalSpeed * Time.unscaledDeltaTime;
        transform.position += planarMove + upDown;

        //Vector3 move = (forward * z + right * x).normalized + upDown.normalized;
        //transform.position += move * moveSpeed * Time.deltaTime;
         //Vector3 move       = planarMove + upDown;

        // 3) scroll wheel zoom (move along forward)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.position += transform.forward * scroll * scrollSpeed * Time.unscaledDeltaTime;

        

        // 4) middle-mouse drag to rotate
        if (Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(2) || Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float yaw   = delta.x * rotationSpeed;
            float pitch = -delta.y * rotationSpeed;
            transform.Rotate(Vector3.up,      yaw,   Space.World);
            transform.Rotate(transform.right, pitch, Space.World);
            lastMousePosition = Input.mousePosition;
        }
    }
}

