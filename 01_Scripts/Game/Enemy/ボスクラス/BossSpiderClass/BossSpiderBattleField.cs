using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine

using UnityEngine.Rendering; 
using UnityEngine.Rendering.Universal; 

public class BossSpiderBattleField : MonoBehaviour
{

    public int spiderDenNumber; // スパイダーデンの数

    public GameObject spiderSkyNet;
    public GameObject[] spiderDen = new GameObject[3]; // スパイダーデンのプレハブ
    public GameObject[] spiderDenBlock = new GameObject[3]; // スパイダーデンのブロック

    public int finishTask = 0;

    public Volume globalVolume;
    public  Vignette vignette; 
    public float vignetteIntensity = 0.56f;
    public float vignetteFadeInTime = 3f;

    public bool isSpiderField = true;

    public void OnEnable()
    {
        EventManager.StartListening("BossPhase2Start", SetUpFieldPhrase2);
        EventManager.StartListening("SpiderDenDestroy",UpdateSpiderDenNumber);
        EventManager.StartListening("StartBossFight", SetUpFieldBossFightStart);
    }

    public void OnDisable()
    {
        EventManager.StopListening("BossPhase2Start", SetUpFieldPhrase2);
        EventManager.StopListening("SpiderDenDestroy", UpdateSpiderDenNumber);
        EventManager.StopListening("StartBossFight", SetUpFieldBossFightStart);
    }


    void Start()
    {
        spiderDenNumber = 3; // スパイダーデンの数を設定

        //get globalVolume  by finding the game object  with tag "GlobalVolume" and if that gameobject if active in hierarchy
        globalVolume = GameObject.FindGameObjectWithTag("GlobalVolume")?.GetComponent<Volume>();

        if (globalVolume != null)
        {
            globalVolume.profile.TryGet<Vignette>(out vignette);
        }

    }

    void Update()
    {

        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    SetUpFieldBossFightStart();
        //}

    }

    public void SetUpFieldBossFightStart()
    {
        Camera.main.transform.position = Vector3.zero;
        Camera.main.transform.rotation = Quaternion.identity;

        transform.DOMoveY(0, 1.5f).SetEase(Ease.OutBounce);
        CameraShake.Instance.StartPhrase2GroundShake();

        if(StageManager.Instance.mapData.mapType == MapType.Desert)
        {
            vignetteIntensity = 0.35f;
        }else
        {
            vignetteIntensity = 0.49f;
        }
        
        globalVolume.gameObject.SetActive(true);
        vignette.active = true;
        DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, vignetteIntensity, vignetteFadeInTime).SetEase(Ease.OutQuad);

    }

    public void SetUpFieldPhrase2()
    {
        if(!isSpiderField) return; // スパイダーフィールドでない場合は何もしない

        //setactive sky net and all spiderden 
        spiderSkyNet.SetActive(true);
        for (int i = 0; i < spiderDen.Length; i++)
        {
            spiderDen[i].SetActive(true);
        }

        for (int i = 0; i < spiderDenBlock.Length; i++)
        {
            spiderDenBlock[i].SetActive(true);
        }

        CameraShake.Instance.StartPhrase2GroundShake();

    }

    public void UpdateSpiderDenNumber()
    {
        if(!isSpiderField) return; // スパイダーフィールドでない場合は何もしない

        spiderDenNumber--; // スパイダーデンの数を減らす
 
        if (spiderDenNumber <=0) // スパイダーデンが全て破壊されたら
        {
            spiderSkyNet.SetActive(false); // スカイネットを非表示にする
            EventManager.EmitEvent("AllSpiderDenDestroyed"); // イベントを発行
            
        }

    }

    


}

