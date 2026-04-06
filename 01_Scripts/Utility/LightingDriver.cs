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
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


//[ExecuteAlways]                 // updates in Edit mode too

public class LightingDriver : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Light sun;            // drag your Directional Light here

    [Header("Day-cycle Data")]
    [SerializeField] AnimationCurve kelvinCurve;
    [SerializeField] AnimationCurve intensityCurve;

    [Header("Time Source")]
    [SerializeField] TimeManager timeMgr;  // optional – auto-find if null

    public bool isBossFightLighting = false; //ボス戦のライティングを有効にするかどうか

    public float kelvinMin = 2100F;
    public float kelvinMax = 7000F;
    public float intensityMin = 1.7f;
    public float intensityMax = 1.7F;

    [Header("Post-Processing")]
    [SerializeField] private Volume globalVolume;
    private Bloom bloom;
    private Vignette vignette;

    void Awake ()
    {
        if (sun == null) sun = GetComponent<Light>();
        if (timeMgr == null) timeMgr = TimeManager.Instance;

        sun.useColorTemperature = true;    


        globalVolume.profile.TryGet(out bloom);
        globalVolume.profile.TryGet(out vignette);
    }

    private void Start()
    {
        switch (StageManager.Instance.mapData.mapType)
        {

            case MapType.None:
                break;
            case MapType.SpiderForest:
                sun.colorTemperature = 7777;
                sun.intensity = 1.7f;
                break;
            case MapType.AncientForest:
                sun.colorTemperature = 7777;
                sun.intensity = 1.7f;
                break;
            case MapType.Castle:
                break;
            case MapType.Desert:
                sun.colorTemperature = 7777;
                sun.intensity = 1.70f;
                break;
            case MapType.Temple:
                sun.colorTemperature = 7900;
                sun.intensity = 0.77f;
                UpdateBloom(0.42f, 0.7f, 1.0f, new Color(1.0f, 0.8f, 0.5f));
                UpdateVignette(new Color(0.0f, 0.0f, 0.0f), 0.359f, 0.77f);
                break;

            default:
                break;
        }
    }

    void Update ()
    {


        switch (StageManager.Instance.mapData.mapType)
        {

            case MapType.None:
                break;
            case MapType.SpiderForest:
                break;
            case MapType.AncientForest:
                UpdateLightAncientForest();
                break;
            case MapType.Castle:
                break;
            case MapType.Desert:
                break;
            case MapType.Temple:
                break;
            default:
                break;
        }


        
    }

    void UpdateLightSpiderForest()
    {
        if (GameManager.Instance.isBossFight && !isBossFightLighting)
        {
            isBossFightLighting = true;
            sun.colorTemperature = 20000;
            sun.intensity = 1.49f;
        }

    }


    void UpdateLightAncientForest()
    {

        if (StageManager.Instance.isEndlessMode)
        {
            sun.colorTemperature = 20000;
            sun.intensity = 1.49f;
            return;
        }

        if (GameManager.Instance.isBossFight && !isBossFightLighting)
        {
            isBossFightLighting = true;
            sun.colorTemperature = 20000;
            sun.intensity = 1.49f;
        }

        if (isBossFightLighting) return;    


        float t = (timeMgr.gameTimePassed % timeMgr.GameTotalTime)/  timeMgr.GameTotalTime;

        float k01   = kelvinCurve.Evaluate(t);     // 0-1
        float i01   = intensityCurve.Evaluate(t);  // 0-1

        sun.colorTemperature = Mathf.Lerp(kelvinMin, kelvinMax, k01);
        sun.intensity        = Mathf.Lerp(intensityMin, intensityMax, i01);
    }


    void InitBloom()
    {

    }

    void InitVignette()
    {

    }

    void UpdateBloom(float threshold, float intensity, float scatter, Color tint)
    {
        if (bloom != null)
        {
            bloom.threshold.Override(threshold);
            bloom.intensity.Override(intensity);
            bloom.scatter.Override(scatter);
            //bloom.tint.Override(tint);
        }
    }

    void UpdateVignette(Color color, float intensity, float smoothness)
    {
        if (vignette != null)
        {
            vignette.active = true;

            //vignette.color.Override(color);
            vignette.intensity.Override(intensity);
            vignette.smoothness.Override(smoothness);
        }
    }



}

