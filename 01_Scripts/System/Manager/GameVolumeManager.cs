using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameVolumeManager : Singleton<GameVolumeManager>
{


    // GameVolumeManager is responsible for managing the game volume settings.

    public Volume globalVolume; 

    Volume fxVolume;
    VolumeProfile fxProfile;

    ChromaticAberration ca;
    LensDistortion      ld;
    Vignette            vig;
    Bloom               bloom;
    MotionBlur          mblur;
    ColorAdjustments    color;

    Sequence currentSeq;


     void Awake()
    {
        if (!globalVolume)
            globalVolume = GameObject.FindGameObjectWithTag("GlobalVolume")?.GetComponent<Volume>();

        EnsureFxVolume();
    }

    private void Update()
    {
        
    }

    public void PlayRoarAttackImapact()
    {
        PlayRoarImpact(2.89f);
        //CameraShake.Instance.StartTurnipaJumpShake();
    }

    public void PlayRoarImpactShort()
    {
        PlayRoarImpact(2.19f);
        CameraShake.Instance.StartPhrase2GroundShake();
    }

    // ===== Public API =========================================================

    /// <summary>
    /// Play a short impact FX: chromatic aberration + lens distortion + vignette etc.
    /// Call this from your dragon roar (AnimationEvent or code).
    /// </summary>
    public void PlayRoarImpact(float duration = 3f)
    {
        if (!fxVolume) return;

        // Kill any previous sequence
        currentSeq?.Kill();
        fxVolume.weight = 0f;

        // Set starting values (off)
        ca.intensity.value   = 0f;           // 0..1
        ld.intensity.value   = 0f;           // -1..1 (negative = barrel)
        ld.xMultiplier.value = 1.0f;
        ld.yMultiplier.value = 1.0f;
        mblur.intensity.value= 0f;           // 0..1

        //vig.intensity.value  = 0f;           // 0..1
        //bloom.intensity.value= 0f;           // ~0..10 (depends on setup)
        //color.saturation.value = 0f;         // -100..100
        //color.contrast.value   = 0f;         // -100..100

        // Build the roar gpunchh: quick in (0.25s), hold, then out.
        float inTime   = 0.1f; //0.25
        float outTime  = 0.1f; //0.35
        float holdTime = Mathf.Max(0f, duration - (inTime + outTime));

        currentSeq = DOTween.Sequence().SetUpdate(true);

        // Ease in (blend the whole transient profile)
        currentSeq.Append(DOTween.To(() => fxVolume.weight, w => fxVolume.weight = w, 1f, inTime).SetEase(Ease.OutQuad));

        // Parameter punches (join so they animate together)
        currentSeq.Join(DOTween.To(() => ca.intensity.value, v => ca.intensity.value = v, 0.7f, inTime));    // strong CA
        currentSeq.Join(DOTween.To(() => ld.intensity.value, v => ld.intensity.value = v, -0.35f, inTime));  // slight barrel warp
        currentSeq.Join(DOTween.To(() => mblur.intensity.value, v => mblur.intensity.value = v, 0.2f, inTime)); // a little blur

        //currentSeq.Join(DOTween.To(() => vig.intensity.value, v => vig.intensity.value = v, 0.35f, inTime)); // vignette
        //currentSeq.Join(DOTween.To(() => bloom.intensity.value, v => bloom.intensity.value = v, 2.0f, inTime)); // subtle flash
        //currentSeq.Join(DOTween.To(() => color.saturation.value, v => color.saturation.value = v, -10f, inTime)); // slight desat
        //currentSeq.Join(DOTween.To(() => color.contrast.value,   v => color.contrast.value   = v, 10f, inTime)); // slight contrast

        // Hold
        if (holdTime > 0f) currentSeq.AppendInterval(holdTime);

        // Ease out (fade whole volume back)
        currentSeq.Append(DOTween.To(() => fxVolume.weight, w => fxVolume.weight = w, 0f, outTime).SetEase(Ease.InQuad));

        // Optional: clean up on complete (weight at 0 means overrides are effectively off)
        currentSeq.OnKill(() => fxVolume.weight = 0f);
    }

    /// <summary>Stop any running transient FX immediately.</summary>
    public void StopAllFx()
    {
        currentSeq?.Kill(true);
        fxVolume.weight = 0f;
    }

    // ===== Internals ==========================================================

    void EnsureFxVolume()
    {
        // Create a dedicated overlay volume so we never bake changes into your main profile.
        if (fxVolume) return;

        var root = globalVolume ? globalVolume.transform : null;
        var go = new GameObject("TransientFXVolume");
        if (root) go.transform.SetParent(root, false);

        fxVolume = go.AddComponent<Volume>();
        fxVolume.isGlobal = true;
        fxVolume.priority = (globalVolume ? globalVolume.priority : 0f) + 10f; // ensure it wins
        fxVolume.weight   = 0f;

        fxProfile = ScriptableObject.CreateInstance<VolumeProfile>();
        fxVolume.sharedProfile = fxProfile;

        ca = GetOrAdd<ChromaticAberration>(fxProfile, o =>
        {
            o.intensity.value = 0f;          // valid in URP
            // If your URP version exposes it:
            // o.maxSamples.value = 6;       // lower = faster, higher = better quality
        });        
        ld    = GetOrAdd<LensDistortion>(fxProfile, null);
        vig   = GetOrAdd<Vignette>(fxProfile, v => { v.rounded.value = true; });
        bloom = GetOrAdd<Bloom>(fxProfile, null);
        mblur = GetOrAdd<MotionBlur>(fxProfile, mb => { mb.quality.value = MotionBlurQuality.High; });
        color = GetOrAdd<ColorAdjustments>(fxProfile, null);

        // Start disabled (weight 0 makes them invisible anyway)
        //ca.active = ld.active = vig.active = bloom.active = mblur.active = color.active = true;

        ca.active = true;
        ld.active = true;
        mblur.active = true;

        vig.active = false;
        bloom.active = false;
        color.active = false;


    }

    static T GetOrAdd<T>(VolumeProfile profile, System.Action<T> init) where T : VolumeComponent, new()
    {
        if (!profile.TryGet<T>(out var comp))
        {
            comp = profile.Add<T>(true);
            init?.Invoke(comp);
        }
        return comp;
    }
}



