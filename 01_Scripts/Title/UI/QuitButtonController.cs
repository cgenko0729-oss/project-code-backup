using TigerForge;
using UnityEngine;

public class QuitButtonController: MonoBehaviour
{

    //PlayerState player;

    private void Start()
    {
        //player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();

    }

    public void OnClick()
    {
                Application.Quit();
        
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif

        //Time.timeScale = 1f;
        //EventManager.EmitEvent("isGameOver");


    }
}
