using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class CameraShake : MonoBehaviour
{
      public static CameraShake Instance { get; private set; }

    private Transform cameraTransform;

    // Shake parameters
    private float shakeDuration = 0f;
    private float shakeAmplitude = 0f;
    private float shakeFrequency = 0f;
    private float rotationAmplitude = 0f;

    // --- DELETED --- We no longer need to store the original state
    // private Vector3 originalPos;
    // private Quaternion originalRot;

    // The neutral position/rotation of the child camera
    private Vector3 neutralPosition = Vector3.zero; // CHANGED
    private Quaternion neutralRotation = Quaternion.identity; // CHANGED

    private Vector3 seed;

    public ShakeProfile defaultFootStepShake;
    public ShakeProfile strongShakeProfile;
    public ShakeProfile turnipaJumpShake;
    public ShakeProfile phrase2GroundShake;

    public ShakeProfile smallShake;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (transform.childCount > 0)
        {
            cameraTransform = transform.GetChild(0);
        }
        else
        {
            Debug.LogError("CameraShake script requires a Camera as a child object.");
            enabled = false;
            return;
        }

        seed = new Vector3(Random.value * 100, Random.value * 100, Random.value * 100);
    }

    // --- DELETED --- We no longer need OnEnable to store state
    // void OnEnable() { ... }

    void LateUpdate()
    {
        if (shakeDuration > 0)
        {
            shakeDuration -= Time.deltaTime;

            float xOffset = (Mathf.PerlinNoise(seed.x + Time.time * shakeFrequency, seed.y) * 2 - 1) * shakeAmplitude;
            float yOffset = (Mathf.PerlinNoise(seed.y, seed.x + Time.time * shakeFrequency) * 2 - 1) * shakeAmplitude;

            // CHANGED: Apply shake relative to the neutral position, not a stale "original" one.
            cameraTransform.localPosition = neutralPosition + new Vector3(xOffset, yOffset, 0);

            float zRotation = (Mathf.PerlinNoise(seed.z, Time.time * shakeFrequency) * 2 - 1) * rotationAmplitude;
            cameraTransform.localRotation = neutralRotation * Quaternion.Euler(0, 0, zRotation);

            if (shakeDuration <= 0)
            {
                shakeDuration = 0f;
                // CHANGED: Reset to the neutral state.
                cameraTransform.localPosition = neutralPosition;
                cameraTransform.localRotation = neutralRotation;
            }
        }
    }

    // The StartShake methods remain unchanged, they work perfectly fine.
    public void StartShake(float duration, float positionalAmplitude, float rotationalAmplitude, float frequency)
    {
        if (positionalAmplitude < this.shakeAmplitude && shakeDuration > 0) return;
        
        shakeDuration = duration;
        shakeAmplitude = positionalAmplitude;
        rotationAmplitude = rotationalAmplitude;
        shakeFrequency = frequency;
        seed = new Vector3(Random.value * 100, Random.value * 100, Random.value * 100);
    }

    public void StartShake(ShakeProfile profile)
    {
        if (profile == null)
        {
            Debug.LogWarning("Shake profile is null.");
            return;
        }
        
        // Allow a new shake to start if the old one is almost finished.
        if (profile.positionalAmplitude < this.shakeAmplitude && shakeDuration > 0.1f) return;

        shakeDuration = profile.duration;
        shakeAmplitude = profile.positionalAmplitude;
        rotationAmplitude = profile.rotationalAmplitude;
        shakeFrequency = profile.frequency;
        seed = new Vector3(Random.value * 100, Random.value * 100, Random.value * 100);
    }

    public void StartSmallShake()
    {
        // Allow a new shake to start if the old one is almost finished.
        if (smallShake.positionalAmplitude < this.shakeAmplitude && shakeDuration > 0.1f) return;
        shakeDuration = smallShake.duration;
        shakeAmplitude = smallShake.positionalAmplitude;
        rotationAmplitude = smallShake.rotationalAmplitude;
        shakeFrequency = smallShake.frequency;
        seed = new Vector3(Random.value * 100, Random.value * 100, Random.value * 100);

    }

     public void StartShake()
    {
       
        // Allow a new shake to start if the old one is almost finished.
        if (defaultFootStepShake.positionalAmplitude < this.shakeAmplitude && shakeDuration > 0.1f) return;

        shakeDuration = defaultFootStepShake.duration;
        shakeAmplitude = defaultFootStepShake.positionalAmplitude;
        rotationAmplitude = defaultFootStepShake.rotationalAmplitude;
        shakeFrequency = defaultFootStepShake.frequency;
        seed = new Vector3(Random.value * 100, Random.value * 100, Random.value * 100);

        ControllerVibrationManager.Instance.Vibrate(0.7f, 0.7f, 0.21f);
    }

    public void StartStrongShake()
    {

        // Allow a new shake to start if the old one is almost finished.
        if (strongShakeProfile.positionalAmplitude < this.shakeAmplitude && shakeDuration > 0.1f) return;

        shakeDuration = strongShakeProfile.duration;
        shakeAmplitude = strongShakeProfile.positionalAmplitude;
        rotationAmplitude = strongShakeProfile.rotationalAmplitude;
        shakeFrequency = strongShakeProfile.frequency;
        seed = new Vector3(Random.value * 100, Random.value * 100, Random.value * 100);


    }

    public void StartTurnipaJumpShake()
    {
        // Allow a new shake to start if the old one is almost finished.
        if (turnipaJumpShake.positionalAmplitude < this.shakeAmplitude && shakeDuration > 0.1f) return;

        shakeDuration = turnipaJumpShake.duration;
        shakeAmplitude = turnipaJumpShake.positionalAmplitude;
        rotationAmplitude = turnipaJumpShake.rotationalAmplitude;
        shakeFrequency = turnipaJumpShake.frequency;
        seed = new Vector3(Random.value * 100, Random.value * 100, Random.value * 100);
    }

    public void StartPhrase2GroundShake()
    {
        if (phrase2GroundShake.positionalAmplitude < this.shakeAmplitude && shakeDuration > 0.1f) return;

        shakeDuration = phrase2GroundShake.duration;
        shakeAmplitude = phrase2GroundShake.positionalAmplitude;
        rotationAmplitude = phrase2GroundShake.rotationalAmplitude;
        shakeFrequency = phrase2GroundShake.frequency;
        seed = new Vector3(Random.value * 100, Random.value * 100, Random.value * 100);

    }



}

