using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using System.Collections;


public class ControllerVibrationManager : Singleton<ControllerVibrationManager>
{
    //ControllerVibrationManager.instance.Vibrate(0.8f, 0.8f, 0.2f);

    private Coroutine stopVibrationCoroutine;

    public void Vibrate(float lowFrequency = 0.14f, float highFrequency = 0.28f, float duration = 0.07f)
    {
        //return;
        if (Gamepad.current == null) return;
        if(SoundEffect.Instance.controllerVibrationType == 0) return;
        if(SoundEffect.Instance.controllerVibrationType == 1)
        {
            lowFrequency *= 0.21f;
            highFrequency *= 0.35f;
            duration *= 0.14f;
        }

        if (stopVibrationCoroutine != null)
        {
            StopCoroutine(stopVibrationCoroutine);
        }

        Gamepad.current.SetMotorSpeeds(0.28f, 0.28f);
        //StartCoroutine(Rumble(lowFrequency, highFrequency, duration));
        stopVibrationCoroutine = StartCoroutine(StopVibrationAfterDelay(0.14f));
    }

    public void VibratePulse(float lowFrequency = 0.7f, float highFrequency = 0.7f, float pulseDuration = 0.14f, int pulseCount = 5)
    {
        //return;
        if (Gamepad.current == null) return;
        if(SoundEffect.Instance.controllerVibrationType == 0) return;
        if(SoundEffect.Instance.controllerVibrationType == 1)
        {
            lowFrequency *= 0.5f;
            highFrequency *= 0.5f;
            pulseDuration *= 0.77f;
        }

        StartCoroutine(PulseRumble(lowFrequency, highFrequency, pulseDuration, pulseCount));
    }

    private IEnumerator Rumble(float lowFrequency, float highFrequency, float duration)
    {

        
        if (Gamepad.current != null)
        {
            if(SoundEffect.Instance.controllerVibrationType == 0) yield break;

            if (SoundEffect.Instance.controllerVibrationType == 1)
            {
                lowFrequency *= 0.5f;
                highFrequency *= 0.5f;
                duration *= 0.77f;
            }

            Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequency);

            yield return new WaitForSeconds(duration);

            StopVibration();
        }
    }

    private IEnumerator StopVibrationAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        StopVibration();
    }


    public void StopVibration()
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0f, 0f);

            Gamepad.current.ResetHaptics();
        }
    }

    public IEnumerator PulseRumble(float lowFrequency, float highFrequency, float pulseDuration, int pulseCount)
    {
        if (Gamepad.current != null)
        {
            for (int i = 0; i < pulseCount; i++)
            {
                Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequency);
                yield return new WaitForSeconds(pulseDuration);
                Gamepad.current.SetMotorSpeeds(0f, 0f);
                yield return new WaitForSeconds(pulseDuration);
            }
        }
    }

    private void OnDisable()
    {
        StopVibration();
    }

    private void OnDestroy()
    {
        StopVibration();
    }
    
    // To call this:
    // StartCoroutine(ControllerVibrationManager.instance.PulseRumble(0.5f, 0.5f, 0.1f, 3));


}

