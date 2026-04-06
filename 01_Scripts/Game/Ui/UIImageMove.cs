using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIImageMove : MonoBehaviour
{
    public Image img;
    public CanvasGroup CG;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // img.rectTransform.anchoredPosition = new Vector2(0, 0);
        // img.rectTransform.DOAnchorPos(new Vector2(0, 0), 3f);    
        img.rectTransform.DOMove(new Vector2(0,-150),3f);    }

    // Update is called once per frame
    void Update()
    {

    }
}
