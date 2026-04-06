using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class VideoShotManager : Singleton<VideoShotManager>
{

    public bool isVideoShotMode = false;

    [Header("録画モード設定関連")]
    [Header("UI非表示")]
    public bool isNoUi = true;    
    [Header("BGM無効")]
    public bool isDisableBGM = true;
    [Header("時間経過カウンター停止")]
    public bool isFreezeTime = true;
    [Header("マウスカーソル非表示(Esc押すと見える)")]
    public bool isHideMouseCursor = true;

    [Header("自由カメラモード")]
    public bool isFreeCamera = true;

    [Header("カメラ固定モード")]
    public bool isFixCamera = true;
    [Header("カメラの固定位置設定")]
    public float fixCameraPosX = 0f;
    public float fixCameraPosY = 10f;
    public float fixCameraPosZ = -10f;

    public float fixCameraRotX = 30f;
    public float fixCameraRotY = 0f;
    public float fixCameraRotZ = 0f;

    public bool isCamCloseShot = false;

    

    [Header("ゲームスピード設定")]
    [Range(0f, 3f)]
    public float gameSpeed = 1f;

    public bool isNoEnemySpawner = false;

    public bool isKillAllEnemy = false;


    [Header("============================")]
    public GameObject uiCanvasObj;  
    public GameObject mainCamObj;
    public GameObject cameraHolderObj;
    private CameraController camController;
    private Transform playerTrans;
    private PlayerState playerState;
    public GameObject spawnerObj;

    
    public bool isCameraTranformInited = false;
    public float oldCameraPosX = 0f;
    public float oldCameraPosY = 10f;
    public float oldCameraPosZ = -10f;

    public float oldCameraRotX = 30f;
    public float oldCameraRotY = 0f;
    public float oldCameraRotZ = 0f;

    public float closeShotCamPosY = 2.7f;
    public float closeShotCamPosZ = -3.8f;
    public float closeShotCamRotX = 21.75f;

    public float playerOldExpNeed = 10f;
    public float playerExpNeedFix = 100000f;

    public float turnTimeOld = 20f;
    public float runTimeOld = 20f;
   
    public float turnTimeFreeze = 20f;
    public float runTimeFreeze = 20f;


    [ContextMenu("Change To ScreenShot Mode")]
    void ChangeToScreenShotModeContext()
    {
        if(!isCameraTranformInited) return;
        ChangeToScreenShotMode();
    }

    [ContextMenu("Reset EveryThing")]
    void ResetEveryThingContext()
    {
        if(!isCameraTranformInited) return;
        ResetEveryThing();
    }


    void Start()
    {
        playerTrans = GameObject.FindWithTag("Player").transform;
        playerState = playerTrans.GetComponent<PlayerState>();

        camController = cameraHolderObj.GetComponent<CameraController>();


        


    }

    public void ResetEveryThing()
    {
        isVideoShotMode = false;

        var cam = Camera.main;
        cam.farClipPlane = 49f;

        playerState.nextLvExp = playerOldExpNeed;     
        
        uiCanvasObj.SetActive(true);
        if(isNoEnemySpawner) spawnerObj.SetActive(true);

        camController.offset = new Vector3(oldCameraPosX, oldCameraPosY, oldCameraPosZ);
        cameraHolderObj.transform.rotation = Quaternion.Euler(oldCameraRotX, oldCameraRotY, oldCameraRotZ);

        AudioManager.Instance.allBGMVolume = 1f;

        Time.timeScale = 1f;

        if (isHideMouseCursor)
        {
            Cursor.visible = true;
            //Cursor.lockState = CursorLockMode.None;

        }

        if (isFreeCamera)
        {
            CameraDebugController freeCam = mainCamObj.GetComponent<CameraDebugController>();
            freeCam.isCameraDebugMode = false;

            //disable this CameraDebugController component :
            freeCam.enabled = false;

            //reset mainCamObj position and rotation to zero :
            mainCamObj.transform.localPosition = Vector3.zero;
            mainCamObj.transform.localRotation = Quaternion.identity;

        }

    }

    public void ChangeToScreenShotMode()
    {
        isVideoShotMode = true;

        var cam = Camera.main;
        cam.farClipPlane = 490f;

        playerOldExpNeed = playerState.nextLvExp;
        playerState.nextLvExp = playerExpNeedFix;

        if (isNoUi)
        {
            uiCanvasObj.SetActive(false);
        }

        if (isNoEnemySpawner)
        {
            spawnerObj.SetActive(false);
        }

        if (isKillAllEnemy)
        {
            var allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in allEnemies)
            {
                Destroy(enemy);
            }

        }

        if (isFreeCamera)
        {
            CameraDebugController freeCam = mainCamObj.GetComponent<CameraDebugController>();

            //enable this CameraDebugController component :
            freeCam.enabled = true;

            freeCam.isCameraDebugMode = true;

        }


        if (isFixCamera)
        {
            if (isCamCloseShot)
            {
                camController.offset = new Vector3(fixCameraPosX, closeShotCamPosY, closeShotCamPosZ);
                cameraHolderObj.transform.rotation = Quaternion.Euler(closeShotCamRotX, fixCameraRotY, fixCameraRotZ);
            }
            else
            {
                //cameraHolderObj.transform.position = new Vector3(fixCameraPosX, fixCameraPosY, fixCameraPosZ);
                //cameraHolderObj.transform.rotation = Quaternion.Euler(fixCameraRotX, fixCameraRotY, fixCameraRotZ);
                camController.offset = new Vector3(fixCameraPosX, fixCameraPosY, fixCameraPosZ);
                cameraHolderObj.transform.rotation = Quaternion.Euler(fixCameraRotX, fixCameraRotY, fixCameraRotZ);
            }
            
        }

        if(isDisableBGM) AudioManager.Instance.allBGMVolume = 0f;
                

        runTimeOld = TimeManager.Instance.gameTimePassed;
        turnTimeOld = TimeManager.Instance.gameTimeLeft;

        if (isHideMouseCursor)
        {
            Cursor.visible = false;
            //Cursor.lockState = CursorLockMode.Locked;

        }

    }



    void Update()
    {

        if (isVideoShotMode)
        {
            if (isFreezeTime)
            {
                TimeManager.Instance.gameTimePassed = runTimeOld;
                TimeManager.Instance.gameTimeLeft = turnTimeOld;
            }

            Time.timeScale = gameSpeed;

        if (isFixCamera)
        {
            if (isCamCloseShot)
            {
                camController.offset = new Vector3(fixCameraPosX, closeShotCamPosY, closeShotCamPosZ);
                cameraHolderObj.transform.rotation = Quaternion.Euler(closeShotCamRotX, fixCameraRotY, fixCameraRotZ);
            }
            else
            {           
                camController.offset = new Vector3(fixCameraPosX, fixCameraPosY, fixCameraPosZ);
                cameraHolderObj.transform.rotation = Quaternion.Euler(fixCameraRotX, fixCameraRotY, fixCameraRotZ);
            }
            
        }


        }


        if (!isCameraTranformInited)
        {
            isCameraTranformInited = true;
            oldCameraPosX = cameraHolderObj.transform.position.x;
            oldCameraPosY = cameraHolderObj.transform.position.y;
            oldCameraPosZ = cameraHolderObj.transform.position.z;
            
            oldCameraRotX = cameraHolderObj.transform.rotation.eulerAngles.x;
            oldCameraRotY = cameraHolderObj.transform.rotation.eulerAngles.y;
            oldCameraRotZ = cameraHolderObj.transform.rotation.eulerAngles.z;

            //oldCameraPosX = mainCamObj.transform.position.x;
            //oldCameraPosY = mainCamObj.transform.position.y;
            //oldCameraPosZ = mainCamObj.transform.position.z;

        }

        
    }
}

