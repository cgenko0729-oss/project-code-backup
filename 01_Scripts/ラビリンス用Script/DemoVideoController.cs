using Cysharp.Threading.Tasks;
using DG.Tweening;
using Hellmade.Sound; //SoundManager
using MonsterLove.StateMachine; //StateMachine
using QFSW.MOP2;                //Object Pool
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TigerForge;               //EventManager
using TMPro;    
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;     

[System.Serializable]
public class MessageData
{
 public string titleMessage;
}

public class DemoVideoController : MonoBehaviour
{
    public float idleTime=0;
    public float videoPlayTime = 3.0f;
    public Vector3 prevMousePos;
    public Vector3 nowMousePos;
    public GameObject demoVideo;
    public bool isPlayVideo = false;



    public RectTransform TitleLogo;

    private Vector2 scaleDownPos = new Vector2(198f, 513f);
    public Vector2 scaleDownScale = new Vector2(0.7f, 0.7f);

    public Vector2 scaleUpPos = new Vector2(310, 450);
    public Vector2 scaleUpScale = new Vector2(0.9f, 0.9f);

    public TextMeshProUGUI titleText;
    public RectTransform textOriginPos;

    public GameObject amenu;
    private IDisposable _inputSubscription;

    private void OnEnable()
    {
        // [FIX 1] Save the subscription token
        _inputSubscription = InputSystem.onAnyButtonPress.Call(ButtonInputVideoControll);
    }

    private void OnDisable()
    {
        // [FIX 1] Dispose properly to prevent memory leaks
        _inputSubscription?.Dispose();
    }

    private void Start()
    {
        demoVideo.SetActive(false);

        LoadMessage();
        textOriginPos = titleText.GetComponent<RectTransform>();
    }

    void LoadMessage()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "message.json");
        if (File.Exists(filePath))
        {
        string dataAsJson = File.ReadAllText(filePath);
        MessageData messageData = JsonUtility.FromJson<MessageData>(dataAsJson);
        titleText.text = messageData.titleMessage;
        }
        else
        {
        Debug.LogError("Cannot find message.json!");
        }

        //set to textOriginPos
       

    }

    void Update()
    {
        if(demoVideo == null) { return; }

        //if press c 
        if (Input.GetKeyDown(KeyCode.C))
        {
            //if amenu is not active, set active
            if (amenu.activeSelf == false)
            {
                amenu.SetActive(true);
            }
            else
            {
                amenu.SetActive(false);
            }
        }
        
        float leftStick =0; 
     
        float rightStick =0; 

        if(Gamepad.current != null)
        {
            leftStick= Gamepad.current.leftStick.ReadValue().sqrMagnitude;
            rightStick = Gamepad.current.rightStick.ReadValue().sqrMagnitude;
        }
        
        nowMousePos = Input.mousePosition;
        if (nowMousePos != prevMousePos ||
            leftStick > 0.04f || rightStick > 0.04f)
        {
            prevMousePos = nowMousePos;
            idleTime = 0;

            if (isPlayVideo == true)
            {
                StartCoroutine(StopVideo());
                ScaleUpLogo();
                 titleText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-128, 202);
                LoadMessage();
            }
        }
        else
        {
            idleTime += Time.fixedDeltaTime;
        }

        if (isPlayVideo == false && idleTime > videoPlayTime)
        {
            StartCoroutine(PlayVideo());
            ScaleDownLogo();
        }
    }

    void ButtonInputVideoControll(InputControl control)
    {
        idleTime = 0;

        if (isPlayVideo == true)
        {
            StartCoroutine(StopVideo());
            ScaleUpLogo();
            titleText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-128, 202);
            LoadMessage();
        }
    }

    System.Collections.IEnumerator PlayVideo()
    {
        demoVideo.SetActive(true);
        //demoVideo.transform.DOScale(Vector3.one, 0.5f);
        demoVideo.transform.DOScale(new Vector3(1.3f,1.3f,1.3f), 0.5f);
        yield return new WaitForSeconds(0.5f);

        isPlayVideo = true;
    }

    System.Collections.IEnumerator StopVideo()
    {
        demoVideo.transform.DOScale(Vector3.zero, 0.5f);
        yield return new WaitForSeconds(0.5f);
        demoVideo.SetActive(false);

        isPlayVideo = false;
    }

    void ScaleUpLogo()
    {
        //dotween to scale up logo position and scale
        TitleLogo.DOAnchorPos(scaleUpPos, 0.35f);
        TitleLogo.DOScale(scaleUpScale, 0.35f);


    }

    void ScaleDownLogo()
    {
        //dotween to scale down logo position and scale
        TitleLogo.DOAnchorPos(scaleDownPos, 0.35f);
        TitleLogo.DOScale(scaleDownScale, 0.35f);

    }

}

