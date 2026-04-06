using UnityEngine;

public class OptionButtonController : MonoBehaviour
{
    public GameObject optionMenu;
    public GameObject titleGroup;
    public GameObject BgmBer;
    public GameObject SEBer;
    public void OnClick()
    {
        titleGroup.SetActive(false);

        CanvasGroup optioncanvas = optionMenu.GetComponent<CanvasGroup>();
        optioncanvas.alpha = 1f; // 透明度を1に設定（表示）
        optioncanvas.interactable = true; // インタラクションを有効化
        optioncanvas.blocksRaycasts = true; // レイキャストを有効化

        //optionMenu.SetActive(true);
        //BgmBer.SetActive(true);
        //SEBer.SetActive(true);
    }

    public void CloseOptionMeu()
    {
        CanvasGroup optioncanvas = optionMenu.GetComponent<CanvasGroup>();
        optioncanvas.alpha = 0f; // 透明度を0に設定（非表示）
        optioncanvas.interactable = false; // インタラクションを無効化
        optioncanvas.blocksRaycasts = false; // レイキャストを無効化

        titleGroup.SetActive(true);
    }

}
