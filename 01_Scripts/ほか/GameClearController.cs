using DG.Tweening;
using TigerForge;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class GameClearController : MonoBehaviour
{
    public Volume globalVolume;
    public GameObject GameClearGroup;
    public GameObject ConfettiEffectObject;

    public float resultDuration = 2.5f;
    public Vector3 targetRotate;
    public Vector3 targetOffset;
    public float defaultTextScale = 0.5f;
    public float textScaleTime = 1.0f;

    private GameObject mainCamera;
    private GameObject player;
    private Vignette vignette;

    public GameObject renderCamera;

    private void OnEnable()
    {
        EventManager.StartListening("isGameClear", OnGameClear);
    }

    private void OnDisable()
    {
        EventManager.StopListening("isGameClear", OnGameClear);
    }

    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        player = GameObject.FindGameObjectWithTag("Player");

        if (mainCamera == null || player == null)
        {
            Debug.Log("Not Find 'MainCamera' or 'Player' Object");
            return;
        }

        if (globalVolume != null)
        {
            globalVolume.profile.TryGet<Vignette>(out vignette);
        }

        if(GameClearGroup != null)
        {
            GameClearGroup.SetActive(false);
        }

        if(ConfettiEffectObject != null)
        {
            ConfettiEffectObject.SetActive(false);
        }
    }

    private void OnGameClear()
    {

        renderCamera.SetActive(true);

        // グローバルライトの影響を無くす
        if (vignette != null)
        {
            vignette.active = false;
        }

        if(GameClearGroup != null)
        {
            var trans = GameClearGroup.transform;
            trans.localScale = new Vector3(defaultTextScale, defaultTextScale);
            trans.DOScale(Vector3.one, textScaleTime);

            GameClearGroup.gameObject.SetActive(true);
        }

        StartCoroutine(CameraMoveRoutine());
    }

    System.Collections.IEnumerator CameraMoveRoutine()
    {
        TransitionCamera();

        yield return new WaitForSeconds(resultDuration);

        if (ConfettiEffectObject != null)
        {
            ConfettiEffectObject.SetActive(true);
        }

        // リザルトUIを表示させる
        ResultMenuController.Instance.ShowResultMenu();
    }

    private void TransitionCamera()
    {
        if (mainCamera == null || player == null) { return; }

        CameraController camController = mainCamera.GetComponentInParent<CameraController>();
        if (camController == null)
        {
            Debug.Log("Not Find 'CameraContorller' Component");
            return;
        }

        // カメラのプレイヤーへの追従を無効化する
        camController.enabled = false;

        CameraViewManager.Instance.enabled = false;

        // プレイヤーからの位置を求める
        Vector3 playerPos = player.transform.position;
        Vector3 targetPos = playerPos + targetOffset;

        // カメラの移動と回転を行う
        camController.gameObject.transform.DOMove(targetPos, resultDuration);
        camController.gameObject.transform.DORotate(targetRotate, resultDuration);
    }
}
