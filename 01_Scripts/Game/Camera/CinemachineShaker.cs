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
using Unity.Cinemachine;
using System.Collections;


public class CinemachineShaker : MonoBehaviour
{

    [SerializeField] CinemachineCamera vcam;                  // assign in Inspector or auto-find
    CinemachineBasicMultiChannelPerlin perlin;

    void Awake()
    {
        // 1) Find the vcam on this object (or its children) if not assigned
        if (!vcam && !TryGetComponent(out vcam))
            vcam = GetComponentInChildren<CinemachineCamera>(true);

        if (!vcam)
        {
            Debug.LogError("[CinemachineShaker] No CinemachineCamera found/assigned.");
            enabled = false;
            return;
        }

       

        perlin = vcam.GetComponent<CinemachineBasicMultiChannelPerlin>();

        perlin.AmplitudeGain = 0f;
        perlin.FrequencyGain = 0f;
    }

    private void OnEnable()
    {
        if (!vcam && !TryGetComponent(out vcam))
        vcam = GetComponentInChildren<CinemachineCamera>(true);
        if (vcam && !perlin)
        perlin = vcam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (perlin) { perlin.AmplitudeGain = 0; perlin.FrequencyGain = 0; }
    }

    void Update()
    {
       
        
    }

    IEnumerator PulseTimescale(float dur)
{
    float prev = Time.timeScale;
    Time.timeScale = 1f;               // let Perlin advance
    yield return new WaitForSecondsRealtime(dur);
    Time.timeScale = prev;
}

    public void DragonRoarImpact()
    {
        

        Shake(2.19f, 0.21f, 14f);
        GameVolumeManager.Instance.PlayRoarImpactShort();
    }

    public void DragonFlyImpact()
    {
        StartCoroutine(PulseTimescale(0.1f));
        Shake(2.8f, 0.21f, 14f);
        
    }

    public void Shake(float duration, float amplitude, float frequency)
    {
        if (!perlin) { Debug.LogWarning("Perlin is missing."); return; }
        StopAllCoroutines();
        StartCoroutine(DoShake(duration, amplitude, frequency));
    }

    IEnumerator DoShake(float duration, float amplitude, float frequency)
    {
        perlin.AmplitudeGain = amplitude;
        perlin.FrequencyGain = frequency;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // smooth falloff
        float startAmp = perlin.AmplitudeGain;
        const float fade = 0.2f;
        for (float f = 0; f < fade; f += Time.deltaTime)
        {
            float k = 1f - (f / fade);
            perlin.AmplitudeGain = startAmp * k;
            yield return null;
        }
        perlin.AmplitudeGain = 0f;
        perlin.FrequencyGain = 0f;
    }


}

