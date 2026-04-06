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
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class BossBattleField : MonoBehaviour
{
    //public Volume globalVolume;
    //public  Vignette vignette; 
    //public float vignetteIntensity = 0.42f;
    //public float vignetteFadeInTime = 3f;

    //public void OnEnable()
    //{
  
    //    EventManager.StartListening("StartBossFight", SetUpFieldBossFightStart);
    //}

    //public void OnDisable()
    //{
    //    EventManager.StopListening("StartBossFight", SetUpFieldBossFightStart);
    //}

    //void Start()
    //{
    //    //globalVolume = GameObject.FindGameObjectWithTag("GlobalVolume")?.GetComponent<Volume>();
    //    globalVolume.profile.TryGet<Vignette>(out vignette);

        

    //}

    //void Update()
    //{
        
    //}


    //public void SetUpFieldBossFightStart()
    //{
    //    Debug.Log("Set Up for Boss Battle Field.");

    //    Camera.main.transform.position = Vector3.zero;
    //    Camera.main.transform.rotation = Quaternion.identity;

    //    transform.DOMoveY(0, 1.5f).SetEase(Ease.OutBounce);
    //    CameraShake.Instance.StartPhrase2GroundShake();
        
    //    globalVolume.gameObject.SetActive(true);
    //    vignette.active = true;
    //    DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, vignetteIntensity, vignetteFadeInTime).SetEase(Ease.OutQuad);

    //    Debug.Log("Set Up vignette for Boss Battle Field.");

    //}



     [SerializeField] Volume globalVolume;          // assign in Inspector
    public Vignette vignette;                             // runtime‑only reference

    [SerializeField] float targetIntensity = 0.42f;
    [SerializeField] float fadeSeconds     = 3f;

    void Awake()                                   // ← safer than Start
    {
        if (!globalVolume)
            Debug.LogError("Global Volume missing on " + name);

        if (!globalVolume.profile.TryGet(out vignette))
            Debug.LogError("Vignette missing from profile");

        // Make sure this component is actually used by URP
        vignette.SetAllOverridesTo(true);          // :contentReference[oaicite:0]{index=0}
        vignette.intensity.overrideState = true;
    }

    void OnEnable()  =>
        EventManager.StartListening("StartBossFight", OnBossFight);

    void OnDisable() =>
        EventManager.StopListening("StartBossFight", OnBossFight);

    void OnBossFight()
    {
        if (!vignette) return;                    // early‑out safety

        globalVolume.weight = 1f;                 // enable volume

        // Reset to 0 every time so the Tween is repeatable
        vignette.intensity.value = 0f;

        DOTween.To(() => vignette.intensity.value,
                   x  => vignette.intensity.value = x,
                   targetIntensity,
                   fadeSeconds).SetEase(Ease.OutQuad);

        Debug.Log("Vignette fade‑in started");
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.V))
        //{
        //    OnBossFight();
        //}
    }



}

