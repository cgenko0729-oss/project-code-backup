using UnityEngine;

public class AfterImageAction : MonoBehaviour
{
    [Header("消える速さ")]
    [SerializeField] private float fadeSpeed = 1.5f;

    private Material[] _materials; // 複数のマテリアルを保存する配列
    private int _colorPropertyId;

    void Start()
    {
        // 初期アニメーション再生
        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.Play("IdleAction");
        }

        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // ★重要：.materials (複数形) で取得すると、全マテリアルのインスタンス配列が取れます
            _materials = renderer.materials;
        }

        // プロパティIDの特定（URPなら_BaseColor、Standardなら_Color）
        // 念のため両方チェックして、存在する方を採用します
        if (_materials != null && _materials.Length > 0)
        {
            if (_materials[0].HasProperty("_BaseColor"))
            {
                _colorPropertyId = Shader.PropertyToID("_BaseColor");
            }
            else
            {
                _colorPropertyId = Shader.PropertyToID("_Color");
            }
        }
    }

    void Update()
    {
        if (_materials == null || _materials.Length == 0) return;

        float currentAlpha = 0f;

        // ★全マテリアルをループして透明度を下げる
        foreach (var mat in _materials)
        {
            Color color = mat.GetColor(_colorPropertyId);

            // アルファ値を減らす
            color.a -= fadeSpeed * Time.deltaTime;

            // 変更を適用
            mat.SetColor(_colorPropertyId, color);

            // 削除判定用に今のアルファ値を記録
            currentAlpha = color.a;
        }

        // 完全に透明になったら削除
        if (currentAlpha <= 0f)
        {
            Destroy(gameObject);
        }
    }
}