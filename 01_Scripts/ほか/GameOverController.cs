using DG.Tweening;
using TigerForge;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameOverController : MonoBehaviour
{
    public Volume globalVolume;
    public GameObject GameOverGroup;

    Vignette vignette;
    public float startIntensity = 0;
    public float endIntensity = 1.0f;
    public float fadeTime = 2.0f;
    public float resultDuration = 2.0f;
    public float defaultTextScale = 0.5f;
    public float textScaleTime = 1.0f;

    public GameObject renderCamera;

    private void OnEnable()
    {
        EventManager.StartListening("isGameOver",OnGameOver);
    }

    private void OnDisable()
    {
        EventManager.StopListening("isGameOver",OnGameOver);
    }

    private void Start()
    {
       globalVolume = GameObject.FindGameObjectWithTag("GlobalVolume")?.GetComponent<Volume>();


        if (globalVolume != null)
        {
            globalVolume.profile.TryGet<Vignette>(out vignette);
        }

        if(GameOverGroup != null)
        {
            GameOverGroup.SetActive(false);
        }
    }

    // ゲームオーバーの演出を再生する
    private void OnGameOver()
    {

        renderCamera.SetActive(true);

        if (vignette != null)
        {
            vignette.active = true;
            vignette.color.value = Color.black;
            vignette.intensity.value = startIntensity;
        }

        if(GameOverGroup != null)
        {
            var trans = GameOverGroup.transform;
            trans.localScale = new Vector3(defaultTextScale, defaultTextScale);
            trans.DOScale(Vector3.one, textScaleTime);
            
            GameOverGroup.gameObject.SetActive(true);
        }

        StartCoroutine(StartFadeOut());
    }

    System.Collections.IEnumerator StartFadeOut()
    {
        if (vignette != null)
        {
            // 最初は「GAMEOVER」の文字だけで
            // 後からシーン遷移のボタンを出す
            DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, endIntensity, fadeTime).SetEase(Ease.OutQuad);

            // プレイヤー周囲のマップオブジェクトを非アクティブにする


            // ゲームオーバーになって一定時間経過したらリザルトを表示
            yield return new WaitForSeconds(resultDuration);
        }
         
        ResultMenuController.Instance.ShowResultMenu();
    }
}
