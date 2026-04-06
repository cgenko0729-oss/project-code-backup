using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // --- Public fields to assign in the Inspector ---
    public Image progressBar;
    public TextMeshProUGUI progressText;

    // --- Static fields to control the loading ---
    private static string sceneToLoad; // The name of the scene we want to load.

    // This is the public method that any other script will call to start the loading process.
    public static void LoadScene(string sceneName)
    {
        // Store the name of the scene that we need to load.
        sceneToLoad = sceneName;

        // Immediately load our dedicated "LoadingScene". This is so fast it's basically instant.
        SceneManager.LoadSceneAsync("testScene");
    }

    // This method is called automatically when the "LoadingScene" starts.
    void Start()
    {
        // Start the coroutine that will load the target scene and update the UI.
        StartCoroutine(LoadTargetSceneAsync());
    }

    // This is the coroutine that does all the work. It's the same logic from your
    // previous script, but now it runs in a clean, dedicated scene.
    IEnumerator LoadTargetSceneAsync()
    {
        // --- SETUP ---
        if (progressBar != null) progressBar.fillAmount = 0;
        if (progressText != null) progressText.text = "0%";

        float currentVisualProgress = 0f;
        float animationSpeed = 3f;

        // Start the async operation to load the target scene.
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        op.allowSceneActivation = false;


        // --- PHASE 1: Animate while the scene is actually loading ---
        while (op.progress < 0.9f)
        {
            float targetProgress = Mathf.Clamp01(op.progress / 0.9f);
            currentVisualProgress = Mathf.Lerp(currentVisualProgress, targetProgress, Time.deltaTime * animationSpeed);

            if (progressBar != null) progressBar.fillAmount = currentVisualProgress;
            if (progressText != null) progressText.text = Mathf.FloorToInt(currentVisualProgress * 100) + "%";
            
            yield return null;
        }

        // --- PHASE 2: Animate the final 90% to 100% ---
        float finalTarget = 1f;
        while (currentVisualProgress < 0.99f)
        {
            currentVisualProgress = Mathf.Lerp(currentVisualProgress, finalTarget, Time.deltaTime * animationSpeed);

            if (progressBar != null) progressBar.fillAmount = currentVisualProgress;
            if (progressText != null) progressText.text = Mathf.FloorToInt(currentVisualProgress * 100) + "%";

            yield return null;
        }

        // --- FINALIZATION ---
        if (progressBar != null) progressBar.fillAmount = 1f;
        if (progressText != null) progressText.text = "100%";
        
        // Allow the scene to activate. The brief freeze will happen here.
        op.allowSceneActivation = true;
    }
}

