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

public class SpotLightColorController : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Drag the Light components of your spotlights here")]
    public List<Light> targetLights;

    [Tooltip("Optional: If your light models have a glowing mesh (bulb), drag their renderers here to change their emission color too.")]
    public List<MeshRenderer> bulbRenderers;

    [Header("Color Settings")]
    public Color colorA = Color.yellow;
    public Color colorB = Color.blue;

    [Header("Timing")]
    [Tooltip("Time in seconds between changes (e.g. 1.5 seconds)")]
    public float duration = 1.5f;

    [Header("Style")]
    [Tooltip("Check this for a smooth gradient. Uncheck for an instant snap switch.")]
    public bool smoothFade = true;

    // Internal reference to keep track of tweens
    private List<Tween> activeTweens = new List<Tween>();

    void Start()
    {
        StartColorLoop();
    }

    public void StartColorLoop()
    {
        // Kill any existing tweens to prevent bugs if called twice
        KillAllTweens();

        // 1. Handle the actual Lights
        foreach (var light in targetLights)
        {
            if (light == null) continue;

            // Set initial color
            light.color = colorA;

            if (smoothFade)
            {
                // MODE A: Smooth Fade (Gradient)
                // Tweens to Blue over 1.5s, then Yoyo (reverses) back to Yellow
                Tween t = light.DOColor(colorB, duration)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo);
                
                activeTweens.Add(t);
            }
            else
            {
                // MODE B: Hard Switch (Snap)
                // Waits 1.5s, Snaps to Blue, Waits 1.5s, Snaps to Yellow
                Sequence seq = DOTween.Sequence();
                seq.AppendInterval(duration);
                seq.AppendCallback(() => light.color = colorB);
                seq.AppendInterval(duration);
                seq.AppendCallback(() => light.color = colorA);
                seq.SetLoops(-1); // Infinite loop
                
                activeTweens.Add(seq);
            }
        }

        // 2. Handle the glowing bulb meshes (Optional)
        // This makes the physical 3D model change color along with the light beam
        foreach (var rend in bulbRenderers)
        {
            if (rend == null) continue;
            
            // Assume the emission is on the first material
            Material mat = rend.material; 
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", colorA);

            if (smoothFade)
            {
                Tween t = mat.DOColor(colorB, "_EmissionColor", duration)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo);
                activeTweens.Add(t);
            }
            else
            {
                Sequence seq = DOTween.Sequence();
                seq.AppendInterval(duration);
                seq.AppendCallback(() => mat.SetColor("_EmissionColor", colorB));
                seq.AppendInterval(duration);
                seq.AppendCallback(() => mat.SetColor("_EmissionColor", colorA));
                seq.SetLoops(-1);
                activeTweens.Add(seq);
            }
        }
    }

    // Cleanup when object is destroyed or disabled
    void OnDestroy()
    {
        KillAllTweens();
    }

    void KillAllTweens()
    {
        foreach (var t in activeTweens)
        {
            if (t != null && t.IsActive()) t.Kill();
        }
        activeTweens.Clear();
    }
}

