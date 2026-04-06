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
using UnityEngine.InputSystem;
using System.Collections;

public class UiCircularProgressBar : MonoBehaviour
{
    Image staminaCircle;
    TextMeshProUGUI keyText;

    public float fillDuration = 3.0f;
    
    public float drainDuration = 1.0f;

    private Coroutine fillCoroutine;

    private void Start()
    {
        staminaCircle = GetComponent<Image>();
        staminaCircle.fillAmount = 0;

        keyText = GetComponentInChildren<TextMeshProUGUI>();

    }

    private void Update()
    {

        //if any key was pressed this frame
        //if (Keyboard.current.anyKey.wasPressedThisFrame)

        if (Keyboard.current.xKey.wasPressedThisFrame)
        {

            //keyText's color alpha gradually becomes 1 over 0.3 seconds.
            //keyText.DOFade(1, 0.3f);

            // If any coroutine is running, stop it.
            if (fillCoroutine != null)
            {
                StopCoroutine(fillCoroutine);
            }
            // Start the FillStamina coroutine and store a reference to it.
            fillCoroutine = StartCoroutine(FillStamina());
        }

        // When the 'X' key is released this frame.
        if (Keyboard.current.xKey.wasReleasedThisFrame)
        {

            //keyText's color alpha gradually becomes 0 over 0.5 seconds.
            //keyText.DOFade(0, 0.5f);


            // If any coroutine is running, stop it.
            if (fillCoroutine != null)
            {
                StopCoroutine(fillCoroutine);
            }
            // Start the DrainStamina coroutine and store a reference to it.
            fillCoroutine = StartCoroutine(DrainStamina());
        }
        
    }


     private IEnumerator FillStamina()
    {
        // The speed at which the bar fills is the total change (1.0) divided by the duration.
        float fillSpeed = 1.0f / fillDuration;
        
        // Continue this loop as long as the fill amount is less than 1.
        while (staminaCircle.fillAmount < 1.0f)
        {
            // Increase the fill amount based on the fill speed and the time since the last frame.
            staminaCircle.fillAmount += fillSpeed * Time.unscaledDeltaTime;
            
            // Wait for the next frame before continuing the loop.
            yield return null; 
        }

        // Just in case of minor floating point inaccuracies, ensure it's exactly 1 at the end.
        staminaCircle.fillAmount = 1.0f;
        CutSceneManager.Instance.EndBossFightCutScene();
    }

    /// <summary>
    /// Coroutine to smoothly decrease the fill amount over time.
    /// </summary>
    private IEnumerator DrainStamina()
    {
        // The speed at which the bar drains.
        float drainSpeed = 1.0f / drainDuration;

        // Continue this loop as long as the fill amount is greater than 0.
        while (staminaCircle.fillAmount > 0.0f)
        {
            // Decrease the fill amount.
            staminaCircle.fillAmount -= drainSpeed * Time.unscaledDeltaTime;

            // Wait for the next frame.
            yield return null;
        }

        // Ensure it's exactly 0 at the end.
        staminaCircle.fillAmount = 0.0f;
    }


}

