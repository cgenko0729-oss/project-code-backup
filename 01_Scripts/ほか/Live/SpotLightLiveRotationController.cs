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

public class SpotLightLiveRotationController : MonoBehaviour
{
    [Header("Target")]
    public Renderer starRenderer;

    [Header("Live Settings")]
    [Tooltip("Main color of the star (Base)")]
    [ColorUsage(true, true)] // Enables HDR Color Picker
    public Color colorA = new Color(1f, 0.8f, 0f, 1f); // Golden Yellow

    [Tooltip("Secondary color to flash to")]
    [ColorUsage(true, true)]
    public Color colorB = new Color(0f, 0.5f, 1f, 1f); // Cyan Blue

    [Header("Animation")]
    [Range(0.1f, 10f)] public float pulseSpeed = 2.0f;
    [Range(0f, 5f)]    public float twinkleIntensity = 1.5f;
    [Range(0f, 1f)]    public float randomOffset = 0f; // Makes stars not blink in unison

    [Header("Mode")]
    public bool isFlickering = true;

    // Optimization: Use PropertyBlock to avoid creating new Materials
    private MaterialPropertyBlock _propBlock;
    private int _emissionColorID;
    private int _baseColorID;
    private float _timeOffset;

    void Awake()
    {
        if (starRenderer == null) starRenderer = GetComponent<Renderer>();
        
        _propBlock = new MaterialPropertyBlock();
        _emissionColorID = Shader.PropertyToID("_EmissionColor");
        _baseColorID = Shader.PropertyToID("_BaseColor"); // Use "_BaseColor" for URP, "_Color" for Built-in
    }

    void Start()
    {
        // Give each star a random start time so they don't all pulse exactly together
        _timeOffset = Random.Range(0f, 10f) * randomOffset;
    }

    void Update()
    {
        if (starRenderer == null) return;

        // 1. Calculate the Pulse (Sine Wave)
        // Moves between 0 and 1 smoothly over time
        float time = (Time.time * pulseSpeed) + _timeOffset;
        float pulse = (Mathf.Sin(time) + 1f) / 2f; // Remap -1..1 to 0..1

        // 2. Calculate Flicker (Perlin Noise)
        // Adds a rapid, organic "candle" or "star" shake effect
        float flicker = 1f;
        if (isFlickering)
        {
            // Noise moves very fast (time * 10)
            flicker = Mathf.PerlinNoise(Time.time * 8f, _timeOffset); 
            // Map noise to a range like 0.5 to 1.5
            flicker = Mathf.Lerp(0.5f, 1.0f + twinkleIntensity, flicker);
        }

        // 3. Mix the Colors
        // Lerp between Yellow and Blue based on the Pulse
        Color finalColor = Color.Lerp(colorA, colorB, pulse);

        // 4. Apply Intensity (Flicker)
        // Multiply the color brightness by the flicker amount
        Color finalEmission = finalColor * flicker * 2f; // *2 makes it brighter for Bloom

        // 5. Apply to GPU using PropertyBlock (Very Efficient)
        starRenderer.GetPropertyBlock(_propBlock);
        
        _propBlock.SetColor(_baseColorID, finalColor);      // Set the physical object color
        _propBlock.SetColor(_emissionColorID, finalEmission); // Set the glowing light color
        
        starRenderer.SetPropertyBlock(_propBlock);
    }
    
    // Call this from your Timeline or EventManager to change excitement level!
    public void SetHighEnergyMode()
    {
        pulseSpeed = 8f;        // Fast pulsing
        twinkleIntensity = 3f;  // Crazy flickering
    }

    public void SetCalmMode()
    {
        pulseSpeed = 1f;        // Slow breathing
        twinkleIntensity = 0.5f;// Gentle shine
    }
}

