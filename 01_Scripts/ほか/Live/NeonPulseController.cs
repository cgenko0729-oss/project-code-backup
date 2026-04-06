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

public class NeonPulseController : MonoBehaviour
{
    [Header("Settings")]
    public Renderer targetRenderer;
    [ColorUsage(true, true)] // Allow HDR selection
    public Color dimColor = new Color(0, 0, 0.5f, 1) * 1f;  // Dim Blue
    [ColorUsage(true, true)] 
    public Color brightColor = new Color(0, 0.5f, 1f, 1) * 4f; // Bright Neon Blue

    [Header("Animation")]
    public float beatDuration = 0.5f; // Duration of one pulse

    private Material targetMat;

    void Start()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        
        // Get the material instance so we don't change ALL shared materials
        targetMat = targetRenderer.material;
        
        // Enable Emission keyword just in case
        targetMat.EnableKeyword("_EMISSION");

        StartPulsing();
    }

    void StartPulsing()
    {
        // 1. Set start color
        targetMat.SetColor("_EmissionColor", dimColor);

        // 2. Create the Loop
        // Tween from Dim to Bright and back
        targetMat.DOColor(brightColor, "_EmissionColor", beatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo); // Infinite Loop, Back and Forth
    }

    // Call this function when a "Drop" happens in the song!
    public void FlashWhite()
    {
        // Quickly flash white then return to loop
        targetMat.DOColor(Color.white * 5f, "_EmissionColor", 0.1f)
            .OnComplete(() => {
                // Return to pulsing
                targetMat.DOColor(dimColor, "_EmissionColor", 0.2f);
            });
    }

    void OnDestroy()
    {
        // Clean up material instance to prevent memory leaks
        if (targetMat != null) Destroy(targetMat);
    }
}

